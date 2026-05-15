# TÀI LIỆU YÊU CẦU DỰ ÁN (PRD) - ALGOSPHERE V3.0
**Dự án:** AlgoSphere - Nền tảng Trực quan hóa Cấu trúc dữ liệu & Thuật toán
**Loại hình:** Đồ án Tốt nghiệp CNTT (Hệ đào tạo Kỹ sư/Cử nhân)
**Trọng tâm:** Gamification, Real-time Visualization, AI-Assisted Learning.

---

## 1. TỔNG QUAN DỰ ÁN
AlgoSphere không chỉ là một trang web minh họa thuật toán thông thường, mà là một hệ sinh thái học tập tương tác. Hệ thống giải quyết bài toán "trừu tượng hóa" của thuật toán thông qua việc đồng bộ hóa mã nguồn (Code) và hình ảnh (Animation) theo thời gian thực, đồng thời áp dụng các cơ chế Game hóa để tăng động lực học tập cho sinh viên.

## 2. HÀNH TRÌNH NGƯỜI DÙNG (USER JOURNEY)
Quy trình trải nghiệm được thiết kế để dẫn dắt người dùng từ sự tò mò đến việc rèn luyện kỹ năng chuyên sâu:

1.  **Landing Page (Ấn tượng đầu tiên):**
    * Giao diện 3D/Animation thuật toán (Sắp xếp, Cây) chạy ngầm mượt mà.
    * Section giới thiệu các tính năng "Unique": AI Tutor, Real-time IDE, Gamification.
    * Bảng xếp hạng (Leaderboard) thời gian thực hiển thị các "Master" đang dẫn đầu.
2.  **Dashboard (Trung tâm điều khiển):**
    * Hiển thị tiến độ học tập (Heatmap tương tự GitHub).
    * Hệ thống "Skill Tree" (Cây kỹ năng): Người dùng phải hoàn thành các bài học cơ bản (Mảng, Danh sách liên kết) mới mở khóa được các bài nâng cao (Đồ thị, Quy hoạch động).
    * Kho lưu trữ các Badge (Huy chương) đã đạt được.
3.  **Workspace (Không gian thực hành):**
    * Giao diện chia 3 khu vực: Visualizer (Trái), Code Editor (Phải), và AI Chat Panel (Cạnh phải/Dưới).
    * Cho phép người dùng viết code, chạy thử và quan sát animation thay đổi tương ứng.

---

## 3. THIẾT KẾ GIAO DIỆN & HOẠT ẢNH (UI/UX & ANIMATION)
* **Design Style:** Modern Dark-theme (Cơ sở: Slate #0F172A, Accent: Emerald #10B981, Indigo #6366F1).
* **Hiệu ứng:** Glassmorphism, Glow effect cho các Node đang được xét.
* **Animation Logic:**
    * **GSAP (GreenSock):** Quản lý các chuyển động UI, di chuyển Node đơn giản, hiệu ứng chuyển trang.
    * **D3.js / Canvas:** Dùng để render các cấu trúc dữ liệu phức tạp (Graph có >100 nodes, Tree sâu) để đảm bảo duy trì ổn định **60 FPS**.
    * **Cơ chế State-Snapshot:** Lưu lại trạng thái của cấu trúc dữ liệu sau mỗi dòng code thực thi để hỗ trợ tính năng **Step Backward** (Tua ngược thời gian).

---

## 4. TÍNH NĂNG KỸ THUẬT CHI TIẾT

### 4.1. Trình trực quan hóa tương tác (Core Visualizer)
* **Chế độ điều khiển:** Play/Pause, Slider điều chỉnh tốc độ (0.25x đến 4x), Step-by-Step (Tới/Lùi từng bước).
* **Phân loại trạng thái:** Node phải có màu sắc khác biệt cho các trạng thái: `Visiting` (Đang xét), `Comparing` (So sánh), `Swapping` (Đổi chỗ), `Found` (Tìm thấy), `Sorted` (Đã xong).

### 4.2. IDE & Đồng bộ hóa Code (Real-time Synchronization)
* **Editor:** Tích hợp **Monaco Editor** (hỗ trợ IntelliSense, Syntax Highlighting).
* **Line Highlighting:** Khi animation chạy đến bước ứng với dòng code nào, dòng code đó trong editor phải được highlight.
* **Variable Tracker:** Hiển thị giá trị các biến (i, j, pivot, temp...) thay đổi theo thời gian thực ngay trên giao diện.

### 4.3. Môi trường thực thi an toàn (Code Execution Sandbox)
* **Vấn đề:** Tránh người dùng viết mã độc (RCE).
* **Giải pháp:**
    * Code được gửi lên Backend (ASP.NET Core).
    * Backend đẩy code vào một **Docker Container** bị giới hạn (No Network, Max 128MB RAM, Max 2s CPU).
    * Sử dụng gRPC hoặc SignalR để truyền kết quả/logs về Frontend nhanh nhất.

### 4.4. Trợ lý AI (Context-Aware AI Tutor)
* **Công nghệ:** Tích hợp Gemini API hoặc OpenAI API.
* **Cơ chế nhắc bài:** AI không chỉ trả lời câu hỏi chung chung. Mỗi request gửi lên AI sẽ kèm theo "System Prompt" ẩn chứa:
    * Trạng thái hiện tại của cấu trúc dữ liệu.
    * Lỗi mà code người dùng đang gặp phải.
    * Lịch sử các bước người dùng vừa thực hiện.
* **Mục tiêu:** AI đóng vai trò người hướng dẫn (Mentor), gợi ý hướng đi thay vì giải hộ toàn bộ bài toán.

---

## 5. KIẾN TRÚC CÔNG NGHỆ (TECH STACK)
* **Frontend:** Vue.js 3 (Composition API) + Pinia (State management) + Vite.
* **Animation:** GSAP, D3.js.
* **Backend:** ASP.NET Core 8 (Clean Architecture).
* **Database:** SQL Server (Lưu User, Progress, Bài tập) + Redis (Cache Leaderboard).
* **Infrastruture:** Docker (Sandbox), Nginx, CI/CD GitHub Actions.

---

## 6. DANH MỤC CÁC CẤU TRÚC DỮ LIỆU & THUẬT TOÁN CẦN CÓ
1.  **Cơ bản:** Array (Sorting: Bubble, Quick, Merge, Heap), Linked List (Singly, Doubly).
2.  **Trung cấp:** Stack, Queue, Hash Table (Collision handling).
3.  **Nâng cao:** Tree (BST, AVL, Red-Black), Graph (BFS, DFS, Dijkstra, Prim, Kruskal).
4.  **Chuyên đề tuyển dụng:** Các bài toán phổ biến trên LeetCode/Hackerrank.

---

## 7. TIÊU CHÍ ĐÁNH GIÁ ĐỒ ÁN (KPIs)
* **Hiệu năng:** Animation không giật lag ở mức 60 FPS với số lượng Node trung bình.
* **Tính bảo mật:** Không thể thực thi lệnh hệ thống thông qua IDE.
* **Độ trễ:** Thời gian từ khi bấm Run Code đến khi có phản hồi < 1s (trong điều kiện mạng ổn định).
* **UX:** Người dùng mới có thể hiểu cách vận hành thuật toán trong vòng 3 phút trải nghiệm.

---
*Tài liệu này là tài sản của dự án AlgoSphere. Nghiêm cấm sao chép cho mục đích thương mại mà không có sự đồng ý của đội ngũ phát triển.*