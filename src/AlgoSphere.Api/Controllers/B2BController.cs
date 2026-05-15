using AlgoSphere.Application.Features.B2B.Queries.GetOrganizationAnalytics;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AlgoSphere.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
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
        var result = await _mediator.Send(new GetOrganizationAnalyticsQuery(orgId));
        if (result == null) return NotFound();
        return Ok(result);
    }
}
