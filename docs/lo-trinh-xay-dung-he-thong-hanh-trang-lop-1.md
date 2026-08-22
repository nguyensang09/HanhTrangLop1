# Lộ trình xây dựng hệ thống Hành Trang Lớp 1

> Ngày lập: 2026-08-22  
> Nguồn tham khảo: `docs/stitch_h_nh_trang_l_p_m_t`  
> Đối tượng chính: bé 5 tuổi chuẩn bị vào lớp 1, phụ huynh, quản trị viên nội dung  
> Đề xuất nền tảng: ASP.NET Core 9, SQL Server, Entity Framework Core, Razor/MVC kết hợp các component tương tác bằng JavaScript/TypeScript

## 1. Tóm tắt định hướng

Hệ thống nên được xây dựng như một nền tảng học tập vui chơi ngắn mỗi ngày, không phải một trang bài tập khô cứng. Trọng tâm của MVP là giúp bé làm quen chữ cái, chữ số, nét viết, toán trực quan và một số kỹ năng nền tảng trước lớp 1. Giao diện mẫu trong thư mục Stitch thể hiện rõ phong cách "Sóc Nâu Đồng Hành": nền sáng ấm, nút lớn, màu cam/nâu làm nhận diện chính, thẻ bo tròn, hình minh họa thân thiện, giọng đọc hướng dẫn và điều hướng rất ít tầng.

Vì source hiện tại là dự án ASP.NET Core MVC mới gần như trống, hướng đi hợp lý là phát triển theo monolith có cấu trúc module rõ ràng trước. Cách này giúp triển khai nhanh MVP, quản lý dữ liệu bằng SQL chặt chẽ, vẫn đủ khả năng tách API hoặc frontend riêng trong tương lai nếu sản phẩm lớn lên.

## 2. Phạm vi sản phẩm

### 2.1. Nhóm người dùng

1. Bé
   - Chọn hồ sơ học.
   - Vào bài học hôm nay.
   - Học theo nhóm kỹ năng: chữ cái, chữ số, toán, tư duy, ghi nhớ, khám phá, kỹ năng sống.
   - Nghe hướng dẫn, làm bài chọn đáp án, kéo thả, nối cặp, sắp xếp, tô/vẽ theo nét.
   - Nhận sao, huy hiệu, vật phẩm trang trí.

2. Phụ huynh
   - Tạo tài khoản và hồ sơ bé.
   - Thiết lập thời lượng học, kỹ năng ưu tiên, âm thanh, mã PIN.
   - Xem báo cáo tiến độ, nội dung bé hay nhầm, gợi ý học ngoài màn hình.

3. Quản trị viên
   - Quản lý nhóm kỹ năng, chủ đề, cấp độ.
   - Tạo bài học, câu hỏi, đáp án, tài nguyên hình/âm thanh.
   - Duyệt, xuất bản, ẩn nội dung.
   - Xem thống kê sử dụng, lỗi thường gặp, tỷ lệ hoàn thành.

### 2.2. MVP đề xuất

MVP nên tập trung vào những phần có giá trị học tập cao nhất và tạo nền dữ liệu đủ tốt:

- Đăng ký/đăng nhập phụ huynh.
- Hồ sơ bé và chọn hồ sơ.
- Trang chào mừng, trang chủ bé, bài học hôm nay.
- Danh sách nhóm kỹ năng và danh sách bài tập.
- Engine làm bài dạng chọn đáp án, nghe-chọn, kéo-thả cơ bản, nối cặp, sắp xếp.
- Module vẽ/tô theo nét mức đầu: xem mẫu, tô nét đậm, tô nét mờ, ghi nhận đường vẽ.
- Lưu phiên học, kết quả từng câu, tiến độ từng kỹ năng.
- Sao, huy hiệu, phần thưởng đơn giản.
- Góc phụ huynh: tổng quan tiến độ, báo cáo chữ cái/chữ số/toán, cài đặt thời gian.
- Admin: dashboard, quản lý bài học, ngân hàng câu hỏi, quản lý tài nguyên, xuất bản nội dung.

