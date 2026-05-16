namespace AlgoSphere.Application.Interfaces;

public interface ITournamentService
{
    /// <summary>
    /// Called after a match ends. Checks if the current round is complete and,
    /// if so, generates next-round matches and broadcasts BracketUpdated via SignalR.
    /// </summary>
    Task AdvanceRoundIfCompleteAsync(int tournamentId, CancellationToken ct = default);

    /// <summary>
    /// Forfeits a match by assigning the winner to the non-forfeiting player.
    /// Used by the timeout worker when a player fails to submit within the deadline.
    /// </summary>
    Task ForfeitMatchAsync(int matchId, int forfeitedUserId, CancellationToken ct = default);
}
