# 🌟 AlgoSphere Enterprise V3.0 — High Performance Gamified EdTech

🚀 **AlgoSphere** là nền tảng công nghệ giáo dục (EdTech) tiên tiến tập trung vào học tập, thi đấu lập trình trực tiếp (PVP Coding Arena) thời gian thực, tích hợp Trí tuệ Nhân tạo (AI) và hệ thống sơ đồ giải đấu (Tournaments) chuyên nghiệp.

Phiên bản **V3.0** được tối ưu hóa toàn diện về khả năng chịu tải cao, tích hợp bộ kiểm thử tự động toàn diện và nâng cấp Trình trực quan hóa thuật toán chuyên sâu.

---

## ⚡ Các Tính Năng Nổi Bật V3.0

### 1. Trình Trực Quan Hóa Thuật Toán Nâng Cao (Visualizer Engine)
Trực quan hóa hoạt động của mã nguồn dòng-sau-dòng thông qua 6 chế độ chuyên dụng thiết kế theo phong cách Premium Glassmorphism (Tuân thủ nghiêm ngặt bảng màu Slate, Emerald Green, Amber và Sky Blue):
- **Stack & Queue**: Mô phỏng ngăn xếp dọc LIFO và băng chuyền FIFO động.
- **HashMap (Buckets)**: Trực quan hóa cơ chế băm phân bổ phần tử và hiển thị va chạm (Collision Slots).
- **Two-Pointer**: Đánh dấu các nhãn chỉ số chuyển động nhảy múa phía trên các khối mảng.
- **DP Table**: Ma trận 2D quy hoạch động, tự động highlight cập nhật ô và tooltip hiển thị công thức toán học chi tiết.
- **Graph (D3.js)**: Đồ thị động học tương tác bằng vật lý lực đẩy, mô phỏng duyệt cây BFS/DFS với trạng thái màu sắc rõ ràng.
- **Recursion Tree (D3.js)**: Cấu trúc hình cây đệ quy phân tích sâu tầng gọi hàm (`depth`), tham số và giá trị trả về (`retVal`).

### 2. Sandbox Thực Thi Mã Nguồn Bảo Mật & Tải Cao
- **Giới hạn an toàn (Tracer Caps)**: Tự động phát hiện vòng lặp vô hạn và ngăn chặn tràn bộ nhớ với giới hạn cứng `MaxTraceSteps = 1000` và sức chứa container `100` phần tử.
- **Hiệu năng xử lý siêu tưởng**: Đo lường chịu tải bằng Stress Test đạt **3.284 requests/giây** với độ trễ phản hồi **p95 cực thấp (chỉ 19ms)**, 100% Request thành công.
- **Anti-cheat & Sandbox isolation**: Container Docker cô lập cách ly hoàn toàn mã nguồn người dùng.

### 3. Đấu Trường Real-time (Coding Arena 1vs1)
- Ghép trận xếp hạng tự động dựa trên Elo.
- Đồng bộ thời gian thực bảng tiến độ (Progress syncing) của đối thủ sử dụng **SignalR WebSockets**.

### 4. Hệ Thống Kiểm Thử Tự Động 100% Xanh
- **Unit Testing (Vitest)**: Rà soát logic tính toán snapshot đồng bộ trạng thái của player.
- **E2E Integration Testing (Playwright)**: Chạy mock isolated browser kiểm tra hiển thị hoàn chỉnh của visualizers và UI Arena.

---

## 🛠️ Công Nghệ Sử Dụng

- **Backend**: ASP.NET Core 8, MediatR (CQRS), Entity Framework Core (Optimistic Concurrency), Redis Sorted Sets, SignalR Hubs.
- **Frontend**: Vue.js 3, TypeScript, Pinia, Tailwind CSS, GSAP Animations, D3.js.
- **Hạ tầng & DevOps**: Docker Desktop, SQL Server, MongoDB, Nginx API Gateway (Rate Limiting, CSP Hardening Headers), GitHub Actions CI/CD Pipeline.

---

## 🚀 Hướng Dẫn Khởi Chạy

### 1. Yêu Cầu
- Docker Desktop
- Node.js 20+
- .NET 8 SDK

### 2. Khởi Chạy Hạ Tầng (Database, Caching, MinIO)
```powershell
docker-compose up -d
```

### 3. Chạy Server Backend (WebApi & Sandbox)
Khởi chạy Solution `AlgoSphere.slnx` bằng Visual Studio 2022 hoặc JetBrains Rider với cấu hình dự án `AlgoSphere.Api`.
API sẽ khả dụng tại: `http://localhost:5000` (Qua Cổng Nginx Gateway) hoặc `http://localhost:5141` (Trực tiếp).

### 4. Khởi Chạy Client Frontend
```powershell
cd src/AlgoSphere.Client
npm install
npm run dev
```
Mở trình duyệt truy cập: `http://localhost:5173`.

### 5. Chạy Kiểm Thử Tự Động (Client)
- Chạy Unit Test:
  ```bash
  npx vitest run
  ```
- Chạy E2E Test:
  ```bash
  npx playwright test e2e/visualizers.spec.ts --workers=1
  ```

---

## 📂 Cấu Trúc Thư Mục Dự Án
- `src/AlgoSphere.Domain`: Các Entity và Domain Core.
- `src/AlgoSphere.Application`: Logic nghiệp vụ (CQRS Features, MediatR Handlers).
- `src/AlgoSphere.Infrastructure`: Persistent DBs, Sandbox, AI Mentor, Identity.
- `src/AlgoSphere.Api`: Controllers, Hubs, Nginx config.
- `src/AlgoSphere.Client`: SPA Vue.js (Trực quan hóa, Arena, B2B Dashboards).

---
© 2026 AlgoSphere Enterprise Team. Bảo lưu mọi quyền lợi.
