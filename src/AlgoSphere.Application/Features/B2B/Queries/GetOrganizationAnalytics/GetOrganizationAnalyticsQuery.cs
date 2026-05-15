using MediatR;
using Microsoft.EntityFrameworkCore;
using AlgoSphere.Application.Interfaces;

namespace AlgoSphere.Application.Features.B2B.Queries.GetOrganizationAnalytics;

public record GetOrganizationAnalyticsQuery(int OrgId) : IRequest<OrgAnalyticsDto>;

public record OrgAnalyticsDto(
    string OrgName, 
    int MemberCount, 
    int TotalSubmissions, 
    double AverageSuccessRate,
    List<TopStudentDto> TopStudents);

public record TopStudentDto(string Username, int Score);

public class GetOrganizationAnalyticsQueryHandler : IRequestHandler<GetOrganizationAnalyticsQuery, OrgAnalyticsDto>
{
    private readonly IAlgoSphereDbContext _context;

    public GetOrganizationAnalyticsQueryHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<OrgAnalyticsDto> Handle(GetOrganizationAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var org = await _context.Organizations
            .Include(o => o.Members)
            .FirstOrDefaultAsync(o => o.Id == request.OrgId, cancellationToken);

        if (org == null) return null!;

        // Mock data cho Analytics
        return new OrgAnalyticsDto(
            org.Name,
            org.Members.Count,
            1250, // Total submissions
            78.5, // Success rate
            new List<TopStudentDto>
            {
                new TopStudentDto("Student_A", 2500),
                new TopStudentDto("Student_B", 2100),
                new TopStudentDto("Student_C", 1950)
            }
        );
    }
}
