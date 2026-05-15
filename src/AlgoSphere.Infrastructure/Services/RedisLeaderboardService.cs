using AlgoSphere.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AlgoSphere.Infrastructure.Services;

public class RedisLeaderboardService : ILeaderboardService
{
    private readonly IDatabase? _db;
    private readonly IAlgoSphereDbContext _context;
    private readonly ILogger<RedisLeaderboardService> _logger;
    private const string LeaderboardKey = "algosphere:leaderboard";

    public RedisLeaderboardService(
        IConfiguration config,
        IAlgoSphereDbContext context,
        ILogger<RedisLeaderboardService> logger)
    {
        _context = context;
        _logger = logger;
        try
        {
            var redis = ConnectionMultiplexer.Connect(
                config.GetConnectionString("Redis") ?? "localhost:6379");
            _db = redis.GetDatabase();
        }
        catch
        {
            _logger.LogWarning("Redis unavailable — leaderboard will query SQL Server directly.");
            _db = null;
        }
    }

    public async Task UpdateScoreAsync(string username, double score)
    {
        if (_db != null)
            await _db.SortedSetAddAsync(LeaderboardKey, username, score);
    }

    public async Task<List<LeaderboardEntry>> GetTopRankingsAsync(int count)
    {
        // Try Redis cache first
        if (_db != null)
        {
            try
            {
                var cached = await _db.SortedSetRangeByRankWithScoresAsync(
                    LeaderboardKey, 0, count - 1, Order.Descending);
                if (cached.Length > 0)
                    return cached.Select((r, i) => new LeaderboardEntry(r.Element!, r.Score, i + 1)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed, falling back to SQL.");
            }
        }

        // Fallback: SQL Server (source of truth from RankPoints)
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => u.Status == "Active")
            .OrderByDescending(u => u.RankPoints)
            .Take(count)
            .Select(u => new { u.Username, u.RankPoints })
            .ToListAsync();

        return users.Select((u, i) => new LeaderboardEntry(u.Username, u.RankPoints, i + 1)).ToList();
    }
}
