using AlgoSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlgoSphere.Application.Features.Admin.Queries;

// ── Admin Users List ──────────────────────────────────────────────────────────
public record GetAdminUsersQuery(string? Search = null) : IRequest<List<AdminUserDto>>;

public record AdminUserDto(
    int Id,
    string Username,
    string Email,
    string Role,
    int Level,
    int RankPoints,
    string Status,
    DateTime CreatedAt
);

public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, List<AdminUserDto>>
{
    private readonly IAlgoSphereDbContext _context;

    public GetAdminUsersQueryHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminUserDto>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var q = request.Search.ToLower();
            query = query.Where(u => u.Username.ToLower().Contains(q) || u.Email.ToLower().Contains(q));
        }

        return await query
            .OrderByDescending(u => u.RankPoints)
            .Select(u => new AdminUserDto(
                u.Id,
                u.Username,
                u.Email,
                u.UserRoles.Any(ur => ur.Role.RoleName == "Admin") ? "Admin" : "Student",
                Math.Max(1, (int)(Math.Sqrt(u.RankPoints / 50.0) + 1)),
                u.RankPoints,
                u.Status,
                u.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}

// ── Admin Exercises List ──────────────────────────────────────────────────────
public record GetAdminExercisesQuery() : IRequest<List<AdminExerciseDto>>;

public record AdminExerciseDto(
    int Id,
    string Title,
    string TopicName,
    string DifficultyLevel,
    int Points,
    int TimeLimitMs
);

public class GetAdminExercisesQueryHandler : IRequestHandler<GetAdminExercisesQuery, List<AdminExerciseDto>>
{
    private readonly IAlgoSphereDbContext _context;

    public GetAdminExercisesQueryHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminExerciseDto>> Handle(GetAdminExercisesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Exercises
            .Include(e => e.Topic)
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .Select(e => new AdminExerciseDto(
                e.Id,
                e.Title,
                e.Topic.Name,
                e.DifficultyLevel,
                e.Points,
                e.TimeLimitMs
            ))
            .ToListAsync(cancellationToken);
    }
}

// ── Admin Dashboard Analytics ─────────────────────────────────────────────────
public record GetAdminAnalyticsQuery() : IRequest<AdminAnalyticsDto>;

public record AdminAnalyticsDto(
    int TotalUsers,
    int ActiveUsers,
    int TotalExercises,
    int TotalSubmissions,
    int AcceptedSubmissions,
    double AcceptanceRate
);

public class GetAdminAnalyticsQueryHandler : IRequestHandler<GetAdminAnalyticsQuery, AdminAnalyticsDto>
{
    private readonly IAlgoSphereDbContext _context;

    public GetAdminAnalyticsQueryHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<AdminAnalyticsDto> Handle(GetAdminAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var activeUsers = await _context.Users.CountAsync(u => u.Status == "Active", cancellationToken);
        var totalExercises = await _context.Exercises.CountAsync(cancellationToken);
        var totalSubs = await _context.Submissions.CountAsync(cancellationToken);
        var acceptedSubs = await _context.Submissions.CountAsync(s => s.Status == "Accepted", cancellationToken);
        var rate = totalSubs > 0 ? Math.Round((double)acceptedSubs / totalSubs * 100, 1) : 0;

        return new AdminAnalyticsDto(totalUsers, activeUsers, totalExercises, totalSubs, acceptedSubs, rate);
    }
}
