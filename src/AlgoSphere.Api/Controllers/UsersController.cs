using AlgoSphere.Application.Features.Identity.Commands.Login;
using AlgoSphere.Application.Features.Identity.Commands.Register;
using AlgoSphere.Application.Features.Identity.Queries.GetMe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlgoSphere.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<int>> Register(RegisterCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginCommand command)
    {
        return await _mediator.Send(command);
    }

    /// <summary>
    /// Returns the profile of the currently authenticated user.
    /// Requires a valid JWT Bearer token.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> GetMe()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue("sub");

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized("Invalid token — no user ID claim.");

        try
        {
            var profile = await _mediator.Send(new GetMeQuery(userId));
            return Ok(profile);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("User not found.");
        }
    }
}
