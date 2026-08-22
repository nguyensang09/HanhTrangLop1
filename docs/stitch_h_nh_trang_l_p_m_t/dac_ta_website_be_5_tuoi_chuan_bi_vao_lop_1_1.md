# Đặc tả website học tập cho trẻ 5 tuổi chuẩn bị vào lớp 1

> Phiên bản: 1.1  
> Đối tượng sử dụng chính: Trẻ khoảng 5 tuổi, phụ huynh và quản trị viên nội dung  
> Mục đích: Làm tài liệu nền để thiết kế giao diện, xây dựng chức năng, biên soạn nội dung và phát triển hệ thống

---

## 1. Mục tiêu của website

Website hỗ trợ trẻ chuẩn bị các năng lực nền tảng trước khi vào lớp 1:

- Nhận biết chữ cái tiếng Việt và làm quen âm của chữ.
- Nhận biết chữ số, số lượng, thứ tự và các quan hệ toán học cơ bản.
- Ưu tiên luyện vẽ theo nét để trẻ ghi nhớ hình dạng, chiều viết của chữ in hoa, chữ in thường và chữ số.
- Làm quen với hoạt động tập đọc và tập viết nhưng không ép trẻ đọc, viết thành thạo trước lớp 1.
- Rèn tư duy logic, quan sát, ghi nhớ và khả năng tập trung.
- Mở rộng hiểu biết về thế giới xung quanh.
- Hình thành kỹ năng tự phục vụ, an toàn và giao tiếp ở trường.
- Tạo thói quen học tập 10–15 phút mỗi ngày theo hướng vui chơi, không gây áp lực.

### 1.1. Nguyên tắc thiết kế

- Mỗi màn hình chỉ có một nhiệm vụ chính.
- Có giọng đọc tiếng Việt cho toàn bộ yêu cầu quan trọng.
- Nút bấm lớn, phù hợp với điện thoại và máy tính bảng.
- Nội dung ngắn, hình ảnh rõ và gần gũi với trẻ em Việt Nam.
- Không sử dụng quảng cáo trong khu vực của trẻ.
- Không dùng bảng xếp hạng công khai giữa các trẻ.
- Không tạo cảm giác thất bại khi trẻ trả lời sai.
- Sau 10–15 phút nên nhắc trẻ nghỉ mắt và vận động.

---

## 2. Nhóm người dùng

### 2.1. Trẻ em

Trẻ có thể:

- Chọn hồ sơ và nhân vật đại diện.
- Vào mục **Bài học hôm nay**.
- Chọn một nhóm kỹ năng để luyện tập.
- Nghe lại hướng dẫn.
- Thực hiện bài chọn đáp án, kéo–thả, nối, sắp xếp, tô nét hoặc nghe–chọn.
- Nhận sao, huy hiệu và vật phẩm trang trí.
- Xem các bài và phần thưởng đã hoàn thành.

### 2.2. Phụ huynh

Phụ huynh có thể:

- Tạo và quản lý hồ sơ trẻ.
- Chọn lộ trình, thời lượng và nhóm kỹ năng ưu tiên.
- Xem tiến độ theo chữ cái, chữ số và từng năng lực.
- Xem những nội dung trẻ thường nhầm.
- Nhận gợi ý luyện tập ngoài màn hình.
- Thiết lập giới hạn thời gian và mã PIN khu vực người lớn.
- Tạo hoặc tải phiếu bài tập khi chức năng này được triển khai.

### 2.3. Quản trị viên nội dung

Quản trị viên có thể:

- Quản lý nhóm kỹ năng, chủ đề và cấp độ.
- Tạo bài học và ngân hàng câu hỏi.
- Tải lên hình ảnh, âm thanh và hoạt ảnh.
- Xem trước bài tập trên nhiều kích thước màn hình.
- Gửi duyệt, xuất bản, tạm ẩn và tạo phiên bản nội dung.
- Xem thống kê lỗi, tỷ lệ hoàn thành và mức độ sử dụng gợi ý.

---

## 3. Cấu trúc chung của bài tập

Mỗi bài tập cần có các trường dữ liệu sau:

| Trường | Mô tả |
|---|---|
| Mã bài | Mã duy nhất của bài tập |
| Tên bài | Ví dụ: Nhận biết số 5 |
| Nhóm kỹ năng | Chữ cái, chữ số, toán, tư duy… |
| Chủ đề | Gia đình, động vật, trường học… |
| Mục tiêu | Kỹ năng cụ thể cần luyện |
| Độ khó | Dễ, trung bình hoặc nâng cao |
| Thời lượng | Thường từ 3–7 phút |
| Dạng tương tác | Chọn, kéo–thả, nối, tô, sắp xếp… |
| Lời hướng dẫn | Văn bản ngắn dành cho trẻ |
| Tệp giọng đọc | Âm thanh đọc hướng dẫn |
| Nội dung câu hỏi | Chữ, số, hình ảnh hoặc tình huống |
| Đáp án đúng | Một hoặc nhiều đáp án |
| Đáp án nhiễu | Các lựa chọn chưa đúng |
| Gợi ý | Gợi ý theo từng mức |
| Phản hồi | Nội dung khi đúng, chưa đúng hoặc bỏ qua |
| Phần thưởng | Sao, huy hiệu hoặc vật phẩm |
| Dữ liệu đánh giá | Số lần thử, thời gian, gợi ý đã dùng… |

### 3.1. Trạng thái bài tập

- Chưa bắt đầu.
- Đang học.
- Đã hoàn thành.
- Cần luyện thêm.
- Đã thành thạo.

### 3.2. Quy tắc phản hồi

- Đúng ngay: phát âm thanh vui ngắn và lời khen.
- Chưa đúng lần đầu: khuyến khích thử lại.
- Chưa đúng lần hai: cung cấp gợi ý trực quan.
- Vẫn gặp khó khăn: minh họa cách làm rồi cho trẻ thử một câu tương tự dễ hơn.
- Không dùng âm thanh thất bại mạnh, dấu X đỏ lớn hoặc lời phê bình.

---

# PHẦN A — CÁC NHÓM HỌC TẬP

## 4. Nhóm ưu tiên: Vẽ theo nét chữ và chữ số

> Đây là nhóm bài tập cần được xây dựng đầu tiên. Trẻ không chỉ nhìn và nhận biết ký hiệu mà còn quan sát thứ tự nét, chiều di chuyển và tự tay vẽ lại. Nhóm này bao gồm **chữ in hoa, chữ in thường và chữ số**.

