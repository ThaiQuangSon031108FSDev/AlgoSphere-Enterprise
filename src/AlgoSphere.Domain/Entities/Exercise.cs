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

    public string EntryPoint { get; set; } = "solve";
    
    public int TopicId { get; set; }
    public Topic Topic { get; set; } = null!;
    
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
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

    /// <summary>None | Low | High | Confirmed — set by AntiCheatService.</summary>
    public string SuspicionLevel { get; set; } = "None";

    public SubmissionCodeDelta? CodeDelta { get; set; }
}

public class SubmissionCodeDelta : BaseEntity
{
    public int SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    /// <summary>
    /// Chuỗi sự kiện gõ phím dạng JSON (keypress, paste, delete, etc.)
    /// </summary>
    public string EventDeltasJson { get; set; } = string.Empty;
}
