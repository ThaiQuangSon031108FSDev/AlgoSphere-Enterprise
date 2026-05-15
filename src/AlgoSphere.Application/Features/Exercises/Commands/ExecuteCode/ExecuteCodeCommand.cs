using AlgoSphere.Application.Interfaces;
using MediatR;

namespace AlgoSphere.Application.Features.Exercises.Commands.ExecuteCode;

public record ExecuteCodeCommand(int ExerciseId, string Code, string Language) : IRequest<ExecutionResult>;

public class ExecuteCodeCommandHandler : IRequestHandler<ExecuteCodeCommand, ExecutionResult>
{
    private readonly IExecutionService _executionService;

    public ExecuteCodeCommandHandler(IExecutionService executionService)
    {
        _executionService = executionService;
    }

    public async Task<ExecutionResult> Handle(ExecuteCodeCommand request, CancellationToken cancellationToken)
    {
        return await _executionService.ExecuteAsync(request.Code, request.Language, request.ExerciseId);
    }
}
