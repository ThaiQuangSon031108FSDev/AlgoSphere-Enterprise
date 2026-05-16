namespace AlgoSphere.Application.Interfaces;

public record ExecutionResult(bool Success, string TraceLog, string Message, int TimeMs, int MemoryKb);

public record TestCaseDto(string InputJson, string ExpectedOutputJson, bool IsHidden);

public interface IExecutionService
{
    Task<ExecutionResult> ExecuteAsync(string code, string language, string entryPoint, IEnumerable<TestCaseDto> testCases);
}
