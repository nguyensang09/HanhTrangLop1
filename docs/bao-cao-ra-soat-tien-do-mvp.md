# Báo cáo rà soát tiến độ MVP

> Ngày rà soát: 2026-08-22  
> Phạm vi: đặc tả gốc trong thư mục Stitch, lộ trình triển khai, source ASP.NET Core MVC, migration, seed và các luồng chạy local

## 1. Kết luận

Source hiện tại là **bản MVP kỹ thuật đang hoạt động**, chưa hoàn thành toàn bộ đặc tả và chưa đạt đủ tiêu chí phát hành MVP ở Giai đoạn 7.

- Giai đoạn 0 đã hoàn thành theo tiêu chí chính.
- Giai đoạn 1 đến 6 đã có luồng nền nhưng vẫn còn hạng mục thiếu hoặc mới mô phỏng.
- Giai đoạn 7 mới hoàn thành phần build, publish, health check và tài liệu vận hành; chưa có test tự động, backup và kiểm thử chất lượng đầy đủ.
- Cơ sở dữ liệu hiện dùng EF Core Code First có migration và seed. Đây là quyết định phù hợp với source hiện tại và đáp ứng việc dựng database trên máy mới; không phải Database First.

Không nên ghi hệ thống “đã hoàn thành toàn bộ Giai đoạn 0-7” tại thời điểm này.

## 2. Kết quả kiểm tra kỹ thuật

| Hạng mục | Kết quả |
|---|---|
| Build Release | Thành công, 0 warning, 0 error |
| Migration SQL Server LocalDB | Database đã ở migration `InitialCreate`, không còn migration chờ chạy |
| Health check | `status = ok`, kết nối DB thành công, 5 nhóm kỹ năng, 7 bài published, 1 hồ sơ mẫu |
| Seed khởi đầu | Có role, admin, phụ huynh mẫu, hồ sơ bé, nhóm kỹ năng, chủ đề, bài học và phần thưởng mẫu |
| Đăng nhập phụ huynh | Thành công, mở được dashboard và báo cáo |
| Đăng nhập admin | Thành công, mở được dashboard và kho nội dung |
| Test tự động | Chưa có test project; lệnh `dotnet test` không phát hiện bài test để chạy |
| Script tạo DB | Sinh được SQL từ migration; có thể sinh lại khi phát hành |

Trong lúc rà soát đã xác nhận lỗi `Update-Database` có thể xảy ra nếu ứng dụng vẫn chạy và khóa file thực thi. Sau khi dừng server, project build và cập nhật database bình thường.

## 3. Đánh giá theo giai đoạn

### Giai đoạn 0: Hoàn thành

Đã có ASP.NET Core Identity, phân quyền, SQL Server provider, `ApplicationDbContext`, migration đầu tiên, connection string theo cấu hình, seed, layout, validation và logging mặc định.

Việc còn nên cải thiện nhưng không chặn tiêu chí giai đoạn: Việt hóa trang lỗi mặc định và tách mật khẩu seed khỏi `appsettings.json` khi triển khai thật.

### Giai đoạn 1: Hoàn thành một phần

Đã có đăng ký/đăng nhập phụ huynh, CRUD hồ sơ bé, chọn hồ sơ, trang chào mừng và trang chủ bé.

Còn thiếu:

- PIN hoặc câu hỏi dành cho người lớn khi bé mở khu phụ huynh.
- Danh sách chọn hồ sơ hiện đọc toàn bộ `ChildProfiles`; chưa giới hạn theo tài khoản hoặc mã gia đình.
- Khu học của bé nhận `childProfileId` trực tiếp và có thể chọn hồ sơ không thuộc tài khoản hiện tại.

Hai điểm cuối là rủi ro riêng tư cần xử lý trước khi có nhiều gia đình sử dụng chung hệ thống.

### Giai đoạn 2: Hoàn thành một phần

Đã có dữ liệu động từ SQL, màn hình làm bài chung, lưu `LearningAttempt`/`QuestionAttempt`, phản hồi, sao và admin tạo bài chọn đáp án.

Còn thiếu:

- Kéo-thả, nối cặp và sắp xếp hiện dùng thao tác chọn một phương án mô phỏng, chưa phải tương tác thật.
- Nút nghe-chọn và biểu tượng âm thanh chưa phát audio thật.
- Chưa quản lý nhiều câu hỏi đầy đủ trong một bài.
- Seed chưa đủ bộ chữ A, số 1-5, đếm 1-5 và phân biệt hình dạng như lộ trình ghi.
- Chưa lưu số lần thử/gợi ý đúng theo nhiều lần tương tác; mỗi lần gửi hiện tạo một attempt mới với `AttemptCount = 1`.

### Giai đoạn 3: Hoàn thành một phần

Đã có service tạo phiên học, `SessionPlanJson`, timeline, cập nhật tiến độ, tổng kết và thống kê phụ huynh.

Còn thiếu:

- Thuật toán chỉ ưu tiên kỹ năng và lấy danh sách theo thứ tự; chưa có quy tắc chắc chắn chống hai dạng tương tác giống nhau liên tiếp.
- Số hoạt động chưa được tính từ thời lượng 10-15 phút một cách chính xác.
- Có định nghĩa phần thưởng nhưng chưa cấp `ChildReward` hoặc vật phẩm khi hoàn thành.
- Một đáp án sai được xem là kết quả cuối ngay, chưa có luồng thử lại trong cùng câu hỏi.

### Giai đoạn 4: Hoàn thành một phần

Đã có canvas dùng pointer events, hỗ trợ hoàn tác/xóa, lưu tọa độ và metrics, bảng `TracingTemplates`, trang admin tạo bài tô nét cơ bản.

