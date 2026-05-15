using AlgoSphere.Application.Interfaces;
using AlgoSphere.Infrastructure.Persistence;
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
        services.AddSingleton<ILeaderboardService, Services.RedisLeaderboardService>();

        return services;
    }
}