## 3. Định hướng giao diện

### 3.1. Tinh thần thiết kế

Giao diện mẫu cho thấy sản phẩm nên đi theo hướng vui, mềm, rõ thao tác:

- Màu nền sáng ấm `#fbf9f1`, điểm nhấn cam/nâu, xanh mint cho trạng thái hoàn thành, xanh dương nhạt cho thông tin.
- Typography thân thiện, ưu tiên Be Vietnam Pro cho tiếng Việt; heading có thể dùng Plus Jakarta Sans nếu muốn giữ phong cách mẫu.
- Nút cho bé phải lớn, có biểu tượng, vùng chạm rộng, phản hồi trạng thái rõ.
- Mỗi màn hình của bé chỉ có một nhiệm vụ chính.
- Điều hướng mobile đặt dưới cùng: Trang chủ, Bài tập, Quà tặng, Phụ huynh.
- Khu phụ huynh và admin dùng layout nhiều thông tin hơn, nhưng vẫn cùng ngôn ngữ màu và icon.

### 3.2. Các màn hình cần dựng trước

1. Chào mừng.
2. Chọn hồ sơ bé.
3. Trang chủ bé.
4. Bài học hôm nay dạng lộ trình.
5. Màn hình làm bài tương tác.
6. Màn hình vẽ/tô theo nét.
7. Tổng kết buổi học.
8. Khu vườn/phần thưởng.
9. Góc phụ huynh tổng quan.
10. Báo cáo kỹ năng.
11. Admin dashboard.
12. Admin quản lý bài học và trình tạo bài.

## 4. Kiến trúc kỹ thuật đề xuất

### 4.1. Backend

Giữ ASP.NET Core 9 hiện tại và tổ chức theo kiến trúc module trong monolith:

```text
HanhTrangLop1/
  Application/
    Auth/
    Learning/
    Progress/
    Rewards/
    Admin/
    Reporting/
  Domain/
    Users/
    LearningContent/
    Attempts/
    Rewards/
  Infrastructure/
    Persistence/
    Storage/
    Audio/
  Web/
    Controllers/
    ViewModels/
    Views/
    wwwroot/
```

Nếu chưa muốn tách project ngay, vẫn có thể dùng các folder trên trong cùng project MVC. Khi hệ thống lớn hơn, tách thành class library sẽ dễ hơn.

### 4.2. Frontend

Giai đoạn đầu nên dùng Razor Views/MVC kết hợp JavaScript/TypeScript cho các bài tương tác. Lý do:

- Phù hợp source ASP.NET Core MVC hiện tại.
- Tốc độ triển khai MVP nhanh.
- SEO không phải ưu tiên lớn vì khu học tập nằm sau tài khoản.
- Có thể tái sử dụng layout, partial view, view component.

Các tương tác phức tạp như kéo-thả, nối cặp, vẽ canvas nên đóng gói thành component JS độc lập trong `wwwroot/js/learning`.

Khi sản phẩm cần app-like nhiều hơn, có thể chuyển dần sang React/Vue cho khu học tập mà không phá vỡ backend.

### 4.3. Cơ sở dữ liệu

Đề xuất dùng SQL Server. Với .NET, SQL Server + EF Core là lựa chọn ổn định, dễ migration, dễ host nội bộ hoặc cloud. Dữ liệu có phần cấu trúc cố định và phần nội dung bài tập linh hoạt, nên dùng mô hình hybrid:

- Bảng quan hệ cho user, hồ sơ bé, bài học, câu hỏi, phiên học, tiến độ, phần thưởng.
- Cột JSON cho cấu hình tương tác, đường nét chuẩn, vùng đáp án, biến thể bài tập.
- File hình/âm thanh lưu ở storage, database chỉ lưu metadata và URL/path.

#### Chiến lược quản lý schema đã chốt

Hệ thống sử dụng **EF Core Code First có migration**, không sử dụng Database First. Model C# và các file trong `Data/Migrations` là nguồn chuẩn để tạo và nâng cấp schema SQL Server.

