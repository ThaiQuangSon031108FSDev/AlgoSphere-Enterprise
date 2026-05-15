using AlgoSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlgoSphere.Application.Features.Identity.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<AuthResponse>;

public record AuthResponse(int Id, string Username, string Email, string Token);

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IAlgoSphereDbContext _context;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IAlgoSphereDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new Exception("Invalid username or password.");
        }

        var token = _tokenService.CreateToken(user.Id, user.Username, user.Email);

        return new AuthResponse(user.Id, user.Username, user.Email, token);
    }
}
