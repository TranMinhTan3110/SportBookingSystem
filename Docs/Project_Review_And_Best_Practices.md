# TÀI LIỆU ĐÁNH GIÁ DỰ ÁN & ĐỊNH HƯỚNG NÂNG CẤP (SPORT BOOKING SYSTEM)

Tài liệu này tổng hợp toàn bộ các kiến thức, đánh giá kiến trúc, các lỗi hiện đọng và định hướng nâng cấp dự án lên cấp độ chuyên nghiệp (Production-ready), rất hữu ích để làm tư liệu review code hoặc trả lời phỏng vấn.

---

## 1. Điểm mạnh của dự án (Strengths)

Dự án đang được thiết kế rất tốt so với mặt bằng chung của Junior/Fresher. Dưới đây là những "điểm ăn tiền" mà bạn đã làm đúng chuẩn:

- **Kiến trúc N-Tier (Service-Layer Pattern):** Bạn đã tách biệt hoàn toàn Controller (tầng giao tiếp) và Service (tầng nghiệp vụ logic). Không nhồi nhét code xử lý vào Controller.
- **Bảo mật (Security):** Bạn đã sử dụng thư viện `BCrypt` để băm (hash) mật khẩu người dùng trước khi lưu xuống Database. Đây là tiêu chuẩn bảo mật bắt buộc thay vì lưu plain-text.
- **Eager Loading (`.Include()`):** Bạn đã chủ động sử dụng `.Include()` của EF Core để nối (JOIN) các bảng. Việc này giúp tránh được thảm họa "Lazy Loading" gây sập server.
- **Tư duy Đồng bộ/Bất đồng bộ chính xác:** Ở hàm `GenerateQrCode` (tạo mã QR), bạn đã viết nó dưới dạng Đồng bộ (Synchronous). Đây là một điểm sáng, vì việc vẽ hình ảnh là tác vụ tính toán (CPU-Bound), việc KHÔNG xài `async` ở đây là hoàn toàn chuẩn xác để tránh hao phí tài nguyên.

---

## 2. Các lỗi thiết kế (Architectural Flaws) & Bad Practices cần khắc phục

Dưới đây là các vị trí cần refactor trước khi đưa dự án ra môi trường thực tế:

### 2.1. Lỗi hiệu năng chí mạng: N+1 Query
- **Vị trí:** `BookingService.cs` (hàm `GetPendingBookingsAsync`).
- **Vấn đề:** Trong vòng lặp `foreach`, bạn gọi lệnh lấy `Transactions` từ database. Nếu có 100 booking, sẽ sinh ra 101 câu query xuống DB.
- **Cách khắc phục:** Query toàn bộ giao dịch bằng `.Contains()` hoặc `.Join()` trong 1 lần duy nhất trước khi vào vòng lặp.

### 2.2. Khóa đồng thời (Concurrency) bị trói vào SQL Server
- **Vị trí:** `BookingService.cs` (hàm `BookPitchAsync`).
- **Vấn đề:** Dùng `EXEC @res = sp_getapplock` (Pessimistic Locking của SQL Server) để chống đặt trùng sân. Điều này làm app bị "trói chết" (Tightly Coupled) và không thể chuyển sang dùng PostgreSQL hay MySQL.
- **Cách khắc phục:** Dùng **Optimistic Concurrency** của EF Core (thêm cột `RowVersion` hoặc `[ConcurrencyCheck]`).

### 2.3. Bỏ qua Validate ở Backend
- **Vị trí:** `LoginController.cs` (hàm `SignUp`).
- **Vấn đề:** Không kiểm tra `if (!ModelState.IsValid)`, vô tình vô hiệu hóa toàn bộ Data Annotations (như `[EmailAddress]`) ở đầu vào, tạo lỗ hổng bảo mật.

### 2.4. Lộ lọt thông tin nhạy cảm qua `ex.Message`
- **Vị trí:** Khắp các thư mục `Controllers`.
- **Vấn đề:** Trả thẳng `ex.Message` ra ngoài bằng file JSON. Hacker có thể lợi dụng để đọc thông tin DB.
- **Cách khắc phục:** Dùng **Global Exception Handling Middleware** để chụp lỗi, ghi log bằng **Serilog**, và chỉ trả về frontend thông báo chung chung ("Đã có lỗi hệ thống").

### 2.5. Đồng bộ (Synchronous) gây nghẽn Thread
- **Vị trí:** `ManageBookingADController.cs` (các dòng gọi `_context...ToList()`).
- **Vấn đề:** Bắt Thread đứng chờ I/O thay vì giải phóng nó.
- **Cách khắc phục:** Luôn luôn xài `.ToListAsync()`, `.FirstOrDefaultAsync()` khi lấy dữ liệu từ `_context`.

---

## 3. Chiến lược Tối ưu hóa Tốc độ (Performance Optimizations) & Scale hệ thống

Để hệ thống chạy nhanh và chịu tải lớn (như Shopee, Facebook), đây là những điểm cốt lõi bạn cần áp dụng:

