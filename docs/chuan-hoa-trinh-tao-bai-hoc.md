# Chuẩn hóa trình tạo bài học theo loại hoạt động

> Cập nhật: 2026-08-22  
> Phạm vi: cấu trúc chương trình, trình tạo bài, thư viện media, bài tô theo nét và runtime khu bé học

## 1. Nguyên tắc thiết kế

- Nhóm kỹ năng và chủ đề là danh mục chương trình cố định, được seed idempotent từ source. Quản trị viên tạo bài học, không tự tạo thêm nhóm hoặc chủ đề.
- Mỗi chủ đề chỉ cho phép các mẫu hoạt động phù hợp về mặt sư phạm. Ma trận này được khai báo tập trung tại `Data/ActivityTemplateCatalog.cs` và được kiểm tra ở cả giao diện lẫn server.
- Mỗi mẫu hoạt động có cấu hình, preview và runtime riêng. Không dùng một biểu mẫu lựa chọn đáp án chung cho mọi loại bài.
- Bài tô theo nét dùng trình tạo riêng vì cần template nét, chế độ hướng dẫn, điểm bắt đầu và số nét dự kiến.
- Bài có hình ảnh hoặc âm thanh được phép tải file lên hoặc chọn tài nguyên đã có trong thư viện.
- Bài cũ có tổ hợp chủ đề/mẫu không hợp lệ không được xuất bản hoặc đưa vào lộ trình học cho đến khi quản trị viên chuẩn hóa lại.

## 2. Cấu hình theo từng mẫu hoạt động

| Mẫu hoạt động | Cấu hình riêng | Dữ liệu bắt buộc chính |
|---|---|---|
| Chọn một đáp án | Danh sách lựa chọn, một đáp án đúng | Tối thiểu hai lựa chọn và một đáp án đúng |
| Chọn nhiều đáp án | Danh sách lựa chọn, tập đáp án đúng | Tối thiểu hai lựa chọn và ít nhất một đáp án đúng |
| Nghe và chọn | Âm thanh, danh sách lựa chọn, một đáp án đúng | Audio, tối thiểu hai lựa chọn và đáp án đúng |
| Kéo vào đích | Vật kéo, vùng đích, đáp án ánh xạ | Danh sách vật kéo, tên vùng đích và đáp án đúng |
| Nối cặp | Danh sách cặp trái - phải | Tối thiểu hai cặp hoàn chỉnh |
| Sắp xếp | Danh sách phần tử và thứ tự đúng | Tối thiểu hai phần tử có thứ tự |
| Đếm đồ vật | Biểu tượng/ảnh đồ vật, số lượng hiển thị, đáp án số | Số lượng hợp lệ và đáp án tương ứng |
| Tạo số lượng | Đối tượng, số lượng mục tiêu | Mục tiêu số lượng hợp lệ |
| So sánh | Hai nhóm đối tượng, phép so sánh | Số lượng hai nhóm và đáp án lớn hơn/nhỏ hơn/bằng nhau |
| Phân loại | Danh sách vật, các nhóm đích, ánh xạ phân loại | Ít nhất hai nhóm và ánh xạ cho từng vật |
| Nghe truyện | Audio truyện, ảnh minh họa, câu hỏi và lựa chọn | Audio, ảnh, câu hỏi và đáp án đúng |

Trình tạo chỉ hiện phần cấu hình liên quan đến mẫu đang chọn. Preview điện thoại thay đổi theo cấu trúc thật của hoạt động, gồm vùng thả, cặp nối, thứ tự, nhóm so sánh, vùng phân loại hoặc nội dung media tương ứng.

## 3. Ma trận chủ đề và hoạt động

Ma trận đầy đủ nằm trong `ActivityTemplateCatalog.TopicRules`. Một số quy tắc quan trọng:

- `Tạo đúng số lượng` chỉ dùng mẫu **Tạo số lượng**.
- `So sánh` chỉ dùng mẫu **So sánh**.
- `Phân loại` chỉ dùng mẫu **Phân loại**.
- `Viết chữ số`, `Nét cơ bản`, `Nối điểm` chỉ dùng trình tạo **Tô theo nét**.
- `Chữ in hoa`, `Chữ in thường` có thể dùng các hoạt động nhận biết phù hợp và bài tô theo nét.
- Chủ đề nghe hiểu, âm thanh hoặc kể chuyện chỉ mở các mẫu có cấu hình audio phù hợp.

