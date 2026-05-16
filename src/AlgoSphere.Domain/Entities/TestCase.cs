using AlgoSphere.Domain.Common;

namespace AlgoSphere.Domain.Entities;

public class TestCase : BaseEntity
{
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    
    // JSON representation of the input parameters, e.g. "[ [2,7,11,15], 9 ]"
    public string InputJson { get; set; } = string.Empty;
    
    // Expected return value in JSON
    public string ExpectedOutputJson { get; set; } = string.Empty;
    
    // If true, this test case is used for grading but not shown in UI
    public bool IsHidden { get; set; }
}
