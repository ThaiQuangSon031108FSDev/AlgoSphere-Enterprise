using AlgoSphere.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AlgoSphere.Infrastructure.AI;

public class GeminiAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiAIService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["AI:GeminiApiKey"] ?? "";
    }

    public async Task<AIResponse> GetHintAsync(string currentCode, string stateJson, string errorMessage)
    {
        // GIAI ĐOẠN MVP: Trả về phản hồi mẫu có cấu trúc 
        // để Frontend có thể hiển thị hiệu ứng highlight lỗi ngay lập tức.
        
        await Task.Delay(1000); // Giả lập AI suy nghĩ

        return new AIResponse(
            "Có vẻ như bạn đang đổi chỗ (swap) sai vị trí ở bước cuối cùng. Hãy kiểm tra lại biến j và j + 1.",
            new List<int> { 0, 1 } // Highlight 2 node đang lỗi
        );

        /*
        // TODO: Kết nối Gemini API thực tế
        // Gửi prompt bao gồm currentCode và stateJson (context) cho Gemini 
        // và parse JSON kết quả trả về.
        */
    }
}
