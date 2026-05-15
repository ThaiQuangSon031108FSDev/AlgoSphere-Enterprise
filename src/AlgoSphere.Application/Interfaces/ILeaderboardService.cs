namespace AlgoSphere.Application.Interfaces;

public record LeaderboardEntry(string Username, double Score, int Rank);

public interface ILeaderboardService
{
    Task UpdateScoreAsync(string username, double score);
    Task<List<LeaderboardEntry>> GetTopRankingsAsync(int count);
}
