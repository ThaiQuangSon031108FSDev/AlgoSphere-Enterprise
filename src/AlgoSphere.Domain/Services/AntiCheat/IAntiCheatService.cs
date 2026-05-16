namespace AlgoSphere.Domain.Services.AntiCheat;

public enum AntiCheatEventType
{
    CharInput = 1,
    Paste = 2,
    Delete = 3,
    TabSwitch = 4
}

public record AntiCheatEvent(AntiCheatEventType Type, int Timestamp, string? Data, int Line, int Column);

public record AntiCheatResult(bool IsSuspicious, string Reason, double ConfidenceScore);

public interface IAntiCheatService
{
    AntiCheatResult Analyze(string finalCode, IEnumerable<AntiCheatEvent> events);
}
