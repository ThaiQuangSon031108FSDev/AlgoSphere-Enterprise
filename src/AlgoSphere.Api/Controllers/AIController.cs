using AlgoSphere.Application.Interfaces;
using AlgoSphere.Infrastructure.AI;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

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

    /// <summary>
    /// One-shot hint — returns full response once AI finishes.
    /// </summary>
    [HttpPost("hint")]
    public async Task<ActionResult<AIResponse>> GetHint([FromBody] AIHintRequest request)
    {
        var result = await _aiService.GetHintAsync(
            request.CurrentCode,
            request.StateJson,
            request.ErrorMessage);

        return Ok(result);
    }

    /// <summary>
    /// SSE streaming hint — streams tokens as Server-Sent Events.
    /// Frontend should use EventSource or fetch with ReadableStream.
    /// </summary>
    [HttpPost("hint/stream")]
    public async Task StreamHint([FromBody] AIHintRequest request, CancellationToken ct)
    {
        // Only GeminiAIService supports streaming
        if (_aiService is not GeminiAIService gemini)
        {
            // Fallback: send one-shot as SSE
            var result = await _aiService.GetHintAsync(
                request.CurrentCode, request.StateJson, request.ErrorMessage);

            Response.ContentType = "text/event-stream";
            await Response.WriteAsync($"data: {JsonSerializer.Serialize(result.Explanation)}\n\n", ct);
            await Response.WriteAsync("data: [DONE]\n\n", ct);
            return;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        await foreach (var chunk in gemini.StreamHintAsync(
            request.CurrentCode, request.StateJson, request.ErrorMessage, ct))
        {
            var payload = JsonSerializer.Serialize(chunk);
            var line = $"data: {payload}\n\n";
            await Response.WriteAsync(line, Encoding.UTF8, ct);
            await Response.Body.FlushAsync(ct);
        }

        await Response.WriteAsync("data: [DONE]\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }
}

public record AIHintRequest(string CurrentCode, string StateJson, string ErrorMessage);