### 4.1. Mục tiêu

- Rèn khả năng phối hợp tay–mắt và điều khiển ngón tay, bút cảm ứng hoặc chuột.
- Ghi nhớ hình dạng của chữ in hoa, chữ in thường và chữ số.
- Làm quen điểm đặt bút, chiều đi và thứ tự các nét.
- Phân biệt chữ hoa với chữ thường của cùng một chữ cái.
- Chuyển dần từ tô theo mẫu sang tự vẽ trong khung trống.
- Giúp trẻ nhận biết ký hiệu thông qua vận động, không chỉ qua quan sát.

### 4.2. Phạm vi nội dung

#### A. Nét tiền viết

Trẻ luyện nét ngang, sổ thẳng, xiên trái, xiên phải, cong hở, cong kín, móc, khuyết, thắt, vòng, gấp khúc, lượn sóng và xoắn đơn giản. Mỗi nét nên gắn với một bối cảnh vui nhộn như đưa ong về tổ hoặc giúp xe đi đúng đường.

#### B. Chữ in hoa

- Có bài riêng cho từng chữ in hoa trong bảng chữ cái tiếng Việt.
- Hiển thị chữ mẫu cỡ lớn, rõ và không dùng phông trang trí.
- Thể hiện số thứ tự từng nét: `1`, `2`, `3`.
- Có chấm màu đánh dấu điểm bắt đầu và mũi tên chỉ chiều đi.
- Có hoạt ảnh vẽ mẫu chậm trước khi trẻ thực hiện.
- Đọc tên chữ khi mở bài và sau khi hoàn thành.

Ví dụ bài chữ `A` in hoa:

1. Xem hoạt ảnh viết chữ `A`.
2. Đi theo nét xiên thứ nhất.
3. Đi theo nét xiên thứ hai.
4. Đi theo nét ngang.
5. Tô lại toàn bộ chữ.
6. Chọn chữ `A` vừa viết trong một nhóm chữ.

#### C. Chữ in thường

- Có bài riêng cho từng chữ in thường.
- Trình bày trên dòng kẻ phù hợp để trẻ thấy phần thân, phần cao và phần kéo xuống.
- Không mặc định chữ thường có cùng cấu trúc nét với chữ hoa.
- Chỉ mở bài chữ thường sau khi trẻ đã luyện nét nền tảng phù hợp.
- Sau khi tô, cho trẻ ghép chữ thường với chữ hoa tương ứng.

Ví dụ bài chữ `a` in thường:

1. Nghe “chữ a”.
2. Xem hoạt ảnh viết mẫu.
3. Tô từng nét theo thứ tự.
4. Tự vẽ lại trong ô có đường kẻ mờ.
5. Ghép `a` với `A`.

#### D. Chữ số

- Có bài riêng cho các chữ số từ `0` đến `9`.
- Hiển thị cách đọc và số lượng minh họa tương ứng.
- Có điểm bắt đầu, mũi tên và thứ tự nét.
- Sau khi tô số, cho trẻ chọn hoặc tạo số lượng tương ứng.

Ví dụ bài số `5`:

1. Nghe “số năm”.
2. Xem hoạt ảnh vẽ số `5`.
3. Tô theo đường nét đậm.
4. Tô theo đường nét mờ.
5. Tự vẽ số `5` trong ô trống.
6. Chọn nhóm có năm đồ vật.

### 4.3. Quy trình một bài vẽ theo nét

1. **Nhận biết:** Hiển thị và đọc tên chữ hoặc số.
2. **Quan sát:** Phát hoạt ảnh vẽ mẫu, thể hiện điểm bắt đầu và thứ tự nét.
3. **Vẽ có dẫn đường:** Trẻ đi theo đường mẫu rộng, nét đang vẽ được làm nổi bật.
4. **Vẽ theo nét mờ:** Giảm độ rõ và độ rộng của đường dẫn.
5. **Tự vẽ:** Trẻ vẽ trong khung hoặc dòng kẻ, chỉ còn mẫu nhỏ để đối chiếu.
6. **Củng cố:** Chọn ký hiệu vừa viết hoặc ghép với chữ hoa, chữ thường, âm hay số lượng tương ứng.

Trẻ có thể dừng ở bước 3 hoặc 4 nếu chưa sẵn sàng tự viết. Hệ thống không ép hoàn thành bước 5 để được nhận phần thưởng.

### 4.4. Các mức hỗ trợ

| Mức | Hình thức hỗ trợ |
|---|---|
| Mức 1 — Làm quen | Đường dẫn rộng, nét đậm, hoạt ảnh tự chạy, mũi tên luôn hiển thị |
| Mức 2 — Luyện tập | Đường dẫn vừa, nét chấm, chỉ hiện gợi ý khi trẻ dừng |
| Mức 3 — Ghi nhớ | Đường dẫn mờ, chỉ đánh dấu điểm bắt đầu |
| Mức 4 — Tự thực hiện | Khung trống hoặc dòng kẻ; mẫu nhỏ đặt bên cạnh |

### 4.5. Giao diện màn hình vẽ

- Tên chữ hoặc số và nút nghe.
- Vùng vẽ chiếm phần lớn màn hình.
- Nút **Xem mẫu**, **Nghe lại**, **Xóa nét**, **Hoàn tác** và **Làm lại**.
- Nút **Tiếp tục** chỉ sáng khi trẻ đã đi qua đủ các vùng chính.
- Chỉ báo nét hiện tại, ví dụ “Nét 1/3”.
- Mẫu nhỏ để đối chiếu ở bước tự vẽ.

Không đặt các nút xóa hoặc thoát quá gần vùng vẽ để tránh trẻ chạm nhầm.

### 4.6. Cách hướng dẫn và ghi nhận đường nét

Mỗi nét được lưu dưới dạng đường chuẩn với:

- `startPoint`: vùng được phép bắt đầu.
- `path`: đường trung tâm của nét mẫu.
- `tolerance`: độ rộng vùng được chấp nhận.
- `checkpoints`: các vùng trẻ cần đi qua theo thứ tự.
- `endPoint`: vùng kết thúc.
- `strokeOrder`: thứ tự của nét.
- `direction`: chiều đi mong muốn.

Khi trẻ vẽ:

- Bắt đầu đúng thì điểm đầu đổi màu và nét được kích hoạt.
- Đi hơi lệch nhưng vẫn trong vùng chấp nhận thì hệ thống tiếp tục ghi nhận.
- Đi lệch nhiều thì làm sáng đường cần đi, không xóa ngay toàn bộ nét.
- Bỏ qua điểm kiểm tra thì yêu cầu quay lại phần còn thiếu.
- Hoàn thành một nét thì tự động chuyển sang nét kế tiếp.

