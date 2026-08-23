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

Ma trận truy vết từng yêu cầu, màn hình và tiêu chí nghiệm thu được lưu tại `docs/ma-tran-doi-chieu-dac-ta-mvp.md`.

## 2. Kết quả kiểm tra kỹ thuật

| Hạng mục | Kết quả |
|---|---|
| Build Release | Thành công, 0 warning, 0 error |
| Migration SQL Server LocalDB | Database đã ở migration `InitialCreate`, không còn migration chờ chạy |
| Health check | Kết nối database thành công |
| Seed khởi đầu | Tạo role, Admin, 10 nhóm/43 chủ đề, 216 bài học nền, 6 tranh chính và 75 pictogram; không tạo phụ huynh, hồ sơ bé, lịch sử học hoặc phần thưởng mẫu |
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

Đã có dữ liệu động từ SQL, danh sách bài đã xuất bản theo từng nhóm kỹ năng, màn hình làm bài chung, lưu `LearningAttempt`/`QuestionAttempt`, phản hồi và sao. Engine đã chạy riêng cho chọn một, chọn nhiều, nghe-chọn, kéo-thả, nối cặp, sắp xếp, đếm, tạo số lượng, so sánh, phân loại và nghe truyện. Ngữ cảnh nhóm được giữ khi mở bài, làm bài và chuyển sang bài tiếp theo.

Còn thiếu:

- Chưa quản lý nhiều câu hỏi đầy đủ trong một bài.
- Đã có upload, kiểm tra định dạng/kích thước và chọn lại hình ảnh/âm thanh từ thư viện; chưa có thu âm trực tiếp và quản lý xóa/sửa metadata tài nguyên.
- Chưa có editor kéo-thả trực quan để đặt vùng đích/hotspot lên hình.
- Chưa lưu số lần thử/gợi ý đúng theo nhiều lần tương tác; mỗi lần gửi hiện tạo một attempt mới với `AttemptCount = 1`.

### Giai đoạn 3: Hoàn thành một phần

Đã có service tạo phiên học, `SessionPlanJson`, timeline, cập nhật tiến độ, tổng kết và thống kê phụ huynh.

Còn thiếu:

- Thuật toán chỉ ưu tiên kỹ năng và lấy danh sách theo thứ tự; chưa có quy tắc chắc chắn chống hai dạng tương tác giống nhau liên tiếp.
- Số hoạt động chưa được tính từ thời lượng 10-15 phút một cách chính xác.
- Có định nghĩa phần thưởng nhưng chưa cấp `ChildReward` hoặc vật phẩm khi hoàn thành.
- Một đáp án sai được xem là kết quả cuối ngay, chưa có luồng thử lại trong cùng câu hỏi.

### Giai đoạn 4: Hoàn thành một phần

Đã có canvas dùng pointer events, hỗ trợ hoàn tác/xóa, lưu tọa độ và metrics, bảng `TracingTemplates`, trang admin tạo/sửa bài tô nét chuyên biệt, lọc chủ đề, chế độ đường viền/tự vẽ, điểm bắt đầu và audio hướng dẫn.

Còn thiếu:

- Canvas chưa đọc `GuideJson` để vẽ checkpoint, thứ tự và chiều nét theo template.
- `coverageScore` hiện được suy ra từ số điểm thu thập, chưa đo độ phủ hoặc độ lệch so với đường chuẩn.
- Server chưa tự kiểm tra metrics; luôn lưu bài tô là hoàn thành và tin dữ liệu client gửi lên.
- Đã phân biệt chế độ đường viền và tự vẽ; chưa có chuỗi bước xem mẫu, nét đậm, luyện riêng nét sai và hoạt ảnh vẽ mẫu.
- Đã có nút nghe lại khi bài được cấu hình audio; chưa có audio mặc định cho từng ký hiệu.
- Seed idempotent đủ 29 chữ hoa, 29 chữ thường và chữ số 0-9 cho bài tô nét; quản trị có thể sửa trạng thái hoặc tạo thêm nội dung.
- Admin mới nhập ký hiệu và số điểm tối thiểu, chưa nhập/chỉnh `TracingTemplate.GuideJson`.

