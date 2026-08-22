# Ma trận đối chiếu đặc tả và sản phẩm MVP

> Ngày đối chiếu: 2026-08-22  
> Nguồn yêu cầu: `docs/stitch_h_nh_trang_l_p_m_t/dac_ta_website_be_5_tuoi_chuan_bi_vao_lop_1_1.md`  
> Quy ước: **Đạt** = đủ hành vi chính; **Một phần** = có nền hoặc giao diện nhưng thiếu nghiệp vụ; **Chưa đạt** = chưa có chức năng sử dụng được

## 1. Kết luận nghiệm thu

**Sản phẩm chưa hoàn thành theo phạm vi MVP trong đặc tả gốc.**

Hệ thống hiện phù hợp để tiếp tục phát triển và trình diễn kiến trúc/luồng mẫu. Chưa phù hợp phát hành cho nhiều gia đình vì còn thiếu bộ nội dung, bảo vệ dữ liệu hồ sơ trẻ, chấm tracing phía server, phần thưởng, admin nâng cao và kiểm thử tự động.

Các phần đã vận hành gồm ASP.NET Core MVC, Identity, SQL Server, migration, danh mục chương trình cố định, hồ sơ phụ huynh, phiên học, lưu attempt, báo cáo cơ bản, trình tạo bài theo mẫu và các runtime tương tác chính.

## 2. Độ phủ nội dung MVP

Đặc tả đề xuất tối thiểu khoảng 190 bài. Source không seed bài học; số lượng thực tế phụ thuộc nội dung quản trị nhập. Tại lần rà soát này, LocalDB chỉ còn 2 bài do người dùng tự tạo sau khi đã xóa toàn bộ bài mẫu cũ. Danh mục hệ thống gồm 10 nhóm và 43 chủ đề, trong đó một số nhóm gần nhau của đặc tả được hợp nhất.

| Nhóm nội dung | Đặc tả | Hiện có | Trạng thái |
|---|---:|---:|---|
| Vẽ theo nét cơ bản | 12 | 0 | Chưa đạt |
| Vẽ chữ in hoa | 29 | Do quản trị nhập | Chưa đủ nội dung |
| Vẽ chữ in thường | 29 | 0 | Chưa đạt |
| Vẽ chữ số 0-9 | 10 | 0 | Chưa đạt |
| Chữ cái | 20 | Do quản trị nhập | Chưa đủ nội dung |
| Nhận biết chữ số | 15 | Do quản trị nhập | Chưa đủ nội dung |
| Số lượng và toán học | 20 | Do quản trị nhập | Chưa đủ nội dung |
| Bổ trợ vận động tinh | 10 | 0 | Chưa đạt |
| Hình dạng và không gian | 10 | 0 | Chưa đạt |
| Tư duy logic | 15 | 0 | Chưa đạt |
| Ghi nhớ | 10 | 0 | Chưa đạt |
| Kỹ năng sống | 10 | 0 | Chưa đạt |

Không tính nhóm/chủ đề là bài học hoàn chỉnh. Trang `/admin/catalogs` tính độ phủ động từ số chủ đề đã có ít nhất một bài; không ghi cứng tỷ lệ vào tài liệu.

## 3. Dạng tương tác tối thiểu

| Yêu cầu | Trạng thái | Bằng chứng/thiếu sót |
|---|---|---|
| Chọn một đáp án | Đạt | Có form lựa chọn, chấm đáp án và lưu attempt |
| Chọn nhiều đáp án | Đạt mức cơ bản | Chọn nhiều, chuẩn hóa tập đáp án và chấm/lưu attempt |
| Kéo-thả | Đạt mức cơ bản | Có kéo vào đích và thao tác chạm dự phòng |
| Nối cặp | Đạt mức cơ bản | Ghép từng cặp và chấm toàn bộ mapping |
| Sắp xếp | Đạt mức cơ bản | Đổi thứ tự bằng nút lên/xuống và chấm chuỗi kết quả |
| Nghe và chọn | Đạt mức cơ bản | Phát audio từ URL, file tải lên hoặc tài nguyên thư viện; chưa có thu âm trực tiếp/fallback |
| Đếm, tạo số lượng, so sánh, phân loại, nghe truyện | Đạt mức cơ bản | Có cấu hình riêng trong admin, runtime riêng và đáp án chuẩn hóa |
| Vẽ theo nét đa bước | Một phần | Có canvas và lưu điểm vẽ; chưa có xem mẫu/nét đậm/nét mờ/tự vẽ |