Trang **Cấu trúc chương trình** hiển thị trực tiếp các nút tạo hợp lệ trên từng chủ đề. Khi mở trình tạo từ đây, nhóm, chủ đề và mẫu hoạt động được truyền đúng theo nút đã chọn.

## 4. Trình tạo bài tô theo nét

Bài tô nét được cấu hình bằng các trường chuyên biệt:

- Ký hiệu cần tô: chữ hoa, chữ thường, chữ số hoặc ký hiệu nét cơ bản.
- Chế độ hướng dẫn `outline`: hiển thị đường viền nét gợi ý.
- Chế độ hướng dẫn `free`: không hiển thị ký tự mờ, dùng cho bước tự vẽ.
- Số nét dự kiến để lưu cùng dữ liệu chấm.
- Bật/tắt điểm bắt đầu.
- Âm thanh hướng dẫn tùy chọn từ URL, file tải lên hoặc thư viện.

Preview và runtime không còn vẽ sẵn các đường chéo cố định. Ký tự hướng dẫn dùng đường viền trong suốt; vì vậy không còn lớp chữ đặc mờ phía sau gây hiểu nhầm là nét bé đã tô.

Các nhóm có thể tạo bài tô nét hiện gồm **Chữ cái**, **Chữ số** và **Vận động tinh**. Danh sách chủ đề tiếp tục được lọc bằng ma trận, không hiển thị các chủ đề nội dung nghe, toán hoặc kỹ năng sống không liên quan.

## 5. Hình ảnh và âm thanh

- File ảnh hỗ trợ: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`; tối đa 5 MB.
- File âm thanh hỗ trợ: `.mp3`, `.wav`, `.ogg`, `.m4a`; tối đa 10 MB.
- File được lưu dưới `wwwroot/uploads/images` hoặc `wwwroot/uploads/audio` và có bản ghi trong `MediaAssets`.
- Trang `/admin/media` cho phép xem ảnh, nghe audio và kiểm tra tài nguyên có thể tái sử dụng.
- Khi tạo bài, quản trị viên có thể chọn tài nguyên đã có, tải file mới hoặc nhập URL. File mới sau khi lưu sẽ xuất hiện trong thư viện.
- Mẫu **Nghe và chọn** bắt buộc có audio. Mẫu **Nghe truyện** bắt buộc có cả audio và ảnh minh họa.

## 6. Kiểm tra và tương thích dữ liệu cũ

Server kiểm tra đồng thời quan hệ nhóm - chủ đề và quan hệ chủ đề - mẫu hoạt động khi lưu hoặc xuất bản. Kiểm tra phía trình duyệt chỉ hỗ trợ trải nghiệm và không thay thế validation phía server.

Với dữ liệu được tạo trước khi có ma trận:

1. Bài hợp lệ tiếp tục hoạt động bình thường.
2. Bài sai tổ hợp được đánh dấu **Cần chuẩn hóa** trong quản trị.
3. Nút xuất bản bị ẩn và server từ chối chuyển sang `published`.
4. Bài sai tổ hợp dù còn trạng thái cũ là `published` cũng bị loại khỏi trang bé học và lộ trình hôm nay.
5. Quản trị viên mở màn sửa; trình tạo sẽ chuyển sang mẫu hợp lệ của chủ đề để cấu hình lại rồi mới xuất bản.

## 7. Giới hạn còn lại

- Mỗi bài hiện quản lý một câu hỏi chính; ngân hàng nhiều câu hỏi trong cùng một bài chưa hoàn thiện.
- Editor đặt hotspot trực tiếp trên ảnh chưa có; vùng đích hiện được cấu hình bằng dữ liệu.
- Tracing chưa có vector/checkpoint cho từng nét và chưa chấm độ lệch đường vẽ ở server.
- Chưa có thu âm trực tiếp trong trình duyệt; quản trị viên tải file âm thanh đã thu hoặc chọn từ thư viện.