### 4.7. Đánh giá kết quả

Không yêu cầu nét vẽ trùng tuyệt đối với mẫu. Các tiêu chí gồm:

| Tiêu chí | Ý nghĩa |
|---|---|
| Điểm bắt đầu | Có bắt đầu trong vùng cho phép không |
| Thứ tự nét | Các nét có theo thứ tự hướng dẫn không |
| Chiều đi | Có đi theo chiều chính của nét không |
| Độ phủ | Có đi qua đủ phần quan trọng không |
| Độ lệch | Phần lớn đường vẽ có nằm trong vùng chấp nhận không |
| Hoàn thành | Đã hoàn thành bao nhiêu nét |
| Mức hỗ trợ | Có dùng xem mẫu hoặc gợi ý không |

Phản hồi cho trẻ chỉ gồm:

- **Hoàn thành tốt:** Đã đi qua đầy đủ các nét chính.
- **Đã hoàn thành:** Hình dạng có thể nhận biết và chỉ lệch nhẹ.
- **Cùng thử lại một phần:** Chỉ luyện lại nét còn thiếu, không vẽ lại toàn bộ.

Không hiển thị phần trăm hoặc điểm thấp cho trẻ. Số liệu chi tiết chỉ dành cho phụ huynh và hệ thống cá nhân hóa.

### 4.8. Dữ liệu cần lưu

- Loại ký hiệu: nét cơ bản, chữ hoa, chữ thường hoặc chữ số.
- Ký tự cụ thể, ví dụ `A`, `a` hoặc `5`.
- Số nét và số nét đã hoàn thành.
- Tọa độ đường vẽ của từng lần thử.
- Điểm bắt đầu, chiều và thứ tự nét.
- Độ phủ và độ lệch trung bình.
- Số lần xóa, làm lại, xem mẫu và dùng gợi ý.
- Mức hỗ trợ cao nhất trẻ có thể hoàn thành.
- Thiết bị nhập: chạm, bút cảm ứng hoặc chuột.
- Ngày luyện gần nhất và trạng thái cần luyện thêm.

### 4.9. Báo cáo dành cho phụ huynh

- Nét cơ bản trẻ đã thực hiện tốt.
- Chữ in hoa đã luyện và cần luyện lại.
- Chữ in thường đã luyện và cần luyện lại.
- Cặp chữ hoa–thường trẻ đã ghép đúng.
- Chữ số đã tô và tự vẽ được.
- Lỗi thường gặp: sai điểm đầu, ngược chiều, thiếu nét hoặc đi quá xa mẫu.
- Mức hỗ trợ hiện tại của từng ký tự.

Ví dụ:

> Bé đã tô tốt chữ `A`, `Ă`, `Â`; đang cần luyện thêm nét cong của chữ `a` và chiều viết số `5`. Bé hoàn thành tốt khi có đường nét mờ nhưng chưa nên chuyển hoàn toàn sang ô trống.

### 4.10. Chức năng quản trị nội dung

Trình tạo bài cần cho phép:

- Chọn loại nét cơ bản, chữ hoa, chữ thường hoặc chữ số.
- Nhập ký tự và tên đọc.
- Tải hoặc chọn mẫu ký tự chuẩn.
- Vẽ đường chuẩn bằng công cụ biên tập và chia thành từng nét.
- Đặt điểm bắt đầu, kết thúc, chiều đi và các điểm kiểm tra.
- Điều chỉnh vùng chấp nhận theo cấp độ.
- Xem hoạt ảnh mô phỏng trước khi xuất bản.
- Kiểm thử bằng chuột, chạm và bút cảm ứng.
- Liên kết bài củng cố: nhận biết chữ, ghép hoa–thường hoặc ghép số–lượng.

### 4.11. Tiêu chí nghiệm thu riêng

- Có bài nét cơ bản trước bài chữ và số.
- Có bài độc lập cho chữ in hoa và chữ in thường.
- Có bài cho đủ chữ số `0–9`.
- Mỗi bài thể hiện rõ điểm bắt đầu, thứ tự và chiều nét.
- Có thể xem lại hoạt ảnh mẫu bất kỳ lúc nào.
- Không xóa toàn bộ kết quả chỉ vì một đoạn đi lệch.
- Hoạt động ổn định bằng chuột, cảm ứng và bút cảm ứng.
- Báo cáo phân biệt chữ hoa, chữ thường, chữ số và nét cơ bản.
- Có bước củng cố nhận biết sau khi vẽ xong.

---

## 5. Nhóm làm quen chữ cái tiếng Việt

### 5.1. Mục tiêu

- Nhận biết 29 chữ cái tiếng Việt.
- Phân biệt chữ in hoa và chữ in thường.
- Làm quen âm của chữ.
- Nhận biết chữ xuất hiện trong từ quen thuộc.
- Phân biệt những chữ có hình dạng gần giống nhau.

### 5.2. Nội dung

#### A. Khám phá từng chữ cái

Mỗi chữ có một trang gồm:

- Chữ in hoa và in thường.
- Nút nghe tên hoặc âm của chữ.
- Hoạt ảnh mô tả hình dạng chữ.
- Từ khóa và hình minh họa.
- Bài luyện nhanh 3–5 câu.

#### B. Tìm chữ theo yêu cầu

Ví dụ: **Hãy tìm tất cả chữ A.**

- Hiển thị từ 4–8 chữ.
- Trẻ chạm vào một hoặc nhiều đáp án.
- Chữ đã chọn được đánh dấu rõ.
- Có nút nghe lại yêu cầu.

#### C. Ghép chữ hoa với chữ thường

- Kéo A vào a, B vào b hoặc Đ vào đ.
- Có thể triển khai dạng nối hai cột hoặc lật thẻ tìm cặp.

#### D. Phân biệt chữ gần giống

Các cặp nên luyện: `b–d`, `p–q`, `m–n`, `u–ư`, `o–ô–ơ`, `d–đ`.

#### E. Tìm chữ trong từ

Ví dụ: **Trong từ CÁ, đâu là chữ C?**

### 5.3. Cấp độ

1. Nhận biết từng chữ riêng lẻ.
2. Chọn một chữ trong hai lựa chọn.
3. Tìm chữ trong nhóm 4–8 chữ.
4. Ghép chữ hoa và chữ thường.
5. Tìm chữ trong từ quen thuộc.

