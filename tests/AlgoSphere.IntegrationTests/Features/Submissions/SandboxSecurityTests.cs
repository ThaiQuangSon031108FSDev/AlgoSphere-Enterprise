using System.Net.Http.Json;
using System.Net.Http.Headers;
using AlgoSphere.IntegrationTests.Base;
using AlgoSphere.Application.Features.Exercises.Commands.ExecuteCode;
using AlgoSphere.Api.Controllers;
using AlgoSphere.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AlgoSphere.IntegrationTests.Features.Submissions;

public class SandboxSecurityTests : IntegrationTestBase
{
    [Fact]
    public async Task ExecuteCode_ShouldReject_SystemAccessAttacks()
    {
        // Arrange
        var maliciousCode = "const fs = require('fs'); return fs.readFileSync('/etc/passwd');";
        var request = new ExecuteCodeRequestDto(1, maliciousCode, "javascript");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/exercises/execute", request);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<ExecuteCodeResponse>();
        result!.Execution.Success.Should().BeFalse();
        result.Execution.Message.Should().ContainAny("ReferenceError", "fs is not defined", "Access denied");
    }

    [Fact]
    public async Task ExecuteCode_ShouldHandle_InfiniteLoops()
    {
        // Arrange
        var infiniteLoopCode = "while(true) { }";
        var request = new ExecuteCodeRequestDto(1, infiniteLoopCode, "javascript");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/exercises/execute", request);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<ExecuteCodeResponse>();
        result!.Execution.Success.Should().BeFalse();
        result.Execution.Message.Should().ContainAny("Timeout", "killed");
    }

    [Fact]
    public async Task ExecuteCode_ShouldHandle_MemoryExhaustion()
    {
        // Arrange
        var memoryLeakCode = "let a = []; while(true) { a.push(new Array(1000000).fill(0)); }";
        var request = new ExecuteCodeRequestDto(1, memoryLeakCode, "javascript");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/exercises/execute", request);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<ExecuteCodeResponse>();
        result!.Execution.Success.Should().BeFalse();
        result.Execution.Message.Should().ContainAny("Memory limit", "OutOfMemory", "killed");
    }
}
