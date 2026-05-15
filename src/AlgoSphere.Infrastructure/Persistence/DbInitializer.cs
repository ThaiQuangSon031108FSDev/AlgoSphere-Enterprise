using AlgoSphere.Domain.Entities;
using AlgoSphere.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlgoSphere.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(AlgoSphereDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Categories.AnyAsync()) return;

        var sortingCategory = new Category { Name = "Sorting Algorithms" };
        var graphCategory = new Category { Name = "Graph Theory" };

        var bubbleSortTopic = new Topic
        {
            Name = "Bubble Sort",
            Description = "Thuật toán sắp xếp nổi bọt cơ bản nhưng quan trọng.",
            OrderIndex = 1,
            Category = sortingCategory
        };

        var quickSortTopic = new Topic
        {
            Name = "Quick Sort",
            Description = "Thuật toán sắp xếp nhanh sử dụng chiến lược chia để trị.",
            OrderIndex = 2,
            Category = sortingCategory
        };

        var bfsTopic = new Topic
        {
            Name = "Breadth-First Search",
            Description = "Duyệt đồ thị theo chiều rộng.",
            OrderIndex = 1,
            Category = graphCategory
        };

        var dijkstraTopic = new Topic
        {
            Name = "Dijkstra's Algorithm",
            Description = "Thuật toán tìm đường đi ngắn nhất trên đồ thị có trọng số.",
            OrderIndex = 2,
            Category = graphCategory
        };

        var bubbleSortExercise = new Exercise
        {
            Title = "Bubble Sort Implementation",
            Content = "Hãy viết thuật toán Bubble Sort để sắp xếp mảng số nguyên theo thứ tự tăng dần.",
            DifficultyLevel = "Easy",
            TimeLimitMs = 1000,
            MemoryLimitKb = 64000,
            Points = 100,
            Topic = bubbleSortTopic
        };

        var quickSortExercise = new Exercise
        {
            Title = "Quick Sort Analysis",
            Content = "Triển khai Quick Sort sử dụng phân hoạch Lomuto.",
            DifficultyLevel = "Medium",
            Points = 200,
            Topic = quickSortTopic
        };

        var bfsExercise = new Exercise
        {
            Title = "Breadth-First Search",
            Content = "Duyệt đồ thị theo chiều rộng từ một đỉnh cho trước.",
            DifficultyLevel = "Medium",
            Points = 150,
            Topic = bfsTopic
        };

        var dijkstraExercise = new Exercise
        {
            Title = "Dijkstra's Algorithm",
            Content = "Tìm đường đi ngắn nhất giữa các đỉnh trong đồ thị có trọng số không âm.",
            DifficultyLevel = "Hard",
            Points = 300,
            Topic = dijkstraTopic
        };

        var generalForum = new Forum
        {
            Title = "Thảo luận chung",
            Description = "Nơi trao đổi về mọi thứ liên quan đến thuật toán và lập trình."
        };

        var sortingForum = new Forum
        {
            Title = "Sorting & Searching",
            Description = "Chuyên mục dành riêng cho các thuật toán sắp xếp và tìm kiếm."
        };

        context.Categories.AddRange(sortingCategory, graphCategory);
        context.Topics.AddRange(bubbleSortTopic, quickSortTopic, bfsTopic, dijkstraTopic);
        context.Exercises.AddRange(bubbleSortExercise, quickSortExercise, bfsExercise, dijkstraExercise);
        context.Forums.AddRange(generalForum, sortingForum);

        var springTournament = new Tournament
        {
            Title = "AlgoSphere Spring Championship 2026",
            Description = "Giải đấu quy mô lớn nhất năm dành cho các lập trình viên chuyên nghiệp.",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(14),
            Status = "Upcoming"
        };

        context.Tournaments.Add(springTournament);

        var fptUniversity = new Organization
        {
            Name = "FPT University",
            Domain = "fpt.edu.vn",
            Type = "School"
        };

        context.Organizations.Add(fptUniversity);

        await context.SaveChangesAsync();
    }
}
