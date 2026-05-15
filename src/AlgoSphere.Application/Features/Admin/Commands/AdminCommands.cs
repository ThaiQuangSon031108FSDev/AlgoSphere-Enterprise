using AlgoSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlgoSphere.Application.Features.Admin.Commands;

// ── Ban / Activate User ───────────────────────────────────────────────────────
public record SetUserStatusCommand(int TargetUserId, string Status) : IRequest<bool>;

public class SetUserStatusCommandHandler : IRequestHandler<SetUserStatusCommand, bool>
{
    private readonly IAlgoSphereDbContext _context;

    public SetUserStatusCommandHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(SetUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.TargetUserId, cancellationToken);
        if (user == null) return false;

        user.Status = request.Status;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// ── Delete Exercise ───────────────────────────────────────────────────────────
public record DeleteExerciseCommand(int ExerciseId) : IRequest<bool>;

public class DeleteExerciseCommandHandler : IRequestHandler<DeleteExerciseCommand, bool>
{
    private readonly IAlgoSphereDbContext _context;

    public DeleteExerciseCommandHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);
        if (exercise == null) return false;

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