### Giai đoạn 5: Hoàn thành một phần

Đã có dashboard, báo cáo chi tiết, lịch sử, biểu đồ 7/14 ngày, gợi ý tích cực, xuất CSV, chỉnh thời lượng và âm thanh. Controller báo cáo đã kiểm tra quyền sở hữu hồ sơ.

Còn thiếu:

- Chưa có giao diện chọn kỹ năng ưu tiên dù model đã có trường JSON.
- Chưa có PIN khu phụ huynh.
- Báo cáo chưa phân tích lỗi chi tiết theo chữ/số/câu hỏi; phần phân biệt nhận biết ký hiệu và hiểu số lượng mới dựa trên nhóm kỹ năng tổng quát.
- Gợi ý hoạt động ngoại tuyến còn là câu gợi ý chung, chưa dựa sâu vào lỗi cụ thể.

### Giai đoạn 6: Hoàn thành một phần

Đã có dashboard admin, màn cấu trúc chương trình chỉ đọc, danh sách bài lọc theo nhóm/trạng thái/dạng, trình tạo ba cột theo giao diện Stitch, xem trước điện thoại, cấu hình riêng cho 11 mẫu hoạt động, trình tạo tô nét riêng, upload/thư viện media, các trạng thái `draft/review/published/archived` và bản ghi `ContentReview`. Nhóm/chủ đề là danh mục hệ thống cố định; quản trị chỉ tạo bài. Ma trận chủ đề - mẫu được dùng để lọc giao diện, kiểm tra server, chặn xuất bản và loại bài cũ sai cấu trúc khỏi khu bé học.

Còn thiếu:

- Ngân hàng nhiều câu hỏi; thư viện media hiện chưa có sửa metadata, xóa tài nguyên và thu âm trực tiếp.
- Preview hiện mới mô phỏng điện thoại, chưa có chế độ tablet/desktop và chưa chạy thử bài ngay trong editor.
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
- Danh mục chương trình cố định: `Data/CurriculumCatalog.cs`.
- Seed idempotent: `Data/SeedDataInitializer.cs`.
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
2. Hoàn thiện upload/media, editor vùng tương tác và luồng thử lại nhiều lần.
3. Hoàn thiện tracing dựa trên template/checkpoint và chấm ở server.
4. Hoàn thiện phần thưởng, cài đặt phụ huynh và báo cáo lỗi chi tiết.
5. Hoàn thiện admin cho ngân hàng câu hỏi/tài nguyên, audit và reviewer; danh mục chương trình tiếp tục là dữ liệu hệ thống chỉ đọc.
6. Thêm unit/integration test, kiểm thử responsive/accessibility và backup/restore.
7. Chỉ sau các bước trên mới đánh dấu Giai đoạn 7 hoàn thành và phát hành MVP cho nhiều gia đình.

## 6. Cập nhật chuẩn hóa trình tạo bài ngày 2026-08-22

- Đã bổ sung ma trận chủ đề - mẫu hoạt động tập trung; giao diện và server cùng từ chối tổ hợp sai.
- Đã tách cấu hình và preview riêng cho đủ 11 mẫu hoạt động thay vì dùng chung cấu trúc chọn đáp án.
- Đã bổ sung upload ảnh/audio, chọn lại tài nguyên từ `MediaAssets` và trang thư viện `/admin/media`.
- Đã tách trình tạo tô nét, lọc còn các nhóm/chủ đề phù hợp, thêm chế độ đường viền hoặc tự vẽ, điểm bắt đầu và số nét dự kiến.
- Runtime tô nét không còn ký tự đặc mờ hoặc các nét cố định gây hiểu nhầm.
- Bài cũ sai ma trận được gắn nhãn **Cần chuẩn hóa**, bị chặn xuất bản và bị loại khỏi khu bé học/lộ trình hôm nay.

