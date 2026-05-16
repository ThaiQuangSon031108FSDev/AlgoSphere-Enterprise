using AlgoSphere.Domain.Entities;
using AlgoSphere.Infrastructure.Persistence;
using Bogus;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AlgoSphere.Infrastructure.Persistence;

/// <summary>
/// Senior Backend Architect's Database Initializer.
/// Optimized for massive data seeding using Bogus, Chunking, and EF Core Performance Tuning.
/// </summary>
public static class DbInitializer
{
    private const int CHUNK_SIZE = 2000;
    private static readonly string[] Statuses = { "Accepted", "Wrong Answer", "Time Limit Exceeded", "Memory Limit Exceeded", "Runtime Error" };
    
    private static readonly string[] CodeSnippets = {
        "// Optimized C# Solution\npublic int Solve(int[] nums) {\n    var map = new Dictionary<int, int>();\n    for(int i=0; i<nums.Length; i++) {\n        if(map.ContainsKey(nums[i])) return i;\n        map[nums[i]] = i;\n    }\n    return -1;\n}",
        "// Time Limit Exceeded (Nested Loops)\nfunction solve(n) {\n    let count = 0;\n    for(let i=0; i<n; i++) {\n        for(let j=0; j<n; j++) {\n            for(let k=0; k<n; k++) count++;\n        }\n    }\n    return count;\n}",
        "// Wrong Answer (Logic error)\ndef solve(a, b):\n    return a - b # Should be a + b",
        "// Memory Limit Exceeded\nimport numpy as np\ndef leak():\n    data = []\n    while True:\n        data.append(np.zeros((1024, 1024)))\n",
        "// Accepted Python\ndef fib(n):\n    if n <= 1: return n\n    a, b = 0, 1\n    for _ in range(n-1):\n        a, b = b, a + b\n    return b"
    };

