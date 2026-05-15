using AlgoSphere.Domain.Common;

namespace AlgoSphere.Domain.Entities;

public class Exercise : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = "Easy";
    public int TimeLimitMs { get; set; }
    public int MemoryLimitKb { get; set; }
    public int Points { get; set; }

    public int TopicId { get; set; }
    public Topic Topic { get; set; } = null!;
    
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}

public class Submission : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    
    public string SourceCode { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    
    public int ExecutionTimeMs { get; set; }
    public int MemoryUsedKb { get; set; }
}
