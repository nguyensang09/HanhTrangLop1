# Hướng dẫn migration database

## Mục tiêu

Hệ thống dùng EF Core Migration để tạo và cập nhật SQL Server database. Khi đóng gói source sang máy khác, chỉ cần chạy migration là tạo được schema ban đầu, sau đó ứng dụng sẽ tự nạp role, tài khoản mẫu và dữ liệu học tập mặc định nếu còn thiếu.

## Hướng quản lý database

Dự án hiện dùng **Code First có migration**, không phải Database First:

- Model C# và `ApplicationDbContext` mô tả cấu trúc dữ liệu.
- `Data/Migrations` lưu lịch sử thay đổi schema và phải được đưa vào source control.
- `SeedDataInitializer` tạo dữ liệu khởi đầu khi ứng dụng chạy.

Database First thường dùng `Scaffold-DbContext` để sinh model từ một database có sẵn. Dự án không dùng quy trình đó. Không nên vừa scaffold từ DB vừa tạo migration từ model vì sẽ có hai nguồn chuẩn cho schema.

Migration giúp dựng lại schema và seed giúp hệ thống có dữ liệu ban đầu; cả hai không thay thế backup dữ liệu người dùng.

## Thông tin hiện tại

- DbContext: `ApplicationDbContext`
- Connection string: `DefaultConnection`
- Database local mặc định: `HanhTrangLop1`
- Migration đầu tiên: `InitialCreate`

## Chạy bằng Package Manager Console

Mở Visual Studio > Tools > NuGet Package Manager > Package Manager Console.

Trước khi chạy migration, dừng ứng dụng bằng nút **Stop Debugging** trong Visual Studio hoặc đóng tiến trình `HanhTrangLop1`. `Update-Database` luôn build project trước; nếu ứng dụng vẫn chạy, file `bin\Debug\net9.0\HanhTrangLop1.exe` có thể bị khóa và gây lỗi `MSB3027`/`MSB3021`.

```powershell
Add-Migration TenMigrationMoi -Context ApplicationDbContext -OutputDir Data\Migrations
```

```powershell
Update-Database -Context ApplicationDbContext
```

Nếu database đã đúng phiên bản, kết quả hợp lệ là `No migrations were applied. The database is already up to date.`

Chạy đến một migration cụ thể:

```powershell
Update-Database -Migration InitialCreate -Context ApplicationDbContext
```

Xóa database local để tạo lại từ đầu:

```powershell
Drop-Database -Context ApplicationDbContext
```

## Chạy bằng terminal

```powershell
dotnet ef migrations add TenMigrationMoi -c ApplicationDbContext -o Data\Migrations
```

```powershell
dotnet ef database update -c ApplicationDbContext
```

Sinh script SQL idempotent để đóng gói hoặc bàn giao cho DBA:

```powershell
dotnet ef migrations script --idempotent -c ApplicationDbContext -o artifacts\database\HanhTrangLop1.sql
```

## Quy trình khi thêm/sửa bảng

1. Dừng ứng dụng đang chạy.
2. Sửa model hoặc mapping trong `ApplicationDbContext`.
3. Tạo migration mới.
4. Kiểm tra file migration trong `Data\Migrations`.
5. Chạy `Update-Database`.
6. Chạy ứng dụng để kiểm tra dữ liệu seed mặc định.

## Xử lý lỗi Build failed trong PMC

PMC thường chỉ hiện dòng tổng quát `Build started... Build failed.`. Chạy lệnh sau trong terminal ở thư mục project để xem lỗi đầy đủ:

```powershell
dotnet build HanhTrangLop1.csproj -c Debug
```

Nếu thấy `The process cannot access ... HanhTrangLop1.exe because it is being used by another process`, hãy dừng Visual Studio debugger, cửa sổ `dotnet run` hoặc tiến trình ứng dụng rồi chạy lại `Update-Database`.

Trong Visual Studio, cần chọn đúng:

- Default project trong PMC: `HanhTrangLop1`.
- Startup Project: project web `HanhTrangLop1`.
- Context: `ApplicationDbContext`.

## Lưu ý dữ liệu mặc định

Không seed mật khẩu trực tiếp bằng SQL. Tài khoản mẫu được tạo bằng `UserManager` để đảm bảo hash mật khẩu đúng chuẩn Identity.

Tài khoản mặc định hiện tại:

- Admin: `admin@hanhtranglop1.local` / `Admin@123456`
- Phụ huynh: `phuhuynh@hanhtranglop1.local` / `Phuhuynh@123456`

Có thể đổi trong `appsettings.json`.

## Backup dữ liệu đang sử dụng

Khi chỉ chuyển source sang máy mới, chạy migration và seed là đủ để có hệ thống khởi đầu.

Khi chuyển một hệ thống đã có phụ huynh và lịch sử học, cần backup database bằng SQL Server Management Studio hoặc công cụ vận hành SQL Server rồi restore ở máy đích. Không dùng migration hoặc seed để sao chép dữ liệu phát sinh.

Ở production, nên chạy migration như một bước triển khai có kiểm soát, backup trước khi cập nhật schema và không dùng mật khẩu tài khoản mẫu trong file cấu hình.