## 4. Chức năng tối thiểu

| Yêu cầu | Trạng thái | Bằng chứng/thiếu sót |
|---|---|---|
| Tài khoản phụ huynh và hồ sơ trẻ | Đạt | Đăng ký, đăng nhập, CRUD và chọn hồ sơ hoạt động |
| Bài học hôm nay | Một phần | Có session/timeline; thuật toán chưa tính đúng thời lượng và đa dạng tương tác |
| Danh mục chữ cái và chữ số riêng | Một phần | Mỗi nhóm kỹ năng đã có danh sách bài riêng; chưa có bản đồ học theo từng chữ và từng số |
| Lưu tiến độ và tiếp tục bài đang học | Một phần | Lưu attempt và timeline; chưa khôi phục câu đang làm/dữ liệu chưa submit khi reload |
| Giọng đọc hướng dẫn | Một phần | Bài nghe phát được URL, file tải lên hoặc audio thư viện; chưa có audio cho mọi hướng dẫn và chưa thu âm trực tiếp |
| Chấm kết quả và gợi ý | Một phần | Chọn đáp án có chấm; gợi ý chưa có luồng sử dụng nhiều mức |
| Sao, huy hiệu và phần thưởng | Một phần | Có model/giao diện nền; không seed định nghĩa và chưa có nghiệp vụ cấp thưởng đầy đủ |
| Báo cáo phụ huynh | Một phần | Có tổng quan/lịch sử; chưa có phân tích chi tiết theo chữ, số và lỗi thường nhầm |
| Trang quản trị và trình tạo bài | Một phần | Có danh mục cố định chỉ đọc, ma trận chủ đề - mẫu, cấu hình/preview riêng cho 11 mẫu, tô nét chuyên biệt, upload/thư viện media và xuất bản; còn thiếu ngân hàng nhiều câu hỏi và phần thưởng |
| Responsive điện thoại/tablet/máy tính | Một phần | CSS responsive và canvas co giãn; chưa có biên bản đủ ba cỡ và tiêu đề tracing mobile còn xuống dòng xấu |

## 5. Danh sách màn hình

### Khu vực trẻ

| Màn hình | Trạng thái |
|---|---|
| Chào mừng | Đạt |
| Chọn hồ sơ | Đạt về UI, chưa đạt quyền riêng tư |
| Trang chủ | Đạt mức cơ bản |
| Bài học hôm nay | Đạt mức cơ bản |
| Danh sách nhóm kỹ năng | Đạt mức cơ bản; từ trang chủ có thể mở nội dung của từng nhóm |
| Bản đồ học chữ cái | Chưa đạt |
| Bản đồ học chữ số | Chưa đạt |
| Danh sách bài tập từng nhóm | Đạt mức cơ bản; chỉ hiển thị bài đã xuất bản và có trạng thái học gần nhất |
| Màn hình làm bài | Đạt mức cơ bản |
| Màn hình gợi ý | Chưa đạt màn hình/luồng riêng |
| Màn hình tạm nghỉ | Chưa đạt |
| Tổng kết buổi học | Đạt mức cơ bản |
| Bộ sưu tập huy hiệu | Một phần, mới hiển thị định nghĩa chung |
| Khu vực nhân vật/khu vườn | Một phần, mới là giao diện tĩnh |

### Khu vực phụ huynh

