using AlgoSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using AlgoSphere.Application.Interfaces;

namespace AlgoSphere.Infrastructure.Persistence;

public class AlgoSphereDbContext : DbContext, IAlgoSphereDbContext
{
    public AlgoSphereDbContext(DbContextOptions<AlgoSphereDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Submission> Submissions => Set<Submission>();

    public DbSet<Forum> Forums => Set<Forum>();
    public DbSet<Discussion> Discussions => Set<Discussion>();
    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Organization> Organizations => Set<Organization>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique indexes
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

        // Composite keys
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<TournamentParticipant>()
            .HasKey(tp => new { tp.TournamentId, tp.UserId });

        // Topic → Category (Restrict to avoid cascade cycle)
        modelBuilder.Entity<Topic>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Topics)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Exercise → Topic (Restrict)
        modelBuilder.Entity<Exercise>()
            .HasOne(e => e.Topic)
            .WithMany(t => t.Exercises)
            .HasForeignKey(e => e.TopicId)
            .OnDelete(DeleteBehavior.Restrict);

        // Submission → User (Restrict — avoids cascade cycle via Exercise)
        modelBuilder.Entity<Submission>()
            .HasOne(s => s.User)
            .WithMany(u => u.Submissions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Submission → Exercise (Restrict)
        modelBuilder.Entity<Submission>()
            .HasOne(s => s.Exercise)
            .WithMany(e => e.Submissions)
            .HasForeignKey(s => s.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Discussion → User (Restrict — avoids cascade cycle via Comment)
        modelBuilder.Entity<Discussion>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Comment → Discussion (Cascade: deleting a discussion removes its comments)
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Discussion)
            .WithMany(d => d.Comments)
            .HasForeignKey(c => c.DiscussionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Comment → User (Restrict — Discussion already has cascade path to User)
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Comment self-reference (threaded replies — no nav props on entity, use shadow FK)
        modelBuilder.Entity<Comment>()
            .HasOne<Comment>()
            .WithMany()
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
