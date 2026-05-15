using Microsoft.AspNetCore.SignalR;

namespace AlgoSphere.Api.Hubs;

public class ArenaHub : Hub
{
    public async Task JoinQueue(string userId)
    {
        // Mock Matchmaking: Sau 3 giây tự động ghép cặp
        await Clients.Caller.SendAsync("ReceiveStatus", "Searching for opponent...");
        await Task.Delay(3000);
        
        string matchId = Guid.NewGuid().ToString();
        await Groups.AddToGroupAsync(Context.ConnectionId, matchId);
        await Clients.Group(matchId).SendAsync("MatchFound", new { MatchId = matchId, Opponent = "Expert_User_99" });
    }

    public async Task SendProgress(string matchId, int progress)
    {
        // Gửi tiến độ cho đối thủ trong phòng
        await Clients.OthersInGroup(matchId).SendAsync("OpponentProgress", progress);
    }
}
