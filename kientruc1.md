# KIẾN TRÚC HỆ THỐNG - PROJECT ALGOSPHERE V3.0 (ENTERPRISE EDITION)

Tài liệu phân tích và thiết kế chi tiết cho nền tảng Trực quan hóa Thuật toán, Đấu trường Lập trình & Trợ lý AI AlgoSphere.

---

## Mở đầu

* **Tầm nhìn dự án:** AlgoSphere không chỉ là một công cụ trực quan hóa (Visualizer) mà định hướng trở thành một hệ sinh thái EdTech toàn diện. Nền tảng kết hợp giữa học tập tương tác, đấu trường thuật toán cạnh tranh theo thời gian thực và mạng xã hội chia sẻ tri thức dành cho cộng đồng lập trình viên.
* **Mục tiêu mở rộng:**
* Quản lý dữ liệu lớn với kiến trúc cơ sở dữ liệu lên tới hơn 30 bảng, thiết kế chuẩn hóa (Normalization 3NF) kết hợp NoSQL để tối ưu truy vấn cấp doanh nghiệp.
* Tích hợp chế độ "Code Arena" cho phép người dùng thi đấu đối kháng (1vs1) theo mùa giải (Seasons) với cơ chế chống gian lận sâu (Record/Replay) và đồng bộ Server-authoritative.
* Hệ thống được thiết kế theo hướng phân tán với **API Gateway**, sẵn sàng scale-out để phục vụ hàng chục ngàn CCU với hạ tầng bảo mật Edge chống DDoS chuyên biệt cho E-Sports.



---

## Chương 1: Cơ sở lý thuyết & Kiến trúc Hệ thống

* **Frontend:** Vue.js 3 (Composition API), Pinia (State Management), D3.js & GSAP (Render Engine 60FPS).
* **Security, Edge & API Gateway:** * Triển khai lớp **WAF & CDN (Cloudflare hoặc AWS WAF)** đứng đầu chiến tuyến để cache các tài nguyên tĩnh và tự động chặn lọc traffic độc hại, bảo vệ hệ thống khỏi các đợt tấn công DDoS cường độ cao trong thời gian diễn ra mùa giải.
* Sử dụng **Ocelot (hoặc Kong)** làm cổng giao tiếp duy nhất (API Gateway) đứng sau WAF để điều phối traffic đến các service nội bộ.
* Tách biệt hệ thống xác thực thành một **Identity Provider độc lập (Keycloak hoặc Duende IdentityServer)** để xử lý linh hoạt logic phân quyền RBAC.


* **Backend Core:** ASP.NET Core 8 Web API áp dụng Clean Architecture, Transactional Outbox Pattern để đồng bộ an toàn.
* **Real-time Engine & Scale-out:** SignalR cho Chat, Notification và broadcast trạng thái thi đấu. Tích hợp **Redis Backplane (hoặc Azure SignalR Service)** để đồng bộ các kết nối WebSocket chéo qua nhiều server instances, đảm bảo tính ổn định khi hệ thống scale-out lên hàng chục ngàn CCU. Message Queue (RabbitMQ/Kafka) xử lý tác vụ chấm điểm bất đồng bộ.
* **Storage & Database Layer:**
* **SQL Server:** Nguồn dữ liệu chân lý (Source of Truth) cho các luồng ghi (Write) quan trọng áp dụng Optimistic Concurrency Control.
* **MongoDB / ElasticSearch:** Đồng bộ dữ liệu luồng đọc (Read) tần suất cao cho mạng xã hội và AI Analytics (CQRS).
* **Object Storage (S3/MinIO):** Lưu trữ chuyên dụng cho các tệp tĩnh phi cấu trúc và dung lượng lớn (Avatar người dùng, File ZIP chứa Test Cases, hình ảnh đính kèm trong Forum/Chat) để giảm tải hoàn toàn I/O cho Database.
* **Redis:** Caching Leaderboard, Session trạng thái thi đấu, Rate limiting.


* **Execution Sandbox (Warm Pool Architecture):** Sử dụng ảo hóa **MicroVM (gVisor hoặc AWS Firecracker)**. Để khắc phục độ trễ Cold Start, hệ thống duy trì một **"Warm Pool"** (các MicroVM được khởi động sẵn). Khi có yêu cầu, code được nạp ngay vào VM có sẵn giúp thời gian khởi chạy dưới 1 giây.
* **Observability (Tính quan sát):** Tích hợp giám sát với **OpenTelemetry, Prometheus và Grafana** để tracing xuyên suốt từ WAF, Gateway qua Queue đến các Worker.

---

## Chương 2: Phân tích & Thiết kế Cơ sở dữ liệu (32 Tables ERD)

