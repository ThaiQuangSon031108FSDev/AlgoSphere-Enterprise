namespace AlgoSphere.Application.Interfaces;

public record AIResponse(string Explanation, List<int> HighlightIds);

public interface IAIService
{
    Task<AIResponse> GetHintAsync(string currentCode, string stateJson, string errorMessage);
}
