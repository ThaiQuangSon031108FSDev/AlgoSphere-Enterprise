namespace AlgoSphere.Application.Interfaces;

public record ExecutionResult(bool Success, string TraceLog, string Message, int TimeMs, int MemoryKb);

public interface IExecutionService
{
    Task<ExecutionResult> ExecuteAsync(string code, string language, int exerciseId);
}