Lựa chọn này giải quyết đúng nhu cầu mang hệ thống sang máy khác:

- Source luôn đi kèm lịch sử migration nên không phụ thuộc file database trên máy phát triển.
- Máy mới chạy `Update-Database` hoặc `dotnet ef database update` để tạo đúng schema.
- Khi ứng dụng khởi động, seed idempotent tạo role, tài khoản khởi đầu và dữ liệu học tập mẫu nếu chưa có.
- Có thể sinh script SQL idempotent từ migration để bàn giao cho DBA hoặc triển khai không cần EF CLI.

Database First là quy trình ngược lại: schema SQL được thiết kế trước rồi dùng `Scaffold-DbContext` để sinh model. Không nên trộn Database First và Code First migration trong cùng một vòng đời schema vì dễ tạo hai nguồn chuẩn xung đột. Nếu sau này tổ chức yêu cầu DBA làm chủ schema, cần lập kế hoạch chuyển đổi riêng sang SQL Database Project/DACPAC và ngừng tạo migration từ model.

Migration và seed chỉ tái tạo **cấu trúc cùng dữ liệu khởi đầu**. Dữ liệu phát sinh của phụ huynh, hồ sơ bé và lịch sử học phải được bảo vệ bằng quy trình backup/restore SQL Server; migration không thay thế backup.

## 5. Mô hình dữ liệu SQL đề xuất

### 5.1. Nhóm tài khoản

#### AspNetUsers

Nếu dùng ASP.NET Core Identity, hệ thống dùng các bảng chuẩn `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`. Vai trò chính:

- `Parent`
- `Admin`
- `ContentEditor`
- `Reviewer`

#### ChildProfiles

Lưu hồ sơ bé.

| Cột | Kiểu | Ghi chú |
|---|---|---|
| Id | uniqueidentifier | PK |
| ParentUserId | nvarchar | FK tới AspNetUsers |
| Nickname | nvarchar(80) | Tên hiển thị của bé |
| BirthYear | int nullable | Không bắt buộc lưu ngày sinh đầy đủ |
| AvatarKey | nvarchar(100) | Nhân vật/avatar |
| DailyLearningMinutes | int | 10, 15, 20 |
| SoundEnabled | bit | Bật/tắt âm thanh |
| PreferredSkillGroupIds | nvarchar/json | Nhóm ưu tiên |
| CreatedAt | datetimeoffset |  |
| UpdatedAt | datetimeoffset |  |

### 5.2. Nhóm nội dung học tập

#### SkillGroups

| Cột | Kiểu | Ghi chú |
|---|---|---|
| Id | uniqueidentifier | PK |
| Code | varchar(50) | alphabet, numbers, math, logic |
| Name | nvarchar(120) | Tên nhóm |
| Description | nvarchar(500) | Mô tả |
| IconKey | nvarchar(80) | Icon |
| Color | varchar(20) | Màu đại diện |
| SortOrder | int | Thứ tự |
| IsActive | bit |  |

#### Topics

Chủ đề như gia đình, động vật, trường học, giao thông, thời tiết.

#### LearningItems

Đơn vị nội dung chính: bài học hoặc bài tập.

| Cột | Kiểu | Ghi chú |
|---|---|---|
| Id | uniqueidentifier | PK |
| Code | varchar(80) | Mã duy nhất |
| Title | nvarchar(200) | Tên bài |
| SkillGroupId | uniqueidentifier | FK |
| TopicId | uniqueidentifier nullable | FK |
| Level | tinyint | 1 dễ, 2 trung bình, 3 nâng cao |
| InteractionType | varchar(50) | single_choice, drag_drop, tracing |
| EstimatedMinutes | int | 3-7 phút |
| InstructionText | nvarchar(500) | Lời hướng dẫn |
| InstructionAudioAssetId | uniqueidentifier nullable | FK |
| ContentJson | nvarchar(max) | Cấu hình bài |
| Status | varchar(30) | draft, review, published, archived |
| Version | int | Phiên bản nội dung |
| PublishedAt | datetimeoffset nullable |  |
| CreatedByUserId | nvarchar |  |
| UpdatedByUserId | nvarchar |  |
| CreatedAt | datetimeoffset |  |
| UpdatedAt | datetimeoffset |  |

