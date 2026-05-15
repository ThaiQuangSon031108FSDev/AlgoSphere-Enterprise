using AlgoSphere.Application.Features.Admin.Commands;
using AlgoSphere.Application.Features.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoSphere.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]  // Phase 2: All Admin endpoints require Admin role
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ── Analytics Dashboard ───────────────────────────────────────────────────
    [HttpGet("analytics")]
    public async Task<ActionResult<AdminAnalyticsDto>> GetAnalytics()
    {
        return await _mediator.Send(new GetAdminAnalyticsQuery());
    }

    // ── Users ─────────────────────────────────────────────────────────────────
    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers([FromQuery] string? search = null)
    {
        return await _mediator.Send(new GetAdminUsersQuery(search));
    }

    [HttpPatch("users/{id}/status")]
    public async Task<ActionResult> SetUserStatus(int id, [FromBody] SetStatusRequest req)
    {
        var ok = await _mediator.Send(new SetUserStatusCommand(id, req.Status));
        return ok ? NoContent() : NotFound();
    }

    // ── Exercises ─────────────────────────────────────────────────────────────
    [HttpGet("exercises")]
    public async Task<ActionResult<List<AdminExerciseDto>>> GetExercises()
    {
        return await _mediator.Send(new GetAdminExercisesQuery());
    }

    [HttpDelete("exercises/{id}")]
    public async Task<ActionResult> DeleteExercise(int id)
    {
        var ok = await _mediator.Send(new DeleteExerciseCommand(id));
        return ok ? NoContent() : NotFound();
    }
}

public record SetStatusRequest(string Status);
