using AlgoSphere.Domain.Services.AntiCheat;
using FluentAssertions;
using Xunit;

namespace AlgoSphere.UnitTests.Services;

public class AntiCheatTests
{
    private readonly AntiCheatService _service;

    public AntiCheatTests()
    {
        _service = new AntiCheatService();
    }

    [Fact]
    public void Analyze_ShouldFlag_WhenPasteIsTooLarge()
    {
        // Arrange
        var code = new string('x', 300); // 300 chars
        var events = new List<AntiCheatEvent>
        {
            new AntiCheatEvent(AntiCheatEventType.Paste, 0, code, 1, 1)
        };


        // Act
        var result = _service.Analyze(code, events);

        // Assert
        result.IsSuspicious.Should().BeTrue();
        result.Reason.Should().Contain("pasting");
    }

    [Fact]
    public void Analyze_ShouldFlag_WhenTypingIsTooFast()
    {
        // Arrange
        var code = "short code";
        var events = new List<AntiCheatEvent>();
        for (int i = 0; i < 100; i++)
        {
            // 100 chars in 100ms = 1000 chars/sec
            events.Add(new AntiCheatEvent(AntiCheatEventType.CharInput, i, "a", 1, i));
        }
        var finalCode = new string('a', 100);

        // Act
        var result = _service.Analyze(finalCode, events);

        // Assert
        result.IsSuspicious.Should().BeTrue();
        result.Reason.Should().Contain("speed");
    }

    [Fact]
    public void Analyze_ShouldNotFlag_NormalHumanTyping()
    {
        // Arrange
        var events = new List<AntiCheatEvent>();
        for (int i = 0; i < 10; i++)
        {
            // 10 chars in 5 seconds (2 chars/sec)
            events.Add(new AntiCheatEvent(AntiCheatEventType.CharInput, i * 500, "a", 1, i));
        }
        var code = "aaaaaaaaaa";

        // Act
        var result = _service.Analyze(code, events);

        // Assert
        result.IsSuspicious.Should().BeFalse();
    }
}
