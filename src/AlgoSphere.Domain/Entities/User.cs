using System.ComponentModel.DataAnnotations;
using AlgoSphere.Domain.Common;

namespace AlgoSphere.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int RankPoints { get; set; }
    public string Status { get; set; } = "Active";

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

