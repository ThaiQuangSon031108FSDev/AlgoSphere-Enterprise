using MediatR;
using Microsoft.EntityFrameworkCore;
using AlgoSphere.Application.Interfaces;
using AlgoSphere.Domain.Entities;

namespace AlgoSphere.Application.Features.B2B.Queries.GetOrganizationAnalytics;

public record GetOrganizationAnalyticsQuery(int OrgId, int RequestingUserId) : IRequest<OrgAnalyticsDto?>;

public record OrgAnalyticsDto(
    string OrgName,
    int MemberCount,
    int ClassroomCount,
    int TotalSubmissions,
    double AverageSuccessRate,
    List<TopStudentDto> TopStudents,
    string RequestingUserRole);

public record TopStudentDto(string Username, int Score, int Submissions);

public class GetOrganizationAnalyticsQueryHandler : IRequestHandler<GetOrganizationAnalyticsQuery, OrgAnalyticsDto?>
{
    private readonly IAlgoSphereDbContext _context;

    public GetOrganizationAnalyticsQueryHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<OrgAnalyticsDto?> Handle(GetOrganizationAnalyticsQuery request, CancellationToken cancellationToken)
    {
        // Role gate: only OrgAdmin and OrgAuditor may view analytics
        var membership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(om =>
                om.OrganizationId == request.OrgId &&
                om.UserId == request.RequestingUserId, cancellationToken);

        if (membership == null)
            return null; // Not a member

        if (membership.Role != OrgRole.OrgAdmin && membership.Role != OrgRole.OrgAuditor)
            return null; // Insufficient permission (Teacher/Student cannot view org analytics)

        // Load organization with members
        var org = await _context.Organizations
            .Include(o => o.OrganizationMembers)
                .ThenInclude(om => om.User)
            .Include(o => o.Classrooms)
            .FirstOrDefaultAsync(o => o.Id == request.OrgId, cancellationToken);

        if (org == null) return null;

        var memberUserIds = org.OrganizationMembers.Select(om => om.UserId).ToList();

        // Aggregate submission stats across all org members
        var submissions = await _context.Submissions
            .Where(s => memberUserIds.Contains(s.UserId))
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Accepted = g.Count(s => s.Status == "Accepted")
            })
            .FirstOrDefaultAsync(cancellationToken);

        int totalSubmissions = submissions?.Total ?? 0;
        double successRate = totalSubmissions > 0
            ? Math.Round((double)(submissions!.Accepted) / totalSubmissions * 100, 1)
            : 0;

        // Top students by RankPoints (limited to org members)
        var topStudents = org.OrganizationMembers
            .Select(om => om.User)
            .OrderByDescending(u => u.RankPoints)
            .Take(5)
            .Select(u => new TopStudentDto(
                u.Username,
                u.RankPoints,
                // Submission count per user from in-memory list (avoid N+1)
                0)) // Will be filled below
            .ToList();

        // Fill in per-user submission counts efficiently
        var submissionCounts = await _context.Submissions
            .Where(s => memberUserIds.Contains(s.UserId))
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);

        topStudents = org.OrganizationMembers
            .Select(om => om.User)
            .OrderByDescending(u => u.RankPoints)
            .Take(5)
            .Select(u => new TopStudentDto(
                u.Username,
                u.RankPoints,
                submissionCounts.GetValueOrDefault(u.Id, 0)))
            .ToList();

        return new OrgAnalyticsDto(
            org.Name,
            org.OrganizationMembers.Count,
            org.Classrooms.Count,
            totalSubmissions,
            successRate,
            topStudents,
            membership.Role.ToString());
    }
}

