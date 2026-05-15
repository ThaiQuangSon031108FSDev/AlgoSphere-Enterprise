using AlgoSphere.Application.Features.Tournaments.Commands.JoinTournament;
using AlgoSphere.Application.Features.Tournaments.Queries.GetTournaments;
using AlgoSphere.Application.Features.Tournaments.Queries.GetTournamentBrackets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlgoSphere.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TournamentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TournamentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<TournamentDto>>> GetTournaments()
    {
        return await _mediator.Send(new GetTournamentsQuery());
    }

    [HttpGet("{id}/brackets")]
    public async Task<ActionResult<List<MatchDto>>> GetBrackets(int id)
    {
        return await _mediator.Send(new GetTournamentBracketsQuery(id));
    }

    [HttpPost("{id}/join")]
    [Authorize]
    public async Task<ActionResult<JoinResult>> Join(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _mediator.Send(new JoinTournamentCommand(id, userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