| Màn hình | Trạng thái |
|---|---|
| Đăng nhập và đăng ký | Đạt |
| Quản lý hồ sơ trẻ | Đạt |
| Tổng quan tiến độ | Đạt mức cơ bản |
| Báo cáo chữ cái | Một phần trong báo cáo chung |
| Báo cáo chữ số và toán | Một phần trong báo cáo chung |
| Báo cáo kỹ năng khác | Một phần trong báo cáo chung |
| Lịch sử học | Một phần, nằm trong dashboard/report |
| Cài đặt lộ trình | Chưa đạt |
| Giới hạn thời gian | Một phần, chỉ có số phút và âm thanh |
| Phiếu bài tập | Chưa đạt |
| Cài đặt tài khoản | Chưa đạt |

### Khu vực quản trị

| Màn hình | Trạng thái |
|---|---|
| Bảng điều khiển | Đạt mức cơ bản |
| Quản lý người dùng | Chưa đạt |
| Cấu trúc nhóm và chủ đề | Đạt theo quyết định thiết kế; 10 nhóm/43 chủ đề cố định, chỉ đọc và có thống kê độ phủ |
| Quản lý bài học | Đạt mức cơ bản; lọc, tạo/sửa câu hỏi đầu tiên bằng mẫu chuyên biệt và quản lý trạng thái |
| Ngân hàng câu hỏi | Chưa đạt |
| Quản lý hình ảnh và âm thanh | Đạt mức cơ bản; có upload, chọn lại và thư viện xem/nghe tài nguyên |
| Kiểm duyệt nội dung | Một phần |
| Thống kê sử dụng | Một phần |
| Cấu hình phần thưởng | Chưa đạt |

## 6. Tiêu chí nghiệm thu cơ bản

### Trải nghiệm trẻ

| Tiêu chí | Trạng thái | Kết quả đối chiếu |
|---|---|---|
| Bắt đầu bài học trong tối đa hai thao tác | Đạt | Trang chủ dẫn trực tiếp tới bài hôm nay |
| Yêu cầu quan trọng có thể nghe | Một phần | Dạng nghe phát URL audio; các màn và hướng dẫn khác chưa có audio đầy đủ |
| Vùng bấm đủ lớn, đáp án không quá sát | Đạt mức cơ bản | Nút chính 52-56 px trong kiểm tra mobile |
| Trạng thái đúng/sai/kéo/đã chọn rõ | Đạt mức cơ bản | Có phản hồi, trạng thái chọn, kéo-thả, nối cặp và phân loại |
| Bài đang làm lưu khi reload/đổi thiết bị | Chưa đạt | Chỉ dữ liệu đã submit được lưu; không có resume câu đang làm |
| Tracing đúng mẫu, điểm đầu và chiều nét | Một phần | Có mẫu/điểm đầu dựng cứng; chưa đọc template và chưa thể hiện chiều nét |

### Chức năng số

| Tiêu chí | Trạng thái | Kết quả đối chiếu |
|---|---|---|
| Nội dung riêng 0-9 và làm quen 10-20 | Chưa đạt | Đã có chủ đề cố định nhưng chưa nhập đủ bài |
| Mỗi số có bài nhận biết, tô và ghép lượng | Chưa đạt | Không đủ 0-9, chưa có tracing chữ số |
| Phân biệt nhận biết chữ số và đếm lượng | Một phần | Có hai nhóm dữ liệu, chưa có taxonomy/chỉ số báo cáo đủ sâu |
| Báo cáo phạm vi đếm và số thường nhầm | Chưa đạt | Báo cáo mới tổng hợp theo nhóm |
| Cộng-bớt có biểu diễn trực quan | Một phần | Có engine số lượng/so sánh và chủ đề, chưa có mẫu phép tính chuyên biệt |

### Phụ huynh và quản trị