*(Các phân hệ dữ liệu giữ nguyên cấu trúc chuẩn hóa, nhưng các trường lưu trữ file như `AvatarUrl`, `LogoUrl` sẽ lưu link trỏ trực tiếp về Object Storage/CDN thay vì lưu file vật lý trên server).*

### Phân hệ 1: Identity & Access Management (Ủy quyền cho Identity Provider)

1. `Users`: Id, Username, Email, AvatarUrl *(S3 CDN Link)*, Status, CreatedAt.
2. `Roles`: Id, RoleName.
3. `UserRoles`: UserId, RoleId.
4. `UserSessions`: Id, UserId, RefreshToken, IpAddress, DeviceInfo, ExpiresAt.
5. `OAuthConnections`: Id, UserId, Provider, ProviderUserId.

### Phân hệ 2: Core Learning & DSA (Nội dung Học tập)

6. `Categories`: Id, Name.
7. `Topics`: Id, CategoryId, Name, Description, OrderIndex.
8. `Exercises`: Id, TopicId, Title, Content, DifficultyLevel, TimeLimitMs, MemoryLimitKb, Points.
9. `SupportedLanguages`: Id, Name, Version, IsActive.
10. `ExerciseBoilerplates`: ExerciseId, LanguageId, TemplateCode.
11. `TestCases`: Id, ExerciseId, InputData, ExpectedOutput, IsHidden, Weight. *(Dữ liệu Test Case lớn sẽ đóng gói file ZIP đẩy lên S3, DB chỉ lưu S3 Object Key).*
12. `ExerciseHints`: Id, ExerciseId, HintText, UnlockCost.

### Phân hệ 3: Submissions & Sandbox Execution (Chấm điểm & Anti-Cheat)

13. `Submissions`: Id, UserId, ExerciseId, LanguageId, Status, SubmittedAt.
14. `SubmissionCodeDeltas`: SubmissionId, **EventDeltas** *(Chuỗi sự kiện gõ phím phục vụ Anti-Cheat).*
15. `ExecutionMetrics`: SubmissionId, ExecutionTimeMs, MemoryUsedKb, CPUCycles.
16. `SubmissionLogs`: SubmissionId, CompilerMessage, ErrorTrace.

### Phân hệ 4: Gamification & Code Arena (Đấu trường & Xếp hạng)

17. `Seasons`: Id, Name, StartDate, EndDate.
18. `Ranks`: Id, RankName, MinPoints, MaxPoints.
19. `UserRanks`: UserId, SeasonId, RankId, CurrentEloPoints, MatchesPlayed, **RowVersion**.
20. `ArenaMatches`: Id, SeasonId, Player1Id, Player2Id, ExerciseId, WinnerId, StartTime, EndTime.
21. `Badges`: Id, Name, Description, IconUrl *(S3 CDN Link)*, CriteriaType.
22. `UserBadges`: UserId, BadgeId, EarnedAt.
23. `DailyQuests`: Id, Title, Description, RewardPoints, GoalCount.
24. `UserQuests`: UserId, QuestId, Progress, IsClaimed, DateAssigned, **RowVersion**.

### Phân hệ 5: Community & Forum (Cộng đồng)

25. `Forums`: Id, Title, Description.
26. `Discussions`: Id, ForumId, UserId, Title, Content, Views, CreatedAt. *(Ảnh đính kèm trong Content được upload qua API riêng và trả về link S3).*
27. `Comments`: Id, DiscussionId, UserId, ParentCommentId, **MaterializedPath**, Content.
28. `DiscussionUpvotes`: UserId, DiscussionId.
29. `CommentUpvotes`: UserId, CommentId.

### Phân hệ 6: AI Analytics & Mentorship

30. `AIChatSessions`: Id, UserId, ExerciseId, StartedAt.
31. `AIMessages`: Id, SessionId, Sender, MessageContext, CodeSnapshot, SentAt.
32. `LearningAnalytics`: UserId, TopicId, TimeSpentSeconds, ErrorFrequency, DropOffRate.

### Phân hệ 7: Monetization & B2C SaaS

33. `SubscriptionPlans`: Id, Name, Price, BillingCycle, FeaturesList.
34. `UserSubscriptions`: Id, UserId, PlanId, StartDate, EndDate, Status.
35. `PaymentTransactions`: Id, UserId, Amount, Currency, PaymentGateway, ExternalTransactionId, Status, CreatedAt.
36. `Invoices`: Id, TransactionId, InvoiceNumber, TaxAmount, TotalAmount, IssuedDate.

### Phân hệ 8: B2B Organizations & Classrooms