### 5.4. Dữ liệu cần lưu

- Chữ đã giới thiệu và chữ đã thành thạo.
- Tỷ lệ đúng của từng chữ.
- Các cặp chữ thường bị nhầm.
- Số lần nghe lại và sử dụng gợi ý.

---

## 6. Nhóm nhận biết chữ số

> Đây là nhóm riêng về **hình dạng, tên gọi và thứ tự của chữ số**. Nhóm này khác với nhóm “Số lượng và toán học” ở phần tiếp theo.

### 6.1. Mục tiêu

- Nhận biết và gọi tên các chữ số từ `0` đến `9`.
- Làm quen các số từ `10` đến `20`.
- Phân biệt những chữ số có hình dạng gần giống.
- Tìm chữ số trong một nhóm ký hiệu.
- Ghép cách đọc, chữ số và số lượng tương ứng.
- Biết số đứng trước, số đứng sau trong phạm vi phù hợp.

### 6.2. Trang khám phá từng chữ số

Mỗi chữ số có một trang riêng gồm:

- Chữ số cỡ lớn.
- Tên số bằng chữ, ví dụ `5 – năm`.
- Nút nghe cách đọc.
- Hoạt ảnh mô tả hướng viết số.
- Nhóm đồ vật biểu diễn số lượng.
- Bài luyện nhanh.

Ví dụ trang số 5:

- Hiển thị số `5`.
- Phát giọng đọc “số năm”.
- Hiển thị năm quả táo.
- Cho trẻ chạm từng quả táo để nghe đếm `1, 2, 3, 4, 5`.

### 6.3. Các dạng bài chức năng

#### A. Nghe và chọn chữ số

Hệ thống đọc: **Hãy chọn số bảy.**

- Hiển thị 2 lựa chọn ở mức dễ.
- Tăng lên 3–5 lựa chọn ở mức cao hơn.
- Trẻ chạm vào số cần chọn.

#### B. Nhìn và đọc tên số

- Hiển thị một chữ số.
- Cho trẻ chọn tên đúng trong các lựa chọn có giọng đọc.
- Ví dụ: màn hình có số `8`; lựa chọn “sáu”, “tám”, “chín”.

#### C. Tìm tất cả chữ số giống mẫu

Ví dụ: **Hãy tìm tất cả số 3.**

- Trộn chữ số cần tìm với chữ số khác.
- Có thể trộn thêm một số chữ cái ở cấp nâng cao để trẻ phân biệt chữ và số.

#### D. Ghép chữ số với cách đọc

- Một bên là `2`, `4`, `6`.
- Một bên là các nút âm thanh “hai”, “bốn”, “sáu”.
- Trẻ nghe rồi nối hoặc kéo cặp tương ứng.

#### E. Phân biệt chữ số gần giống

Các cặp gợi ý:

- `6` và `9`.
- `2` và `5`.
- `1` và `7` tùy kiểu chữ.
- `3` và `8` ở mức quan sát hình dạng.

Không nên dùng phông chữ trang trí làm thay đổi hình dạng chuẩn của chữ số.

#### F. Điền số còn thiếu trong dãy

Ví dụ:

- `1 – 2 – _ – 4`.
- `_ – 6 – 7`.
- `8 – 9 – _`.

Ở mức đầu, luôn có trục số hoặc hình minh họa hỗ trợ.

#### G. Số đứng trước và số đứng sau

Ví dụ:

- Số nào đứng sau số 4?
- Số nào đứng trước số 8?

#### H. Sắp xếp chữ số

- Sắp xếp từ 1 đến 5.
- Sắp xếp từ bé đến lớn trong phạm vi 10.
- Ở mức nâng cao có thể sắp xếp một nhóm không bắt đầu từ 1, ví dụ `4, 5, 6, 7`.

#### I. Ghép chữ số với số lượng

- Kéo số `4` vào nhóm có bốn đồ vật.
- Đây là bài chuyển tiếp từ nhận biết ký hiệu sang hiểu khái niệm số lượng.

#### J. Tô và viết chữ số

- Hiển thị điểm bắt đầu và mũi tên chỉ hướng.
- Cho trẻ tô theo nét mờ.
- Có thể cho viết lại trong ô trống sau khi hoàn thành nét mẫu.

### 6.4. Lộ trình đề xuất

| Giai đoạn | Nội dung |
|---|---|
| 1 | Nhận biết số 1–3 |
| 2 | Nhận biết số 4–5 |
| 3 | Ôn số 1–5 và ghép với số lượng |
| 4 | Nhận biết số 0 và ý nghĩa “không có” |
| 5 | Nhận biết số 6–10 |
| 6 | Thứ tự, trước–sau trong phạm vi 10 |
| 7 | Làm quen số 11–20 bằng nhóm chục và đơn vị trực quan |

### 6.5. Quy tắc riêng khi dạy số 0

- Giải thích bằng tình huống “không còn đồ vật nào”.
- Ví dụ: có ba quả bóng, lần lượt cất đi, cuối cùng còn 0 quả bóng.
- Không chỉ yêu cầu nhận biết hình dạng `0`; cần gắn với ý nghĩa số lượng bằng không.

### 6.6. Dữ liệu cần lưu

- Chữ số đã học và đã nhận biết ổn định.
- Chữ số trẻ thường chọn nhầm.
- Khả năng nối tên số với chữ số.
- Khả năng xác định số trước và sau.
- Phạm vi số trẻ đang làm tốt: 0–5, 0–10 hoặc 0–20.
- Độ chính xác khi tô từng chữ số.

---

## 7. Nhóm số lượng và toán học ban đầu

> Nhóm này tập trung vào **ý nghĩa của số**, đếm, so sánh, tách–gộp, cộng–bớt và giải quyết tình huống. Trẻ nhận ra ký hiệu `5` chưa đồng nghĩa với việc trẻ hiểu số lượng 5.

### 7.1. Mục tiêu

- Đếm đúng số lượng trong phạm vi 5, 10 và tiến tới 20.
- Hiểu mỗi đồ vật chỉ được đếm một lần.
- Hiểu số cuối cùng đọc được biểu thị tổng số đồ vật.
- Ghép đúng chữ số với số lượng.
- So sánh nhiều hơn, ít hơn và bằng nhau.
- Sắp xếp số lượng từ ít đến nhiều.
- Hiểu thêm vào làm số lượng tăng và lấy bớt làm số lượng giảm.
- Làm quen tách–gộp và cộng–bớt trực quan trong phạm vi 10.

