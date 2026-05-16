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

        // Seed roles
        var adminRole = new Role { RoleName = "Admin" };
        var teacherRole = new Role { RoleName = "Teacher" };
        var studentRole = new Role { RoleName = "Student" };
        context.Roles.AddRange(adminRole, teacherRole, studentRole);
        await context.SaveChangesAsync();

        // Seed users with BCrypt
        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Status = "Active",
            RankPoints = 1000
        };

        var studentUser = new User
        {
            Username = "student",
            Email = "student@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Status = "Active",
            RankPoints = 0
        };

        context.Users.AddRange(adminUser, studentUser);
        await context.SaveChangesAsync();

        // Assign roles
        context.UserRoles.Add(new UserRole { User = adminUser, Role = adminRole });
        context.UserRoles.Add(new UserRole { User = studentUser, Role = studentRole });
        await context.SaveChangesAsync();

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
            EntryPoint = "bubbleSort",
            Topic = bubbleSortTopic,
            TestCases = new List<TestCase>
            {
                new TestCase { InputJson = "[[64, 34, 25, 12, 22, 11, 90]]", ExpectedOutputJson = "[]", IsHidden = false }
            }
        };

        var quickSortExercise = new Exercise
        {
            Title = "Quick Sort Analysis",
            Content = "Triển khai Quick Sort sử dụng phân hoạch Lomuto.",
            DifficultyLevel = "Medium",
            Points = 200,
            EntryPoint = "quickSort",
            Topic = quickSortTopic,
            TestCases = new List<TestCase>
            {
                new TestCase { InputJson = "[[64, 34, 25, 12, 22, 11, 90]]", ExpectedOutputJson = "[]", IsHidden = false }
            }
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

        // --- Bổ sung các bài tập kinh điển (LeetCode-style) ---
        var twoSumTopic = new Topic { Name = "Two Sum", Description = "Tìm cặp số có tổng bằng target.", OrderIndex = 1, Category = sortingCategory };
        var containsDuplicateTopic = new Topic { Name = "Contains Duplicate", Description = "Kiểm tra mảng có phần tử trùng lặp.", OrderIndex = 2, Category = sortingCategory };
        var validPalindromeTopic = new Topic { Name = "Valid Palindrome", Description = "Kiểm tra chuỗi đối xứng.", OrderIndex = 3, Category = sortingCategory };
        var binarySearchTopic = new Topic { Name = "Binary Search", Description = "Tìm kiếm nhị phân trên mảng đã sắp xếp.", OrderIndex = 4, Category = sortingCategory };
        var singleNumberTopic = new Topic { Name = "Single Number", Description = "Tìm số duy nhất không bị lặp lại.", OrderIndex = 5, Category = sortingCategory };
        var climbStairsTopic = new Topic { Name = "Climbing Stairs", Description = "Quy hoạch động cơ bản.", OrderIndex = 6, Category = sortingCategory };
        var reverseListTopic = new Topic { Name = "Reverse Linked List", Description = "Đảo ngược danh sách liên kết.", OrderIndex = 7, Category = sortingCategory };

        context.Topics.AddRange(twoSumTopic, containsDuplicateTopic, validPalindromeTopic, binarySearchTopic, singleNumberTopic, climbStairsTopic, reverseListTopic);

        var twoSumExercise = new Exercise { Title = "Two Sum", Content = "Cho mảng nums và target, trả về index của 2 số có tổng bằng target.", DifficultyLevel = "Easy", Points = 50, EntryPoint = "twoSum", Topic = twoSumTopic, TestCases = new List<TestCase> { new TestCase { InputJson = "[[2,7,11,15], 9]", ExpectedOutputJson = "[0,1]" } } };
        var containsDuplicateExercise = new Exercise { Title = "Contains Duplicate", Content = "Trả về true nếu có giá trị xuất hiện ít nhất 2 lần.", DifficultyLevel = "Easy", Points = 50, EntryPoint = "containsDuplicate", Topic = containsDuplicateTopic, TestCases = new List<TestCase> { new TestCase { InputJson = "[[1,2,3,1]]", ExpectedOutputJson = "true" } } };
        var validPalindromeExercise = new Exercise { Title = "Valid Palindrome", Content = "Chuỗi s có phải là Palindrome không?", DifficultyLevel = "Easy", Points = 50, EntryPoint = "isPalindrome", Topic = validPalindromeTopic, TestCases = new List<TestCase> { new TestCase { InputJson = "[\"racecar\"]", ExpectedOutputJson = "true" } } };
        var binarySearchExercise = new Exercise { Title = "Binary Search", Content = "Tìm target trong mảng nums (đã sắp xếp).", DifficultyLevel = "Easy", Points = 50, EntryPoint = "binarySearch", Topic = binarySearchTopic, TestCases = new List<TestCase> { new TestCase { InputJson = "[[11,12,22,25,34,64,90], 25]", ExpectedOutputJson = "3" } } };
        var singleNumberExercise = new Exercise { Title = "Single Number", Content = "Tìm phần tử xuất hiện 1 lần duy nhất trong mảng.", DifficultyLevel = "Easy", Points = 50, EntryPoint = "singleNumber", Topic = singleNumberTopic, TestCases = new List<TestCase> { new TestCase { InputJson = "[[4,1,2,1,2]]", ExpectedOutputJson = "4" } } };
        var climbStairsExercise = new Exercise { Title = "Climbing Stairs", Content = "Có bao nhiêu cách để leo n bậc thang?", DifficultyLevel = "Easy", Points = 50, EntryPoint = "climbStairs", Topic = climbStairsTopic, TestCases = new List<TestCase> { new TestCase { InputJson = "[10]", ExpectedOutputJson = "89" } } };
        var reverseListExercise = new Exercise { Title = "Reverse Linked List", Content = "Đảo ngược danh sách liên kết.", DifficultyLevel = "Easy", Points = 50, EntryPoint = "reverseList", Topic = reverseListTopic, TestCases = new List<TestCase> { new TestCase { InputJson = "[[1,2,3,4,5]]", ExpectedOutputJson = "[5,4,3,2,1]" } } };


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
        context.Exercises.AddRange(
            bubbleSortExercise, quickSortExercise, bfsExercise, dijkstraExercise,
            twoSumExercise, containsDuplicateExercise, validPalindromeExercise,
            binarySearchExercise, singleNumberExercise, climbStairsExercise, reverseListExercise
        );
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