Chi tiết cấu hình và quy tắc tương thích được mô tả tại `docs/chuan-hoa-trinh-tao-bai-hoc.md`.

## 7. Cập nhật đồng bộ quản trị - runtime và dữ liệu nền

- Kho bài học hiển thị tên tiếng Việt của dạng bài/trạng thái, phân trang 25 bài, có sửa và xóa với xác nhận.
- Route chi tiết dẫn vào đúng editor chuyên biệt; editor chứa chung luồng duyệt, xuất bản, lưu trữ và xóa.
- Runtime lựa chọn dùng bước chọn rõ ràng rồi mới bấm **Kiểm tra**; trả lời đúng mới hiện điều hướng tiếp theo.
- Bài nối cặp vẽ đường nối có màu; lựa chọn, phân loại và vùng đích có trạng thái màu trực quan.
- Bộ màu chuyển sang nền xanh rất nhạt, bề mặt trắng và điểm nhấn cam/mint/xanh.
- Seed idempotent tạo 216 bài nền, vượt ngưỡng 190 của đặc tả: 80 bài tô nét, chuỗi chữ/số có thứ tự, đủ 11 dạng tương tác và đủ chỉ tiêu từng nhóm nội dung.

## 7. Cập nhật lộ trình nội dung và thứ tự học ngày 2026-08-22

- `LearningItems.SortOrder` lưu thứ tự học trong SQL; migration `AddLearningItemSortOrder` tạo cột và chỉ mục theo nhóm/chủ đề/thứ tự.
- Admin hiển thị và cho sửa thứ tự. Bài mới để `0` được tự xếp sau bài cuối của chủ đề; bài cũ chưa có thứ tự được chuẩn hóa một lần khi seed chạy.
- Khu bé học, nút bài tiếp theo và lộ trình hôm nay đều sắp theo thứ tự nhóm, chủ đề và bài; chữ cái dùng thứ tự tiếng Việt, số dùng thứ tự giá trị.
- Cấu hình so sánh tách rõ `more`, `less`, `equal`; chấm chọn nhiều, nối cặp và phân loại không phụ thuộc thứ tự chuỗi gửi từ trình duyệt.
- Sáu tranh bài học chính và 75 pictogram được đóng gói cục bộ; cả 216 bài có voice hướng dẫn, câu hỏi và phản hồi, bài nghe có thể dùng file audio hoặc giọng đọc `speechSynthesis` tiếng Việt.

## 8. Cập nhật ảnh quan sát và hành vi tương tác ngày 2026-08-22

- Bổ sung 10 ảnh chụp độ phân giải cao từ Wikimedia Commons cho táo, cam, cà rốt, bắp cải, mèo, chó, vịt, cá, tôm và gà; mọi tệp được đóng gói cục bộ và ghi nguồn trong `THIRD_PARTY_NOTICES.md`.
- Cấu hình `itemMedia` ánh xạ ảnh riêng theo nhãn đáp án/vật/cặp nối; trình tạo, trình sửa, preview và runtime dùng chung hợp đồng JSON.
- Bài nối dùng đường SVG, điểm neo và khoảng nối rõ; bài phân loại đặt thẻ vật thật vào nhóm; bài sắp xếp hỗ trợ kéo thả; bài đếm hiển thị thứ tự vật đã chạm.
- Đã kiểm tra desktop và màn 390 x 844: ảnh tải đủ, nhãn không co chữ, không cuộn ngang, vùng chạm giữ kích thước phù hợp.
- Không tạo migration cho thay đổi này vì dữ liệu mở rộng nằm trong `Questions.PayloadJson`; SQL chỉ được cập nhật qua seed idempotent.
