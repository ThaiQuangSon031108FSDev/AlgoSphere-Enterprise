using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AlgoSphere.Application.Interfaces;
using StackExchange.Redis;

namespace AlgoSphere.Application.Features.Topics.Queries.GetTopics;

public record GetTopicsQuery : IRequest<List<TopicDto>>;

public record TopicDto(int Id, string Name, string Description, int OrderIndex);

public class GetTopicsQueryHandler : IRequestHandler<GetTopicsQuery, List<TopicDto>>
{
    private readonly IAlgoSphereDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<GetTopicsQueryHandler> _logger;

    public GetTopicsQueryHandler(
        IAlgoSphereDbContext context,
        IConfiguration config,
        ILogger<GetTopicsQueryHandler> logger)
    {
        _context = context;
        _config  = config;
        _logger  = logger;
    }

    public async Task<List<TopicDto>> Handle(GetTopicsQuery request, CancellationToken cancellationToken)
    {
        const string CacheKey = "algosphere:topics";

        // Try Redis — but never let it crash the request
        var cache = TryGetCache();
        if (cache is not null)
        {
            try
            {
                var cached = await cache.StringGetAsync(CacheKey);
                if (!cached.IsNullOrEmpty)
                    return System.Text.Json.JsonSerializer.Deserialize<List<TopicDto>>((string)cached!)!;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for key '{Key}', falling back to DB.", CacheKey);
            }
        }

        // DB fallback
        var topics = await _context.Topics
            .Select(t => new TopicDto(t.Id, t.Name, t.Description, t.OrderIndex))
            .ToListAsync(cancellationToken);

        // Best-effort write back to cache
        if (cache is not null)
        {
            try
            {
                await cache.StringSetAsync(
                    CacheKey,
                    System.Text.Json.JsonSerializer.Serialize(topics),
                    TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis write failed for key '{Key}'.", CacheKey);
            }
        }

        return topics;
    }

    private IDatabase? TryGetCache()
    {
        try
        {
            var connString = _config.GetConnectionString("Redis") ?? "localhost:6379";
            var opts = ConfigurationOptions.Parse(connString);
            opts.AbortOnConnectFail = false;        // never throw on connect
            opts.ConnectTimeout     = 1_000;        // 1 s timeout — fail fast
            opts.SyncTimeout        = 1_000;
            var mux = ConnectionMultiplexer.Connect(opts);
            return mux.IsConnected ? mux.GetDatabase() : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis unavailable — cache disabled for this request.");
            return null;
        }
    }
}
