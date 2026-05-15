using AlgoSphere.Application.Features.Forums.Commands.CreateDiscussion;
using AlgoSphere.Application.Features.Forums.Commands.PostComment;
using AlgoSphere.Application.Features.Forums.Queries.GetDiscussionDetails;
using AlgoSphere.Application.Features.Forums.Queries.GetForums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("discussions/{id}")]
    public async Task<ActionResult<DiscussionDetailsDto>> GetDiscussion(int id)
    {
        var result = await _mediator.Send(new GetDiscussionDetailsQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("discussions")]
    public async Task<ActionResult<int>> CreateDiscussion(CreateDiscussionCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpPost("comments")]
    public async Task<ActionResult<int>> PostComment(PostCommentCommand command)
    {
        return await _mediator.Send(command);
    }
}
