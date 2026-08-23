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
- Trường **Ảnh riêng theo nội dung** nhận mỗi dòng theo cú pháp `Nhãn = đường dẫn`. Nhãn phải trùng chính xác với đáp án, vật phân loại, mục sắp xếp hoặc một đầu cặp nối. Dữ liệu được lưu trong `payload.itemMedia`, không cần thêm cột SQL.
- Runtime ưu tiên ảnh riêng, sau đó mới dùng pictogram vector. Preview quản trị đọc cùng ánh xạ nên không tạo ra hai cách trình bày khác nhau.
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

## 8. Bộ dữ liệu học tập nền

`Data/LearningContentSeed.cs` tạo 216 bài dữ liệu nền idempotent theo mã bài ổn định:

- 29 bài tô chữ in hoa và 29 bài tô chữ in thường theo bảng chữ cái tiếng Việt.
- 10 bài tô số và 10 bài nhận biết số từ 0 đến 9.
- Tối thiểu 3 bài cho từng dạng: chọn nhiều, nghe và chọn, kéo vào đích, nối cặp, sắp xếp, đếm, tạo số lượng, so sánh, phân loại và nghe truyện.
- Mười sáu ảnh được đăng ký trong `MediaAssets`: sáu tranh bài học chính và mười ảnh chụp rõ nét cho rau quả, con vật. Nguồn và giấy phép nằm trong `THIRD_PARTY_NOTICES.md`.
- 75 pictogram OpenMoji được đóng gói trong `wwwroot/images/pictograms`. Runtime tự ánh xạ pictogram theo nội dung đáp án cho con vật, đồ vật, rau quả, phương tiện, trang phục, thời tiết, kỹ năng sống và các nhóm phân loại.
- Bài nghe nền dùng `speechText` để trình duyệt đọc tiếng Việt khi chưa có file audio; bài quản trị vẫn có thể chọn hoặc tải audio thật.
- Mỗi bài có `SortOrder`; các chuỗi chữ và số dùng thứ tự sư phạm, còn bài bổ sung được xếp sau nội dung nền của chủ đề.
- Bài so sánh lưu `comparisonMode` riêng cho nhiều hơn, ít hơn hoặc bằng nhau. Các cấu hình mới có `schemaVersion` và `activityType` để nhận diện hợp đồng dữ liệu.

Seed chỉ bổ sung mã bài còn thiếu. Bài do quản trị tạo, lịch sử học và dữ liệu người dùng không bị sửa hoặc xóa.

## 9. Trải nghiệm nghe và thao tác của bé

- Màn làm bài dùng chế độ tập trung, ẩn điều hướng chung và giảm độ nổi của tên dạng bài để ưu tiên câu hỏi, hình minh họa và vùng thao tác.
- Cả 216 bài nền đều có voice cho hướng dẫn, câu hỏi, phản hồi đúng và phản hồi thử lại. Quản trị viên có thể cấu hình file/URL `questionAudioUrl`; khi chưa có file, trình duyệt dùng giọng đọc tiếng Việt của thiết bị.
- Phản hồi đúng hoặc cần thử lại được đọc tự động; chữ trên màn hình chỉ là tín hiệu ngắn đi cùng biểu tượng trực quan.
- Các dạng chọn một đáp án, nghe và chọn, truyện, kéo thả, đếm và so sánh tự chấm sau thao tác quyết định. Dạng nhiều bước vẫn dùng nút `Xong` để tránh chấm khi bé chưa hoàn tất.
- Ảnh chỉ được gắn vào bài có nội dung khớp chính xác. Ảnh không được tự áp đại trà theo chủ đề vì có thể làm sai số lượng hoặc ngữ nghĩa câu hỏi.
- Trình tạo bài tách âm thanh nội dung khỏi giọng đọc câu hỏi, đồng thời hỗ trợ thư viện media, tải file, URL và mô tả ảnh cho phụ huynh hoặc thiết bị hỗ trợ.
- Đáp án có chiều cao tối thiểu 84 px trên desktop và 78 px trên điện thoại. Pictogram, hình học, màu sắc và nhãn chữ dùng cùng một cấu trúc trình bày để giữ vùng chạm rõ ràng giữa các dạng bài.
- Mỗi lượt học hiển thị ba trạng thái thật: nghe câu hỏi, làm bài và hoàn thành. Dải trạng thái không dùng phần trăm hoặc số bài giả khi runtime chưa có dữ liệu tổng.
- Đáp án không có hình chỉ hiển thị nội dung chữ lớn, không thêm huy hiệu A/B/C. Đáp án có pictogram dùng khung ảnh sáng. Trạng thái rê, nhấn và đã chọn thay đổi đồng thời nền, viền và độ nổi của thẻ.
- Màu viền trên của vùng bài thay đổi theo nhóm tương tác: nghe, toán, thao tác, phân loại. Màu chỉ hỗ trợ nhận biết và không thay thế nội dung câu hỏi.
- Kết quả đúng dùng khung minh họa hoàn thành; kết quả sai chỉ hiện dải nhắc gọn phía trên câu hỏi để bé làm lại mà không phải cuộn trang.
- Pictogram OpenMoji dùng giấy phép CC BY-SA 4.0; thông tin ghi nhận nằm trong `THIRD_PARTY_NOTICES.md`.