#### Questions

Mỗi `LearningItem` có thể có nhiều câu hoặc bước.

| Cột | Kiểu | Ghi chú |
|---|---|---|
| Id | uniqueidentifier | PK |
| LearningItemId | uniqueidentifier | FK |
| PromptText | nvarchar(500) | Câu hỏi/yêu cầu |
| PromptAudioAssetId | uniqueidentifier nullable |  |
| QuestionType | varchar(50) | choice, matching, ordering, tracing_step |
| PayloadJson | nvarchar(max) | Hình, vị trí, vùng tương tác |
| CorrectAnswerJson | nvarchar(max) | Đáp án |
| HintJson | nvarchar(max) | Gợi ý nhiều mức |
| FeedbackJson | nvarchar(max) | Phản hồi đúng/sai |
| SortOrder | int |  |

#### TracingTemplates

Dành riêng cho vẽ/tô chữ, số, nét.

| Cột | Kiểu | Ghi chú |
|---|---|---|
| Id | uniqueidentifier | PK |
| SymbolType | varchar(30) | basic_stroke, uppercase, lowercase, digit |
| Symbol | nvarchar(10) | A, a, 5 |
| DisplayName | nvarchar(100) | Chữ A, số năm |
| CanvasWidth | int | Kích thước chuẩn |
| CanvasHeight | int |  |
| GuideJson | nvarchar(max) | startPoint, path, tolerance, checkpoints |
| PreviewAssetId | uniqueidentifier nullable |  |
| CreatedAt | datetimeoffset |  |

### 5.3. Nhóm phiên học và kết quả

#### LearningSessions

| Cột | Kiểu | Ghi chú |
|---|---|---|
| Id | uniqueidentifier | PK |
| ChildProfileId | uniqueidentifier | FK |
| StartedAt | datetimeoffset |  |
| EndedAt | datetimeoffset nullable |  |
| PlannedMinutes | int |  |
| ActualSeconds | int |  |
| Status | varchar(30) | active, completed, paused, abandoned |
| SessionPlanJson | nvarchar(max) | Danh sách bài trong buổi học |

#### LearningAttempts

| Cột | Kiểu | Ghi chú |
|---|---|---|
| Id | uniqueidentifier | PK |
| SessionId | uniqueidentifier | FK |
| ChildProfileId | uniqueidentifier | FK |
| LearningItemId | uniqueidentifier | FK |
| StartedAt | datetimeoffset |  |
| CompletedAt | datetimeoffset nullable |  |
| Status | varchar(30) | started, completed, needs_practice |
| ScoreInternal | int | Chỉ dùng phân tích |
| StarsEarned | int | 0-3 |
| HintsUsed | int |  |
| MistakeCount | int |  |
| DurationSeconds | int |  |
| DeviceInputType | varchar(30) | touch, pen, mouse |

#### QuestionAttempts

| Cột | Kiểu | Ghi chú |
|---|---|---|
| Id | uniqueidentifier | PK |
| LearningAttemptId | uniqueidentifier | FK |
| QuestionId | uniqueidentifier | FK |
| AnswerJson | nvarchar(max) | Lựa chọn/kéo-thả/đường vẽ |
| IsCorrect | bit nullable | Với vẽ có thể không nhị phân |
| AttemptCount | int | Số lần thử |
| HintLevelUsed | int | 0-3 |
| MetricsJson | nvarchar(max) | Độ phủ, độ lệch, checkpoint |
| CreatedAt | datetimeoffset |  |

#### SkillProgress

Tổng hợp tiến độ theo bé và kỹ năng.

