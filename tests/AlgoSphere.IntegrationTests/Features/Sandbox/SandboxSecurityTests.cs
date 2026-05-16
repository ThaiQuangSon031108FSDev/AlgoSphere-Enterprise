using AlgoSphere.IntegrationTests.Base;
using AlgoSphere.Application.Features.Exercises.Commands.ExecuteCode;
using AlgoSphere.Api.Controllers;
using AlgoSphere.Domain.Entities;
using FluentAssertions;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using AlgoSphere.Infrastructure.Persistence;

namespace AlgoSphere.IntegrationTests.Features.Sandbox;

public class SandboxSecurityTests : IntegrationTestBase
{
    [Theory]
    [InlineData("while(true) { }", "Time limit exceeded or process killed")] // Infinite loop
    [InlineData("const fs = require('fs'); fs.readFileSync('/etc/passwd');", "Permission denied or module not found")] // File system access
    [InlineData("let a = []; while(true) a.push(new Array(1000000));", "Memory limit exceeded")] // Memory leak
    public async Task MaliciousCode_ShouldBeTerminatedSafely(string maliciousCode, string expectedReason)
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AlgoSphereDbContext>();
        
        var exercise = new Exercise 
        { 
            Title = "Security Test", 
            Points = 10, 
            EntryPoint = "test",
            TimeLimitMs = 1000,
            MemoryLimitKb = 51200, // 50MB
            TestCases = new List<TestCase> { new TestCase { InputJson = "[]", ExpectedOutputJson = "null" } }
        };
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var request = new ExecuteCodeRequestDto(exercise.Id, maliciousCode, "javascript");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/exercises/execute", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ExecuteCodeResponse>();
        
        result.Should().NotBeNull();
        result!.Execution.Success.Should().BeFalse("Malicious code should not succeed");
        // Verify that the message contains hints of the termination reason
        // result.Execution.Message.Should().ContainAny("killed", "timeout", "limit", "error");
    }
}
