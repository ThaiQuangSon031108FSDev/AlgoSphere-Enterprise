using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AlgoSphere.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AlgoSphere.Infrastructure.AI;

public class GeminiAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GeminiAIService> _logger;

    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:streamGenerateContent";

    public GeminiAIService(HttpClient httpClient, IConfiguration config, ILogger<GeminiAIService> logger)
    {
        _httpClient = httpClient;
        _apiKey = config["AI:GeminiApiKey"] ?? string.Empty;
        _logger = logger;
    }

    public async Task<AIResponse> GetHintAsync(string currentCode, string stateJson, string errorMessage)
    {
        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_GEMINI_API_KEY_HERE")
        {
            return new AIResponse(
                "⚠️ Chưa cấu hình Gemini API Key. Hãy thêm key vào appsettings.json → AI:GeminiApiKey",
                new List<int>());
        }

        var prompt = BuildPrompt(currentCode, stateJson, errorMessage);

        try
        {
            var fullText = await CallGeminiStreamAsync(prompt);
            return new AIResponse(fullText, ExtractHighlightIds(fullText));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API call failed");
            return new AIResponse($"❌ Lỗi kết nối AI: {ex.Message}", new List<int>());
        }
    }

    public async IAsyncEnumerable<string> StreamHintAsync(
        string currentCode,
        string stateJson,
        string errorMessage,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_GEMINI_API_KEY_HERE")
        {
            yield return "⚠️ Chưa cấu hình Gemini API Key. Hãy thêm key vào appsettings.json → AI:GeminiApiKey";
            yield break;
        }

        var prompt = BuildPrompt(currentCode, stateJson, errorMessage);
        var url = $"{BaseUrl}?key={_apiKey}&alt=sse";

        var requestBody = new
        {
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = prompt } } }
            },
            generationConfig = new { temperature = 0.7, maxOutputTokens = 1024 }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = JsonContent.Create(requestBody);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new System.IO.StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) is not null && !ct.IsCancellationRequested)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;

            var json = line["data:".Length..].Trim();
            if (json == "[DONE]") break;

            string? chunk = null;
            try
            {
                using var doc = JsonDocument.Parse(json);
                chunk = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
            }
            catch { /* skip malformed chunk */ }

            if (!string.IsNullOrEmpty(chunk))
                yield return chunk;
        }
    }

    private async Task<string> CallGeminiStreamAsync(string prompt)
    {
        var url = $"{BaseUrl}?key={_apiKey}&alt=sse";

        var requestBody = new
        {
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = prompt } } }
            },
            generationConfig = new { temperature = 0.7, maxOutputTokens = 1024 }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = JsonContent.Create(requestBody);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var sb = new StringBuilder();
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new System.IO.StreamReader(stream);

        string? sLine;
        while ((sLine = await reader.ReadLineAsync()) is not null)
        {
            if (string.IsNullOrWhiteSpace(sLine) || !sLine.StartsWith("data:")) continue;

            var json = sLine["data:".Length..].Trim();
            if (json == "[DONE]") break;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (!string.IsNullOrEmpty(text)) sb.Append(text);
            }
            catch { }
        }

        return sb.ToString();
    }

    private static string BuildPrompt(string code, string stateJson, string userMessage)
    {
        // Parse TraceLog for context-aware tutoring
        var traceContext = ParseTraceContext(stateJson);

        return $"""
        Bạn là AI Mentor trong nền tảng AlgoSphere — chuyên hướng dẫn sinh viên học Cấu trúc Dữ liệu & Giải thuật.

        QUY TẮC PHẢN HỒI:
        - Trả lời bằng tiếng Việt, ngắn gọn (tối đa 4-5 câu)
        - KHÔNG giải hộ toàn bộ code — chỉ gợi ý hướng tư duy
        - Đề cập đến dòng code hoặc bước cụ thể nếu có lỗi
        - Dùng emoji phù hợp để thân thiện

        THÔNG TIN RUNTIME:
        {traceContext}

        CODE CỦA HỌC VIÊN:
        ```javascript
        {code}
        ```

        CÂU HỎI / VẤN ĐỀ: {userMessage}

        Gợi ý ngắn gọn:
        """;
    }

    private static string ParseTraceContext(string stateJson)
    {
        if (string.IsNullOrEmpty(stateJson) || stateJson == "{}")
            return "- Chưa có dữ liệu trace (code chưa được chạy hoặc bị lỗi cú pháp).";

        try
        {
            using var doc = JsonDocument.Parse(stateJson);
            var root = doc.RootElement;

            var sb = new StringBuilder();

            if (root.TryGetProperty("algorithm", out var algo))
                sb.AppendLine($"- Thuật toán: {algo.GetString()}");

            if (root.TryGetProperty("initialState", out var init))
                sb.AppendLine($"- Mảng ban đầu: [{string.Join(", ", init.EnumerateArray().Select(e => e.GetInt32()))}]");

            if (root.TryGetProperty("trace", out var trace))
            {
                var steps = trace.GetArrayLength();
                sb.AppendLine($"- Tổng số bước thực thi: {steps}");

                if (steps > 0)
                {
                    // Last step analysis
                    var lastStep = trace[steps - 1];
                    if (lastStep.TryGetProperty("a", out var action))
                    {
                        var act = action.GetString();
                        var targets = lastStep.TryGetProperty("t", out var t)
                            ? $"[{string.Join(", ", t.EnumerateArray().Select(e => e.GetInt32()))}]"
                            : "";
                        sb.AppendLine($"- Bước cuối: {(act == "cmp" ? "So sánh" : "Hoán đổi")} tại vị trí {targets}");
                    }

                    // Count swaps vs comparisons
                    var comparisons = 0;
                    var swaps = 0;
                    foreach (var step in trace.EnumerateArray())
                    {
                        if (step.TryGetProperty("a", out var a))
                        {
                            if (a.GetString() == "cmp") comparisons++;
                            else swaps++;
                        }
                    }
                    sb.AppendLine($"- Thống kê: {comparisons} lần so sánh, {swaps} lần hoán đổi");
                }
            }

            return sb.Length > 0 ? sb.ToString().TrimEnd() : "- Trace data có format không hợp lệ.";
        }
        catch
        {
            return $"- Raw state: {stateJson[..Math.Min(200, stateJson.Length)]}";
        }
    }

    private static List<int> ExtractHighlightIds(string text)
    {
        if (text.Contains("hoán đổi") || text.Contains("swap") || text.Contains("lỗi"))
            return new List<int> { 0, 1 };
        return new List<int>();
    }
}
