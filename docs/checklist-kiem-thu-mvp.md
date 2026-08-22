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

- Đăng nhập bằng `phuhuynh@hanhtranglop1.local`.
- Mở `/parent/dashboard`.
- Xem thẻ hồ sơ bé và biểu đồ 7 ngày.
- Mở báo cáo chi tiết từng bé.
- Xuất CSV báo cáo.
- Tạo/sửa hồ sơ bé.

## Admin

- Đăng nhập bằng `admin@hanhtranglop1.local`.
- Mở `/admin`.
- Mở `/admin/learning-items`.
- Lọc theo trạng thái và dạng bài.
- Tạo bài chọn đáp án.
- Tạo bài tô nét.
- Mở chi tiết bài học.
- Sửa metadata và câu hỏi.
- Chuyển trạng thái nháp, chờ duyệt, xuất bản, lưu trữ.

## Dữ liệu

- Kiểm tra bảng `__EFMigrationsHistory`.
- Kiểm tra có role `Admin`, `Parent`, `ContentEditor`, `Reviewer`.
- Kiểm tra có ít nhất một bài `published`.
- Kiểm tra `LearningAttempts` tăng sau khi bé học.
- Kiểm tra `QuestionAttempts` lưu dữ liệu nét vẽ sau bài tô nét.
