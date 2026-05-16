using AlgoSphere.Domain.Common;

namespace AlgoSphere.Domain.Entities;

// ─── B2B Organization Role ───────────────────────────────────────────────────

/// <summary>
/// Role of a user within a specific Organization.
/// OrgAuditor: read-only access to analytics/dashboard.
/// Teacher: manages multiple Classrooms within the org.
/// </summary>
public enum OrgRole { OrgAdmin, Teacher, OrgAuditor, Student }

public class OrganizationMember
{
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public OrgRole Role { get; set; } = OrgRole.Student;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

// ─── Tournaments ─────────────────────────────────────────────────────────────

public class Tournament : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, Ongoing, Completed

    /// <summary>Time allowed per round in minutes (used by timeout worker).</summary>
    public int RoundDurationMinutes { get; set; } = 60;

    public int MinParticipants { get; set; } = 8;
    public int MaxParticipants { get; set; } = 64;

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

    /// <summary>e.g. "Round1-Match1". Round number is parsed from prefix for automation.</summary>
    public string BracketPosition { get; set; } = string.Empty;

    /// <summary>Pending | Ongoing | Completed | Forfeited</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>Deadline for this match; timeout worker forfeits if null submissions by this time.</summary>
    public DateTime? RoundDeadlineUtc { get; set; }
}

// ─── B2B Classroom ───────────────────────────────────────────────────────────

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Type { get; set; } = "School"; // School, Company

    public ICollection<OrganizationMember> OrganizationMembers { get; set; } = new List<OrganizationMember>();
    public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
}

/// <summary>A Teacher can manage multiple Classrooms within the same Organization.</summary>
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
