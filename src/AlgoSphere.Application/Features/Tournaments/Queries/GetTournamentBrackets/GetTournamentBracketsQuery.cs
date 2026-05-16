using MediatR;
using Microsoft.EntityFrameworkCore;
using AlgoSphere.Application.Interfaces;

namespace AlgoSphere.Application.Features.Tournaments.Queries.GetTournamentBrackets;

public record GetTournamentBracketsQuery(int TournamentId) : IRequest<List<MatchDto>>;

public record MatchDto(
    int Id, 
    string Player1, 
    string Player2, 
    int? WinnerId, 
    string? WinnerName, 
    string BracketPosition,
    DateTime? RoundDeadlineUtc,
    string Status);

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
                m.WinnerId,
                m.WinnerId != null ? (m.Player1 != null && m.Player1Id == m.WinnerId ? m.Player1.Username : (m.Player2 != null ? m.Player2.Username : "Winner")) : null,
                m.BracketPosition,
                m.RoundDeadlineUtc,
                m.Status))
            .ToListAsync(cancellationToken);
    }
}
