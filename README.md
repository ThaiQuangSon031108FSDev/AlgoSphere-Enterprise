# AlgoSphere Enterprise V3.0

🚀 **AlgoSphere** là nền tảng EdTech hiện đại tập trung vào việc học và thi đấu thuật toán thời gian thực, tích hợp Trí tuệ nhân tạo (AI) và các tính năng E-Sports chuyên nghiệp.

## 🌟 Tính năng nổi bật
- **Visualizer Engine:** Trực quan hóa thuật toán bằng D3.js & GSAP với cơ chế Trace Log Delta.
- **Code Arena (1vs1):** Ghép cặp thi đấu lập trình thời gian thực sử dụng SignalR.
- **AI Tutor Mentor:** Trợ lý ảo hỗ trợ giải đáp và gợi ý code dựa trên ngữ cảnh (Gemini API).
- **Tournament Engine:** Hệ thống giải đấu quy mô lớn với sơ đồ chia bảng (Brackets).
- **B2B Analytics:** Dashboard quản trị dành cho các trường học và tổ chức giáo dục.

## 🛠️ Công nghệ sử dụng
- **Backend:** ASP.NET Core 8, MediatR (CQRS), Entity Framework Core, JWT.
- **Caching & Real-time:** Redis Sorted Sets, SignalR.
- **Frontend:** Vue.js 3, TypeScript, Tailwind CSS, Pinia, D3.js.
- **Infrastructure:** Docker, SQL Server, MongoDB, Redis, Nginx API Gateway.

## 🚀 Hướng dẫn khởi chạy

### 1. Yêu cầu hệ thống
- Docker Desktop
- .NET 8 SDK
- Node.js 20+

### 2. Khởi chạy hạ tầng (Database & Caching)
```powershell
docker-compose up -d
```

### 3. Khởi chạy Backend
Mở Solution `AlgoSphere.slnx` bằng Visual Studio 2022 hoặc JetBrains Rider và chạy project `AlgoSphere.Api`.
API sẽ tự động chạy tại: `https://localhost:7141` hoặc `http://localhost:5141`.

### 4. Khởi chạy Frontend
```powershell
cd src/AlgoSphere.Client
npm install
npm run dev
```
Truy cập ứng dụng tại: `http://localhost:5173`.

## 📂 Cấu trúc dự án
- `src/AlgoSphere.Domain`: Các thực thể nghiệp vụ cốt lõi.
- `src/AlgoSphere.Application`: Logic nghiệp vụ (CQRS Features, Interfaces).
- `src/AlgoSphere.Infrastructure`: Persistence, AI, Sandbox, Identity.
- `src/AlgoSphere.Api`: Endpoints, Hubs, Controllers.
- `src/AlgoSphere.Client`: Giao diện người dùng Vue.js.

---
© 2026 AlgoSphere Enterprise Team.