| Tiêu chí | Trạng thái | Kết quả đối chiếu |
|---|---|---|
| Phụ huynh xem tiến độ theo nhóm | Đạt mức cơ bản | Có `SkillProgress` và báo cáo theo nhóm |
| Khu phụ huynh tránh thao tác vô tình của trẻ | Chưa đạt | Chưa có PIN/câu hỏi người lớn |
| Admin tạo và xuất bản bài không sửa code | Đạt mức cơ bản | Builder hỗ trợ 11 mẫu, tự lọc/chọn chủ đề, preview, sửa cấu hình và đổi trạng thái |
| Bài nháp không xuất hiện cho trẻ | Đạt | Danh sách, phiên học, route xem bài và các POST làm bài đều yêu cầu trạng thái `published` |

## 7. Đối chiếu giao diện mẫu

### Phần phù hợp

- Bảng màu sáng ấm, cam/nâu, xanh mint và xanh nhạt gần tinh thần mẫu.
- Font tiếng Việt, nút lớn, thẻ bài và điều hướng đơn giản phù hợp trẻ nhỏ.
- Trang chủ mobile không tràn ngang; canvas tracing co từ 720 px xuống khoảng 323 px ở viewport 390 px.
- Trang desktop có bố cục rõ và vùng học tập lớn.

### Phần chưa đạt

- Chưa sử dụng hình minh họa Sóc Nâu/tài nguyên hình ảnh thật như giao diện mẫu; nhiều vùng chỉ là chữ hoặc hình CSS.
- Phụ thuộc Google Fonts và Material Symbols từ Internet, chưa có asset local/fallback phát hành.
- Tiêu đề `Tập vẽ chữ A` trên mobile xuống dòng thành ba dòng, chữ `A` đứng riêng.
- Chưa kiểm thử đầy đủ tablet, landscape, bàn phím và screen reader.
- Nhiều biểu tượng chỉ có `title` hoặc tên ligature tiếng Anh, chưa có accessible name tiếng Việt ổn định.

## 8. Dữ liệu và vận hành

| Hạng mục | Trạng thái |
|---|---|
| SQL Server + EF Core migration | Đạt |
| Seed role/Admin/danh mục cố định | Đạt; idempotent, không tạo bài học hay dữ liệu người dùng mẫu |
| Dựng DB trên máy mới | Đạt qua migration + seed |
| Backup/restore dữ liệu người dùng | Chưa kiểm thử |
| Media assets | Có upload ảnh/audio, chọn tài nguyên đã có và trang `/admin/media`; chưa có thu âm, sửa metadata hoặc xóa tài nguyên |
| Audit admin | Có bảng, chưa có bản ghi/nghiệp vụ |
| Content review | Có nghiệp vụ ghi khi gửi duyệt, quy trình còn đơn giản |
| Unit/integration test | Chưa có test project |

## 9. Lỗi chặn phát hành nhiều gia đình

1. `/profiles` đang trả toàn bộ hồ sơ trẻ trong database.
2. `/kids/home?childProfileId=...` và session chưa kiểm tra hồ sơ thuộc gia đình nào.
3. Server tin metrics tracing từ client và luôn ghi hoàn thành.
4. Không có cổng người lớn/PIN khi trẻ mở khu phụ huynh.
5. Tài khoản/mật khẩu seed mặc định còn nằm trong cấu hình dùng chung.
6. Chưa có test tự động cho quyền dữ liệu, attempt và migration.

## 10. Điều kiện để đánh dấu hoàn thành MVP

Chỉ đánh dấu sản phẩm hoàn thành khi:

1. Xử lý toàn bộ lỗi chặn phát hành tại Mục 9.
2. Hoàn thiện kho media/audio, editor vùng tương tác và luồng thử lại; các runtime tương tác chính đã có mức cơ bản.
3. Hoàn thiện tracing đa bước có template/checkpoint và chấm server.
4. Bổ sung đủ bộ nội dung MVP đã thống nhất; nếu giảm từ 190 bài phải cập nhật đặc tả và được chấp thuận.
5. Hoàn thiện phần thưởng, báo cáo chi tiết và admin còn thiếu.
6. Có unit test, integration test, kiểm thử responsive/accessibility và backup/restore.
7. Chạy lại toàn bộ ma trận này và không còn mục bắt buộc ở trạng thái `Chưa đạt` hoặc `Một phần`.
