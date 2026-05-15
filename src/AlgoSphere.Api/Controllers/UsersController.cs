using AlgoSphere.Application.Features.Identity.Commands.Login;
using AlgoSphere.Application.Features.Identity.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
}
