using Microsoft.EntityFrameworkCore;
using AlgoSphere.Domain.Entities;

namespace AlgoSphere.Application.Interfaces;

public interface IAlgoSphereDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Topic> Topics { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<TestCase> TestCases { get; }
    DbSet<Submission> Submissions { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Forum> Forums { get; }
    DbSet<Discussion> Discussions { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Tournament> Tournaments { get; }
    DbSet<TournamentParticipant> TournamentParticipants { get; }
    DbSet<Match> Matches { get; }
    DbSet<Organization> Organizations { get; }
    DbSet<Classroom> Classrooms { get; }
    DbSet<Assignment> Assignments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}

