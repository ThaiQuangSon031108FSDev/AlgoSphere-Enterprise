using AlgoSphere.Application.Interfaces;
using AlgoSphere.Infrastructure.Persistence;
using AlgoSphere.Infrastructure.Services;
using AlgoSphere.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlgoSphere.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AlgoSphereDbContext>(options =>
            options.UseSqlServer(connectionString,
                builder => builder.MigrationsAssembly(typeof(AlgoSphereDbContext).Assembly.FullName)));

        // Register interface so MediatR handlers can inject IAlgoSphereDbContext
        services.AddScoped<IAlgoSphereDbContext>(
            provider => provider.GetRequiredService<AlgoSphereDbContext>());

        services.AddScoped<ITokenService, Identity.TokenService>();
        services.AddScoped<IExecutionService, Sandbox.DockerExecutionService>();
        services.AddHttpClient<IAIService, AI.GeminiAIService>();
        services.AddScoped<ILeaderboardService, RedisLeaderboardService>();

        // Anti-Cheat (two-layer: velocity + AST structural similarity)
        services.AddScoped<IAntiCheatService, AntiCheatService>();

        // Tournament automation (event-driven + scheduled fallback)
        services.AddScoped<ITournamentService, TournamentService>();
        services.AddHostedService<TournamentTimeoutWorker>();

        // Object Storage (MinIO in dev, AWS S3 in prod — config-driven)
        services.AddSingleton<IStorageService, S3StorageService>();

        return services;
    }
}

