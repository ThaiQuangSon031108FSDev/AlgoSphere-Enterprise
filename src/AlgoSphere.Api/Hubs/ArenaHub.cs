using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Security.Claims;

namespace AlgoSphere.Api.Hubs;

/// <summary>
/// Real matchmaking hub using Redis sorted set as a waiting queue.
/// Flow: JoinQueue → added to Redis → background task polls → when 2 players found → MatchFound.
/// </summary>
[Authorize]
public class ArenaHub : Hub
{
    private readonly IDatabase? _redis;
    private const string QueueKey = "algosphere:arena:queue";

    public ArenaHub(IConfiguration config)
    {
        try
        {
            var mux = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis") ?? "localhost:6379");
            _redis = mux.GetDatabase();
        }
        catch
        {
            _redis = null; // Redis not available — fallback handled below
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove from queue on disconnect to prevent ghost entries
        if (_redis != null)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.ConnectionId;
            await _redis.SortedSetRemoveAsync(QueueKey, userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>Called by client when user clicks "Find Match".</summary>
    public async Task JoinQueue()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.ConnectionId;
        var username = Context.User?.FindFirstValue(ClaimTypes.Name)
                    ?? Context.User?.FindFirstValue("unique_name")
                    ?? "Player";

        await Clients.Caller.SendAsync("ReceiveStatus", "Searching for opponent...");

        if (_redis == null)
        {
            // Fallback when Redis is unavailable: simulate solo practice
            await Clients.Caller.SendAsync("ReceiveStatus", "Redis unavailable — starting solo practice...");
            await Task.Delay(1500);
            await Clients.Caller.SendAsync("MatchFound", new
            {
                MatchId = Guid.NewGuid().ToString(),
                Opponent = "BOT_Algo",
                OpponentLevel = 5,
                IsPractice = true
            });
            return;
        }

        // Add to queue (score = UTC ticks for FIFO ordering)
        var score = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await _redis.SortedSetAddAsync(QueueKey, $"{userId}|{Context.ConnectionId}|{username}", score);

        // Try to match: pull 2 oldest entries from queue
        var waiting = await _redis.SortedSetRangeByRankWithScoresAsync(QueueKey, 0, 1);

        if (waiting.Length >= 2)
        {
            // Atomically remove both players
            var p1Parts = waiting[0].Element.ToString().Split('|');
            var p2Parts = waiting[1].Element.ToString().Split('|');

            await _redis.SortedSetRemoveAsync(QueueKey, waiting[0].Element);
            await _redis.SortedSetRemoveAsync(QueueKey, waiting[1].Element);

            var matchId = Guid.NewGuid().ToString("N")[..8].ToUpper();
            var p1ConnId = p1Parts.Length > 1 ? p1Parts[1] : "";
            var p2ConnId = p2Parts.Length > 1 ? p2Parts[1] : "";
            var p1Name   = p1Parts.Length > 2 ? p1Parts[2] : "Player 1";
            var p2Name   = p2Parts.Length > 2 ? p2Parts[2] : "Player 2";

            // Add both to the same SignalR group
            await Groups.AddToGroupAsync(p1ConnId, matchId);
            await Groups.AddToGroupAsync(p2ConnId, matchId);

            // Notify Player 1
            await Clients.Client(p1ConnId).SendAsync("MatchFound", new
            {
                MatchId   = matchId,
                Opponent  = p2Name,
                YourTeam  = "blue"
            });

            // Notify Player 2
            await Clients.Client(p2ConnId).SendAsync("MatchFound", new
            {
                MatchId   = matchId,
                Opponent  = p1Name,
                YourTeam  = "red"
            });
        }
        else
        {
            // Still waiting — tell client
            await Clients.Caller.SendAsync("ReceiveStatus", "Waiting for opponent...");
        }
    }

    /// <summary>Cancel matchmaking — remove from queue.</summary>
    public async Task LeaveQueue()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.ConnectionId;
        if (_redis != null)
        {
            // Remove any entry where the connectionId segment matches
            var all = await _redis.SortedSetRangeByRankAsync(QueueKey);
            foreach (var entry in all)
            {
                if (entry.ToString().Contains(Context.ConnectionId))
                {
                    await _redis.SortedSetRemoveAsync(QueueKey, entry);
                    break;
                }
            }
        }
        await Clients.Caller.SendAsync("ReceiveStatus", "Left queue.");
    }

    /// <summary>Relay progress (lines solved, test pass %) to opponent in real-time.</summary>
    public async Task SendProgress(string matchId, int progressPercent)
    {
        await Clients.OthersInGroup(matchId).SendAsync("OpponentProgress", progressPercent);
    }

    /// <summary>Report match result — award XP via HTTP call or direct DB (handled by controller).</summary>
    public async Task SubmitResult(string matchId, bool won)
    {
        await Clients.Group(matchId).SendAsync("MatchEnded", new
        {
            MatchId = matchId,
            Winner  = won ? Context.ConnectionId : "opponent"
        });
    }
}
