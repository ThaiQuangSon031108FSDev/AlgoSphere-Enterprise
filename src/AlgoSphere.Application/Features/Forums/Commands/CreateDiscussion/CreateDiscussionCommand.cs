using AlgoSphere.Application.Interfaces;
using AlgoSphere.Domain.Entities;
using MediatR;

namespace AlgoSphere.Application.Features.Forums.Commands.CreateDiscussion;

public record CreateDiscussionCommand(int ForumId, int UserId, string Title, string Content) : IRequest<int>;

public class CreateDiscussionCommandHandler : IRequestHandler<CreateDiscussionCommand, int>
{
    private readonly IAlgoSphereDbContext _context;

    public CreateDiscussionCommandHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateDiscussionCommand request, CancellationToken cancellationToken)
    {
        var discussion = new Discussion
        {
            ForumId = request.ForumId,
            UserId = request.UserId,
            Title = request.Title,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.Discussions.Add(discussion);
        await _context.SaveChangesAsync(cancellationToken);

        return discussion.Id;
    }
}
