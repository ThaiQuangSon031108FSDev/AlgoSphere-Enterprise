using AlgoSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlgoSphere.Application.Features.Forums.Queries.GetDiscussionsByForum;

public record GetDiscussionsByForumQuery(int ForumId, int Page = 1, int PageSize = 20) : IRequest<List<DiscussionSummaryDto>>;

public record DiscussionSummaryDto(
    int Id,
    string Title,
    string AuthorUsername,
    int CommentCount,
    int Views,
    DateTime CreatedAt
);

public class GetDiscussionsByForumQueryHandler : IRequestHandler<GetDiscussionsByForumQuery, List<DiscussionSummaryDto>>
{
    private readonly IAlgoSphereDbContext _context;

    public GetDiscussionsByForumQueryHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<List<DiscussionSummaryDto>> Handle(GetDiscussionsByForumQuery request, CancellationToken cancellationToken)
    {
        return await _context.Discussions
            .Where(d => d.ForumId == request.ForumId)
            .Include(d => d.User)
            .Include(d => d.Comments)
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DiscussionSummaryDto(
                d.Id,
                d.Title,
                d.User.Username,
                d.Comments.Count,
                d.Views,
                d.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}
