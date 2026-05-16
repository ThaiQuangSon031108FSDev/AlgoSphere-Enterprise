using AlgoSphere.Application.Interfaces;
using AlgoSphere.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlgoSphere.Application.Features.Exercises.Commands.ExecuteCode;

public record ExecuteCodeResponse(
    ExecutionResult Execution,
    GamificationDto? Gamification
);

public record GamificationDto(
    int XpEarned,
    int BonusXp,
    int TotalXp,
    int Level,
    bool IsLevelUp,
    bool IsSpeedDemon,
    bool IsMemoryMaster,
    int CurrentLevelXp,
    int NextLevelXp
);

public record ExecuteCodeCommand(int UserId, int ExerciseId, string Code, string Language, List<object>? Deltas) : IRequest<ExecuteCodeResponse>;

public class ExecuteCodeCommandHandler : IRequestHandler<ExecuteCodeCommand, ExecuteCodeResponse>
{
    private readonly IExecutionService _executionService;
    private readonly IAlgoSphereDbContext _context;

    public ExecuteCodeCommandHandler(IExecutionService executionService, IAlgoSphereDbContext context)
    {
        _executionService = executionService;
        _context = context;
    }

    public async Task<ExecuteCodeResponse> Handle(ExecuteCodeCommand request, CancellationToken cancellationToken)
    {
        // 1. Load Exercise with TestCases
        var exercise = await _context.Exercises
            .Include(e => e.TestCases)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);
            
        if (exercise == null) throw new Exception("Exercise not found");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        // Track Deltas for Anti-Cheat analysis
        var deltasJson = request.Deltas != null ? System.Text.Json.JsonSerializer.Serialize(request.Deltas) : "[]";

        // 2. Prepare test cases and run the code in Sandbox
        var testCases = exercise.TestCases.Select(tc => new TestCaseDto(tc.InputJson, tc.ExpectedOutputJson, tc.IsHidden)).ToList();
        var result = await _executionService.ExecuteAsync(request.Code, request.Language, exercise.EntryPoint, testCases);

        // 3. Save Submission
        var submission = new Submission
        {
            UserId = request.UserId,
            ExerciseId = request.ExerciseId,
            SourceCode = request.Code,
            Language = request.Language,
            Status = result.Success ? "Accepted" : "Failed",
            ExecutionTimeMs = result.TimeMs,
            MemoryUsedKb = result.MemoryKb,
            CreatedAt = DateTime.UtcNow,
            IsSuspicious = AnalyzeCheat(request.Deltas, request.Code),
            CodeDelta = new SubmissionCodeDelta
            {
                EventDeltasJson = deltasJson
            }
        };
        _context.Submissions.Add(submission);

        // 4. Award XP if Accepted

        GamificationDto? gami = null;
        
        int retryCount = 0;
        bool saved = false;

        while (!saved && retryCount < 3)
        {
            try
            {
                if (result.Success && user != null && exercise != null)
                {
                    // Performance Badges logic
                    bool isSpeedDemon = result.TimeMs <= (exercise.TimeLimitMs * 0.3);
                    bool isMemoryMaster = result.MemoryKb <= (exercise.MemoryLimitKb * 0.3);

                    // Check if user already solved this exercise
                    var alreadySolved = await _context.Submissions
                        .AnyAsync(s => s.UserId == request.UserId && s.ExerciseId == request.ExerciseId && s.Status == "Accepted", cancellationToken);

                    int baseXp = 0;
                    int bonusXp = 0;
                    bool isLevelUp = false;

                    if (!alreadySolved)
                    {
                        baseXp = exercise.Points > 0 ? exercise.Points : 20;
                        
                        double multiplier = 1.0;
                        if (isSpeedDemon) multiplier += 0.1;
                        if (isMemoryMaster) multiplier += 0.1;

                        int finalXp = (int)(baseXp * multiplier);
                        bonusXp = finalXp - baseXp;
                        
                        int oldLevel = (int)Math.Sqrt(user.RankPoints / 50.0) + 1;
                        user.RankPoints += finalXp;
                        int newLevel = (int)Math.Sqrt(user.RankPoints / 50.0) + 1;
                        
                        isLevelUp = newLevel > oldLevel;
                    }

                    var level = (int)Math.Sqrt(user.RankPoints / 50.0) + 1;
                    var levelFloor = (int)Math.Pow(level - 1, 2) * 50;
                    var levelCeil = (int)Math.Pow(level, 2) * 50;

                    gami = new GamificationDto(
                        XpEarned: baseXp,
                        BonusXp: bonusXp,
                        TotalXp: user.RankPoints,
                        Level: level,
                        IsLevelUp: isLevelUp,
                        IsSpeedDemon: isSpeedDemon,
                        IsMemoryMaster: isMemoryMaster,
                        CurrentLevelXp: user.RankPoints - levelFloor,
                        NextLevelXp: levelCeil - levelFloor
                    );
                }

                await _context.SaveChangesAsync(cancellationToken);
                saved = true;
            }
            catch (DbUpdateConcurrencyException)
            {
                retryCount++;
                if (retryCount >= 3) throw;

                // Reload user to get latest RankPoints and RowVersion
                if (user != null)
                {
                    await _context.Entry(user).ReloadAsync(cancellationToken);
                }
            }
        }

        return new ExecuteCodeResponse(result, gami);

    }

    private bool AnalyzeCheat(List<object>? deltas, string code)
    {
        if (deltas == null || deltas.Count == 0) return code.Length > 100; // Flag if no deltas for significant code

        int pasteCount = 0;
        int typeCount = 0;
        int pasteLen = 0;

        foreach (var d in deltas)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(d);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("a", out var action))
            {
                var actionStr = action.GetString();
                if (actionStr == "paste")
                {
                    pasteCount++;
                    if (root.TryGetProperty("l", out var len)) pasteLen += len.GetInt32();
                }
                else if (actionStr == "type")
                {
                    typeCount++;
                }
            }
        }

        // Flag if more than 70% of code is pasted in large blocks
        if (code.Length > 50 && (double)pasteLen / code.Length > 0.7) return true;

        // Flag if very few "type" events for long code
        if (code.Length > 200 && typeCount < 20) return true;

        return false;
    }
}