### 7.2. Các dạng bài chức năng

#### A. Đếm đồ vật

Ví dụ: **Có bao nhiêu con cá?**

Chức năng:

- Trẻ chạm từng đồ vật để đếm.
- Đồ vật đã chạm đổi màu hoặc di chuyển nhẹ.
- Hệ thống đọc số theo từng lần chạm.
- Sau khi đếm, trẻ chọn chữ số tương ứng.
- Vị trí đồ vật có thể xếp hàng ở mức dễ và rải rác ở mức cao hơn.

#### B. Tạo đúng số lượng

Ví dụ: **Hãy cho 4 củ cà rốt vào giỏ.**

- Trẻ kéo đồ vật vào vùng đích.
- Hiển thị số lượng đã kéo.
- Cho phép kéo đồ vật ra nếu vượt quá số yêu cầu.

#### C. Ghép số với lượng

- Một cột là chữ số.
- Một cột là các nhóm đồ vật.
- Trẻ nối hoặc kéo đúng cặp.

#### D. So sánh nhiều–ít–bằng nhau

Ví dụ:

- Nhóm nào có nhiều quả hơn?
- Nhóm nào có ít con vật hơn?
- Hai nhóm có bằng nhau không?

Giai đoạn đầu dùng từ ngữ và hình ảnh; chưa bắt buộc ký hiệu `>`, `<`, `=`.

#### E. Làm cho hai nhóm bằng nhau

Ví dụ:

- Bên trái có 3 chiếc bánh, bên phải có 2 bạn nhỏ.
- Trẻ thêm hoặc bớt để mỗi bạn có một chiếc bánh.

#### F. Tách một nhóm thành hai phần

Ví dụ:

- Có 5 quả bóng.
- Hãy đặt một số vào giỏ đỏ và số còn lại vào giỏ xanh.
- Hệ thống có thể ghi nhận các cách tách `1–4`, `2–3`, `3–2`, `4–1`.

#### G. Gộp hai nhóm

Ví dụ:

- Có 2 con vịt ở hồ và 3 con vịt đi tới.
- Trẻ kéo hai nhóm lại rồi đếm tất cả.

#### H. Cộng bằng hình ảnh

Quy trình một câu:

1. Hiển thị nhóm ban đầu.
2. Cho thêm đồ vật bằng hoạt ảnh.
3. Đọc tình huống.
4. Cho trẻ thao tác hoặc đếm.
5. Chọn kết quả.

Ví dụ: **Có 2 quả táo, thêm 1 quả. Có tất cả bao nhiêu quả?**

#### I. Bớt bằng hình ảnh

Ví dụ: **Có 5 con cá, 2 con bơi đi. Còn lại bao nhiêu con?**

- Đồ vật được lấy đi phải biến mất hoặc chuyển sang vùng khác rõ ràng.
- Trẻ đếm nhóm còn lại trước khi chọn kết quả.

#### J. Bài toán tình huống đơn giản

- Câu hỏi chỉ nên có một phép biến đổi.
- Có giọng đọc và hình minh họa.
- Không dùng câu dài hoặc dữ kiện thừa ở giai đoạn đầu.

#### K. Quy luật số lượng

Ví dụ:

- Một chấm, hai chấm, một chấm, hai chấm, ...
- 1 quả, 2 quả, 3 quả, ... nhóm tiếp theo có bao nhiêu?

### 7.3. Cấp độ

| Cấp | Phạm vi và kỹ năng |
|---|---|
| 1 | Đếm và tạo số lượng 1–3 |
| 2 | Đếm, ghép và so sánh trong phạm vi 5 |
| 3 | Đếm và so sánh trong phạm vi 10 |
| 4 | Tách–gộp trong phạm vi 5 |
| 5 | Cộng–bớt trực quan trong phạm vi 5 |
| 6 | Đếm đến 20; cộng–bớt trực quan trong phạm vi 10 |

### 7.4. Dữ liệu cần lưu

- Phạm vi đếm chính xác.
- Khả năng đếm đồ vật xếp hàng và rải rác.
- Khả năng ghép chữ số với số lượng.
- Khả năng so sánh hai nhóm.
- Mức độ hiểu tách, gộp, thêm và bớt.
- Loại tình huống trẻ thường sai.
- Việc trẻ có đếm lặp hoặc bỏ sót đồ vật hay không.

---

## 8. Nhóm tiền tập đọc và phát triển ngôn ngữ

### 8.1. Mục tiêu

- Mở rộng vốn từ theo chủ đề.
- Nghe và hiểu yêu cầu ngắn.
- Ghép tranh với từ quen thuộc.
- Nhận biết âm đầu ở mức làm quen.
- Hiểu trình tự và nội dung câu chuyện ngắn.

### 8.2. Dạng bài

- Nghe từ và chọn tranh.
- Ghép từ với tranh; luôn có nút nghe nếu trẻ chưa đọc được.
- Tìm hai từ có âm đầu giống nhau.
- Chọn chữ còn thiếu trong từ có hình minh họa.
- Sắp xếp 3–4 tranh thành câu chuyện.
- Nghe câu chuyện 3–6 câu và trả lời 2–3 câu hỏi.
- Chọn nhân vật, địa điểm, sự việc hoặc cảm xúc xuất hiện trong truyện.

### 8.3. Dữ liệu cần lưu

- Vốn từ theo chủ đề.
- Khả năng nghe hiểu.
- Khả năng nhận biết âm đầu.
- Khả năng hiểu trình tự trước–sau.
- Số lần nghe lại.

---

## 9. Nhóm bài bổ trợ vận động tinh

### 9.1. Mục tiêu

- Rèn phối hợp tay–mắt.
- Làm quen các nét cơ bản.
- Tập điều khiển ngón tay, chuột hoặc bút cảm ứng.
- Tạo nền vận động để trẻ thực hiện tốt nhóm vẽ theo nét ở Mục 4.

### 9.2. Dạng bài

- Tô nét ngang, thẳng, xiên, cong, móc, khuyết và vòng.
- Nối điểm theo thứ tự số để tạo hình.
- Đi theo đường mê cung.
- Sao chép hình đơn giản vào ô trống.
- Kéo vật theo đường hẹp, vòng quanh chướng ngại và nối hai vật liên quan.

### 9.3. Yêu cầu kỹ thuật

- Hỗ trợ chuột, màn hình cảm ứng và bút cảm ứng.
- Có điểm bắt đầu và mũi tên chỉ hướng.
- Lưu đường vẽ dưới dạng tọa độ.
- Cho phép xóa, hoàn tác và viết lại.
- Vùng chấp nhận cần đủ rộng, không đánh giá khắt khe như chữ viết người lớn.

