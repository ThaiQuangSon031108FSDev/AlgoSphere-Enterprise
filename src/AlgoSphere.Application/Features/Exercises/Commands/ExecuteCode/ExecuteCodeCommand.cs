using AlgoSphere.Application.Interfaces;
using AlgoSphere.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlgoSphere.Application.Features.Exercises.Commands.ExecuteCode;

public record ExecuteCodeCommand(int UserId, int ExerciseId, string Code, string Language) : IRequest<ExecutionResult>;

public class ExecuteCodeCommandHandler : IRequestHandler<ExecuteCodeCommand, ExecutionResult>
{
    private readonly IExecutionService _executionService;
    private readonly IAlgoSphereDbContext _context;

    public ExecuteCodeCommandHandler(IExecutionService executionService, IAlgoSphereDbContext context)
    {
        _executionService = executionService;
        _context = context;
    }

    public async Task<ExecutionResult> Handle(ExecuteCodeCommand request, CancellationToken cancellationToken)
    {
        // 1. Run the code in Sandbox
        var result = await _executionService.ExecuteAsync(request.Code, request.Language, request.ExerciseId);

        // 2. Save Submission
        var submission = new Submission
        {
            UserId = request.UserId,
            ExerciseId = request.ExerciseId,
            SourceCode = request.Code,
            Language = request.Language,
            Status = result.Success ? "Accepted" : "Failed",
            ExecutionTimeMs = result.TimeMs,
            MemoryUsedKb = result.MemoryKb,
            CreatedAt = DateTime.UtcNow
        };
        _context.Submissions.Add(submission);

        // 3. Award XP if Accepted
        if (result.Success)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user != null)
            {
                // In MVP: Every accepted submission awards 10 RankPoints
                user.RankPoints += 10;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }
}
