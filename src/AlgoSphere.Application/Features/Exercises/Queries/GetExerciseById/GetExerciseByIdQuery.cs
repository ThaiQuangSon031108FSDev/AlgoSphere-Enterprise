using MediatR;
using Microsoft.EntityFrameworkCore;
using AlgoSphere.Application.Interfaces;

namespace AlgoSphere.Application.Features.Exercises.Queries.GetExerciseById;

public record GetExerciseByIdQuery(int Id) : IRequest<ExerciseDetailDto>;

public record ExerciseDetailDto(int Id, string Title, string Content, string DifficultyLevel, int TimeLimitMs, int MemoryLimitKb);

public class GetExerciseByIdQueryHandler : IRequestHandler<GetExerciseByIdQuery, ExerciseDetailDto>
{
    private readonly IAlgoSphereDbContext _context;

    public GetExerciseByIdQueryHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<ExerciseDetailDto> Handle(GetExerciseByIdQuery request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (exercise == null) return null!;

        return new ExerciseDetailDto(
            exercise.Id, 
            exercise.Title, 
            exercise.Content, 
            exercise.DifficultyLevel, 
            exercise.TimeLimitMs, 
            exercise.MemoryLimitKb);
    }
}
