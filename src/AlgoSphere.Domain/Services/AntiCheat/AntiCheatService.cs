namespace AlgoSphere.Domain.Services.AntiCheat;

public class AntiCheatService : IAntiCheatService
{
    public AntiCheatResult Analyze(string finalCode, IEnumerable<AntiCheatEvent> events)
    {
        if (string.IsNullOrEmpty(finalCode)) return new AntiCheatResult(false, "No code provided", 0);
        if (events == null || !events.Any()) return new AntiCheatResult(true, "No event logs found", 1.0);

        var eventList = events.OrderBy(e => e.Timestamp).ToList();
        int totalCharsInput = eventList.Count(e => e.Type == AntiCheatEventType.CharInput);
        int totalPastes = eventList.Count(e => e.Type == AntiCheatEventType.Paste);
        int pasteLength = eventList.Where(e => e.Type == AntiCheatEventType.Paste).Sum(e => e.Data?.Length ?? 0);
        
        // 1. Check for massive pastes
        if (pasteLength > finalCode.Length * 0.8 && finalCode.Length > 200)
        {
            return new AntiCheatResult(true, "Excessive code pasting detected (>80%)", 0.9);
        }

        // 2. Check for "Too fast to be human"
        var durationMs = eventList.Last().Timestamp - eventList.First().Timestamp;
        double charsPerSecond = finalCode.Length / (durationMs / 1000.0 + 1);
        
        if (charsPerSecond > 50 && totalCharsInput > 10) // > 50 chars/sec is superhuman typing
        {
            return new AntiCheatResult(true, $"Superhuman typing speed: {charsPerSecond:F1} chars/sec", 0.85);
        }

        // 3. Tab switching check (optional - many switch to check docs)
        int tabSwitches = eventList.Count(e => e.Type == AntiCheatEventType.TabSwitch);
        if (tabSwitches > 50)
        {
            return new AntiCheatResult(true, "Extreme tab switching activity", 0.6);
        }

        return new AntiCheatResult(false, "Normal activity", 0);
    }
}
