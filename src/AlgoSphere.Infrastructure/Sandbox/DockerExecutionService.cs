using AlgoSphere.Application.Interfaces;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace AlgoSphere.Infrastructure.Sandbox;

public class DockerExecutionService : IExecutionService
{
    private readonly DockerClient _client;
    private readonly ILogger<DockerExecutionService> _logger;

    public DockerExecutionService(ILogger<DockerExecutionService> logger)
    {
        _client = new DockerClientConfiguration().CreateClient();
        _logger = logger;
    }

    public async Task<ExecutionResult> ExecuteAsync(string code, string language, int exerciseId)
    {
        // GIAI ĐOẠN MVP: Giả lập thực thi (Mock) để user có thể chạy được ngay 
        // trong khi chờ thiết lập Docker Daemon thực tế.
        
        _logger.LogInformation("Đang giả lập chạy code cho bài tập {ExerciseId}...", exerciseId);
        
        await Task.Delay(800); // Giả lập độ trễ Sandbox

        // Mock Trace Log cho thuật toán Sắp xếp (Bubble Sort)
        var mockTrace = @"
        {
          ""initialState"": [15, 2, 8, 1, 9],
          ""trace"": [
            { ""s"": 1, ""l"": 5, ""a"": ""cmp"", ""t"": [0, 1], ""v"": { ""j"": 1 } },
            { ""s"": 2, ""a"": ""swp"", ""t"": [0, 1], ""v"": { ""temp"": 15 } },
            { ""s"": 3, ""l"": 5, ""a"": ""cmp"", ""t"": [1, 2], ""v"": { ""j"": 2 } }
          ]
        }";

        return new ExecutionResult(true, mockTrace, "Success", 120, 4096);
        
        /* 
        // TODO: Logic Docker thực tế (Enterprise Edition)
        // 1. Tạo container từ image tương ứng với language (python:alpine, node:alpine, etc.)
        // 2. Map code vào tệp tmp bên trong container
        // 3. Chạy lệnh thực thi và lắng nghe stream stdout/stderr
        // 4. Thu thập kết quả và xóa container (Pool management)
        */
    }
}