| Cột | Kiểu | Ghi chú |
|---|---|---|
| Id | uniqueidentifier | PK |
| ChildProfileId | uniqueidentifier | FK |
| SkillGroupId | uniqueidentifier | FK |
| MasteryLevel | decimal(5,2) | 0-100 |
| CompletedItems | int |  |
| NeedsPracticeItems | int |  |
| LastPracticedAt | datetimeoffset nullable |  |
| SummaryJson | nvarchar(max) | Nội dung hay nhầm |

### 5.4. Nhóm phần thưởng

#### RewardDefinitions

Định nghĩa sao, huy hiệu, vật phẩm.

#### ChildRewards

Phần thưởng bé đã nhận.

#### GardenItems

Vật phẩm trang trí trong khu vườn/nhân vật.

### 5.5. Nhóm tài nguyên

#### MediaAssets

| Cột | Kiểu | Ghi chú |
|---|---|---|
| Id | uniqueidentifier | PK |
| AssetType | varchar(30) | image, audio, animation |
| FileName | nvarchar(255) |  |
| ContentType | varchar(100) |  |
| StoragePath | nvarchar(500) | Đường dẫn file/blob |
| AltText | nvarchar(500) | Cho hình |
| DurationMs | int nullable | Cho audio |
| UploadedByUserId | nvarchar |  |
| CreatedAt | datetimeoffset |  |

### 5.6. Nhóm kiểm duyệt và audit

#### ContentReviews

Lưu quy trình soạn, duyệt, xuất bản.

#### AuditLogs

Lưu hành động quan trọng trong admin: tạo bài, sửa, duyệt, xuất bản, ẩn.

## 6. Luồng nghiệp vụ chính

### 6.1. Bé học bài hôm nay

1. Bé chọn hồ sơ.
2. Hệ thống kiểm tra thời lượng ngày, tiến độ gần nhất và kỹ năng ưu tiên.
3. Tạo `LearningSession` với 4-6 hoạt động trong 10-15 phút.
4. Bé làm từng `LearningItem`.
5. Mỗi câu/bước ghi `QuestionAttempts`.
6. Kết thúc bài ghi `LearningAttempts`, sao và phần thưởng.
7. Kết thúc buổi cập nhật `SkillProgress`, `ChildRewards`, gợi ý phụ huynh.

### 6.2. Bài vẽ/tô theo nét

1. Load `TracingTemplates`.
2. Hiển thị chữ/số mẫu, điểm bắt đầu, hướng nét, checkpoint.
3. Bé vẽ trên canvas.
4. Client tính sơ bộ: đi qua checkpoint, sai hướng, độ phủ.
5. Server lưu đường vẽ và metrics rút gọn.
6. Hệ thống phản hồi nhẹ nhàng: hoàn thành tốt, đã hoàn thành, cùng thử lại một phần.

Không hiển thị điểm thấp hoặc phần trăm thất bại cho bé. Số liệu chi tiết chỉ dùng cho báo cáo phụ huynh và cá nhân hóa.

### 6.3. Phụ huynh xem báo cáo

1. Chọn hồ sơ bé.
2. Xem tổng quan thời gian, số bài, huy hiệu, xu hướng.
3. Xem tiến độ theo nhóm kỹ năng.
4. Xem lỗi hay gặp: chữ/số hay nhầm, dạng toán còn yếu, mức gợi ý thường dùng.
5. Nhận gợi ý hoạt động ngoài màn hình.

### 6.4. Admin tạo nội dung

1. Chọn nhóm kỹ năng và mẫu tương tác.
2. Nhập hướng dẫn, câu hỏi, đáp án, gợi ý, phản hồi.
3. Gắn hình/âm thanh.
4. Preview trên mobile/tablet/desktop.
5. Lưu nháp, gửi duyệt.
6. Reviewer duyệt và xuất bản.
7. Nội dung published mới xuất hiện cho bé.

## 7. Lộ trình triển khai

### Giai đoạn 0: Nền móng dự án

Thời lượng đề xuất: 3-5 ngày.

