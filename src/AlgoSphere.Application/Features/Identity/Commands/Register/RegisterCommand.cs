using AlgoSphere.Domain.Entities;
using AlgoSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlgoSphere.Application.Features.Identity.Commands.Register;

public record RegisterCommand(string Username, string Email, string Password) : IRequest<int>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, int>
{
    private readonly IAlgoSphereDbContext _context;

    public RegisterCommandHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email || u.Username == request.Username, cancellationToken))
        {
            throw new Exception("User already exists.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RankPoints = 0,
            Status = "Active"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
