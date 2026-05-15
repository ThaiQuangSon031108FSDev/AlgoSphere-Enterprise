using AlgoSphere.Application.Features.Forums.Commands.CreateDiscussion;
using AlgoSphere.Application.Features.Forums.Commands.PostComment;
using AlgoSphere.Application.Features.Forums.Queries.GetDiscussionDetails;
using AlgoSphere.Application.Features.Forums.Queries.GetDiscussionsByForum;
using AlgoSphere.Application.Features.Forums.Queries.GetForums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlgoSphere.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ForumsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ForumsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ForumDto>>> GetForums()
    {
        return await _mediator.Send(new GetForumsQuery());
    }

    [HttpGet("{forumId}/discussions")]
    public async Task<ActionResult<List<DiscussionSummaryDto>>> GetDiscussions(int forumId, [FromQuery] int page = 1)
    {
        return await _mediator.Send(new GetDiscussionsByForumQuery(forumId, page));
    }

    [HttpGet("discussions/{id}")]
    public async Task<ActionResult<DiscussionDetailsDto>> GetDiscussion(int id)
    {
        var result = await _mediator.Send(new GetDiscussionDetailsQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("discussions")]
    [Authorize]
    public async Task<ActionResult<int>> CreateDiscussion([FromBody] CreateDiscussionRequestDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        return await _mediator.Send(new CreateDiscussionCommand(dto.ForumId, userId, dto.Title, dto.Content));
    }

    [HttpPost("comments")]
    [Authorize]
    public async Task<ActionResult<int>> PostComment([FromBody] PostCommentRequestDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        return await _mediator.Send(new PostCommentCommand(dto.DiscussionId, userId, dto.Content, dto.ParentCommentId));
    }
}

public record PostCommentRequestDto(int DiscussionId, string Content, int? ParentCommentId = null);
public record CreateDiscussionRequestDto(int ForumId, string Title, string Content);
