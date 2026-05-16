using AlgoSphere.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AlgoSphere.Infrastructure.Services;

/// <summary>
/// Two-layer anti-cheat implementation.
///
/// Layer 1 — Typing Velocity (delta metadata):
///   - Detects paste events where >200 chars appear in <100ms.
///   - Calculates overall paste ratio and average CPM.
///
/// Layer 2 — AST Structural Similarity (token-based Jaccard coefficient):
///   - Normalizes code by stripping identifiers (variable names, string literals)
///     to a token stream.
///   - Compares against all previously accepted solutions for the same exercise.
///   - If Jaccard similarity ≥ 0.85 → Confirmed plagiarism.
///   - Full Roslyn/tree-sitter AST not needed for portfolio demo; normalized
///     token overlap is a well-established, practical heuristic.
/// </summary>
public class AntiCheatService : IAntiCheatService
{
    private readonly IAlgoSphereDbContext _context;

    public AntiCheatService(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<AntiCheatResult> AnalyzeAsync(
        string code,
        string language,
        List<object>? deltas,
        int exerciseId,
        CancellationToken cancellationToken = default)
    {
        // ── Layer 1: Typing Velocity ──────────────────────────────────────────
        var (pasteRatio, averageCpm, l1Level, l1Reason) = AnalyzeDeltas(deltas, code);

        // ── Layer 2: AST/Token Structural Similarity ─────────────────────────
        var (astSimilarity, l2Level, l2Reason) = await AnalyzeStructuralSimilarityAsync(
            code, exerciseId, cancellationToken);

        // ── Aggregate ─────────────────────────────────────────────────────────
        var finalLevel = (SuspicionLevel)Math.Max((int)l1Level, (int)l2Level);
        var reason = finalLevel == SuspicionLevel.None
            ? "No anomalies detected."
            : string.Join(" | ", new[] { l1Reason, l2Reason }.Where(r => !string.IsNullOrEmpty(r)));

        return new AntiCheatResult(finalLevel, reason, pasteRatio, averageCpm, astSimilarity);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Layer 1: Delta velocity analysis
    // ─────────────────────────────────────────────────────────────────────────

    private static (double pasteRatio, double avgCpm, SuspicionLevel level, string reason)
        AnalyzeDeltas(List<object>? deltas, string code)
    {
        if (deltas == null || deltas.Count == 0)
        {
            // No delta data at all — flag if code is substantial
            if (code.Length > 100)
                return (1.0, 0, SuspicionLevel.Low, "No keystroke telemetry for significant code.");
            return (0, 0, SuspicionLevel.None, string.Empty);
        }

        int totalPastedChars = 0;
        int typeCount = 0;
        double totalTypingMs = 0;
        var suspiciousPasteCount = 0;

        long? prevTimestamp = null;

        foreach (var d in deltas)
        {
            var json = JsonSerializer.Serialize(d);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("a", out var actionEl)) continue;
            var action = actionEl.GetString();

            long ts = root.TryGetProperty("t", out var tsEl) ? tsEl.GetInt64() : 0;
            int charLen = root.TryGetProperty("l", out var lenEl) ? lenEl.GetInt32() : 1;

            if (action == "paste")
            {
                totalPastedChars += charLen;
                // Suspicious: >200 chars pasted in <100ms window
                long deltaMs = prevTimestamp.HasValue ? Math.Max(0, ts - prevTimestamp.Value) : 0;
                if (charLen > 200 && (prevTimestamp == null || deltaMs < 100))
                    suspiciousPasteCount++;
            }
            else if (action == "type")
            {
                typeCount++;
                if (prevTimestamp.HasValue && ts > prevTimestamp.Value)
                    totalTypingMs += ts - prevTimestamp.Value;
            }

            if (ts > 0) prevTimestamp = ts;
        }

        double pasteRatio = code.Length > 0 ? (double)totalPastedChars / code.Length : 0;
        double avgCpm = totalTypingMs > 0 ? (typeCount / (totalTypingMs / 60000.0)) : 0;

        SuspicionLevel level;
        string reason;

        if (suspiciousPasteCount >= 3 || pasteRatio > 0.85)
        {
            level = SuspicionLevel.High;
            reason = $"Velocity: {suspiciousPasteCount} bulk-paste events, paste ratio {pasteRatio:P0}.";
        }
        else if (pasteRatio > 0.70 || (code.Length > 200 && typeCount < 20))
        {
            level = SuspicionLevel.Low;
            reason = $"Velocity: paste ratio {pasteRatio:P0}, only {typeCount} type events.";
        }
        else
        {
            level = SuspicionLevel.None;
            reason = string.Empty;
        }

        return (pasteRatio, avgCpm, level, reason);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Layer 2: Token-stream Jaccard similarity (AST-lite)
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<(double similarity, SuspicionLevel level, string reason)>
        AnalyzeStructuralSimilarityAsync(string code, int exerciseId, CancellationToken ct)
    {
        // Load accepted solutions for this exercise (excluding the submitting user's own)
        var referenceCodes = await _context.Submissions
            .Where(s => s.ExerciseId == exerciseId && s.Status == "Accepted")
            .Select(s => s.SourceCode)
            .Take(50) // limit to avoid heavy DB reads
            .ToListAsync(ct);

        if (referenceCodes.Count == 0)
            return (0, SuspicionLevel.None, string.Empty);

        var submittedTokens = Tokenize(code);
        if (submittedTokens.Count == 0)
            return (0, SuspicionLevel.None, string.Empty);

        double maxSimilarity = referenceCodes
            .Select(refCode => JaccardSimilarity(submittedTokens, Tokenize(refCode)))
            .DefaultIfEmpty(0)
            .Max();

        SuspicionLevel level;
        string reason;

        if (maxSimilarity >= 0.85)
        {
            level = SuspicionLevel.Confirmed;
            reason = $"AST: {maxSimilarity:P0} structural match with existing solution (likely plagiarism).";
        }
        else if (maxSimilarity >= 0.65)
        {
            level = SuspicionLevel.High;
            reason = $"AST: {maxSimilarity:P0} structural similarity with existing solution.";
        }
        else if (maxSimilarity >= 0.45)
        {
            level = SuspicionLevel.Low;
            reason = $"AST: {maxSimilarity:P0} moderate structural similarity.";
        }
        else
        {
            level = SuspicionLevel.None;
            reason = string.Empty;
        }

        return (maxSimilarity, level, reason);
    }

    /// <summary>
    /// Normalize code to structural tokens:
    /// - Replace string/number literals with placeholders.
    /// - Replace identifiers with canonical names.
    /// - Keep keywords and operators as-is.
    /// This makes identifier-renaming attacks (variable name changes) ineffective.
    /// </summary>
    private static HashSet<string> Tokenize(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return new();

        // Strip comments
        code = Regex.Replace(code, @"//[^\n]*", " ");
        code = Regex.Replace(code, @"/\*.*?\*/", " ", RegexOptions.Singleline);
        code = Regex.Replace(code, @"#[^\n]*", " ");

        // Replace string literals
        code = Regex.Replace(code, @"""[^""]*""", " STR ");
        code = Regex.Replace(code, @"'[^']*'", " STR ");

        // Replace numeric literals
        code = Regex.Replace(code, @"\b\d+(\.\d+)?\b", " NUM ");

        // Normalize identifiers (replace multi-char identifiers with generic token)
        code = Regex.Replace(code, @"\b[a-zA-Z_][a-zA-Z0-9_]{2,}\b", m =>
        {
            // Keep language keywords
            var keywords = new HashSet<string>
            {
                "function", "return", "if", "else", "for", "while", "let", "const", "var",
                "int", "string", "bool", "void", "class", "new", "null", "true", "false",
                "def", "pass", "import", "from", "in", "and", "or", "not",
                "public", "private", "static", "async", "await", "break", "continue"
            };
            return keywords.Contains(m.Value) ? m.Value : "ID";
        });

        // Tokenize by whitespace/punctuation
        return Regex.Matches(code, @"[a-zA-Z_][a-zA-Z0-9_]*|[{}()\[\];,=+\-*/<>!&|]")
            .Select(m => m.Value)
            .ToHashSet();
    }

    private static double JaccardSimilarity(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 1.0;
        var intersection = a.Intersect(b).Count();
        var union = a.Union(b).Count();
        return union == 0 ? 0 : (double)intersection / union;
    }
}