Còn thiếu:

- Canvas chưa đọc `GuideJson` để vẽ checkpoint, thứ tự và chiều nét theo template.
- `coverageScore` hiện được suy ra từ số điểm thu thập, chưa đo độ phủ hoặc độ lệch so với đường chuẩn.
- Server chưa tự kiểm tra metrics; luôn lưu bài tô là hoàn thành và tin dữ liệu client gửi lên.
- Chưa có các bước xem mẫu, nét đậm, nét mờ, tự vẽ và luyện lại riêng nét sai.
- Chưa có nút nghe lại/hoạt ảnh vẽ mẫu.
- Seed mới có chữ `A`, chưa có `a` và số `5` dạng tô nét.
- Admin mới nhập ký hiệu và số điểm tối thiểu, chưa nhập/chỉnh `TracingTemplate.GuideJson`.

### Giai đoạn 5: Hoàn thành một phần

Đã có dashboard, báo cáo chi tiết, lịch sử, biểu đồ 7/14 ngày, gợi ý tích cực, xuất CSV, chỉnh thời lượng và âm thanh. Controller báo cáo đã kiểm tra quyền sở hữu hồ sơ.

Còn thiếu:

- Chưa có giao diện chọn kỹ năng ưu tiên dù model đã có trường JSON.
- Chưa có PIN khu phụ huynh.
- Báo cáo chưa phân tích lỗi chi tiết theo chữ/số/câu hỏi; phần phân biệt nhận biết ký hiệu và hiểu số lượng mới dựa trên nhóm kỹ năng tổng quát.
- Gợi ý hoạt động ngoại tuyến còn là câu gợi ý chung, chưa dựa sâu vào lỗi cụ thể.

### Giai đoạn 6: Hoàn thành một phần

Đã có dashboard admin, danh sách/lọc/chi tiết/sửa bài, tạo bài chọn và tô nét, các trạng thái `draft/review/published/archived`, cùng bản ghi `ContentReview` khi gửi duyệt.

Còn thiếu:

- CRUD riêng cho nhóm kỹ năng, chủ đề, ngân hàng câu hỏi và tài nguyên hình/âm thanh.
- Preview bài trên các kích thước màn hình.
- Chưa ghi `AuditLogs` cho thao tác admin dù đã có bảng.
- Chưa tách quyền `ContentEditor` và `Reviewer`; admin hiện tự đổi mọi trạng thái.
- Dashboard chưa có tỷ lệ hoàn thành theo bài và danh sách lỗi phổ biến.
- Quy trình duyệt chưa có người duyệt, ghi chú kết quả và luật chuyển trạng thái đầy đủ.

### Giai đoạn 7: Chưa hoàn thành

Đã có build/publish Release, health endpoint, cấu hình production mẫu, checklist thủ công và hướng dẫn phát hành.

Còn thiếu trước khi nghiệm thu:

- Unit test cho `TodayLessonService`, tính tiến độ và chấm kết quả.
- Integration test cho đăng nhập, quyền sở hữu hồ sơ, luồng học, attempt và báo cáo.
- Kiểm thử responsive/accessibility có biên bản trên mobile, tablet và desktop.
- Audio fallback và tài nguyên audio/hình thực tế.
- Quy trình backup/restore SQL Server đã chạy thử.
- Cấu hình bí mật production; không dùng mật khẩu seed mặc định trong file cấu hình phát hành.
- Cơ chế tắt tự động chạy migration khi khởi động production và chuyển migration thành bước triển khai có kiểm soát.
- Kiểm tra bảo mật khu hồ sơ bé và khu phụ huynh.

## 4. Kiến trúc database được xác nhận

### Nguồn chuẩn

- Model và mapping: `Models/*`, `Data/ApplicationDbContext.cs`.
- Lịch sử schema: `Data/Migrations/*`.
- Dữ liệu khởi đầu: `Data/SeedDataInitializer.cs`.
- Database local mặc định: `(localdb)\\MSSQLLocalDB`, database `HanhTrangLop1`.

Đây là **Code First có migration**. Không có thao tác scaffold model từ database nên không được mô tả là DB First.

### Khi chuyển sang máy khác

1. Cài SQL Server/LocalDB và .NET SDK phù hợp.
2. Cấu hình `DefaultConnection`.
3. Chạy `Update-Database -Context ApplicationDbContext` hoặc `dotnet ef database update -c ApplicationDbContext`.
4. Chạy ứng dụng để seed dữ liệu khởi đầu.
5. Kiểm tra `/health`.

Có thể sinh script SQL bàn giao:

```powershell
dotnet ef migrations script --idempotent -c ApplicationDbContext -o artifacts\database\HanhTrangLop1.sql
```

Script/migration không chứa dữ liệu học phát sinh. Khi chuyển hệ thống đang có người dùng, phải backup và restore database thay vì chỉ chạy migration.

## 5. Thứ tự công việc tiếp theo

1. Khóa quyền truy cập hồ sơ bé theo gia đình/tài khoản và bổ sung cổng người lớn.
2. Hoàn thiện engine tương tác thật cùng audio và luồng thử lại.
3. Hoàn thiện tracing dựa trên template/checkpoint, chấm ở server và seed `A`, `a`, `5`.
4. Hoàn thiện phần thưởng, cài đặt phụ huynh và báo cáo lỗi chi tiết.
5. Hoàn thiện admin cho nhóm/chủ đề/câu hỏi/tài nguyên, audit và reviewer.
6. Thêm unit/integration test, kiểm thử responsive/accessibility và backup/restore.
7. Chỉ sau các bước trên mới đánh dấu Giai đoạn 7 hoàn thành và phát hành MVP cho nhiều gia đình.
