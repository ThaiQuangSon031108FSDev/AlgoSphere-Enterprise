using AlgoSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlgoSphere.Application.Features.Learning.Queries.GetSkillTree;

public record GetSkillTreeQuery(int UserId) : IRequest<SkillTreeDto>;

public record SkillTreeDto(
    List<SkillLaneDto> Lanes,
    int TotalXp,
    int Level,
    int LevelXp,
    int NextLevelXp
);

public record SkillLaneDto(
    string Lane,
    string Color,
    List<SkillNodeDto> Nodes
);

public record SkillNodeDto(
    int TopicId,
    string Title,
    string Subtitle,
    string Status, // completed | unlocked | locked
    int Xp,
    int TotalExercises,
    int CompletedExercises,
    string Color
);

public class GetSkillTreeQueryHandler : IRequestHandler<GetSkillTreeQuery, SkillTreeDto>
{
    private readonly IAlgoSphereDbContext _context;

    public GetSkillTreeQueryHandler(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    public async Task<SkillTreeDto> Handle(GetSkillTreeQuery request, CancellationToken cancellationToken)
    {
        // Get all categories + topics + exercise counts
        var categories = await _context.Categories
            .Include(c => c.Topics)
                .ThenInclude(t => t.Exercises)
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);

        // Get user's solved exercise IDs
        var solvedExerciseIds = await _context.Submissions
            .Where(s => s.UserId == request.UserId && s.Status == "Accepted")
            .Select(s => s.ExerciseId)
            .Distinct()
            .ToHashSetAsync(cancellationToken);

        // Get user xp
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        var totalXp = user?.RankPoints ?? 0;
        var level = Math.Max(1, (int)Math.Sqrt(totalXp / 50.0) + 1);
        var levelFloor = (int)Math.Pow(level - 1, 2) * 50;
        var levelCeil = (int)Math.Pow(level, 2) * 50;

        // Lane color mapping (by category order)
        var laneColors = new[] { "#10B981", "#3B82F6", "#F97316", "#FBBF24" };

        var lanes = new List<SkillLaneDto>();
        bool prevLaneCompleted = true;

        for (int ci = 0; ci < categories.Count; ci++)
        {
            var cat = categories[ci];
            var color = laneColors[ci % laneColors.Length];
            var nodes = new List<SkillNodeDto>();
            bool prevNodeCompleted = prevLaneCompleted;

            foreach (var topic in cat.Topics.OrderBy(t => t.Id))
            {
                var total = topic.Exercises.Count;
                var completed = topic.Exercises.Count(e => solvedExerciseIds.Contains(e.Id));
                var topicXp = total * 25; // 25 XP per exercise slot

                string status;
                if (completed == total && total > 0) status = "completed";
                else if (prevNodeCompleted) status = "unlocked";
                else status = "locked";

                nodes.Add(new SkillNodeDto(
                    TopicId:             topic.Id,
                    Title:               topic.Name,
                    Subtitle:            topic.Description,
                    Status:              status,
                    Xp:                  topicXp,
                    TotalExercises:      total,
                    CompletedExercises:  completed,
                    Color:               color
                ));

                prevNodeCompleted = (status == "completed");
            }

            prevLaneCompleted = nodes.All(n => n.Status == "completed");
            lanes.Add(new SkillLaneDto(cat.Name, color, nodes));
        }

        return new SkillTreeDto(
            Lanes:       lanes,
            TotalXp:     totalXp,
            Level:       level,
            LevelXp:     totalXp - levelFloor,
            NextLevelXp: levelCeil - levelFloor
        );
    }
}
