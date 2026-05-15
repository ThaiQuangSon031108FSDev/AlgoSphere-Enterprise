using MediatR;
using Microsoft.EntityFrameworkCore;
using AlgoSphere.Application.Interfaces;

namespace AlgoSphere.Application.Features.Forums.Queries.GetDiscussionDetails;

public record GetDiscussionDetailsQuery(int Id) : IRequest<DiscussionDetailsDto>;

public record DiscussionDetailsDto(
    int Id, 
    string Title, 
    string Content, 
    string Author, 
    DateTime CreatedAt, 
    List<CommentDto> Comments);

public record CommentDto(int Id, string Content, string Author, DateTime CreatedAt);

public class GetDiscussionDetailsQueryHandler : IRequestHandler<GetDiscussionDetailsQuery, DiscussionDetailsDto>
{
    private readonly IAlgoSphereDbContext _context;

    public GetDiscussionDetailsQueryHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<DiscussionDetailsDto> Handle(GetDiscussionDetailsQuery request, CancellationToken cancellationToken)
    {
        var discussion = await _context.Discussions
            .Include(d => d.User)
            .Include(d => d.Comments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (discussion == null) return null!;

        return new DiscussionDetailsDto(
            discussion.Id,
            discussion.Title,
            discussion.Content,
            discussion.User.Username,
            discussion.CreatedAt,
            discussion.Comments.Select(c => new CommentDto(c.Id, c.Content, c.User.Username, c.CreatedAt)).ToList()
        );
    }
}
