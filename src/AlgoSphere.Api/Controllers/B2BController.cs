using AlgoSphere.Application.Features.B2B.Queries.GetOrganizationAnalytics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlgoSphere.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class B2BController : ControllerBase
{
    private readonly IMediator _mediator;

    public B2BController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("analytics/{orgId}")]
    public async Task<ActionResult<OrgAnalyticsDto>> GetAnalytics(int orgId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _mediator.Send(new GetOrganizationAnalyticsQuery(orgId, userId));
        if (result == null) return Forbid(); // Not a member or insufficient role
        return Ok(result);
    }
}

