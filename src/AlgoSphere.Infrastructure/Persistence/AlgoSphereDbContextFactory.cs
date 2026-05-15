using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlgoSphere.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core Tools (migrations, database update).
/// EF Tools cannot use the DI-registered DbContext, so we build options manually.
/// Connection string priority: env var → hardcoded fallback.
/// </summary>
public class AlgoSphereDbContextFactory : IDesignTimeDbContextFactory<AlgoSphereDbContext>
{
    public AlgoSphereDbContext CreateDbContext(string[] args)
    {
        // Read from environment variable set by docker-compose or CI,
        // then fall back to a local developer default.
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost,1433;Database=AlgoSphereDb;User Id=sa;Password=Password123!;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<AlgoSphereDbContext>();
        optionsBuilder.UseSqlServer(connectionString,
            b => b.MigrationsAssembly(typeof(AlgoSphereDbContext).Assembly.FullName));

        return new AlgoSphereDbContext(optionsBuilder.Options);
    }
}
