using AlgoSphere.Application.Interfaces;
using AlgoSphere.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AlgoSphere.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SeedController : ControllerBase
{
    private readonly IAlgoSphereDbContext _context;

    public SeedController(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult> SeedData()
    {
        return await RunSeeding();
    }

    [HttpPost("force-seed")]
    public async Task<ActionResult> ForceSeed()
    {
        // Clear old data for a fresh tree
        _context.Submissions.RemoveRange(_context.Submissions);
        _context.Exercises.RemoveRange(_context.Exercises);
        _context.Topics.RemoveRange(_context.Topics);
        _context.Categories.RemoveRange(_context.Categories);
        await _context.SaveChangesAsync(default);

        return await RunSeeding();
    }

    private async Task<ActionResult> RunSeeding()
    {
        // 1. Seed Categories & Topics
        if (!await _context.Categories.AnyAsync())
        {
            var cat1 = new Category { Name = "Data Structures", CreatedAt = DateTime.UtcNow };
            var cat2 = new Category { Name = "Algorithms & Techniques", CreatedAt = DateTime.UtcNow };
            _context.Categories.AddRange(cat1, cat2);
            await _context.SaveChangesAsync(default);

            var ds1 = new Topic { CategoryId = cat1.Id, Name = "Arrays & Hashing", Description = "Mảng và băm.", OrderIndex = 1, CreatedAt = DateTime.UtcNow };
            var ds2 = new Topic { CategoryId = cat1.Id, Name = "Linked Lists", Description = "Làm quen với các Node và con trỏ bộ nhớ.", OrderIndex = 2, CreatedAt = DateTime.UtcNow };
            var ds3 = new Topic { CategoryId = cat1.Id, Name = "Stacks & Queues", Description = "Ứng dụng nguyên lý LIFO và FIFO trong thực tế.", OrderIndex = 3, CreatedAt = DateTime.UtcNow };
            var ds4 = new Topic { CategoryId = cat1.Id, Name = "Trees & BST", Description = "Nền tảng của cấu trúc dữ liệu phân cấp.", OrderIndex = 4, CreatedAt = DateTime.UtcNow };
            var ds5 = new Topic { CategoryId = cat1.Id, Name = "Heaps & Priority Queues", Description = "Quản lý phần tử có mức độ ưu tiên cao/thấp nhất.", OrderIndex = 5, CreatedAt = DateTime.UtcNow };
            var ds6 = new Topic { CategoryId = cat1.Id, Name = "Graphs", Description = "Biểu diễn mạng lưới, bản đồ và các mối liên kết.", OrderIndex = 6, CreatedAt = DateTime.UtcNow };
            var ds7 = new Topic { CategoryId = cat1.Id, Name = "Tries", Description = "Cấu trúc tối ưu hóa cho bài toán tìm kiếm chuỗi, từ điển.", OrderIndex = 7, CreatedAt = DateTime.UtcNow };
            var ds8 = new Topic { CategoryId = cat1.Id, Name = "Disjoint Set / Union-Find", Description = "Cấu trúc tối ưu để gom nhóm các phần tử và kiểm tra tính liên thông trong mạng lưới.", OrderIndex = 8, CreatedAt = DateTime.UtcNow };
            var ds9 = new Topic { CategoryId = cat1.Id, Name = "Segment Trees & Fenwick Trees", Description = "Vũ khí hạng nặng để xử lý các truy vấn tổng/khoảng và cập nhật liên tục.", OrderIndex = 9, CreatedAt = DateTime.UtcNow };

            var al1 = new Topic { CategoryId = cat2.Id, Name = "Two Pointers", Description = "Kỹ thuật hai con trỏ.", OrderIndex = 1, CreatedAt = DateTime.UtcNow };
            var al2 = new Topic { CategoryId = cat2.Id, Name = "Sliding Window", Description = "Kỹ thuật tối ưu cho các bài toán về mảng con liên tiếp.", OrderIndex = 2, CreatedAt = DateTime.UtcNow };
            var al3 = new Topic { CategoryId = cat2.Id, Name = "Binary Search", Description = "Tìm kiếm nhị phân cơ bản O(log n).", OrderIndex = 3, CreatedAt = DateTime.UtcNow };
            var al4 = new Topic { CategoryId = cat2.Id, Name = "Sorting", Description = "Các thuật toán sắp xếp kinh điển.", OrderIndex = 4, CreatedAt = DateTime.UtcNow };
            var al5 = new Topic { CategoryId = cat2.Id, Name = "Recursion & Backtracking", Description = "Kỹ thuật rẽ nhánh và duyệt qua mọi khả năng của bài toán.", OrderIndex = 5, CreatedAt = DateTime.UtcNow };
            var al6 = new Topic { CategoryId = cat2.Id, Name = "Graph Traversal", Description = "Các thuật toán tìm đường đi và loang không gian mẫu kinh điển (BFS/DFS).", OrderIndex = 6, CreatedAt = DateTime.UtcNow };
            var al7 = new Topic { CategoryId = cat2.Id, Name = "Greedy Algorithms", Description = "Đưa ra lựa chọn tối ưu cục bộ tại mỗi bước để giải bài toán.", OrderIndex = 7, CreatedAt = DateTime.UtcNow };
            var al8 = new Topic { CategoryId = cat2.Id, Name = "1D Dynamic Programming", Description = "Lưu trữ kết quả bài toán con để tối ưu hóa thời gian chạy.", OrderIndex = 8, CreatedAt = DateTime.UtcNow };
            var al9 = new Topic { CategoryId = cat2.Id, Name = "2D Dynamic Programming", Description = "Giải quyết các bài toán di chuyển trên lưới và ma trận phức tạp.", OrderIndex = 9, CreatedAt = DateTime.UtcNow };
            var al10 = new Topic { CategoryId = cat2.Id, Name = "Bit Manipulation", Description = "Xử lý dữ liệu ở cấp độ nhị phân (AND, OR, XOR) để đạt tốc độ thực thi O(1).", OrderIndex = 10, CreatedAt = DateTime.UtcNow };
            var al11 = new Topic { CategoryId = cat2.Id, Name = "Fast & Slow Pointers", Description = "Thuật toán Rùa và Thỏ chuyên trị các bài toán phát hiện chu trình.", OrderIndex = 11, CreatedAt = DateTime.UtcNow };
            var al12 = new Topic { CategoryId = cat2.Id, Name = "Advanced Graphs", Description = "Đi sâu vào thuật toán tìm đường đi ngắn nhất, cây khung nhỏ nhất và sắp xếp topo.", OrderIndex = 12, CreatedAt = DateTime.UtcNow };
            var al13 = new Topic { CategoryId = cat2.Id, Name = "String Matching", Description = "Thuật toán KMP và Rabin-Karp để tìm kiếm chuỗi con siêu tốc.", OrderIndex = 13, CreatedAt = DateTime.UtcNow };
            var al14 = new Topic { CategoryId = cat2.Id, Name = "Math & Geometry", Description = "Sàng nguyên tố Eratosthenes, UCLN/BCNN (GCD/LCM), toán modulo và hình học tính toán.", OrderIndex = 14, CreatedAt = DateTime.UtcNow };

            _context.Topics.AddRange(ds1, ds2, ds3, ds4, ds5, ds6, ds7, ds8, ds9, al1, al2, al3, al4, al5, al6, al7, al8, al9, al10, al11, al12, al13, al14);
            await _context.SaveChangesAsync(default);

            // 2. Seed Exercises to ensure the tree renders out
            _context.Exercises.AddRange(
                new Exercise { TopicId = ds1.Id, Title = "Two Sum", DifficultyLevel = "Easy", Content = "Cho mảng số nguyên nums và số đích target, trả về chỉ số của 2 số có tổng bằng target.", Points = 10, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = ds1.Id, Title = "Contains Duplicate", DifficultyLevel = "Easy", Content = "Kiểm tra xem mảng có phần tử trùng lặp hay không.", Points = 10, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = ds2.Id, Title = "Reverse Linked List", DifficultyLevel = "Easy", Content = "Đảo ngược một danh sách liên kết đơn.", Points = 10, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = ds3.Id, Title = "Valid Parentheses", DifficultyLevel = "Easy", Content = "Dùng Stack kiểm tra ngoặc hợp lệ.", Points = 15, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = ds4.Id, Title = "Invert Binary Tree", DifficultyLevel = "Easy", Content = "Lật ngược một cây nhị phân.", Points = 15, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = ds5.Id, Title = "Kth Largest Element", DifficultyLevel = "Medium", Content = "Tìm phần tử lớn thứ k trong mảng bằng Min Heap.", Points = 20, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = ds6.Id, Title = "Number of Islands", DifficultyLevel = "Medium", Content = "Đếm số lượng hòn đảo trên lưới 2D.", Points = 25, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = ds7.Id, Title = "Implement Trie", DifficultyLevel = "Medium", Content = "Cài đặt cây tiền tố Trie với Insert, Search, StartsWith.", Points = 30, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = ds8.Id, Title = "Redundant Connection", DifficultyLevel = "Medium", Content = "Tìm cạnh gây ra chu trình trong đồ thị vô hướng.", Points = 30, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = ds9.Id, Title = "Range Sum Query", DifficultyLevel = "Hard", Content = "Tính tổng đoạn [L, R] và cập nhật giá trị liên tục.", Points = 45, CreatedAt = DateTime.UtcNow },

                new Exercise { TopicId = al1.Id, Title = "Valid Palindrome", DifficultyLevel = "Easy", Content = "Kiểm tra chuỗi đối xứng.", Points = 10, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = al2.Id, Title = "Best Time to Buy and Sell Stock", DifficultyLevel = "Easy", Content = "Tính lợi nhuận cao nhất.", Points = 15, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = al3.Id, Title = "Binary Search", DifficultyLevel = "Easy", Content = "Tìm kiếm nhị phân cơ bản O(log n).", Points = 15, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = al4.Id, Title = "Bubble Sort", DifficultyLevel = "Easy", Content = "Cài đặt thuật toán sắp xếp nổi bọt.", Points = 15, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = al4.Id, Title = "Merge Sort", DifficultyLevel = "Medium", Content = "Sắp xếp trộn O(n log n).", Points = 30, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = al5.Id, Title = "Subsets", DifficultyLevel = "Medium", Content = "Sinh tất cả các tập con.", Points = 25, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = al6.Id, Title = "Word Ladder", DifficultyLevel = "Hard", Content = "Tìm đường đi ngắn nhất biến đổi chuỗi dùng BFS.", Points = 40, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = al7.Id, Title = "Jump Game", DifficultyLevel = "Medium", Content = "Kiểm tra có thể nhảy tới đích hay không.", Points = 20, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = al8.Id, Title = "Climbing Stairs", DifficultyLevel = "Easy", Content = "Bài toán cái túi leo bậc thang kinh điển.", Points = 15, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = al9.Id, Title = "Unique Paths", DifficultyLevel = "Medium", Content = "Đếm số đường đi trên ma trận mxn.", Points = 25, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = al10.Id, Title = "Single Number", DifficultyLevel = "Easy", Content = "Tìm số xuất hiện 1 lần dùng XOR.", Points = 15, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = al11.Id, Title = "Linked List Cycle", DifficultyLevel = "Easy", Content = "Dùng Rùa và Thỏ kiểm tra chu trình.", Points = 15, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = al12.Id, Title = "Network Delay Time", DifficultyLevel = "Medium", Content = "Dijkstra tìm thời gian truyền tin tối đa.", Points = 35, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = al13.Id, Title = "Implement strStr()", DifficultyLevel = "Medium", Content = "Cài đặt KMP.", Points = 35, CreatedAt = DateTime.UtcNow },
                new Exercise { TopicId = al14.Id, Title = "Count Primes", DifficultyLevel = "Medium", Content = "Sàng nguyên tố Eratosthenes đếm số nguyên tố.", Points = 20, CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync(default);
        }

        // 3. Seed Users (Test Admin & Test Student)
        if (!await _context.Users.AnyAsync(u => u.Email == "admin@test.com"))
        {
            _context.Users.Add(new User { Username = "AdminTest", Email = "admin@test.com", PasswordHash = HashPassword("123456"), RankPoints = 0, Status = "Active", CreatedAt = DateTime.UtcNow });
        }
        if (!await _context.Users.AnyAsync(u => u.Email == "student@test.com"))
        {
            _context.Users.Add(new User { Username = "StudentTest", Email = "student@test.com", PasswordHash = HashPassword("123456"), RankPoints = 1250, Status = "Active", CreatedAt = DateTime.UtcNow });
        }
        await _context.SaveChangesAsync(default);

        // Assign Roles
        var admin = await _context.Users.FirstAsync(u => u.Email == "admin@test.com");
        var student = await _context.Users.FirstAsync(u => u.Email == "student@test.com");

        if (!await _context.UserRoles.AnyAsync(ur => ur.UserId == admin.Id))
        {
            _context.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = 1 });
        }
        if (!await _context.UserRoles.AnyAsync(ur => ur.UserId == student.Id))
        {
            _context.UserRoles.Add(new UserRole { UserId = student.Id, RoleId = 2 });
        }
        await _context.SaveChangesAsync(default);

        // 4. Seed Forums & Tournaments
        if (!await _context.Forums.AnyAsync())
        {
            var forum = new Forum { Title = "Q&A Thuật Toán", Description = "Hỏi đáp các vấn đề thuật toán.", CreatedAt = DateTime.UtcNow };
            _context.Forums.Add(forum);
            await _context.SaveChangesAsync(default);

            var discussion = new Discussion { ForumId = forum.Id, UserId = student.Id, Title = "Làm sao để tối ưu Two Sum xuống O(n)?", Content = "Mình dùng 2 vòng lặp lồng nhau đang bị O(n^2), có cách nào dùng Hash Map không mọi người?", Views = 42, CreatedAt = DateTime.UtcNow };
            _context.Discussions.Add(discussion);
            await _context.SaveChangesAsync(default);
        }

        if (!await _context.Tournaments.AnyAsync())
        {
            _context.Tournaments.Add(new Tournament { Title = "Weekly Contest 142", Description = "Giải đấu thuật toán tuần 142", Status = "Scheduled", StartDate = DateTime.UtcNow.AddDays(2), EndDate = DateTime.UtcNow.AddDays(2).AddHours(2), CreatedAt = DateTime.UtcNow });
            _context.Tournaments.Add(new Tournament { Title = "E-Sports Code Division", Description = "Giải đấu chia bracket loại trực tiếp.", Status = "Active", StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync(default);
        }

        return Ok("Seed data thành công! Cây kỹ năng đã được mở rộng. Test với: admin@test.com / 123456 và student@test.com / 123456");
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
