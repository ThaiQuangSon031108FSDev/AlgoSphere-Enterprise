namespace AlgoSphere.Application.Interfaces;

/// <summary>
/// Abstraction over SignalR broadcasting for tournament events.
/// Keeps Infrastructure layer decoupled from AlgoSphere.Api assembly.
/// The implementation in Api resolves IHubContext&lt;ArenaHub&gt; from DI.
/// </summary>
public interface ITournamentNotifier
{
    Task BroadcastBracketUpdatedAsync(int tournamentId, CancellationToken ct = default);
    Task BroadcastMatchReadyAsync(int tournamentId, int matchId, int? player1Id, int? player2Id, DateTime deadline, CancellationToken ct = default);
    Task BroadcastTournamentCompletedAsync(int tournamentId, int? championId, CancellationToken ct = default);
}