- Chuẩn hóa cấu trúc folder.
- Thêm EF Core, SQL Server provider, ASP.NET Core Identity.
- Tạo DbContext, migration đầu tiên.
- Cấu hình connection string theo môi trường.
- Thiết lập seed dữ liệu: role, admin mặc định, nhóm kỹ năng, chủ đề mẫu.
- Thiết lập layout chính, design token màu/font/icon.
- Tạo trang lỗi, logging, validation cơ bản.

Tiêu chí hoàn thành:

- Chạy được migration tạo database.
- Đăng nhập/đăng xuất hoạt động.
- Có layout chung và navigation cơ bản.

### Giai đoạn 1: Hồ sơ bé và trải nghiệm vào học

Thời lượng đề xuất: 1 tuần.

- Tạo tài khoản phụ huynh.
- CRUD hồ sơ bé.
- Trang chào mừng.
- Trang chọn hồ sơ.
- Trang chủ bé với trạng thái sao, bài hôm nay, nhóm kỹ năng.
- Cơ chế khóa khu phụ huynh bằng PIN hoặc xác nhận người lớn.

Tiêu chí hoàn thành:

- Phụ huynh tạo được hồ sơ bé.
- Bé vào được trang chủ với tối đa hai thao tác.
- Giao diện mobile-first giống tinh thần mẫu.

### Giai đoạn 2: Nội dung học tập và engine bài cơ bản

Thời lượng đề xuất: 2 tuần.

- Bảng `SkillGroups`, `Topics`, `LearningItems`, `Questions`.
- Admin nhập bài dạng chọn một đáp án, nghe-chọn, kéo-thả cơ bản.
- Màn hình làm bài chung.
- Lưu attempt, số lần thử, gợi ý, sao.
- Seed bộ nội dung mẫu: chữ A, số 1-5, đếm 1-5, phân biệt hình dạng.

Tiêu chí hoàn thành:

- Admin tạo và xuất bản được bài không cần sửa code.
- Bé làm được ít nhất 3 dạng bài.
- Kết quả được lưu và reload không mất trạng thái quan trọng.

### Giai đoạn 3: Bài học hôm nay và tiến độ

Thời lượng đề xuất: 1-2 tuần.

- Thuật toán tạo `SessionPlanJson` cho bài học 10-15 phút.
- Luồng bài học dạng timeline như mẫu.
- Cập nhật `SkillProgress`.
- Màn hình tổng kết buổi học.
- Sao, huy hiệu đơn giản.

Tiêu chí hoàn thành:

- Mỗi ngày hệ thống đề xuất một buổi học ngắn.
- Có xen kẽ dạng bài, không lặp một kiểu tương tác liên tiếp.
- Phụ huynh thấy tổng thời gian, số bài, kỹ năng nổi bật.

### Giai đoạn 4: Module vẽ/tô theo nét

Thời lượng đề xuất: 2-3 tuần.

- Thiết kế `TracingTemplates`.
- Canvas vẽ bằng pointer events, hỗ trợ chuột/chạm/bút.
- Hiển thị nét mẫu, điểm bắt đầu, checkpoint, nút nghe lại, xóa, hoàn tác, làm lại.
- Ghi nhận đường vẽ và metrics.
- Admin nhập template nét cơ bản bằng JSON trước; editor trực quan để sau MVP.
- Seed chữ A, a và số 5 theo mẫu.

Tiêu chí hoàn thành:

- Bé tô được một chữ/số theo nhiều bước.
- Hệ thống nhận biết hoàn thành dựa trên checkpoint và độ phủ.
- Không đánh giá quá khắt khe, phản hồi tích cực.

### Giai đoạn 5: Góc phụ huynh và báo cáo

Thời lượng đề xuất: 1-2 tuần.

- Dashboard phụ huynh.
- Báo cáo theo chữ cái, chữ số/toán, tư duy.
- Lịch sử học.
- Gợi ý học ngoài màn hình.
- Cài đặt thời lượng học, âm thanh, kỹ năng ưu tiên.

Tiêu chí hoàn thành:

- Báo cáo phân biệt nhận biết chữ số và hiểu số lượng.
- Hiển thị nội dung cần luyện thêm bằng ngôn ngữ tích cực.
- Không lộ dữ liệu của hồ sơ khác.

