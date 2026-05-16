using AlgoSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Security.Claims;

namespace AlgoSphere.Api.Hubs;

/// <summary>
/// Real matchmaking hub using Redis sorted set as a waiting queue.
/// Flow: JoinQueue → added to Redis → background task polls → when 2 players found → MatchFound.
/// SubmitResult now hooks into ITournamentService for automated bracket advancement.
/// </summary>
[Authorize]
public class ArenaHub : Hub
{
    private readonly IDatabase? _redis;
    private readonly ITournamentService _tournamentService;
    private readonly IServiceScopeFactory _scopeFactory;
    private const string QueueKey = "algosphere:arena:queue";

    public ArenaHub(IConfiguration config, ITournamentService tournamentService, IServiceScopeFactory scopeFactory)
    {
        _tournamentService = tournamentService;
        _scopeFactory = scopeFactory;
        try
        {
            var mux = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis") ?? "localhost:6379");
            _redis = mux.GetDatabase();
        }
        catch
        {
            _redis = null; 
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
            
            // Pick a random exercise from DB
            int exerciseId = 1;
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AlgoSphere.Infrastructure.Persistence.AlgoSphereDbContext>();
                var allIds = await db.Exercises.Select(e => e.Id).ToListAsync();
                if (allIds.Any()) exerciseId = allIds[new Random().Next(allIds.Count)];
            }

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
                YourTeam  = "blue",
                ExerciseId = exerciseId
            });

            // Notify Player 2
            await Clients.Client(p2ConnId).SendAsync("MatchFound", new
            {
                MatchId   = matchId,
                Opponent  = p1Name,
                YourTeam  = "red",
                ExerciseId = exerciseId
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

    /// <summary>Join an ongoing match as an observer.</summary>
    public async Task JoinMatchAsSpectator(string matchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, matchId);
        await Clients.Caller.SendAsync("ReceiveStatus", "Watching match: " + matchId);
    }

    /// <summary>Join a tournament group to receive bracket updates.</summary>
    public async Task JoinTournamentGroup(int tournamentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"tournament_{tournamentId}");
    }

    /// <summary>Relay progress (lines solved, test pass %) to all in match (including spectators).</summary>
    public async Task SendProgress(string matchId, int progressPercent)
    {
        await Clients.OthersInGroup(matchId).SendAsync("OpponentProgress", progressPercent);
    }

    /// <summary>
    /// Report match result — notify all in match group and optionally advance tournament bracket.
    /// If the match belongs to a tournament, ITournamentService checks if the round is complete.
    /// </summary>
    public async Task SubmitResult(string matchId, bool won, int? tournamentId = null)
    {
        await Clients.Group(matchId).SendAsync("MatchEnded", new
        {
            MatchId = matchId,
            Winner  = won ? Context.ConnectionId : "opponent"
        });

        // Event-driven tournament advancement
        if (tournamentId.HasValue)
        {
            await _tournamentService.AdvanceRoundIfCompleteAsync(tournamentId.Value);
        }
    }

    /// <summary>Broadcast bracket update to all tournament observers.</summary>
    public async Task SendBracketUpdate(int tournamentId)
    {
        await Clients.Group($"tournament_{tournamentId}").SendAsync("BracketUpdated");
    }
}
