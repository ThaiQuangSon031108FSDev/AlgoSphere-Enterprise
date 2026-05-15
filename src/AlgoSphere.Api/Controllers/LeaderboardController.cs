using AlgoSphere.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AlgoSphere.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    [HttpGet("top")]
    public async Task<ActionResult<List<LeaderboardEntry>>> GetTopRankings([FromQuery] int count = 10)
    {
        return await _leaderboardService.GetTopRankingsAsync(count);
    }
}
