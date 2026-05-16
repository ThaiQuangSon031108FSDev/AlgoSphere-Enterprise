using AlgoSphere.Application.Interfaces;
using AlgoSphere.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlgoSphere.Application.Features.Tournaments.Commands.GenerateBrackets;

public record GenerateBracketsCommand(int TournamentId) : IRequest<bool>;

public class GenerateBracketsCommandHandler : IRequestHandler<GenerateBracketsCommand, bool>
{
    private readonly IAlgoSphereDbContext _context;

    public GenerateBracketsCommandHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(GenerateBracketsCommand request, CancellationToken cancellationToken)
    {
        var tournament = await _context.Tournaments
            .Include(t => t.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);

        if (tournament == null || tournament.Participants.Count < 2)
            return false;

        // 1. Sort participants by RankPoints (ELO) descending
        var seededParticipants = tournament.Participants
            .OrderByDescending(p => p.User.RankPoints)
            .ToList();

        // 2. Generate First Round Matches
        // Algorithm: Seed 1 vs Seed Last, Seed 2 vs Seed Second Last, etc.
        int participantCount = seededParticipants.Count;
        int matchCount = participantCount / 2;

        var matches = new List<Match>();
        for (int i = 0; i < matchCount; i++)
        {
            var p1 = seededParticipants[i];
            var p2 = seededParticipants[participantCount - 1 - i];

            matches.Add(new Match
            {
                TournamentId = tournament.Id,
                Player1Id = p1.UserId,
                Player2Id = p2.UserId,
                BracketPosition = $"Round1-Match{i + 1}"
            });
        }

        // 3. Handle odd number of participants (Bye)
        if (participantCount % 2 != 0)
        {
            var pLast = seededParticipants[matchCount]; // Middle one if sorted 1..N
             matches.Add(new Match
            {
                TournamentId = tournament.Id,
                Player1Id = pLast.UserId,
                Player2Id = null, // Bye
                WinnerId = pLast.UserId,
                BracketPosition = $"Round1-Match{matchCount + 1}"
            });
        }

        _context.Matches.AddRange(matches);
        tournament.Status = "Ongoing";

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
