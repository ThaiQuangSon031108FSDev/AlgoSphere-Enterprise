using Microsoft.EntityFrameworkCore;
using AlgoSphere.Domain.Entities;

namespace AlgoSphere.Application.Interfaces;

public interface IAlgoSphereDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Topic> Topics { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Forum> Forums { get; }
    DbSet<Discussion> Discussions { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Tournament> Tournaments { get; }
    DbSet<Match> Matches { get; }
    DbSet<Organization> Organizations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