## 10. Chuẩn hành vi trực quan theo dạng bài

- **Nối cặp:** chọn bên trái làm nổi các đích có thể nối; chọn bên phải tạo đường SVG màu, hai điểm neo và giữ đường đến khi bé đổi cặp.
- **Kéo vào đích:** hỗ trợ kéo chuột/chạm và cách chọn vật rồi chạm vùng đích; vùng đích đổi trạng thái khi sẵn sàng nhận.
- **Sắp xếp:** mỗi hàng có tay nắm kéo, hỗ trợ kéo thả và nút lên/xuống cho thiết bị không thuận tiện kéo.
- **Phân loại:** sau khi chọn vật, các vùng nhóm được nhấn sáng; vật đã đặt xuất hiện thành thẻ có ảnh và nhãn ngay trong nhóm.
- **Đếm:** mỗi vật được chạm sẽ có số thứ tự đếm, giúp bé không đếm lặp hoặc bỏ sót.
- **Chọn đáp án:** ảnh, hình dạng hoặc pictogram nằm trong thẻ lớn; trạng thái đã chọn dùng đồng thời viền, nền và độ nổi.
- **Tạo số lượng và so sánh:** số vật thay đổi trực tiếp trên vùng thao tác; đáp án chỉ sẵn sàng khi có thao tác hợp lệ.

Ảnh chụp được dùng cho nội dung cần quan sát vật thật. Chữ, số, hình học, ký hiệu thao tác và vật đếm lặp ưu tiên vector để nét không vỡ và không làm sai số lượng.

## 11. Phân vùng màn học

Màn học sử dụng cùng một thứ tự thị giác gọn cho mọi dạng bài:

1. **Không gian chung:** chỉ có nút quay lại, nghe lại và timeline ba bước thu gọn.
2. **Tiêu đề:** chỉ hiển thị tên bài, không lặp trạng thái hoặc tên dạng tương tác.
3. **Câu hỏi:** câu hỏi cùng nút nghe riêng, dùng nền và viền độc lập.
4. **Nội dung quan sát:** chỉ xuất hiện khi bài có tranh hoặc media chính, không thêm nhãn gây nhiễu.
5. **Khung đáp án:** chứa trực tiếp nội dung và thao tác trả lời; không lặp nhãn “Vùng trả lời” hoặc “Nội dung trả lời”.

Bài tô nét dùng cùng cấu trúc nhưng bảng canvas thay cho danh sách đáp án. Khung chung, tiêu đề, câu hỏi và đáp án dùng cùng chiều rộng. Trên màn 390 x 844, bài phân loại bốn vật hiển thị trọn khung, không cuộn ngang và nút hoàn thành nằm trong màn hình.
