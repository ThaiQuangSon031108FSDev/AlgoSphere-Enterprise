using AlgoSphere.Application.Features.Learning.Queries.GetSkillTree;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlgoSphere.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class LearningController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearningController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>GET /api/v1/learning/skill-tree — Returns user's skill tree with real progress</summary>
    [HttpGet("skill-tree")]
    public async Task<ActionResult<SkillTreeDto>> GetSkillTree()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        return await _mediator.Send(new GetSkillTreeQuery(userId));
    }
}
