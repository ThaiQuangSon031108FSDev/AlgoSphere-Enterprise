using AlgoSphere.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AlgoSphere.Api.Hubs;

/// <summary>
/// Implements ITournamentNotifier using IHubContext&lt;ArenaHub&gt;.
/// Registered in Api DI, so Infrastructure never references Api assembly directly.
/// </summary>
public class ArenaHubTournamentNotifier : ITournamentNotifier
{
    private readonly IHubContext<ArenaHub> _hub;

    public ArenaHubTournamentNotifier(IHubContext<ArenaHub> hub)
    {
        _hub = hub;
    }

    public Task BroadcastBracketUpdatedAsync(int tournamentId, CancellationToken ct = default)
        => _hub.Clients.Group($"tournament_{tournamentId}").SendAsync("BracketUpdated", cancellationToken: ct);

    public Task BroadcastMatchReadyAsync(int tournamentId, int matchId, int? player1Id, int? player2Id, DateTime deadline, CancellationToken ct = default)
        => _hub.Clients.Group($"tournament_{tournamentId}").SendAsync("MatchReady", new
        {
            MatchId = matchId,
            Player1Id = player1Id,
            Player2Id = player2Id,
            Deadline = deadline
        }, cancellationToken: ct);

    public Task BroadcastTournamentCompletedAsync(int tournamentId, int? championId, CancellationToken ct = default)
        => _hub.Clients.Group($"tournament_{tournamentId}").SendAsync("TournamentCompleted", new
        {
            TournamentId = tournamentId,
            ChampionId = championId
        }, cancellationToken: ct);
}
