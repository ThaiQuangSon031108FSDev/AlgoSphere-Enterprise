using AlgoSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlgoSphere.Application.Features.Identity.Queries.GetMe;

public record GetMeQuery(int UserId) : IRequest<UserProfileDto>;

public record UserProfileDto(
    int Id,
    string Username,
    string Email,
    string? AvatarUrl,
    int RankPoints,
    string Status,
    int TotalSubmissions,
    int SolvedCount,
    int Level,
    int Xp,
    int NextLevelXp,
    string Rank,
    IReadOnlyList<string> Badges
);

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, UserProfileDto>
{
    private readonly IAlgoSphereDbContext _context;

    public GetMeQueryHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        var totalSubmissions = await _context.Submissions.CountAsync(s => s.UserId == request.UserId, cancellationToken);
        var solvedCount = await _context.Submissions
            .Where(s => s.UserId == request.UserId && s.Status == "Accepted")
            .Select(s => s.ExerciseId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Derive level from RankPoints (100 XP per level, exponential-ish)
        var xp = user.RankPoints;
        var level = Math.Max(1, (int)Math.Sqrt(xp / 50.0) + 1);
        var levelFloor = (int)Math.Pow((level - 1), 2) * 50;
        var levelCeil  = (int)Math.Pow(level, 2) * 50;
        var nextXp = levelCeil;

        var rank = xp switch
        {
            >= 5000 => "Diamond",
            >= 2000 => "Platinum",
            >= 800  => "Gold",
            >= 300  => "Silver",
            _       => "Bronze",
        };

        // Static badge logic (can be migrated to DB later)
        var badges = new List<string>();
        if (xp > 0)        badges.Add("First Blood");
        if (xp >= 200)     badges.Add("Streak Master");
        if (xp >= 500)     badges.Add("Array Conqueror");
        if (xp >= 1000)    badges.Add("Arena Warrior");
        if (rank == "Gold" || rank == "Platinum" || rank == "Diamond")
            badges.Add("Speed Coder");

        return new UserProfileDto(
            Id:               user.Id,
            Username:         user.Username,
            Email:            user.Email,
            AvatarUrl:        user.AvatarUrl,
            RankPoints:       xp,
            Status:           user.Status,
            TotalSubmissions: totalSubmissions,
            SolvedCount:      solvedCount,
            Level:            level,
            Xp:               xp - levelFloor,
            NextLevelXp:      nextXp - levelFloor,
            Rank:             rank,
            Badges:           badges.AsReadOnly()
        );
    }
}