### 9.4. Dữ liệu cần lưu

- Nét đã luyện.
- Tỷ lệ đường vẽ nằm trong vùng mẫu.
- Chiều vẽ và điểm bắt đầu.
- Số lần viết lại.
- Dạng nét vận động trẻ còn gặp khó khăn; dữ liệu vẽ chữ và số được lưu ở nhóm Mục 4.

---

## 10. Nhóm hình dạng, đo lường và không gian

### 10.1. Mục tiêu

- Nhận biết hình tròn, vuông, tam giác, chữ nhật và bầu dục.
- Nhận biết màu sắc và kích thước.
- Hiểu trên–dưới, trước–sau, trái–phải, trong–ngoài, gần–xa và ở giữa.
- So sánh dài–ngắn, cao–thấp, lớn–nhỏ ở mức trực quan.

### 10.2. Dạng bài

- Nghe tên và chọn hình.
- Tìm hình trong đồ vật đời sống.
- Phân loại đồ vật theo hình dạng.
- Ghép hai nửa của hình.
- Đặt vật vào đúng vị trí theo lời đọc.
- Sắp xếp từ nhỏ đến lớn, thấp đến cao hoặc ngắn đến dài.
- Chọn vật có thể chứa nhiều hơn bằng quan sát trực quan.

### 10.3. Dữ liệu cần lưu

- Hình và quan hệ không gian đã thành thạo.
- Khái niệm trẻ thường nhầm.
- Khả năng phân loại theo một hoặc hai đặc điểm.

---

## 11. Nhóm tư duy logic

### 11.1. Mục tiêu

- Quan sát và phát hiện điểm giống, khác.
- Phân loại theo đặc điểm.
- Nhận biết quy luật.
- Suy luận và giải quyết vấn đề đơn giản.

### 11.2. Dạng bài

- Tìm hình khác biệt.
- Phân loại theo màu, hình, kích thước, công dụng hoặc nơi sống.
- Hoàn thành quy luật hình ảnh.
- Tìm phần còn thiếu của bức tranh.
- Ghép đồ vật với bóng.
- Mê cung có cấp độ tăng dần.
- Sắp xếp các bước của một hoạt động.
- Chọn đồ vật phù hợp để giải quyết tình huống.

### 11.3. Dữ liệu cần lưu

- Tiêu chí phân loại trẻ đã hiểu.
- Loại quy luật trẻ nhận biết được.
- Thời gian xử lý.
- Số lần thử và mức gợi ý.

---

## 12. Nhóm ghi nhớ và tập trung

### 12.1. Mục tiêu

- Rèn trí nhớ hình ảnh và âm thanh.
- Tăng thời gian duy trì chú ý.
- Thực hiện yêu cầu gồm một đến ba bước.

### 12.2. Dạng bài

- Lật thẻ tìm hai hình giống nhau.
- Ghép chữ hoa–thường hoặc số–số lượng bằng thẻ nhớ.
- Nhớ vị trí đồ vật.
- Tìm vật vừa biến mất.
- Nghe và thực hiện lần lượt các thao tác.
- Tìm đồ vật trong bức tranh lớn.

### 12.3. Dữ liệu cần lưu

- Số lượng đối tượng trẻ nhớ được.
- Khả năng nhớ vị trí.
- Số bước hướng dẫn có thể thực hiện.
- Thời gian hoàn thành và số lần cần gợi ý.

---

## 13. Nhóm khám phá thế giới

### 13.1. Chủ đề

- Động vật: tên, tiếng kêu, thức ăn và nơi sống.
- Thực vật: bộ phận của cây và quá trình phát triển.
- Cơ thể người: bộ phận, giác quan, vệ sinh và sức khỏe.
- Nghề nghiệp: công việc, công cụ và nơi làm việc.
- Phương tiện: đường bộ, đường sắt, đường thủy và hàng không.
- Thời tiết: nắng, mưa, gió, nóng và lạnh.
- Môi trường: bỏ rác đúng nơi, tiết kiệm nước và chăm sóc cây.

### 13.2. Dạng bài

- Nghe và chọn.
- Đoán âm thanh.
- Ghép cặp hoặc phân loại.
- Sắp xếp quá trình.
- Chọn trang phục phù hợp với thời tiết.
- Trả lời câu hỏi sau hoạt ảnh ngắn.

---

## 14. Nhóm kỹ năng sống và an toàn

### 14.1. Nội dung

- Tự phục vụ: mặc đồ, chuẩn bị cặp, rửa tay và sắp xếp đồ dùng.
- Ở trường: ngồi học, giơ tay, lắng nghe, xếp hàng và giữ đồ dùng.
- Giao thông: đèn tín hiệu, vỉa hè, sang đường và đội mũ bảo hiểm.
- An toàn cá nhân: không đi theo người lạ, tìm người lớn đáng tin cậy, nhận biết vùng riêng tư.
- An toàn trong nhà: tránh ổ điện, lửa, thuốc và vật sắc nhọn.

### 14.2. Dạng bài tình huống

Mỗi tình huống gồm:

1. Tranh hoặc hoạt cảnh.
2. Giọng đọc mô tả.
3. Hai hoặc ba lựa chọn.
4. Giải thích vì sao hành động an toàn hoặc chưa an toàn.
5. Gợi ý để phụ huynh trao đổi thêm với trẻ.

Không chỉ báo đúng–sai; hệ thống phải giải thích ngắn gọn bằng ngôn ngữ phù hợp với trẻ.

---

## 15. Nhóm cảm xúc và kỹ năng xã hội

### 15.1. Mục tiêu

- Nhận biết và gọi tên cảm xúc.
- Biết diễn đạt nhu cầu và nhờ trợ giúp.
- Biết chờ lượt, chia sẻ và hợp tác.
- Làm quen cách xử lý mâu thuẫn.

### 15.2. Nội dung

- Cảm xúc: vui, buồn, tức giận, sợ, ngạc nhiên, lo lắng và tự hào.
- Bình tĩnh: hít thở, đếm chậm và tìm người lớn.
- Giao tiếp: chào hỏi, cảm ơn, xin lỗi, xin phép và nhờ giúp đỡ.
- Hợp tác: chờ lượt, chia sẻ, tuân thủ luật chơi và tôn trọng sự khác biệt.

### 15.3. Nguyên tắc đánh giá

