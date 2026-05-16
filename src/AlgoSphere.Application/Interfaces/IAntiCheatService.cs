namespace AlgoSphere.Application.Interfaces;

/// <summary>Suspicion level returned by the two-layer anti-cheat pipeline.</summary>
public enum SuspicionLevel { None, Low, High, Confirmed }

public record AntiCheatResult(
    SuspicionLevel Level,
    string Reason,
    double PasteRatio,
    double AverageTypingCpm,
    double AstSimilarity   // 0.0–1.0; -1 = not computed
);

/// <summary>
/// Two-layer anti-cheat detection:
/// Layer 1 — Typing Velocity (client-side delta metadata)
/// Layer 2 — AST Structural Similarity (server-side, compared against prior accepted solutions)
/// </summary>
public interface IAntiCheatService
{
    /// <param name="code">Submitted source code.</param>
    /// <param name="language">Language identifier (javascript, csharp, python).</param>
    /// <param name="deltas">Keystroke delta events recorded by the editor.</param>
    /// <param name="exerciseId">Used to load reference solutions for AST comparison.</param>
    Task<AntiCheatResult> AnalyzeAsync(
        string code,
        string language,
        List<object>? deltas,
        int exerciseId,
        CancellationToken cancellationToken = default);
}
