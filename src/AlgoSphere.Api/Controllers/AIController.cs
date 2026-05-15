using AlgoSphere.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AlgoSphere.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("hint")]
    public async Task<ActionResult<AIResponse>> GetHint(AIHintRequest request)
    {
        return await _aiService.GetHintAsync(request.CurrentCode, request.StateJson, request.ErrorMessage);
    }
}

public record AIHintRequest(string CurrentCode, string StateJson, string ErrorMessage);
