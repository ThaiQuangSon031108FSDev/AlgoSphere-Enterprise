using MediatR;
using Microsoft.EntityFrameworkCore;
using AlgoSphere.Application.Interfaces;

namespace AlgoSphere.Application.Features.Tournaments.Queries.GetTournamentBrackets;

public record GetTournamentBracketsQuery(int TournamentId) : IRequest<List<MatchDto>>;

public record MatchDto(int Id, string Player1, string Player2, string Winner, string BracketPosition);

public class GetTournamentBracketsQueryHandler : IRequestHandler<GetTournamentBracketsQuery, List<MatchDto>>
{
    private readonly IAlgoSphereDbContext _context;

    public GetTournamentBracketsQueryHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<List<MatchDto>> Handle(GetTournamentBracketsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Matches
            .Where(m => m.TournamentId == request.TournamentId)
            .Select(m => new MatchDto(
                m.Id, 
                m.Player1 != null ? m.Player1.Username : "TBD", 
                m.Player2 != null ? m.Player2.Username : "TBD",
                m.WinnerId.HasValue ? "Winner" : "TBD",
                m.BracketPosition))
            .ToListAsync(cancellationToken);
    }
}
