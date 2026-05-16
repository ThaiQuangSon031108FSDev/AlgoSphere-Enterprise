using AlgoSphere.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AlgoSphere.Infrastructure.Services;

/// <summary>
/// Scheduled fallback for tournament automation.
/// Runs every 5 minutes and checks for matches whose RoundDeadlineUtc has passed
/// without a winner being recorded (e.g., disconnected player, sandbox timeout).
///
/// Action: forfeit the non-submitting player → ITournamentService advances the round.
/// </summary>
public class TournamentTimeoutWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TournamentTimeoutWorker> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    public TournamentTimeoutWorker(IServiceScopeFactory scopeFactory, ILogger<TournamentTimeoutWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TournamentTimeoutWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredMatchesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TournamentTimeoutWorker.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessExpiredMatchesAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<IAlgoSphereDbContext>();
        var tournamentService = scope.ServiceProvider.GetRequiredService<ITournamentService>();

        var now = DateTime.UtcNow;

        // Find all expired matches: deadline passed, no winner, tournament is Ongoing
        var expiredMatches = await context.Matches
            .Include(m => m.Tournament)
            .Where(m =>
                m.WinnerId == null &&
                m.Status == "Pending" &&
                m.RoundDeadlineUtc != null &&
                m.RoundDeadlineUtc < now &&
                m.Tournament.Status == "Ongoing")
            .ToListAsync(ct);

        if (expiredMatches.Count == 0) return;

        _logger.LogWarning("TournamentTimeoutWorker: {Count} expired matches found. Processing forfeits.", expiredMatches.Count);

        foreach (var match in expiredMatches)
        {
            // Determine who to forfeit:
            // If both players are present, forfeit Player2 (Player1 had priority/higher seed).
            // If one slot is null (bye), this shouldn't happen, but guard anyway.
            int? forfeitUserId = match.Player2Id ?? match.Player1Id;

            if (forfeitUserId == null)
            {
                _logger.LogWarning("Match {MatchId} has no valid players to forfeit. Skipping.", match.Id);
                continue;
            }

            _logger.LogInformation("Forfeiting Match {MatchId} — user {UserId} timed out.", match.Id, forfeitUserId.Value);
            await tournamentService.ForfeitMatchAsync(match.Id, forfeitUserId.Value, ct);
        }
    }
}