37. `Organizations`: Id, Name, Domain, ContactEmail, MaxSeats, IsActive.
38. `OrganizationUsers`: OrgId, UserId, OrgRole.
39. `Classrooms`: Id, OrgId, Name, Description, InviteCode, CreatedById.
40. `ClassroomMembers`: ClassroomId, UserId, JoinedAt.
41. `Assignments`: Id, ClassroomId, Title, Description, StartTime, DueDate.
42. `AssignmentExercises`: AssignmentId, ExerciseId, CustomPoints.
43. `AssignmentSubmissions`: Id, AssignmentId, UserId, ExerciseId, SubmissionId, FinalScore.

### Phân hệ 9: Tournaments & Team E-Sports

44. `Teams`: Id, Name, TagCode, LogoUrl *(S3 CDN Link)*, CaptainId, CreatedAt.
45. `TeamMembers`: TeamId, UserId, Role, JoinedAt.
46. `Tournaments`: Id, Name, Description, StartDate, EndDate, Format, PrizePool.
47. `TournamentRegistrations`: TournamentId, TeamId, Status, RegisteredAt.
48. `TournamentMatches`: Id, TournamentId, RoundNumber, Team1Id, Team2Id, WinnerId, MatchStartTime, Status.

---

## Chương 3: Phân rã chức năng (WBS) Tỉ mỉ

* **Module Học tập (Student Workspace):**
* Code Editor (Tự động lưu nháp local, Ghi nhận Delta Events).
* Visualizer Engine (Tương tác với các Node 60fps).


* **Module Đấu trường & Anti-Cheat sâu (Arena Mode):**
* Matchmaking kết hợp **Redis Backplane** để định tuyến WebSocket ổn định qua lại giữa các Node.
* Kiến trúc Server-authoritative.
* Anti-Cheat Record/Replay chống paste code hàng loạt.


* **Module Mạng Xã Hội:**
* Đăng bài hỏi đáp, upload ảnh tĩnh lên S3 Object Storage qua Pre-signed URL.


* **Module Quản trị (Admin CMS & DevOps):**
* Quản lý Warm Pool MicroVM.
* Cấu hình giải đấu, update WAF Rules khi phát hiện lưu lượng bất thường.



---

## Chương 4: Thiết kế API Endpoints

| Method | Endpoint | Authorization | Description |
| --- | --- | --- | --- |
| `POST` | `/api/v1/arena/matchmake` | Bearer Token | Đưa User vào hàng đợi. Cấp ngay MicroVM từ Warm Pool. |
| `WS` | `/ws/arena/{matchId}` | WebSocket/SignalR | Kênh Server-authoritative broadcast trạng thái (Scale bằng Redis Backplane). |
| `GET` | `/api/v1/storage/presigned-url` | Bearer Token | Cấp URL tạm thời để Client tự upload file trực tiếp lên S3/MinIO (Bypass API Server để giảm tải băng thông). |
| `POST` | `/api/v1/forums/posts` | Bearer Token | Đăng bài thảo luận (Link ảnh lấy từ kết quả upload S3). |

---

## Chương 5: Kịch bản Kiểm thử Hệ thống (Mở rộng)

| ID | Module | Kịch bản / Các bước thực hiện | Kết quả mong đợi |
| --- | --- | --- | --- |
| **SEC-02** | Security | 100,000 Request rác đổ dồn vào API Đăng nhập cùng lúc. | **WAF/Cloudflare** tự động kích hoạt Rate Limiting và chặn IP lạ ở Edge, hệ thống Backend bên dưới không bị ảnh hưởng. |
| **SCALE-01** | Real-time | 10,000 Users vào chung 1 phòng xem chung kết E-sports (Spectator Mode). | API Server tự động Scale-out thành 5 instances. **Redis Backplane** đồng bộ tin nhắn Chat và trạng thái trận đấu mượt mà cho tất cả người xem mà không rớt gói tin. |
| **STO-01** | Storage | Admin upload file Test Cases dung lượng 50MB. | Client gọi API lấy Pre-signed URL, sau đó đẩy file thẳng lên S3/MinIO. Server không bị chiếm dụng RAM và Bandwidth xử lý file. |

---

## Kết luận & Định hướng Tương lai

Với cấu trúc phòng ngự kiên cố từ ngoài vào trong: **WAF/Cloudflare** cản phá DDoS ở rìa (Edge), **Object Storage** san sẻ gánh nặng băng thông, **API Gateway** điều phối linh hoạt, và **Redis Backplane** bảo chứng cho hàng vạn luồng Real-time, AlgoSphere Enterprise nay đã sở hữu một "bộ khung xương" thép. Kiến trúc này không chỉ sẵn sàng cho môi trường học thuật mà đã đủ tiêu chuẩn để vận hành các giải đấu E-Sports cấp quốc gia một cách mượt mà và bất khả xâm phạm.