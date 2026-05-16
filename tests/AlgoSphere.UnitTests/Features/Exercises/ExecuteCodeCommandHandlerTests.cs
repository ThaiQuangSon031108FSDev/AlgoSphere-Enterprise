using AlgoSphere.Application.Features.Exercises.Commands.ExecuteCode;
using AlgoSphere.Application.Interfaces;
using AlgoSphere.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AlgoSphere.UnitTests.Features.Exercises;

public class ExecuteCodeCommandHandlerTests
{
    private readonly Mock<IExecutionService> _executionServiceMock;
    private readonly IAlgoSphereDbContext _context;

    public ExecuteCodeCommandHandlerTests()
    {
        _executionServiceMock = new Mock<IExecutionService>();
        
        // Use InMemory Database for Unit Testing the Handler Logic
        var options = new DbContextOptionsBuilder<AlgoSphere.Infrastructure.Persistence.AlgoSphereDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AlgoSphere.Infrastructure.Persistence.AlgoSphereDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldAwardBonusXP_WhenPerformanceIsExcellent()
    {
        // Arrange
        var userId = 1;
        var exerciseId = 1;
        var user = new User { Id = userId, Username = "testuser", RankPoints = 100, RowVersion = new byte[0] };
        var exercise = new Exercise 

        { 
            Id = exerciseId, 
            Points = 100, 
            TimeLimitMs = 1000, 
            MemoryLimitKb = 10000,
            TestCases = new List<TestCase> { new TestCase { InputJson = "[]", ExpectedOutputJson = "[]" } }
        };
        
        _context.Users.Add(user);
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        var executionResult = new ExecutionResult(
            Success: true,
            TraceLog: "[]",
            Message: "All tests passed",
            TimeMs: 200, // < 30% of 1000
            MemoryKb: 2000 // < 30% of 10000
        );

        _executionServiceMock.Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<TestCaseDto>>()))
            .ReturnsAsync(executionResult);


        var handler = new ExecuteCodeCommandHandler(_executionServiceMock.Object, _context);
        var command = new ExecuteCodeCommand(userId, exerciseId, "print(1)", "python");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Gamification.Should().NotBeNull();
        result.Gamification!.IsSpeedDemon.Should().BeTrue();
        result.Gamification!.IsMemoryMaster.Should().BeTrue();
        
        // Multiplier = 1.0 + 0.1 (Speed) + 0.1 (Memory) = 1.2
        // Final XP = 100 * 1.2 = 120. Bonus = 20.
        result.Gamification!.XpEarned.Should().Be(100);
        result.Gamification!.BonusXp.Should().Be(20);
        result.Gamification!.TotalXp.Should().Be(220);
    }
}