    public static async Task InitializeAsync(AlgoSphereDbContext context)
    {
        var timer = Stopwatch.StartNew();
        Console.WriteLine("🚀 [DbInitializer] Starting massive seeding process...");

        // Ensure database is clean or created
        await context.Database.EnsureCreatedAsync();

        if (await context.Users.AnyAsync(u => u.Username == "admin")) 
        {
            Console.WriteLine("✅ Data already exists. Skipping seed.");
            return;
        }

        // --- OPTIMIZATION: Performance Tuning ---
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        var faker = new Faker("vi");

        // 1. Roles (Static & Few)
        var roles = new List<Role>
        {
            new Role { RoleName = "Admin" },
            new Role { RoleName = "Teacher" },
            new Role { RoleName = "OrgAuditor" },
            new Role { RoleName = "Student" }
        };
        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();

        // 2. Organizations (20)
        var orgFaker = new Faker<Organization>("vi")
            .RuleFor(o => o.Name, f => f.Company.CompanyName() + " " + f.PickRandom("University", "Institute", "Academy", "Tech"))
            .RuleFor(o => o.Domain, f => f.Internet.DomainName())
            .RuleFor(o => o.Type, f => f.PickRandom("School", "Enterprise"));
        
        var organizations = orgFaker.Generate(20);
        context.Organizations.AddRange(organizations);
        await context.SaveChangesAsync();
        var orgIds = organizations.Select(o => o.Id).ToList();

        // 3. Users (5000 Students, 100 Teachers, 5 Admins)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");
        var userFaker = new Faker<User>("vi")
            .RuleFor(u => u.Username, f => f.Internet.UserName().ToLower() + f.UniqueIndex)
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Username))
            .RuleFor(u => u.PasswordHash, _ => passwordHash)
            .RuleFor(u => u.RankPoints, f => f.Random.Int(0, 5000))
            .RuleFor(u => u.Status, _ => "Active");

        Console.WriteLine("👥 Generating users...");
        var adminList = new List<User> {
            new User { 
                Username = "admin", 
                Email = "admin@algosphere.com", 
                PasswordHash = passwordHash, 
                RankPoints = 9999, 
                Status = "Active" 
            }
        };
        adminList.AddRange(userFaker.Generate(4)); // 4 more random admins

        var teacherList = new List<User> {
            new User { 
                Username = "teacher", 
                Email = "teacher@fpt.edu.vn", 
                PasswordHash = passwordHash, 
                RankPoints = 2000, 
                Status = "Active" 
            }
        };
        teacherList.AddRange(userFaker.Generate(99));

        var studentList = new List<User> {
            new User { 
                Username = "student", 
                Email = "student@test.com", 
                PasswordHash = passwordHash, 
                RankPoints = 500, 
                Status = "Active" 
            }
        };
        studentList.AddRange(userFaker.Generate(4999));

        var allUsers = adminList.Concat(teacherList).Concat(studentList).ToList();
        await SaveInChunks(context, adminList);
        await SaveInChunks(context, teacherList);
        await SaveInChunks(context, studentList);

        // 4. Role & Org Assignments (Using IDs to avoid IDENTITY_INSERT errors after Clear())
        Console.WriteLine("🔗 Assigning Roles & Organizations...");
        var roleAssignments = new List<UserRole>();
        var orgMembers = new List<OrganizationMember>();

        foreach (var u in adminList) roleAssignments.Add(new UserRole { UserId = u.Id, RoleId = roles[0].Id });
        foreach (var u in teacherList) {
            roleAssignments.Add(new UserRole { UserId = u.Id, RoleId = roles[1].Id });
            orgMembers.Add(new OrganizationMember { UserId = u.Id, OrganizationId = faker.PickRandom(orgIds), Role = OrgRole.Teacher });
        }
        foreach (var u in studentList) {
            roleAssignments.Add(new UserRole { UserId = u.Id, RoleId = roles[3].Id });
            orgMembers.Add(new OrganizationMember { UserId = u.Id, OrganizationId = faker.PickRandom(orgIds), Role = OrgRole.Student });
        }

        await SaveInChunks(context, roleAssignments);
        await SaveInChunks(context, orgMembers);

        // 5. Topics & Exercises
        var categoryNames = new[] { "Cấu trúc dữ liệu", "Giải thuật cơ bản", "Quy hoạch động", "Đồ thị", "Toán học", "Trí tuệ nhân tạo" };
        var categories = categoryNames.Select(name => new Category { Name = name }).ToList();
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // 5.1 Tournaments (E-SPORTS DIVISION)
        Console.WriteLine("🏆 Seeding E-Sports Division (Capacity Awareness)...");
        var tournaments = new List<Tournament>
        {
            new Tournament { 
                Title = "Spring Championship 2026", 
                Description = "Giải đấu đã đủ số lượng, đang chờ khởi tạo sơ đồ thi đấu.", 
                Status = "Ongoing", 
                StartDate = DateTime.UtcNow.AddDays(-5), 
                EndDate = DateTime.UtcNow.AddDays(2),
                RoundDurationMinutes = 120,
                MinParticipants = 64,
                MaxParticipants = 64
            },
            new Tournament { 
                Title = "Code Arena Master", 
                Description = "Chờ đủ số lượng chiến binh tham gia để kích hoạt Bracket tự động.", 
                Status = "Ongoing", 
                StartDate = DateTime.UtcNow.AddDays(-1), 
                EndDate = DateTime.UtcNow.AddDays(1),
                RoundDurationMinutes = 60,
                MinParticipants = 50,
                MaxParticipants = 50
            },
            new Tournament { 
                Title = "Summer Grand Prix 2026", 
                Description = "Giải đấu sắp khởi tranh. Đăng ký ngay!", 
                Status = "Upcoming", 
                StartDate = DateTime.UtcNow.AddDays(15), 
                EndDate = DateTime.UtcNow.AddDays(22),
                MinParticipants = 32,
                MaxParticipants = 128
            }
        };
        context.Tournaments.AddRange(tournaments);
        await context.SaveChangesAsync();

        // Add participants
        var participants = new List<TournamentParticipant>();
        
        // 1. Spring Championship (64/64 - Full)
        var top64 = studentList.OrderByDescending(s => s.RankPoints).Take(64);
        foreach(var s in top64) participants.Add(new TournamentParticipant { TournamentId = tournaments[0].Id, UserId = s.Id });

        // 2. Code Arena Master (48/50 - Waiting for 2 more)
        var next48 = studentList.OrderByDescending(s => s.RankPoints).Skip(64).Take(48);
        foreach(var s in next48) participants.Add(new TournamentParticipant { TournamentId = tournaments[1].Id, UserId = s.Id });

        await SaveInChunks(context, participants);

        // 5. Topics & Exercises (Hardcoded Real Names for Solution Matching)
        var categoryNames = new[] { "Cấu trúc dữ liệu", "Giải thuật cơ bản", "Quy hoạch động", "Xử lý mảng", "Đồ thị", "Toán học" };
        var categories = categoryNames.Select(name => new Category { Name = name }).ToList();
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        Console.WriteLine("📚 Seeding Real-World Topics & Exercises...");
        var topics = new List<Topic>
        {
            new Topic { Name = "Giải thuật cơ bản", Description = "Sắp xếp và Tìm kiếm kinh điển.", OrderIndex = 1, CategoryId = categories[1].Id },
            new Topic { Name = "Cấu trúc dữ liệu", Description = "Stack, Queue, Linked List, Tree.", OrderIndex = 2, CategoryId = categories[0].Id },
            new Topic { Name = "Quy hoạch động", Description = "Tối ưu hóa bài toán bằng DP.", OrderIndex = 3, CategoryId = categories[2].Id },
            new Topic { Name = "Xử lý mảng", Description = "Kỹ thuật thao tác mảng O(n).", OrderIndex = 4, CategoryId = categories[3].Id }
        };
        context.Topics.AddRange(topics);
        await context.SaveChangesAsync();

        var exercises = new List<Exercise>();
        
        // Topic 1: Algorithms
        exercises.Add(new Exercise { Title = "Bubble Sort", TopicId = topics[0].Id, DifficultyLevel = "Easy", Points = 100, Content = "Viết thuật toán sắp xếp nổi bọt để sắp xếp mảng tăng dần.", TimeLimitMs = 1000, MemoryLimitKb = 64000 });
        exercises.Add(new Exercise { Title = "Binary Search", TopicId = topics[0].Id, DifficultyLevel = "Easy", Points = 150, Content = "Tìm kiếm nhị phân trên mảng đã sắp xếp.", TimeLimitMs = 1000, MemoryLimitKb = 64000 });
        exercises.Add(new Exercise { Title = "Merge Sort", TopicId = topics[0].Id, DifficultyLevel = "Medium", Points = 300, Content = "Sắp xếp trộn O(n log n).", TimeLimitMs = 1500, MemoryLimitKb = 128000 });
        exercises.Add(new Exercise { Title = "Quick Sort", TopicId = topics[0].Id, DifficultyLevel = "Medium", Points = 350, Content = "Sắp xếp nhanh dùng Pivot.", TimeLimitMs = 1500, MemoryLimitKb = 128000 });

        // Topic 2: Data Structures
        exercises.Add(new Exercise { Title = "Valid Parentheses", TopicId = topics[1].Id, DifficultyLevel = "Easy", Points = 100, Content = "Kiểm tra dấu ngoặc hợp lệ dùng Stack.", TimeLimitMs = 1000, MemoryLimitKb = 64000 });
        exercises.Add(new Exercise { Title = "Reverse Linked List", TopicId = topics[1].Id, DifficultyLevel = "Medium", Points = 250, Content = "Đảo ngược danh sách liên kết đơn.", TimeLimitMs = 1000, MemoryLimitKb = 64000 });

        // Topic 3: DP
        exercises.Add(new Exercise { Title = "Climbing Stairs", TopicId = topics[2].Id, DifficultyLevel = "Easy", Points = 100, Content = "Bài toán leo cầu thang (Fibonacci DP).", TimeLimitMs = 1000, MemoryLimitKb = 64000 });
        exercises.Add(new Exercise { Title = "Fibonacci", TopicId = topics[2].Id, DifficultyLevel = "Easy", Points = 50, Content = "Tính số Fibonacci thứ n.", TimeLimitMs = 1000, MemoryLimitKb = 64000 });

        // Topic 4: Arrays
        exercises.Add(new Exercise { Title = "Two Sum", TopicId = topics[3].Id, DifficultyLevel = "Easy", Points = 100, Content = "Tìm 2 số có tổng bằng Target.", TimeLimitMs = 1000, MemoryLimitKb = 64000 });
        exercises.Add(new Exercise { Title = "Contains Duplicate", TopicId = topics[3].Id, DifficultyLevel = "Easy", Points = 80, Content = "Kiểm tra mảng có phần tử trùng lặp.", TimeLimitMs = 1000, MemoryLimitKb = 64000 });
        exercises.Add(new Exercise { Title = "Single Number", TopicId = topics[3].Id, DifficultyLevel = "Easy", Points = 120, Content = "Tìm số duy nhất xuất hiện 1 lần dùng XOR.", TimeLimitMs = 1000, MemoryLimitKb = 64000 });
        exercises.Add(new Exercise { Title = "Max Subarray", TopicId = topics[3].Id, DifficultyLevel = "Medium", Points = 200, Content = "Tìm tổng mảng con lớn nhất (Kadane).", TimeLimitMs = 1000, MemoryLimitKb = 64000 });
        exercises.Add(new Exercise { Title = "Best Time to Buy Stock", TopicId = topics[3].Id, DifficultyLevel = "Easy", Points = 150, Content = "Tối ưu lợi nhuận mua bán chứng khoán.", TimeLimitMs = 1000, MemoryLimitKb = 64000 });

        // Generate some extra random ones for other topics to fill the database
        for (int i = 4; i < 30; i++)
        {
            var extraTopic = new Topic { Name = faker.Commerce.ProductName(), OrderIndex = i + 1, CategoryId = faker.PickRandom(categories).Id };
            context.Topics.Add(extraTopic);
            await context.SaveChangesAsync();
            for (int j = 0; j < 5; j++)
            {
                exercises.Add(new Exercise {
                    Title = faker.Company.CatchPhrase(),
                    TopicId = extraTopic.Id,
                    DifficultyLevel = faker.PickRandom("Easy", "Medium", "Hard"),
                    Points = faker.Random.Int(50, 500),
                    Content = faker.Lorem.Paragraph(),
                    TimeLimitMs = 1000,
                    MemoryLimitKb = 64000
                });
            }
        }

        await SaveInChunks(context, exercises);
        var exerciseIds = exercises.Select(e => e.Id).ToList();

        // 6. Forums & Discussions
        var forums = new List<Forum> {
            new Forum { Title = "Thảo luận thuật toán", Description = "Nơi trao đổi các bài tập khó" },
            new Forum { Title = "Góc tuyển dụng", Description = "Chia sẻ kinh nghiệm phỏng vấn Tech" }
        };
        context.Forums.AddRange(forums);
        await context.SaveChangesAsync();

        var discussions = new List<Discussion>();
        for (int i = 1; i <= 50; i++)
        {
            discussions.Add(new Discussion {
                ForumId = faker.PickRandom(forums).Id,
                UserId = faker.PickRandom(studentList).Id,
                Title = faker.Lorem.Sentence(5),
                Content = faker.Lorem.Paragraph(),
                Views = faker.Random.Int(0, 10000)
            });
        }
        await SaveInChunks(context, discussions);

        // 7. Comments (Hierarchical)
        Console.WriteLine("💬 Generating hierarchical comments...");
        var comments = new List<Comment>();
        foreach (var disc in discussions)
        {
            int commentCount = faker.Random.Int(10, 20); // Reduced for speed in seed
            for (int k = 1; k <= commentCount; k++)
            {
                var parent = new Comment {
                    DiscussionId = disc.Id,
                    UserId = faker.PickRandom(studentList).Id,
                    Content = faker.Lorem.Sentence(),
                    MaterializedPath = $"{k}/"
                };
                comments.Add(parent);
            }
        }
        await SaveInChunks(context, comments);

        // 8. Submissions (50,000 records)
        Console.WriteLine("⚡ Generating 50,000 submissions (Chunked)...");
        var studentIds = studentList.Select(s => s.Id).ToList();
        
        for (int batch = 0; batch < 25; batch++)
        {
            var batchSubmissions = new List<Submission>();
            for (int i = 0; i < CHUNK_SIZE; i++)
            {
                var snippetIndex = faker.Random.Int(0, CodeSnippets.Length - 1);
                var status = faker.PickRandom(Statuses);
                var sub = new Submission
                {
                    UserId = faker.PickRandom(studentIds),
                    ExerciseId = faker.PickRandom(exerciseIds),
                    Status = status,
                    SourceCode = CodeSnippets[snippetIndex],
                    Language = snippetIndex switch {
                        0 => "C#",
                        1 => "JavaScript",
                        2 => "Python",
                        3 => "Python",
                        4 => "Python",
                        _ => "C#"
                    },
                    CreatedAt = faker.Date.Past(6),
                    ExecutionTimeMs = status switch {
                        "Time Limit Exceeded" => 1001,
                        "Accepted" => faker.Random.Int(10, 200),
                        _ => faker.Random.Int(50, 500)
                    },
                    MemoryUsedKb = status == "Memory Limit Exceeded" ? 65000 : faker.Random.Int(1024, 8192),
                    SuspicionLevel = faker.Random.Bool(0.05f) ? "Low" : "None"
                };
                batchSubmissions.Add(sub);
            }
            await SaveInChunks(context, batchSubmissions);
            Console.WriteLine($"✅ Batch {batch + 1}/25 finished ({ (batch + 1) * CHUNK_SIZE } total)");
        }

        // --- FINALIZATION ---
        context.ChangeTracker.AutoDetectChangesEnabled = true;
        timer.Stop();
        Console.WriteLine($"\n✨ Seeding completed in {timer.Elapsed.TotalSeconds:F2}s");
    }

    private static async Task SaveInChunks<T>(AlgoSphereDbContext context, IEnumerable<T> entities) where T : class
    {
        var list = entities.ToList();
        for (int i = 0; i < list.Count; i += CHUNK_SIZE)
        {
            var chunk = list.Skip(i).Take(CHUNK_SIZE);
            context.Set<T>().AddRange(chunk);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear(); 
        }
    }
}
