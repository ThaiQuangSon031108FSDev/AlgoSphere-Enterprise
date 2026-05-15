using MediatR;
using Microsoft.EntityFrameworkCore;
using AlgoSphere.Application.Interfaces;

namespace AlgoSphere.Application.Features.Exercises.Queries.GetExercisesByTopic;

public record GetExercisesByTopicQuery(int TopicId) : IRequest<List<ExerciseLookupDto>>;

public record ExerciseLookupDto(int Id, string Title, string DifficultyLevel, int Points);

public class GetExercisesByTopicQueryHandler : IRequestHandler<GetExercisesByTopicQuery, List<ExerciseLookupDto>>
{
    private readonly IAlgoSphereDbContext _context;

    public GetExercisesByTopicQueryHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExerciseLookupDto>> Handle(GetExercisesByTopicQuery request, CancellationToken cancellationToken)
    {
        return await _context.Exercises
            .Where(e => e.TopicId == request.TopicId)
            .Select(e => new ExerciseLookupDto(e.Id, e.Title, e.DifficultyLevel, e.Points))
            .ToListAsync(cancellationToken);
    }
}
