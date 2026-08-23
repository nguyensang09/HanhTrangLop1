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
- Từ `/admin/catalogs`, xác nhận mỗi chủ đề chỉ hiện các nút mẫu hoạt động hợp lệ.
- Chọn lần lượt 11 mẫu và xác nhận chỉ các nhóm trường cấu hình liên quan được bật.
- Với bài nghe và chọn, tải audio hoặc chọn audio trong thư viện rồi kiểm tra phát lại.
- Với bài nghe truyện, tải/chọn đủ audio và ảnh, kiểm tra preview và runtime đều hiển thị đúng.
- Mở `/admin/media`, kiểm tra ảnh và audio vừa tải có thể tái sử dụng.
- Nhập `Táo = /images/photos/apple.jpg` trong **Ảnh riêng theo nội dung** và xác nhận preview hiện đúng ảnh cạnh nhãn Táo.
- Tạo bài tô nét ở chế độ đường viền và tự vẽ; chế độ tự vẽ không được hiện ký tự mờ.
- Thử xuất bản một bài có tổ hợp chủ đề - mẫu sai và xác nhận server từ chối.
- Xác nhận bài cũ gắn nhãn `Cần chuẩn hóa` không xuất hiện trong khu bé học.

## Dữ liệu

- Kiểm tra bảng `__EFMigrationsHistory`.
- Kiểm tra có role `Admin`, `Parent`, `ContentEditor`, `Reviewer`.
- Tạo và xuất bản ít nhất một bài trước khi kiểm thử khu bé học.
- Kiểm tra seed có đủ 216 bài học nền, 16 ảnh trong thư viện media và 75 pictogram cục bộ, nhưng không tự tạo phụ huynh, hồ sơ bé, lịch sử học hoặc phần thưởng mẫu.
- Kiểm tra màn học tự đọc hướng dẫn, câu hỏi, phản hồi; nút nghe lại hoạt động và dùng file giọng đọc câu hỏi khi bài có cấu hình.
- Kiểm tra bài một thao tác tự chấm, bài nhiều bước chỉ chấm khi bấm `Xong`.
- Kiểm tra màn 390 x 844 không cuộn ngang, không chồng nội dung và vùng chọn vẫn đủ lớn để chạm.
- Xác nhận màn học chỉ còn không gian chung thu gọn, tiêu đề, câu hỏi, nội dung quan sát khi cần và khung đáp án; không lặp tên dạng bài, nhãn vùng hoặc huy hiệu A/B/C.
- Kiểm tra đủ 11 dạng tương tác không có ảnh lỗi; đáp án có pictogram đúng ngữ nghĩa, nhãn chữ rõ và không dùng ảnh để vô tình gợi riêng đáp án đúng.
- Nối một cặp và xác nhận có một đường SVG cùng hai điểm neo; đổi cặp không để lại đường cũ.
- Phân loại một vật và xác nhận thẻ vật xuất hiện trong vùng nhóm, có ảnh và nhãn không bị co chữ.
- Sắp xếp bằng cả tay nắm kéo và nút lên/xuống; thứ tự gửi lên thay đổi theo danh sách đang thấy.
- Chạm lần lượt các vật trong bài đếm và xác nhận huy hiệu số đếm được đánh lại đúng khi bỏ chọn.
- Kiểm tra thứ tự chữ `A, Ă, Â, B...`, số `0-9` và nút chuyển bài tiếp theo đều theo `LearningItems.SortOrder`.
- Khởi động ứng dụng lần hai và xác nhận seed không tạo trùng mã bài hoặc tài nguyên media.
- Kiểm tra `LearningAttempts` tăng sau khi bé học.
- Kiểm tra `QuestionAttempts` lưu dữ liệu nét vẽ sau bài tô nét.
