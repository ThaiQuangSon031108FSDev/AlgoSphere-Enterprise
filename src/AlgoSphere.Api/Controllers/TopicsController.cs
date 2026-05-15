using AlgoSphere.Application.Features.Topics.Queries.GetTopics;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AlgoSphere.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TopicsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<TopicDto>>> Get()
    {
        return await _mediator.Send(new GetTopicsQuery());
    }
}
