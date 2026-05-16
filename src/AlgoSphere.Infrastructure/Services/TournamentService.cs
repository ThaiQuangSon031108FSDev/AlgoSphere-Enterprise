using AlgoSphere.Application.Interfaces;
using AlgoSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlgoSphere.Infrastructure.Services;

/// <summary>
/// Event-driven tournament automation.
///
/// Flow (happy path):
///   Match ends → SubmitResult in ArenaHub → calls AdvanceRoundIfCompleteAsync
///   → checks if ALL matches in current round have a winner
///   → if yes: generate next round's Match rows + broadcast BracketUpdated
///   → if it was the final round: mark Tournament as Completed.
///
/// Forfeit flow (timeout):
///   TournamentTimeoutWorker polls every 5 min → finds matches past RoundDeadlineUtc
///   → calls ForfeitMatchAsync → then AdvanceRoundIfCompleteAsync.
/// </summary>
public class TournamentService : ITournamentService
{
    private readonly IAlgoSphereDbContext _context;
    private readonly ITournamentNotifier _notifier;

    public TournamentService(IAlgoSphereDbContext context, ITournamentNotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task AdvanceRoundIfCompleteAsync(int tournamentId, CancellationToken ct = default)
    {
        var tournament = await _context.Tournaments
            .Include(t => t.Matches)
            .FirstOrDefaultAsync(t => t.Id == tournamentId && t.Status == "Ongoing", ct);

        if (tournament == null) return;

        var allMatches = tournament.Matches.ToList();

        // Determine the highest round number currently in play
        var currentRound = allMatches
            .Where(m => m.Status != "Completed" && m.Status != "Forfeited")
            .Select(m => ParseRoundNumber(m.BracketPosition))
            .DefaultIfEmpty(0)
            .Max();

        if (currentRound == 0)
        {
            // All rounds complete → mark tournament done
            tournament.Status = "Completed";
            await _context.SaveChangesAsync(ct);
            await _notifier.BroadcastTournamentCompletedAsync(tournamentId, null, ct);
            return;
        }

        var roundMatches = allMatches
            .Where(m => ParseRoundNumber(m.BracketPosition) == currentRound)
            .ToList();

        // All matches in this round must have a winner
        if (roundMatches.Any(m => m.WinnerId == null))
            return; // Round not yet complete

        // Generate next round
        var winners = roundMatches
            .Where(m => m.WinnerId.HasValue)
            .Select(m => m.WinnerId!.Value)
            .ToList();

        if (winners.Count == 1)
        {
            // Grand finals completed — mark tournament done
            tournament.Status = "Completed";
            await _context.SaveChangesAsync(ct);
            await _notifier.BroadcastTournamentCompletedAsync(tournamentId, winners[0], ct);
            return;
        }

        int nextRound = currentRound + 1;
        var nextRoundDeadline = DateTime.UtcNow.AddMinutes(tournament.RoundDurationMinutes);
        var newMatches = new List<Match>();

        for (int i = 0; i < winners.Count / 2; i++)
        {
            newMatches.Add(new Match
            {
                TournamentId = tournamentId,
                Player1Id = winners[i * 2],
                Player2Id = winners[i * 2 + 1],
                BracketPosition = $"Round{nextRound}-Match{i + 1}",
                Status = "Pending",
                RoundDeadlineUtc = nextRoundDeadline
            });
        }

        // Bye for odd winners
        if (winners.Count % 2 != 0)
        {
            var byeWinner = winners[^1];
            newMatches.Add(new Match
            {
                TournamentId = tournamentId,
                Player1Id = byeWinner,
                Player2Id = null,
                WinnerId = byeWinner,
                BracketPosition = $"Round{nextRound}-Match{winners.Count / 2 + 1}",
                Status = "Completed",
                RoundDeadlineUtc = nextRoundDeadline
            });
        }

        _context.Matches.AddRange(newMatches);
        await _context.SaveChangesAsync(ct);

        // Broadcast to all tournament observers
        await _notifier.BroadcastBracketUpdatedAsync(tournamentId, ct);

        // Notify matched players individually
        foreach (var match in newMatches.Where(m => m.Status == "Pending"))
        {
            await _notifier.BroadcastMatchReadyAsync(
                tournamentId, match.Id, match.Player1Id, match.Player2Id, nextRoundDeadline, ct);
        }
    }

    public async Task ForfeitMatchAsync(int matchId, int forfeitedUserId, CancellationToken ct = default)
    {
        var match = await _context.Matches
            .FirstOrDefaultAsync(m => m.Id == matchId, ct);

        if (match == null || match.WinnerId.HasValue) return;

        // The non-forfeiting player wins
        match.WinnerId = match.Player1Id == forfeitedUserId
            ? match.Player2Id
            : match.Player1Id;

        match.Status = "Forfeited";
        await _context.SaveChangesAsync(ct);

        await AdvanceRoundIfCompleteAsync(match.TournamentId, ct);
    }

    private static int ParseRoundNumber(string bracketPosition)
    {
        // Expects format "RoundN-MatchM"
        var match = System.Text.RegularExpressions.Regex.Match(bracketPosition, @"Round(\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }
}
