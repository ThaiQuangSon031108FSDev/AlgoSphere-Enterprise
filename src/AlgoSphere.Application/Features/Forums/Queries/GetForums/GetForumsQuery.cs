using MediatR;
using Microsoft.EntityFrameworkCore;
using AlgoSphere.Application.Interfaces;

namespace AlgoSphere.Application.Features.Forums.Queries.GetForums;

public record GetForumsQuery() : IRequest<List<ForumDto>>;

public record ForumDto(int Id, string Title, string Description, int DiscussionCount);

public class GetForumsQueryHandler : IRequestHandler<GetForumsQuery, List<ForumDto>>
{
    private readonly IAlgoSphereDbContext _context;

    public GetForumsQueryHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<List<ForumDto>> Handle(GetForumsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Forums
            .Select(f => new ForumDto(f.Id, f.Title, f.Description, f.Discussions.Count))
            .ToListAsync(cancellationToken);
    }
}
