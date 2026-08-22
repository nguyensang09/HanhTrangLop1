# Phát hành MVP Hành Trang Lớp 1

## Yêu cầu máy chạy

- .NET SDK 9
- SQL Server LocalDB hoặc SQL Server
- Visual Studio 2022 hoặc terminal có `dotnet ef`

## Cấu hình database

Mặc định dùng LocalDB:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=HanhTrangLop1;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False;TrustServerCertificate=True"
```

Khi chuyển sang SQL Server khác, đổi `DefaultConnection` trong `appsettings.json` hoặc biến môi trường.

## Tạo database mới

Dừng ứng dụng trước khi chạy migration để tránh file thực thi bị khóa trong bước build.

Package Manager Console:

```powershell
Update-Database -Context ApplicationDbContext
```

Terminal:

```powershell
dotnet ef database update -c ApplicationDbContext
```

Khi ứng dụng chạy lần đầu, hệ thống tự nạp role, tài khoản Admin và danh mục cố định 10 nhóm/43 chủ đề. Hệ thống không tạo phụ huynh, hồ sơ bé, bài học hoặc phần thưởng mẫu.

Dự án dùng Code First có migration. Các file `Data/Migrations` đi cùng source là gói mô tả schema; không cần chép file `.mdf` từ máy phát triển.

## Tài khoản mặc định

- Admin: `admin@hanhtranglop1.local` / `Admin@123456`
- Tài khoản phụ huynh không được tạo sẵn; người dùng tự đăng ký khi bắt đầu sử dụng.

Nên đổi mật khẩu khi triển khai thật.

## Chạy MVP local

```powershell
dotnet run --urls http://127.0.0.1:5152
```

Các đường dẫn kiểm tra nhanh:

- `/health`
- `/kids`
- `/kids/today`
- `/parent/login`
- `/parent/dashboard`
- `/admin`
- `/admin/learning-items`

## Checklist trước khi bàn giao

- `dotnet build` thành công.
- `dotnet ef database update -c ApplicationDbContext` chạy thành công trên DB mới.
- `/health` trả `status = ok`.
- Tự đăng ký phụ huynh, tạo hồ sơ bé và xem được báo cáo.
- Đăng nhập Admin nền tảng và xem được kho nội dung.
- Xác nhận trang cấu trúc chương trình có 10 nhóm/43 chủ đề và không cho sửa danh mục.
- Tạo, xuất bản rồi chạy thử ít nhất một bài học do quản trị nhập.
- Kiểm tra canvas tô nét trên thiết bị chuột hoặc cảm ứng.
- Backup và restore thử database nếu bàn giao cả dữ liệu đang sử dụng.
- Xác nhận không còn dùng mật khẩu seed mặc định ở production.

## Ghi chú vận hành

- `App:UseHttpsRedirection` đang để `false` cho local MVP để tránh cảnh báo khi chạy HTTP.
- Khi triển khai production có HTTPS, đặt `App:UseHttpsRedirection = true`.
- Không lưu mật khẩu hash bằng SQL seed. Tài khoản Admin nền tảng được tạo qua ASP.NET Identity để đúng chuẩn bảo mật.
- Migration/seed chỉ tạo schema và dữ liệu khởi đầu, không thay thế backup lịch sử học.
- Trạng thái hoàn thành chi tiết của MVP được theo dõi tại `docs/bao-cao-ra-soat-tien-do-mvp.md`.
