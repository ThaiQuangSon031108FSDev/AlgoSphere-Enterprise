using AlgoSphere.Application.Interfaces;
using AlgoSphere.Domain.Entities;
using MediatR;

namespace AlgoSphere.Application.Features.Forums.Commands.PostComment;

public record PostCommentCommand(int DiscussionId, int UserId, string Content, int? ParentCommentId = null) : IRequest<int>;

public class PostCommentCommandHandler : IRequestHandler<PostCommentCommand, int>
{
    private readonly IAlgoSphereDbContext _context;

    public PostCommentCommandHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(PostCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = new Comment
        {
            DiscussionId = request.DiscussionId,
            UserId = request.UserId,
            Content = request.Content,
            ParentCommentId = request.ParentCommentId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}