- Không chấm điểm cảm xúc theo kiểu cứng nhắc.
- Lưu chủ đề đã học và lựa chọn cần phụ huynh trao đổi thêm.
- Chấp nhận nhiều cách diễn đạt phù hợp trong một số tình huống.

---

# PHẦN B — CHỨC NĂNG HỆ THỐNG

## 16. Bài học hôm nay

### 16.1. Cấu trúc một buổi học

1. Một hoạt động khởi động bằng nét cơ bản hoặc vận động tinh.
2. Một bài vẽ theo nét chữ hoa, chữ thường hoặc chữ số đang học.
3. Một bài nhận biết chữ cái/ngôn ngữ hoặc chữ số/số lượng liên quan.
4. Một bài tư duy, hình dạng hoặc ghi nhớ.
5. Một tình huống kỹ năng sống ngắn.
6. Màn hình tổng kết và phần thưởng.

### 16.2. Quy tắc đề xuất

- Tổng thời lượng khoảng 10–15 phút.
- Xen kẽ nghe, nhìn, chạm và kéo–thả.
- Ưu tiên kỹ năng trẻ đang cần luyện.
- Không đưa quá nhiều kiến thức mới trong một buổi.
- Không lặp liên tiếp một dạng tương tác.
- Lưu vị trí nếu trẻ dừng giữa bài.

---

## 17. Hệ thống cấp độ và cá nhân hóa

| Mức | Đặc điểm |
|---|---|
| Dễ | Hai lựa chọn, hình rõ, hướng dẫn trực tiếp, có thao tác mẫu |
| Trung bình | Ba hoặc bốn lựa chọn, giảm gợi ý |
| Nâng cao | Kết hợp nhiều kỹ năng và có câu hỏi vận dụng |

Quy tắc điều chỉnh:

- Đúng 3–5 câu liên tiếp: tăng nhẹ độ khó.
- Sai lần đầu: cho thử lại.
- Sai lần hai: đưa gợi ý.
- Sai nhiều lần: đưa ví dụ dễ hơn thay vì tiếp tục tăng áp lực.
- Không tăng độ khó chỉ dựa vào tốc độ.
- Phụ huynh có thể tắt tự động điều chỉnh.

---

## 18. Chấm kết quả và phần thưởng

### 18.1. Điểm nội bộ

- Đúng lần đầu: 3 điểm.
- Đúng sau khi thử lại: 2 điểm.
- Đúng sau gợi ý: 1 điểm.
- Chưa hoàn thành: 0 điểm.

Điểm này dùng để phân tích, không cần hiển thị trực tiếp cho trẻ.

### 18.2. Nội dung trẻ nhìn thấy

- Sao hoặc sticker.
- Huy hiệu theo nhóm kỹ năng.
- Vật phẩm cho nhân vật hoặc khu vườn ảo.
- Lời động viên.
- Thanh tiến độ của chính trẻ, không so sánh với người khác.

---

## 19. Khu vực phụ huynh

### 19.1. Hồ sơ trẻ

- Tên hoặc biệt danh.
- Năm sinh hoặc độ tuổi.
- Nhân vật đại diện.
- Thời lượng học mong muốn.
- Nhóm kỹ năng ưu tiên.
- Cài đặt âm thanh.

Hạn chế thu thập dữ liệu cá nhân không cần thiết.

### 19.2. Báo cáo tổng quan

- Số buổi và tổng thời gian học.
- Số bài đã hoàn thành.
- Kỹ năng nổi bật và kỹ năng cần luyện thêm.
- Mức độ sử dụng gợi ý.
- Nội dung trẻ thường nhầm.

### 19.3. Báo cáo chữ số và toán học

Báo cáo phải tách rõ:

| Báo cáo | Ví dụ |
|---|---|
| Nhận biết chữ số | Nhận biết tốt 0–7; còn nhầm 6 và 9 |
| Thứ tự số | Xác định tốt số trước–sau trong phạm vi 5 |
| Đếm số lượng | Đếm chính xác tối đa 8 đồ vật xếp hàng |
| Ghép số với lượng | Ghép đúng 1–5; cần luyện 6–10 |
| So sánh | Hiểu nhiều–ít; chưa ổn định với bằng nhau |
| Tách–gộp | Thực hiện được trong phạm vi 5 |
| Cộng–bớt | Hiểu bằng hình ảnh trong phạm vi 5 |

### 19.4. Gợi ý hoạt động ngoại tuyến

Ví dụ:

- Tìm chữ số trên lịch, đồng hồ hoặc biển số.
- Đếm thìa khi dọn bàn.
- Chia sáu quả thành hai nhóm khác nhau.
- So sánh hai rổ đồ chơi.
- Tìm các hình tròn trong nhà.

### 19.5. Giới hạn sử dụng

- Chọn 10, 15 hoặc 20 phút mỗi ngày.
- Nhắc nghỉ giữa buổi.
- Chọn ngày học trong tuần.
- Khóa khu vực người lớn bằng mã PIN hoặc câu hỏi dành cho người lớn.

---

## 20. Trang quản trị nội dung

### 20.1. Quản lý nhóm và chủ đề

- Thêm, sửa, sắp xếp hoặc tạm ẩn nhóm.
- Đặt biểu tượng, màu sắc và mô tả.
- Chọn điều kiện mở khóa.

### 20.2. Trình tạo bài tập

Quản trị viên cần có thể:

- Chọn mẫu tương tác.
- Nhập lời hướng dẫn.
- Tải giọng đọc hoặc tạo tệp đọc từ nội dung đã duyệt.
- Tải hình ảnh và đặt vùng tương tác.
- Chọn đáp án đúng, đáp án nhiễu và gợi ý.
- Chọn phạm vi số hoặc chữ cái liên quan.
- Xem trước trên điện thoại, máy tính bảng và máy tính.
- Lưu nháp, gửi duyệt và xuất bản.

### 20.3. Mẫu bài dành riêng cho chữ số/toán

- Chọn chữ số theo âm thanh.
- Tìm chữ số giống mẫu.
- Điền số vào dãy.
- Sắp xếp số.
- Đếm đối tượng.
- Tạo số lượng bằng kéo–thả.
- Ghép số với lượng.
- So sánh hai nhóm.
- Tách một nhóm.
- Gộp hai nhóm.
- Cộng hoặc bớt bằng hoạt ảnh.
- Bài toán tình huống một bước.

### 20.4. Kiểm duyệt

