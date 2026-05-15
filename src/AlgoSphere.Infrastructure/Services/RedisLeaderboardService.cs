using AlgoSphere.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace AlgoSphere.Infrastructure.Services;

public class RedisLeaderboardService : ILeaderboardService
{
    private readonly IDatabase _db;
    private const string LeaderboardKey = "algosphere:leaderboard";

    public RedisLeaderboardService(IConfiguration config)
    {
        var redis = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis") ?? "localhost:6379");
        _db = redis.GetDatabase();
    }

    public async Task UpdateScoreAsync(string username, double score)
    {
        await _db.SortedSetAddAsync(LeaderboardKey, username, score);
    }

    public async Task<List<LeaderboardEntry>> GetTopRankingsAsync(int count)
    {
        var results = await _db.SortedSetRangeByRankWithScoresAsync(LeaderboardKey, 0, count - 1, Order.Descending);
        
        var leaderboard = new List<LeaderboardEntry>();
        for (int i = 0; i < results.Length; i++)
        {
            leaderboard.Add(new LeaderboardEntry(results[i].Element!, results[i].Score, i + 1));
        }
        
        // Mock data nếu Redis chưa có dữ liệu
        if (leaderboard.Count == 0)
        {
            leaderboard.Add(new LeaderboardEntry("Master_Coder", 5000, 1));
            leaderboard.Add(new LeaderboardEntry("Algo_King", 4200, 2));
            leaderboard.Add(new LeaderboardEntry("Dev_Pro", 3800, 3));
        }

        return leaderboard;
    }
}
