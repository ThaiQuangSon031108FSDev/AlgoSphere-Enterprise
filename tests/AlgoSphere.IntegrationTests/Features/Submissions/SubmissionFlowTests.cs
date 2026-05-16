using System.Net.Http.Json;
using System.Net.Http.Headers;
using AlgoSphere.IntegrationTests.Base;
using AlgoSphere.Application.Features.Exercises.Commands.ExecuteCode;
using AlgoSphere.Api.Controllers;
using AlgoSphere.Domain.Entities;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AlgoSphere.Infrastructure.Persistence;

namespace AlgoSphere.IntegrationTests.Features.Submissions;

public class SubmissionFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task ExecuteCode_ShouldSaveSubmissionAndAwardXP()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AlgoSphereDbContext>();
        
        var user = new User { Username = "tester", Email = "test@test.com", RankPoints = 0 };
        context.Users.Add(user);
        
        var exercise = new Exercise 
        { 
            Title = "Sum Test", 
            Points = 100, 
            EntryPoint = "sum",
            TestCases = new List<TestCase> { new TestCase { InputJson = "[1, 2]", ExpectedOutputJson = "3" } }
        };
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        var request = new ExecuteCodeRequestDto(exercise.Id, "return a + b", "javascript");
        
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/exercises/execute", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ExecuteCodeResponse>();
        
        result.Should().NotBeNull();
        result!.Gamification.Should().NotBeNull();
        result.Gamification!.XpEarned.Should().BeGreaterThan(0);

        // Verify DB state
        var dbSubmission = await context.Submissions.FindAsync(1); // Assuming ID 1
        dbSubmission.Should().NotBeNull();
        dbSubmission!.UserId.Should().Be(1);
    }
}
