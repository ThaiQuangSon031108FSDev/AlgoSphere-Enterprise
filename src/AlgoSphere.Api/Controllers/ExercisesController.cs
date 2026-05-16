using AlgoSphere.Application.Features.Exercises.Commands.ExecuteCode;
using AlgoSphere.Application.Features.Exercises.Queries.GetExerciseById;
using AlgoSphere.Application.Features.Exercises.Queries.GetExercisesByTopic;
using AlgoSphere.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    [Authorize]
    public async Task<ActionResult<ExecuteCodeResponse>> Execute([FromBody] ExecuteCodeRequestDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var command = new ExecuteCodeCommand(userId, dto.ExerciseId, dto.Code, dto.Language, dto.Deltas);
        return await _mediator.Send(command);
    }
}

public record ExecuteCodeRequestDto(int ExerciseId, string Code, string Language, List<object>? Deltas);