### Giai đoạn 6: Admin hoàn chỉnh cho MVP

Thời lượng đề xuất: 2 tuần.

- Admin dashboard.
- Quản lý nhóm kỹ năng, chủ đề.
- Quản lý bài học, câu hỏi, tài nguyên.
- Trạng thái draft/review/published/archived.
- Audit log.
- Thống kê tỷ lệ hoàn thành và lỗi phổ biến.

Tiêu chí hoàn thành:

- Nội dung draft không xuất hiện cho bé.
- Có quy trình duyệt tối thiểu.
- Admin xem được bài nào có tỷ lệ lỗi cao.

### Giai đoạn 7: Hoàn thiện chất lượng và phát hành MVP

Thời lượng đề xuất: 1-2 tuần.

- Kiểm thử responsive mobile/tablet/desktop.
- Kiểm thử accessibility cơ bản: vùng chạm, màu, text, audio fallback.
- Tối ưu asset hình/âm thanh.
- Bổ sung test đơn vị cho service quan trọng.
- Bổ sung integration test cho luồng học, lưu attempt, báo cáo.
- Chuẩn bị seed nội dung MVP.
- Cấu hình production: HTTPS, logging, backup database.

Tiêu chí hoàn thành:

- Bé hoàn thành một buổi học từ đầu đến cuối.
- Phụ huynh xem được báo cáo sau buổi học.
- Admin tạo/xuất bản được bài.
- Database có backup và migration rõ ràng.

## 8. Ưu tiên backlog

### Must have

- Identity và phân quyền.
- Hồ sơ bé.
- Nội dung học tập động từ database.
- Engine bài chọn đáp án/nghe-chọn/kéo-thả.
- Lưu phiên học, kết quả, tiến độ.
- Bài học hôm nay.
- Báo cáo phụ huynh.
- Admin quản lý bài học.
- SQL migration và seed dữ liệu.

### Should have

- Canvas vẽ/tô theo nét.
- Huy hiệu và khu vườn phần thưởng.
- Kiểm duyệt nội dung.
- Gợi ý học ngoài màn hình.
- Thống kê lỗi thường gặp cho admin.

### Could have

- Editor trực quan cho đường nét.
- Xuất phiếu bài tập PDF.
- Offline mode.
- Nhiều giọng đọc.
- Câu chuyện tương tác theo tuần.

### Won't have trong MVP

- Bảng xếp hạng công khai.
- Quảng cáo trong khu vực bé.
- Nhận diện giọng nói của trẻ.
- AI tự động chấm cảm xúc/kỹ năng xã hội.
- Microservice tách rời.

## 9. Rủi ro và cách xử lý

| Rủi ro | Cách xử lý |
|---|---|
| Scope quá rộng | MVP chỉ lấy nhóm chữ, số, toán trực quan, vài bài tư duy/kỹ năng sống |
| Vẽ theo nét khó chấm chính xác | Chấm bằng checkpoint và độ phủ, không yêu cầu trùng tuyệt đối |
| Nội dung khó quản trị nếu hard-code | Từ đầu lưu bài trong SQL với `ContentJson` |
| Giao diện quá nhiều chữ với bé | Mỗi màn hình một nhiệm vụ, hướng dẫn bằng audio |
| Thu thập dữ liệu trẻ em quá mức | Chỉ lưu nickname/năm sinh, tránh dữ liệu cá nhân không cần thiết |
| Asset nặng | Lưu file ngoài DB, tối ưu ảnh/âm thanh, cache tĩnh |
| Báo cáo phụ huynh gây áp lực | Dùng ngôn ngữ tích cực, không so sánh bé với bé khác |

## 10. Quy ước kỹ thuật nên áp dụng

