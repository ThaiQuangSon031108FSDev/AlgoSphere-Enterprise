using AlgoSphere.Application.Features.Exercises.Commands.ExecuteCode;
using AlgoSphere.Application.Features.Exercises.Queries.GetExerciseById;
using AlgoSphere.Application.Features.Exercises.Queries.GetExercisesByTopic;
using AlgoSphere.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AlgoSphere.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExercisesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("topic/{topicId}")]
    public async Task<ActionResult<List<ExerciseLookupDto>>> GetByTopic(int topicId)
    {
        return await _mediator.Send(new GetExercisesByTopicQuery(topicId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDetailDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetExerciseByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("execute")]
    public async Task<ActionResult<ExecutionResult>> Execute(ExecuteCodeCommand command)
    {
        return await _mediator.Send(command);
    }
}
