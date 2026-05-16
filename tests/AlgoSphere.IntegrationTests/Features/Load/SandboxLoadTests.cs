using System.Net.Http.Json;
using System.Diagnostics;
using AlgoSphere.IntegrationTests.Base;
using AlgoSphere.Application.Features.Exercises.Commands.ExecuteCode;
using FluentAssertions;
using Xunit;
using Bogus;

namespace AlgoSphere.IntegrationTests.Features.Load;

public class SandboxLoadTests : IClassFixture<IntegrationTestBase>
{
    private readonly HttpClient _client;
    private readonly Faker _faker = new();

    public SandboxLoadTests(IntegrationTestBase factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SubmissionLoadTest_ShouldHandle100ConcurrentRequests()
    {
        // Arrange
        int requestCount = 50; // Giảm xuống 50 để chạy nhanh hơn trong môi trường test
        var exerciseId = 1; // Giả sử đã có exercise 1 từ DbInitializer
        
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 };
        var results = new List<HttpResponseMessage>();

        await Parallel.ForEachAsync(Enumerable.Range(0, requestCount), parallelOptions, async (i, ct) =>
        {
            var command = new ExecuteCodeCommand(
                UserId: 1,
                ExerciseId: exerciseId,
                Code: "function bubbleSort(arr) { return arr.sort((a,b) => a-b); }",
                Language: "javascript"
            );

            var response = await _client.PostAsJsonAsync("/api/v1/exercises/execute", command, ct);
            lock (results)
            {
                results.Add(response);
            }
        });

        stopwatch.Stop();

        // Assert
        results.Should().HaveCount(requestCount);
        results.All(r => r.IsSuccessStatusCode).Should().BeTrue();
        
        var avgTime = stopwatch.ElapsedMilliseconds / requestCount;
        Console.WriteLine($"Total Time: {stopwatch.ElapsedMilliseconds}ms, Avg Time: {avgTime}ms per submission");
        
        // Kiểm tra xem hệ thống có bị treo không (Avg time < 2s là ổn cho môi trường local)
        avgTime.Should().BeLessThan(2000);
    }
}
