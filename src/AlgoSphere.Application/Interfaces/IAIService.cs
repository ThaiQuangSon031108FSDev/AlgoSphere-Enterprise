namespace AlgoSphere.Application.Interfaces;

public record AIResponse(string Explanation, List<int> HighlightIds);

public interface IAIService
{
    /// <summary>One-shot: waits for full response then returns.</summary>
    Task<AIResponse> GetHintAsync(string currentCode, string stateJson, string errorMessage);
}
