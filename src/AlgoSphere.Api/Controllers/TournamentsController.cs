using AlgoSphere.Application.Features.Tournaments.Queries.GetTournaments;
using AlgoSphere.Application.Features.Tournaments.Queries.GetTournamentBrackets;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
}
