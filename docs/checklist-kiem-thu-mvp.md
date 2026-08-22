# Checklist kiểm thử MVP

## Hệ thống

- Build source bằng `dotnet build`.
- Cập nhật DB bằng `dotnet ef database update -c ApplicationDbContext`.
- Chạy app bằng `dotnet run --urls http://127.0.0.1:5152`.
- Mở `/health` và kiểm tra `status` là `ok`.

## Khu học của bé

- Mở `/kids`.
- Chọn hồ sơ bé.
- Mở `/kids/today`.
- Vào từng bài trong lộ trình.
- Làm đúng một bài chọn đáp án.
- Làm sai một bài chọn đáp án và xem phản hồi thử lại.
- Vẽ/tô nét trên canvas và hoàn thành bài.
- Xem màn tổng kết buổi học.

## Góc phụ huynh

- Đăng ký một tài khoản phụ huynh mới rồi đăng nhập.
- Mở `/parent/dashboard`.
- Xem thẻ hồ sơ bé và biểu đồ 7 ngày.
- Mở báo cáo chi tiết từng bé.
- Xuất CSV báo cáo.
- Tạo/sửa hồ sơ bé.

## Admin

- Đăng nhập bằng `admin@hanhtranglop1.local`.
- Mở `/admin`.
- Mở `/admin/learning-items`.
- Lọc theo nhóm kỹ năng, trạng thái và dạng bài.
- Mở `/admin/catalogs`, xác nhận danh mục chỉ đọc có 10 nhóm và 43 chủ đề.
- Tạo bài tương tác, đổi nhóm và kiểm tra chủ đề đầu tiên của nhóm mới được chọn tự động.
- Đổi lần lượt các mẫu hoạt động và xác nhận bảng thuộc tính cùng preview thay đổi đúng.
- Nhập các lựa chọn và kiểm tra danh sách đáp án đúng tự cập nhật.
- Tạo bài tô nét.
- Mở chi tiết bài học.
- Sửa metadata và cấu hình bằng đúng trình tạo của dạng bài; bài tô nét không được đổi sang mẫu khác trong màn sửa tô nét.
- Chuyển trạng thái nháp, chờ duyệt, xuất bản, lưu trữ.

## Dữ liệu

- Kiểm tra bảng `__EFMigrationsHistory`.
- Kiểm tra có role `Admin`, `Parent`, `ContentEditor`, `Reviewer`.
- Tạo và xuất bản ít nhất một bài trước khi kiểm thử khu bé học.
- Kiểm tra seed không tự tạo bài học, phụ huynh, hồ sơ bé hoặc phần thưởng mẫu.
- Kiểm tra `LearningAttempts` tăng sau khi bé học.
- Kiểm tra `QuestionAttempts` lưu dữ liệu nét vẽ sau bài tô nét.
