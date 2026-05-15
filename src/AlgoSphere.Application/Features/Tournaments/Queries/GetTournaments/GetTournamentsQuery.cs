using MediatR;
using Microsoft.EntityFrameworkCore;
using AlgoSphere.Application.Interfaces;

namespace AlgoSphere.Application.Features.Tournaments.Queries.GetTournaments;

public record GetTournamentsQuery() : IRequest<List<TournamentDto>>;

public record TournamentDto(int Id, string Title, string Description, DateTime StartDate, string Status, int ParticipantCount);

public class GetTournamentsQueryHandler : IRequestHandler<GetTournamentsQuery, List<TournamentDto>>
{
    private readonly IAlgoSphereDbContext _context;

    public GetTournamentsQueryHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<List<TournamentDto>> Handle(GetTournamentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tournaments
            .Select(t => new TournamentDto(t.Id, t.Title, t.Description, t.StartDate, t.Status, t.Participants.Count))
            .ToListAsync(cancellationToken);
    }
}