- Dùng `Guid` cho khóa chính nghiệp vụ.
- Dùng `datetimeoffset` cho thời gian.
- Dùng soft archive cho nội dung học, không xóa bài đã có attempt.
- Lưu nội dung linh hoạt bằng JSON nhưng vẫn có bảng quan hệ cho các trục báo cáo.
- Không lưu file binary trực tiếp trong SQL, chỉ lưu metadata.
- Tách rõ trạng thái nội dung: draft, review, published, archived.
- Với dữ liệu kết quả học, không sửa đè; ghi attempt mới để giữ lịch sử.
- Mọi hành động admin quan trọng cần audit.

## 11. Gợi ý package cần thêm

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.*" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.*" />
<PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="9.*" />
```

Nếu dùng TypeScript/build frontend:

```text
Vite hoặc esbuild cho wwwroot/js
interact.js hoặc dnd-kit nếu chuyển sang React cho kéo-thả
```

Với MVP MVC thuần, có thể chưa cần React.

## 12. Cấu trúc route đề xuất

```text
/                         Trang chào mừng
/profiles                 Chọn hồ sơ bé
/kids/home                Trang chủ bé
/kids/today               Bài học hôm nay
/kids/learn/{itemId}      Làm bài
/kids/rewards             Quà tặng/khu vườn

/parent/dashboard         Tổng quan phụ huynh
/parent/profiles          Hồ sơ bé
/parent/reports/skills    Báo cáo kỹ năng
/parent/settings          Thiết lập

/admin                    Dashboard admin
/admin/learning-items     Quản lý bài học
/admin/learning-items/new Tạo bài
/admin/skills             Nhóm kỹ năng
/admin/media              Tài nguyên
/admin/reviews            Kiểm duyệt
```

## 13. Tiêu chí nghiệm thu MVP

Trạng thái triển khai thực tế không được suy ra chỉ từ việc có route hoặc màn hình. Khi nghiệm thu, dùng ma trận `docs/ma-tran-doi-chieu-dac-ta-mvp.md` để kiểm tra đủ hành vi, dữ liệu, quyền truy cập và kiểm thử tương ứng.

### Trải nghiệm bé

- Bé bắt đầu bài học hôm nay với tối đa hai thao tác từ trang chủ.
- Các nút chính đủ lớn cho mobile.
- Yêu cầu quan trọng có nút nghe lại.
- Sai lần đầu được khuyến khích thử lại, không có thông báo tiêu cực.
- Kết quả học không mất khi reload sau khi đã đồng bộ.

### Trải nghiệm phụ huynh

- Tạo và quản lý được hồ sơ bé.
- Xem được tổng thời gian, bài đã hoàn thành, kỹ năng cần luyện.
- Báo cáo chữ số tách rõ nhận biết ký hiệu và hiểu số lượng.
- Có thiết lập thời lượng học/ngày.

### Trải nghiệm admin

- Tạo, sửa, preview, xuất bản bài học.
- Bài nháp không hiện ở khu học của bé.
- Xem được thống kê cơ bản và lỗi thường gặp.

### Kỹ thuật

- Database tạo bằng migration.
- Có seed dữ liệu chạy được ở môi trường dev.
- Có phân quyền Parent/Admin.
- Có test tối thiểu cho service tạo bài học hôm nay, lưu attempt và tính tiến độ.

## 14. Kết luận đề xuất

Nên bắt đầu bằng ASP.NET Core MVC monolith + SQL Server + EF Core, xây chắc nền dữ liệu học tập, sau đó dựng trải nghiệm bé theo phong cách giao diện mẫu. Cần ưu tiên mô hình nội dung động trong SQL ngay từ đầu để admin có thể tạo bài mà không sửa code. Module vẽ/tô theo nét là điểm khác biệt lớn của sản phẩm, nhưng nên triển khai sau khi engine bài cơ bản và lưu tiến độ đã ổn.

Thứ tự hợp lý nhất:

1. Nền tảng dữ liệu, tài khoản, hồ sơ bé.
2. Giao diện bé và bài học hôm nay.
3. Engine bài cơ bản.
4. Lưu tiến độ và báo cáo phụ huynh.
5. Vẽ/tô theo nét.
6. Admin nội dung hoàn chỉnh.
7. Hoàn thiện chất lượng và phát hành MVP.