1. Soạn nội dung.
2. Kiểm tra kiến thức và mức độ phù hợp với trẻ.
3. Kiểm tra tiếng Việt, hình ảnh và giọng đọc.
4. Xem trước và kiểm thử thao tác.
5. Xuất bản.
6. Theo dõi phản hồi và tạo phiên bản cập nhật.

---

## 21. Danh sách màn hình

### 21.1. Khu vực trẻ

1. Chào mừng.
2. Chọn hồ sơ.
3. Trang chủ.
4. Bài học hôm nay.
5. Danh sách nhóm kỹ năng.
6. Bản đồ học chữ cái.
7. Bản đồ học chữ số.
8. Danh sách bài tập của từng nhóm.
9. Màn hình làm bài.
10. Màn hình gợi ý.
11. Màn hình tạm nghỉ.
12. Tổng kết buổi học.
13. Bộ sưu tập huy hiệu.
14. Khu vực nhân vật hoặc khu vườn.

### 21.2. Khu vực phụ huynh

1. Đăng nhập và đăng ký.
2. Quản lý hồ sơ trẻ.
3. Tổng quan tiến độ.
4. Báo cáo chữ cái.
5. Báo cáo chữ số và toán học.
6. Báo cáo các kỹ năng khác.
7. Lịch sử học.
8. Cài đặt lộ trình.
9. Giới hạn thời gian.
10. Phiếu bài tập.
11. Cài đặt tài khoản.

### 21.3. Khu vực quản trị

1. Bảng điều khiển.
2. Quản lý người dùng.
3. Quản lý nhóm và chủ đề.
4. Quản lý bài học.
5. Ngân hàng câu hỏi.
6. Quản lý hình ảnh và âm thanh.
7. Kiểm duyệt nội dung.
8. Thống kê sử dụng.
9. Cấu hình phần thưởng.

---

## 22. Phạm vi phiên bản MVP

### 22.1. Nội dung tối thiểu

| Nhóm | Số lượng đề xuất |
|---|---:|
| Vẽ theo nét cơ bản | 12 bài |
| Vẽ chữ in hoa | Tối thiểu 29 bài, mỗi chữ một bài |
| Vẽ chữ in thường | Tối thiểu 29 bài, mỗi chữ một bài |
| Vẽ chữ số | 10 bài cho các số 0–9 |
| Chữ cái | 20 bài |
| Nhận biết chữ số | 15 bài |
| Số lượng và toán học | 20 bài |
| Bổ trợ vận động tinh | 10 bài |
| Hình dạng và không gian | 10 bài |
| Tư duy logic | 15 bài |
| Ghi nhớ | 10 bài |
| Kỹ năng sống | 10 tình huống |

### 22.2. Dạng tương tác tối thiểu

- Chọn một đáp án.
- Chọn nhiều đáp án.
- Kéo–thả.
- Nối cặp.
- Sắp xếp.
- Nghe và chọn.
- Vẽ theo nét đa bước: xem mẫu, tô nét đậm, tô nét mờ và tự vẽ.

### 22.3. Chức năng tối thiểu

- Tài khoản phụ huynh và hồ sơ trẻ.
- Bài học hôm nay.
- Danh mục chữ cái và chữ số riêng.
- Lưu tiến độ và tiếp tục bài đang học.
- Giọng đọc hướng dẫn.
- Chấm kết quả và gợi ý.
- Sao, huy hiệu và phần thưởng đơn giản.
- Báo cáo phụ huynh.
- Trang quản trị và trình tạo bài cơ bản.
- Giao diện đáp ứng cho điện thoại, máy tính bảng và máy tính.

---

## 23. Tiêu chí nghiệm thu cơ bản

### 23.1. Trải nghiệm của trẻ

- Trẻ có thể bắt đầu bài học với tối đa hai thao tác từ trang chủ.
- Tất cả yêu cầu quan trọng đều có thể nghe bằng âm thanh.
- Vùng bấm đủ lớn và không đặt các đáp án quá sát nhau.
- Trạng thái đúng, chưa đúng, đang kéo và đã chọn dễ nhận biết.
- Bài đang làm được lưu khi tải lại trang hoặc đổi thiết bị sau khi đồng bộ.
- Bài vẽ hiển thị đúng mẫu chữ hoa, chữ thường hoặc chữ số, có điểm đầu và chiều nét rõ ràng.

### 23.2. Chức năng số

- Có nội dung riêng cho chữ số `0–9` và làm quen `10–20`.
- Có ít nhất một bài nhận biết, một bài tô và một bài ghép lượng cho mỗi chữ số `0–9`.
- Hệ thống phân biệt dữ liệu “nhận biết chữ số” với “đếm số lượng”.
- Báo cáo được phạm vi đếm và các chữ số trẻ thường nhầm.
- Bài cộng–bớt luôn có biểu diễn trực quan trong MVP.

### 23.3. Phụ huynh và quản trị

- Phụ huynh xem được tiến độ theo từng nhóm.
- Khu vực phụ huynh được bảo vệ khỏi thao tác vô tình của trẻ.
- Quản trị viên có thể tạo và xuất bản bài mà không sửa mã nguồn.
- Nội dung nháp không xuất hiện trong tài khoản trẻ.

---

## 24. Hướng phát triển sau MVP

- Lộ trình học cá nhân hóa sâu hơn.
- Phiếu bài tập PDF và chế độ in.
- Học ngoại tuyến rồi đồng bộ tiến độ.
- Nhiều giọng đọc và tốc độ đọc.
- Câu chuyện tương tác dài theo tuần.
- Hoạt động cùng phụ huynh ngoài màn hình.
- Hỗ trợ tiếp cận cho trẻ có nhu cầu đặc biệt.
- Nhận diện giọng nói chỉ khi đã kiểm thử kỹ độ chính xác, quyền riêng tư và sự phù hợp với giọng trẻ em.

---

## 25. Kết luận

Website nên xem chữ cái và chữ số là hai năng lực nền tảng song song. Riêng phần số cần được thiết kế thành hai lớp:

1. **Nhận biết chữ số:** hình dạng, tên gọi, cách đọc, cách viết và thứ tự.
2. **Hiểu số và toán học:** số lượng, đếm, so sánh, tách–gộp, thêm–bớt và tình huống thực tế.

Việc tách hai lớp này giúp hệ thống đánh giá chính xác hơn: một trẻ có thể nhận ra số `8` nhưng vẫn chưa đếm đúng tám đồ vật, hoặc đếm đúng nhưng chưa chọn được ký hiệu tương ứng. Đây cũng là cơ sở để xây dựng lộ trình và báo cáo phụ huynh có giá trị thực tế.