### 3.1. Caching tĩnh với Redis (Distributed Cache)
- **Vấn đề hiện tại:** Mỗi lần khách hàng vào trang chủ xem danh sách sân bóng, hệ thống lại chui xuống SQL Server để query. Dữ liệu sân bóng thì ít khi thay đổi, việc query liên tục làm chậm web và tốn tải DB. Hơn nữa, việc dùng `IMemoryCache` cho hệ thống OTP (trong `LoginServices.cs`) sẽ bị lỗi nếu chạy trên nhiều Server.
- **Giải pháp thực tế:** Dùng **Redis**. Ném danh sách sân bóng, bảng giá, khung giờ vào Redis. Tốc độ lấy từ Redis nhanh gấp 10 lần SQL Server (< 1ms). Dùng Redis lưu OTP để đảm bảo tính đồng bộ trên mọi máy chủ.

### 3.2. Dùng `.AsNoTracking()` cho các truy vấn Read-only
- **Vấn đề hiện tại:** Quên dùng `.AsNoTracking()` khi query danh sách sân để hiển thị.
- **Giải pháp:** Đối với các API chỉ để XEM (không chỉnh sửa data), luôn thêm `.AsNoTracking()`. Ví dụ: `_context.Pitches.AsNoTracking().ToListAsync()`. Điều này dỡ bỏ "Camera theo dõi" của EF Core, tiết kiệm cực nhiều RAM và CPU.

### 3.3. Hàng đợi (Message Queue: RabbitMQ / Kafka)
- **Vấn đề hiện tại:** Hàm Đặt sân làm quá nhiều việc nặng (Trừ tiền, Tạo QR, Ghi Log, Update DB), khiến thời gian phản hồi bị kéo dài, dễ nghẽn cổ chai lúc cao điểm.
- **Giải pháp:** Đẩy luồng đặt sân vào **RabbitMQ**. API phản hồi ngay lập tức "Đang xử lý". Ở background, các Worker sẽ âm thầm móc Queue ra để xử lý các việc nặng. Giúp Database không bị đột quỵ lúc săn sale.

---

## 4. Chiến lược Triển khai (Deployment) lên AWS EC2

Dự án hiện tại là mô hình **MVC (Monolith - Khối nguyên)**, tức là Front-end (.cshtml) và Back-end (C#) được đóng gói chung thành một khối duy nhất. Để đưa hệ thống lên máy ảo EC2 của Amazon Web Services (AWS), có 2 cách tiếp cận phổ biến:

### Cách 1: Triển khai trực tiếp lên Ubuntu (Nginx Reverse Proxy)
- **Bước 1 (Publish):** Đóng gói code thành file thực thi bằng lệnh `dotnet publish -c Release`.
- **Bước 2 (Cài đặt):** Trên máy ảo Ubuntu EC2, cài đặt `.NET Runtime` và `Nginx`.
- **Bước 3 (Run):** Chạy ứng dụng ngầm ở localhost (cổng 5000) bằng lệnh `dotnet SportBookingSystem.dll`.
- **Bước 4 (Reverse Proxy):** Cấu hình Nginx hứng mọi luồng truy cập HTTP/HTTPS từ người dùng (cổng 80/443) và "đá" luồng dữ liệu đó vào cổng 5000 cho ứng dụng .NET xử lý.

### Cách 2: Triển khai bằng Docker (Best Practice - Khuyên dùng)
Dự án đã có sẵn `Dockerfile` và `docker-compose.yml`, đây là lợi thế cực lớn.
- Không cần cài .NET hay Nginx lằng nhằng lên EC2. Chỉ cần cài duy nhất `Docker`.
- Kéo code về EC2 và gõ lệnh: `docker-compose up -d --build`.
- Docker sẽ tự động tải môi trường, biên dịch code, và bọc ứng dụng lại trong một Container. Đảm bảo ứng dụng chạy trên EC2 y hệt như chạy trên máy tính cá nhân của bạn, không bao giờ bị lỗi "thiếu môi trường".

*(Lưu ý: Luôn nhớ trỏ Connection String trong `appsettings.json` sang một Cloud Database thực tế như AWS RDS thay vì dùng localhost).*

---

## 5. Lộ trình nâng cấp lên Giai đoạn 5 (Senior/Production-ready)

1. **Clean Architecture & SOLID:** Giữ Logic nghiệp vụ ở trung tâm (Domain), tách biệt hoàn toàn khỏi ASP.NET và Database. Phá bỏ sự phụ thuộc chồng chéo.
2. **Repository & Unit of Work:** Trừu tượng hóa hoàn toàn `DbContext`, giúp dễ dàng viết **Unit Test** và thay đổi Database.
3. **Authentication (JWT & Identity):** Đổi từ Cookie Auth sang JSON Web Token (JWT) để hỗ trợ viết App Mobile (iOS/Android). Áp dụng ASP.NET Core Identity để quản lý user chuẩn chỉnh.
4. **Background Jobs (Hangfire):** Quản lý các tác vụ dọn dẹp, hủy đơn quá giờ một cách chuyên nghiệp, có UI theo dõi lỗi thay vì dùng Hosted Service tự chế.
5. **Docker & Cloud (Azure/AWS):** Đóng gói toàn bộ ứng dụng thành Container để chạy nhất quán trên mọi môi trường, chuẩn bị cho việc Deploy lên mây.
