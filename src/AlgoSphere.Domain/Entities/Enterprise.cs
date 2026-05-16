using AlgoSphere.Domain.Common;

namespace AlgoSphere.Domain.Entities;

public class Tournament : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, Ongoing, Completed
    
    public ICollection<TournamentParticipant> Participants { get; set; } = new List<TournamentParticipant>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
}

public class TournamentParticipant
{
    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int FinalRank { get; set; }
}

public class Match : BaseEntity
{
    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;
    
    public int? Player1Id { get; set; }
    public User? Player1 { get; set; }
    
    public int? Player2Id { get; set; }
    public User? Player2 { get; set; }
    
    public int? WinnerId { get; set; }
    public string BracketPosition { get; set; } = string.Empty; // e.g. "Round1-Match1"
}

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Type { get; set; } = "School"; // School, Company
    
    public ICollection<User> Members { get; set; } = new List<User>();
    public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
}

public class Classroom : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string JoinCode { get; set; } = string.Empty;
    
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    
    public int TeacherId { get; set; }
    public User Teacher { get; set; } = null!;
    
    public ICollection<User> Students { get; set; } = new List<User>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}

public class Assignment : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    
    public int ClassroomId { get; set; }
    public Classroom Classroom { get; set; } = null!;
    
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
}
