using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication;
using Testcontainers.MsSql;

using Testcontainers.Redis;
using Testcontainers.RabbitMq;
using Xunit;
using AlgoSphere.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlgoSphere.IntegrationTests.Base;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .Build();

    protected HttpClient Client { get; private set; } = default!;
    protected IServiceProvider Services { get; private set; } = default!;
    private WebApplicationFactory<Program> _factory = default!;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _dbContainer.StartAsync(),
            _redisContainer.StartAsync(),
            _rabbitMqContainer.StartAsync()
        );

        _factory = new WebApplicationFactory<Program>()

            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace DbContext
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AlgoSphereDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<AlgoSphereDbContext>(options =>
                    {
                        options.UseSqlServer(_dbContainer.GetConnectionString());
                    });

                    // Add Mock Auth
                    services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, options => { });

                    // Override other services if needed (Redis, RabbitMQ connection strings)

                    // builder.Configuration["Redis:ConnectionString"] = _redisContainer.GetConnectionString();
                });
            });

        Client = _factory.CreateClient();
        Services = _factory.Services;

        // Ensure database is created
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AlgoSphereDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await Task.WhenAll(
            _dbContainer.StopAsync(),
            _redisContainer.StopAsync(),
            _rabbitMqContainer.StopAsync()
        );
    }
}
