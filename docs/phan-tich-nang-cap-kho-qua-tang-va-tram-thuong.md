# Phân tích nâng cấp Kho quà tặng và Trạm thưởng

> Phạm vi: `/kids/rewards`, bản đồ nhóm kỹ năng `/kids/skills/{id}`, luồng hoàn thành bài, sao, huy hiệu, vật phẩm và việc học lại.
>
> Mục tiêu sản phẩm: biến phần thưởng thành một vòng lặp tạo động lực thật, trong đó mỗi **10 bài hoàn thành lần đầu trong một nhóm kỹ năng** mở một rương; học lại không sinh lại rương cũ.

## 1. Kết luận điều hành

Chức năng hiện tại có giao diện khá đầy đủ nhưng phần thưởng chưa gắn với trạm theo nghĩa nghiệp vụ:

- Rương trên map chỉ là HTML minh họa. Không có bản ghi rương, trạng thái có thể nhận, thao tác mở hay quà gắn với rương.
- Mốc hiện là 5 bài và rương đang chiếm chính dòng hiển thị của bài số 5, thay vì là một node riêng sau bài số 5.
- Quà thật chỉ được xét khi bé mở trang tổng kết. Nếu không đi qua trang này thì phần thưởng không được cập nhật.
- Tiến độ hiện đếm **lượt làm** thay vì **bài duy nhất đã hoàn thành**, nên học lại có thể làm tăng sao và chạm mốc không đúng bản chất.
- Một lần luyện lại sai có thể làm trạng thái gần nhất của bài thành `needs_practice`, dù bé đã vượt bài trước đó.
- 10 nhóm kỹ năng chưa có tiến độ rương độc lập; kho quà chỉ là danh sách 18 định nghĩa chung.

Hướng nâng cấp được đề xuất:

1. Tách bạch `lượt làm`, `hoàn thành lần đầu` và `quyền nhận quà`.
2. Tính mốc riêng cho từng nhóm: 10, 20, 30... bài duy nhất.
3. Biến rương thành node riêng, có ba trạng thái `locked`, `claimable`, `claimed`.
4. Ghi nhận quyền nhận quà ngay khi bé hoàn thành bài thứ 10, không phụ thuộc trang Summary.
5. Dùng khóa duy nhất trong database để mỗi bé chỉ nhận mỗi rương một lần, kể cả khi double-click, refresh hoặc hai request đến cùng lúc.

## 2. Hiện trạng qua source

### 2.1. Map nhóm kỹ năng

`Views/Kids/Skill.cshtml` xác định rương bằng `stepNumber % 5 == 0`. Rương nằm trong chuỗi `if/else` của chính bài học:

- Nếu bài đang hoạt động: hiển thị trạm học, không hiển thị rương.
- Nếu bài đã hoàn thành: hiển thị trạm hoàn thành, rương biến mất.
- Chỉ khi bài bị khóa và số thứ tự chia hết cho 5 thì hiển thị rương khóa.
- Click rương gọi chung `handleLockedStation`; không có endpoint mở rương.

Như vậy rương hiện không phải một mốc thưởng độc lập và cũng không thể tồn tại sau khi đã mở.

### 2.2. Cách map tính trạng thái trạm

Controller lấy lượt làm mới nhất theo bài. View lại suy ra:

- Mọi index nhỏ hơn `currentItemIndex` là đã hoàn thành.
- Index bằng `currentItemIndex` là trạm hiện tại.
- Mọi index lớn hơn là bị khóa.

Cách suy diễn này không dựa trực tiếp vào việc bài đã từng hoàn thành hay chưa. Khi bé quay lại luyện một bài cũ, `LastPracticedItemId` có thể kéo “trạm hiện tại” lùi về phía sau và khóa lại các bài đã mở.

### 2.3. Cách cấp thưởng

`EvaluateAndAwardBadgesAsync` chỉ được gọi trong action `Summary`. Luồng hiện tại có các vấn đề:

- `totalAttempts.Count` đếm tất cả lượt `completed`, không `Distinct` theo `LearningItemId`.
- `totalStars` cộng sao của tất cả lượt làm; cùng một bài có thể cộng sao nhiều lần.
- Huy hiệu nhóm được mở chỉ sau một bài bất kỳ thuộc nhóm, trong khi tên và mô tả mang ý nghĩa “chinh phục”.
- `streak-3d` và `streak-7d` thực tế đếm tổng số session, không kiểm tra ngày liên tiếp.
- Truy cập Summary khi session còn `active` sẽ đánh dấu session `completed`, kể cả khi chưa hoàn thành kế hoạch.
- `ChildRewards` chưa có unique index `(ChildProfileId, RewardDefinitionId)`; code có kiểm tra trước khi thêm nhưng vẫn có thể trùng khi có request cạnh tranh.

### 2.4. Kho quà

`/kids/rewards` hiện là collection grid với bộ lọc huy hiệu/vật phẩm/đã đạt. Giao diện đã theo claymorphism của Kids Zone, nhưng thiếu:

- Tiến độ đến rương kế tiếp của từng nhóm.
- Phân biệt quà “có thể nhận ngay” với quà “đã sở hữu”.
- Nguồn gốc quà: nhận từ nhóm nào, rương mốc bao nhiêu.
- Lịch sử mở rương.
- Tác dụng thật của `item`. Model `GardenItem` có trong database nhưng chưa được nối vào kho quà hay một khu vườn có thể trang trí.

Ngoài ra, nội dung ghi “18 danh hiệu” đang hard-code trong view. Con số này sẽ sai ngay khi catalog thay đổi.

### 2.5. Catalog thưởng

18 phần thưởng đang được khai báo lặp lại trong `SeedDataInitializer` và `KidsController`. `RuleJson` mang tên JSON nhưng hiện lưu chuỗi mô tả tiếng Việt, nên không thể dùng làm rule engine.

Catalog nên có một nguồn duy nhất. Việc đánh giá điều kiện phải dựa trên dữ liệu có cấu trúc, không dựa trên `Code` và các `if` rời rạc trong controller.

### 2.6. Chức năng quản trị xóa tiến độ của bé

Action `POST /admin/kids/{id}/clear-progress` hiện xóa:

- `QuestionAttempts` thuộc các lượt làm của bé.
- `LearningAttempts`.
- `LearningSessions`.
- `SkillProgress`.
- `ChildRewards`.

Như vậy phần huy hiệu/quà trong `ChildRewards` đã được xóa, nhưng `GardenItems` chưa được xóa. Sau reset có thể xuất hiện trạng thái không nhất quán: bé không còn sở hữu quà nhưng vật phẩm cũ vẫn đang được đặt trong vườn.

Sau khi bổ sung `ChildLessonProgress`, `RewardGrant` và inventory theo thiết kế mới, nếu action reset không được nâng cấp cùng lúc thì rương/quà cũ sẽ tự xuất hiện lại ngay cả khi `LearningAttempts` đã bị xóa. Đây là hạng mục bắt buộc, không phải cải tiến tùy chọn.

Các action xóa hồ sơ bé và xóa tài khoản phụ huynh cũng đang xóa `ChildRewards`, nhưng danh sách xóa thủ công phải được cập nhật khi thêm bất kỳ bảng tiến độ/phần thưởng mới nào.

## 3. Nguyên tắc sản phẩm đề xuất

### 3.1. Mỗi 10 bài duy nhất trong một nhóm = một rương

Mốc rương được tính **riêng từng nhóm kỹ năng**, không gộp chung toàn hệ thống:

```text
Số bài hoàn thành lần đầu trong nhóm = distinct LearningItemId có EverCompleted = true
Số rương đã đủ điều kiện              = floor(số bài duy nhất / 10)
Mốc kế tiếp                               = (số rương đã đủ điều kiện + 1) * 10
Tiến độ trong chặng hiện tại               = số bài duy nhất % 10
```

Ví dụ:

| Nhóm | Bài duy nhất đã xong | Kết quả |
|---|---:|---|
| Chữ cái | 9 | Rương mốc 10 còn khóa, hiển thị 9/10 |
| Chữ cái | 10 | Rương mốc 10 chuyển sang có thể nhận |
| Chữ cái | 23 | Rương 10 và 20 đã đủ điều kiện; rương 30 hiển 3/10 |
| Tư duy logic | 7 | Tiến độ độc lập 7/10, không bị ảnh hưởng bởi Chữ cái |

Nếu một nhóm có phần dư dưới 10 bài ở cuối hành trình, không tạo thêm “rương 10 bài” sai quy tắc. Thay vào đó có thể cấp **huy hiệu hoàn thành nhóm** khi đạt 100%; đây là loại thành tích khác với rương chặng.

### 3.2. Học lại không làm mất tiến độ và không sinh quà trùng

Cần lưu hai khái niệm riêng:

- `EverCompleted`: bé đã từng vượt bài; một khi `true` thì không quay lại `false`.
- `LatestAttemptStatus`: kết quả lần luyện gần nhất; có thể là `needs_practice` mà không khóa lại map.

Quy tắc học lại:

| Tình huống | Tiến độ rương | Quà mốc | Sao thành tích | Dữ liệu khác |
|---|---|---|---|---|
| Hoàn thành bài lần đầu | +1 bài duy nhất | Có thể mở nếu chạm mốc 10 | Ghi nhận `BestStars` | Tạo `FirstCompletedAt` |
| Học lại và làm đúng | Không đổi | Không cấp lại | Chỉ nâng `BestStars` nếu cao hơn | Tăng `ReviewCount` |
| Học lại và làm sai | Không đổi | Không thu hồi | Không trừ | Cập nhật `LatestAttemptStatus` |
| Refresh/double submit | Không đổi | Không trùng | Không trùng | Unique index chặn lặp |

Có thể thưởng nhẹ cho việc ôn tập bằng “điểm luyện tập” hoặc streak riêng, nhưng không nên dùng cùng đơn vị với sao thành tích và không được làm tiến độ rương tăng. Nếu triển khai, nên giới hạn mỗi bài một lần/ngày để tránh farm.

### 3.3. Rương là node riêng

Sau bài 10, 20, 30... view chèn một node rương, không thay thế node bài học. Mỗi node có ba trạng thái:

- `locked`: chưa đủ 10 bài; hiển `7/10` và số bài còn lại.
- `claimable`: đã đủ điều kiện; rương phát sáng, nút “Mở quà” có thể bấm.
- `claimed`: giữ nguyên trên map, hiển quà đã nhận và ngày mở.

Việc giữ node `claimed` là quan trọng: bé nhìn thấy hành trình đã chinh phục, thay vì rương biến mất sau khi nhận.

## 4. Thiết kế độ phủ cho 10 nhóm

Tất cả nhóm active đều phải tham gia cơ chế, không hard-code chỉ một số nhóm như rule huy hiệu hiện tại.

| Nhóm kỹ năng | Chủ đề quà gợi ý | Huy hiệu hoàn thành nhóm |
|---|---|---|
| Chữ cái | Sách, bút, thẻ chữ | Ngôi Sao Chữ Cái |
| Chữ số | Khối số, la bàn số | Nhà Thám Hiểm Con Số |
| Số lượng và toán học | Que tính, vương miện toán | Nhà Toán Học Nhí |
| Tư duy logic | Mảnh ghép, kính lúp | Thám Tử Thông Minh |
| Kỹ năng sống và cảm xúc | Trái tim, huy hiệu tự lập | Bé Ngoan Tự Lập |
| Tiền tập đọc và ngôn ngữ | Sách truyện, micro | Nhà Kể Chuyện Nhí |
| Hình dạng và không gian | Khối hình, bản đồ | Kiến Trúc Sư Tí Hon |
| Ghi nhớ và tập trung | Pha lê ký ức, đồng hồ | Siêu Sao Tập Trung |
| Vận động tinh | Bút màu, bàn tay khéo | Bàn Tay Khéo Léo |
| Khám phá thế giới | Tên lửa, kính viễn vọng | Nhà Khám Phá Nhí |

Danh sách nhóm phải đọc động từ `SkillGroups`. Bảng trên chỉ là theme nội dung; không được dùng là danh sách logic cố định. Khi quản trị thêm nhóm mới, hệ thống vẫn tự sinh mốc 10 bài cho nhóm đó.

### Phân phối quà

Không nên random ngay lúc render vì refresh có thể đổi quà. Chọn quà theo một trong hai cách:

1. Quản trị cấu hình `RewardPool` cho từng nhóm và mốc; đây là phương án nên dùng lâu dài.
2. Giai đoạn MVP dùng phân phối xác định theo `(SkillGroupId, MilestoneNumber)`, nhưng phải chốt `RewardDefinitionId` ngay khi tạo quyền nhận quà.

Mỗi chặng nên có một phần thưởng chính; mốc lớn 30/50/100 có thể thêm huy hiệu hiếm, nhưng không nên cấp nhiều popup cùng lúc.

## 5. Mô hình dữ liệu đề xuất

### 5.1. `ChildLessonProgress`

Một dòng cho mỗi cặp bé-bài học:

| Trường | Ý nghĩa |
|---|---|
| `Id` | Khóa chính |
| `ChildProfileId` | Bé |
| `LearningItemId` | Bài học |
| `FirstCompletedAt` | Lần đầu vượt bài; nullable |
| `LastAttemptedAt` | Lần làm gần nhất |
| `LatestStatus` | Kết quả gần nhất |
| `BestStars` | Sao cao nhất của bài, không phải tổng mọi lần |
| `AttemptCount` | Tổng lượt làm |
| `ReviewCount` | Số lần học lại sau khi đã vượt |

Unique index bắt buộc: `(ChildProfileId, LearningItemId)`.

`LearningAttempts` vẫn giữ nguyên là nhật ký chi tiết; `ChildLessonProgress` là snapshot để tính map và phần thưởng đúng, nhanh.

### 5.2. `RewardGrant`

Biểu diễn một quyền nhận quà cụ thể:

| Trường | Ý nghĩa |
|---|---|
| `Id` | Khóa chính |
| `ChildProfileId` | Bé sở hữu quyền nhận |
| `RewardDefinitionId` | Quà đã được chốt |
| `SourceType` | `skill_milestone`, `group_completion`, `streak`, `special` |
| `SourceKey` | Ví dụ `skill:1111...:milestone:20` |
| `SkillGroupId` | Nhóm nguồn, nullable với quà chung |
| `MilestoneValue` | 10, 20, 30... |
| `State` | `claimable` hoặc `claimed` |
| `UnlockedAt` | Lúc đủ điều kiện |
| `ClaimedAt` | Lúc bé mở rương |

Unique index bắt buộc: `(ChildProfileId, SourceType, SourceKey)`.

Khóa này giải quyết triệt để việc học lại, refresh và request cạnh tranh. `ChildReward` hiện tại có thể được thay bằng `RewardGrant`, hoặc giữ là bảng sở hữu và thêm bảng grant riêng. Phương án bảng grant riêng rõ nghiệp vụ hơn.

### 5.3. Kho vật phẩm

Nếu một vật phẩm có thể nhận nhiều lần, cần `ChildInventoryItem` với `Quantity`. Huy hiệu thì unique; vật phẩm trang trí có thể có số lượng.

`GardenItem` chỉ nên ghi vị trí đã đặt. Khi bé đặt hoặc gỡ vật phẩm, phải kiểm tra số lượng đang sở hữu trừ số lượng đang được đặt.

### 5.4. Rule catalog có cấu trúc

Thay `RuleJson` dạng văn bản bằng JSON hợp lệ, ví dụ:

```json
{
  "type": "skill_unique_completions",
  "threshold": 10,
  "skillGroupCode": "chu-cai",
  "displayText": "Hoàn thành 10 bài Chữ cái khác nhau"
}
```

Mô tả hiển thị không nên kiêm nhiệm quy tắc tính toán. Tốt hơn là tách `RuleType`, `Threshold`, `ScopeCode`, `DisplayDescription` thành cột; nếu giữ JSON thì phải có parser và validation trong trang quản trị.

## 6. Luồng nghiệp vụ đề xuất

### 6.1. Khi nộp bài

```text
POST nộp bài
  -> ghi LearningAttempt + QuestionAttempt
  -> upsert ChildLessonProgress
  -> nếu đây là lần chuyển EverCompleted false -> true
       -> đếm unique completion trong SkillGroup
       -> xác định mốc 10 vừa vượt
       -> INSERT RewardGrant claimable với SourceKey duy nhất
  -> commit cùng một transaction
  -> trả về thông tin NewMilestone (nếu có)
```

Việc tạo `RewardGrant` phải cùng transaction với hoàn thành lần đầu. Không nên để controller tự tính bằng nhiều truy vấn rời rạc.

### 6.2. Khi mở rương

Endpoint đề xuất:

```text
POST /kids/rewards/grants/{grantId}/claim
```

Yêu cầu:

- Có antiforgery token.
- `grantId` phải thuộc bé đang được chọn trong session.
- Chỉ cho chuyển `claimable -> claimed`.
- Nếu request lặp lại, trả kết quả idempotent “đã nhận”, không cộng kho lần hai.
- Cập nhật grant và inventory trong cùng transaction.
- Không thay đổi dữ liệu bằng GET.

### 6.3. Khi mở map

Controller trả về các danh sách rõ ràng:

- `EverCompletedItemIds`.
- `LatestAttemptByItemId` để gợi ý ôn tập, không dùng để khóa tiến trình.
- `CurrentItemId` = bài đầu tiên chưa từng hoàn thành theo thứ tự.
- `Milestones` gồm mốc, trạng thái, grant và reward.

Không nên để Razor view tự suy ra nghiệp vụ bằng so sánh index.

### 6.4. Khi mở Kho quà

Trang kho quà chỉ đọc dữ liệu. Mọi phần thưởng vừa đủ điều kiện đã được ghi thành `RewardGrant` từ thời điểm hoàn thành bài, nên không cần “sửa dữ liệu khi xem trang” như `EnsureDefaultRewardsExistAsync` hiện nay.

### 6.5. Khi quản trị xóa tiến độ học của bé

Quy ước sản phẩm đề xuất: **xóa tiến độ học = đưa bé về trạng thái chưa từng học và chưa từng nhận quà**. Không giữ lại rương, huy hiệu, vật phẩm hay trang trí cũ.

Ma trận dữ liệu:

| Dữ liệu | Xử lý khi clear progress một bé | Lý do |
|---|---|---|
| `QuestionAttempts` | Xóa | Chi tiết kết quả học cũ |
| `LearningAttempts` | Xóa | Nguồn lịch sử bài đã làm và sao cũ |
| `LearningSessions` | Xóa | Hành trình/ngày học cũ |
| `SkillProgress` | Xóa | Snapshot tiến độ nhóm cũ |
| `ChildLessonProgress` mới | Xóa | Nếu giữ lại, map vẫn coi bài đã hoàn thành |
| `RewardGrant` mới | Xóa cả `claimable` và `claimed` | Nếu giữ lại, rương cũ vẫn tồn tại |
| `ChildRewards` hiện tại | Xóa | Xóa quyền sở hữu huy hiệu/quà cũ |
| `ChildInventoryItems` mới | Xóa | Kho vật phẩm phải trở về rỗng |
| `GardenItems` | Xóa | Không để vật phẩm đã đặt tồn tại khi kho đã rỗng |
| `RewardDefinitions` | **Giữ** | Đây là catalog dùng chung cho mọi bé |
| `LearningItems`, `Questions` | **Giữ** | Đây là nội dung bài học dùng chung |
| `ChildProfile` | **Giữ** | Clear progress không phải xóa hồ sơ bé |
| `AuditLogs` | Giữ và ghi thêm sự kiện reset | Cần truy vết thao tác quản trị |

Luồng xóa phải nằm trong một transaction:

```text
Admin xác nhận reset
  -> kiểm tra bé tồn tại
  -> khóa/phân phiên thao tác reset cho ChildProfileId
  -> xóa GardenItems và inventory
  -> xóa RewardGrant và ChildRewards
  -> xóa ChildLessonProgress và SkillProgress
  -> xóa QuestionAttempts, LearningAttempts, LearningSessions
  -> ghi AuditLog với số bản ghi đã xóa
  -> commit
```

Nếu bất kỳ bước nào lỗi, toàn bộ transaction phải rollback. Không được có trạng thái “đã xóa bài học nhưng còn quà” hoặc ngược lại.

Sau reset, request của bé có thể vẫn mang `CurrentLearningSessionId` cũ trong cookie/session. Mọi luồng Kids phải xử lý session id không còn tồn tại bằng cách tạo phiên mới; không được khôi phục tiến độ hoặc grant từ payload cũ.

#### Chống dữ liệu quay lại do request đang chạy

Có thể xảy ra tình huống bé đang nộp bài đúng lúc admin reset. Chỉ xóa nhiều bảng trong transaction chưa đủ để ngăn request cũ commit sau thao tác reset.

Khuyến nghị bổ sung `ProgressEpoch` trên `ChildProfile`:

- Mỗi attempt/session/progress/grant ghi epoch hiện tại của bé.
- Clear progress tăng `ProgressEpoch` trước khi xóa dữ liệu.
- Request học chỉ được commit nếu epoch lúc nộp vẫn bằng epoch hiện tại.
- Request cũ bị từ chối và chuyển bé về map đã reset.

Đây là phương án chắc chắn hơn việc cố gắng xóa session trong bộ nhớ, vì admin và bé có thể ở hai trình duyệt hoặc hai thiết bị khác nhau.

#### Phân biệt ba thao tác xóa

- **Clear progress của bé:** giữ `ChildProfile`, xóa toàn bộ dữ liệu học và quà thuộc bé.
- **Xóa hồ sơ bé/phụ huynh:** xóa toàn bộ các bảng trên rồi xóa profile/account; nên dùng quan hệ cascade có kiểm soát hoặc một service xóa chung.
- **Xóa một bài học khỏi catalog:** không tự động thu hồi quà bé đã nhận; grant lịch sử vẫn được giữ. Nếu muốn tính lại mốc thì phải là một nghiệp vụ quản trị riêng, có preview ảnh hưởng.

UI xác nhận clear progress phải nói rõ: “Thao tác sẽ xóa lịch sử học, sao, tiến độ map, rương chờ mở, quà đã nhận và vật phẩm đã trang trí của bé; hồ sơ bé vẫn được giữ.”

## 7. Thiết kế giao diện

Giữ nguyên ngôn ngữ hình ảnh hiện tại: `kid-screen`, `child-zone-header`, `clay-card`, `clay-button`, màu pastel theo nhóm, mascot Sóc Nâu, Material Symbols và bottom navigation.

### 7.1. Trên map

- Chèn rương sau mỗi 10 node bài học.
- Rương khóa hiển “Còn 3 bài nữa” thay vì chỉ rung và báo bị khóa.
- Rương `claimable` có halo nhẹ, animation có thể tắt theo `prefers-reduced-motion`, CTA tối thiểu 44px.
- Rương `claimed` hiển icon quà thật và check badge.
- Sau khi chạm mốc, trang hoàn thành hiển celebration ngắn và nút “Mở rương”; không chặn nút “Bài tiếp theo”.

### 7.2. Trên `/kids/rewards`

Cấu trúc đề xuất:

1. Hero: tổng huy hiệu, vật phẩm, rương chờ mở; không hard-code tổng số.
2. Khu “Quà đang chờ bé”: ưu tiên các grant `claimable`.
3. “Hành trình 10 nhóm”: mỗi card có màu/icon nhóm, `x/10`, mốc kế tiếp và link quay lại map đúng nhóm.
4. Bộ sưu tập: tách Huy hiệu, Vật phẩm, Đã sở hữu.
5. Lịch sử gần đây: tối đa 5 mốc để trang không quá dài.

### 7.3. Phản hồi tạo động lực

- Phản hồi tập trung vào nỗ lực: “Con đã mở khóa rương 10 bài Chữ cái!”.
- Cho xem trước loại quà hoặc silhouette để tạo mục tiêu; không dùng cơ chế ngẫu nhiên mang cảm giác cờ bạc.
- Không thu hồi quà, không làm mất sao và không phạt khi bé luyện sai.
- Âm thanh celebration tôn trọ `SoundEnabled`; animation tôn trọ reduced motion.

## 8. Migration dữ liệu hiện có

Migration phải bảo toàn lịch sử, không xóa `LearningAttempts` hay `ChildRewards`.

### Bước backfill

1. Tạo `ChildLessonProgress` từ toàn bộ `LearningAttempts`, group theo `(ChildProfileId, LearningItemId)`.
2. `FirstCompletedAt` = thời điểm sớm nhất có `Status = completed`.
3. `BestStars` = `MAX(StarsEarned)`, giới hạn theo thang sao hợp lệ.
4. `LatestStatus` = status của attempt mới nhất.
5. `AttemptCount` = tổng attempt; `ReviewCount` = attempt sau `FirstCompletedAt`.
6. Với từng bé và nhóm, tính số bài unique đã xong và tạo grant cho các mốc 10, 20... đã đạt.
7. Grant backfill nên ở trạng thái `claimable` để bé vẫn có trải nghiệm mở quà. Nếu `ChildReward` cũ đã thể hiện cùng một mốc, đánh dấu `claimed` để không cấp trùng.
8. Không quy đổi `totalStars` cũ một cách âm thầm. UI có thể giữ “tổng sao lịch sử”, đồng thời dùng `SUM(BestStars)` làm “sao thành tích” cho rule mới.

Migration và backfill phải idempotent, có unique index trước khi bật service cấp thưởng mới.

## 9. Kiến trúc triển khai đề xuất

Tách nghiệp vụ khỏi `KidsController`:

- `LearningProgressService`: ghi snapshot first completion, best score, latest attempt.
- `RewardProgressionService`: phát hiện mốc và tạo grant idempotent.
- `RewardClaimService`: claim grant và cập nhật inventory.
- `ChildDataResetService`: xóa đồng bộ mọi dữ liệu học/quà của một bé và ghi audit.
- `RewardCatalog`: nguồn duy nhất cho seed/cấu hình quà.

Controller chỉ điều phối request/response. Cả answer thường và tracing phải gọi cùng một API hoàn thành bài, tránh hai luồng có quy tắc thưởng khác nhau.

Nên có một `CompletionResult` chung:

```csharp
public sealed record CompletionResult(
    bool IsFirstCompletion,
    int BestStars,
    int UniqueCompletedInSkill,
    RewardGrant? NewlyUnlockedGrant);
```

## 10. Thứ tự triển khai

### Giai đoạn 1 — Nền dữ liệu và tính đúng

- Thêm `ChildLessonProgress`, `RewardGrant`, unique indexes và migration backfill.
- Sửa map dựa trên `EverCompleted`, không dựa trên attempt gần nhất/index suy diễn.
- Tính sao thành tích bằng best result theo bài.
- Tách catalog thưởng khỏi controller.
- Nâng cấp service clear progress để xóa đồng bộ progress, grant, quà, inventory và garden trong một transaction.
- Bổ sung cơ chế epoch hoặc concurrency tương đương để request học cũ không ghi dữ liệu trở lại sau reset.

### Giai đoạn 2 — Rương mỗi 10 bài

- Tạo `RewardProgressionService` và gọi ngay trong luồng nộp bài.
- Chèn node rương riêng sau bài 10/20/30...
- Thêm ba trạng thái rương và endpoint claim idempotent.
- Hiển thị celebration tại thời điểm chạm mốc.

### Giai đoạn 3 — Nâng cấp Kho quà

- Thêm danh sách quà chờ mở và card tiến độ theo tất cả nhóm active.
- Thêm nguồn gốc và lịch sử quà.
- Loại bỏ con số hard-code 18.
- Bổ sung empty state và responsive state cho iPad/mobile.

### Giai đoạn 4 — Kho vật phẩm có tác dụng

- Thêm inventory quantity.
- Nối vật phẩm với khu vườn/trang trí.
- Cho phụ huynh xem thành tích nhưng không can thiệp nhận quà của bé, trừ khi có yêu cầu sản phẩm khác.

## 11. Tiêu chí chấp nhận

### Tiến độ và học lại

- Hoàn thành 9 bài khác nhau trong một nhóm: chưa có grant mốc 10.
- Hoàn thành bài duy nhất thứ 10: tạo chính xác một grant mốc 10.
- Làm lại bất kỳ bài nào 20 lần: số unique completion và số rương không đổi.
- Làm sai khi ôn bài đã qua: trạm vẫn completed; chỉ hiển gợi ý cần ôn.
- Hoàn thành bài ở nhóm B không làm tiến độ rương nhóm A thay đổi.

### Tính duy nhất và an toàn

- Double-click nộp bài hoặc claim chỉ tạo một progress/grant/inventory increment.
- Bé A không thể claim grant của bé B bằng cách sửa URL.
- Refresh trang claim không nhận lại quà.
- Archive/xóa mềm bài học không xóa quà bé đã nhận.
- Clear progress của bé A xóa toàn bộ tiến độ, sao, rương, quà, inventory và garden của A, nhưng không ảnh hưởng bé B.
- Sau clear progress, `/kids/rewards` của bé trở về trạng thái chưa có quà và mọi map trở về trạm đầu tiên.
- Request nộp bài được tạo trước reset nhưng commit sau reset không thể khôi phục attempt, progress hoặc grant cũ.

### Giao diện

- Rương là node riêng sau mỗi 10 bài, không che hoặc thay thế bài thứ 10.
- Cả ba state locked/claimable/claimed đều dễ phân biệt mà không chỉ dựa vào màu.
- Kho quà hiển thị đủ mọi nhóm active và tiến độ tính từ database.
- Không có tổng số phần thưởng hard-code.
- Giao diện dùng lại design token/class của Kids Zone và hoạt động ổn định trên kích thước iPad/mobile.

## 12. Kiểm thử bắt buộc

- Unit test các mốc 0, 1, 9, 10, 11, 19, 20 và học lại.
- Integration test transaction nộp bài + progress + grant.
- Integration test hai request cùng hoàn thành bài thứ 10.
- Authorization test claim chéo giữa hai bé.
- Migration test với bé chưa học, bé có attempt trùng và bé đã có `ChildReward`.
- Integration test clear progress khi bé có mốc locked, grant claimable/claimed, huy hiệu, inventory và `GardenItems`.
- Concurrency test clear progress đúng lúc bé nộp bài hoặc claim rương.
- Isolation test reset một bé trong gia đình có nhiều bé.
- Regression test xóa hồ sơ bé và xóa phụ huynh sau khi thêm các bảng reward mới.
- UI test rương ở map dài, nhóm có ít hơn 10 bài, nhóm có số bài không chia hết cho 10.
- Regression test luồng answer thường, tracing, Today, Skill, Summary và Rewards.

## 13. Các quyết định nên chốt trước khi lập trình

1. Bé tự bấm mở rương hay hệ thống tự mở? Khuyến nghị: tạo grant tự động, bé tự bấm mở.
2. Vật phẩm có thể nhận trùng và có quantity không? Khuyến nghị: có quantity; huy hiệu thì unique.
3. Quà mốc được xem trước hay là bất ngờ? Khuyến nghị: cho xem chủ đề/silhouette, không random kiểu loot box.
4. Sao luyện lại có giữ trong tổng lịch sử không? Khuyến nghị: giữ để không làm bé mất thành quả cũ, nhưng rule mới dùng `BestStars` theo bài.
5. Mốc hoàn thành nhóm có quà riêng không? Khuyến nghị: có một huy hiệu duy nhất, tách khỏi rương mỗi 10 bài.

## 14. Phạm vi MVP khuyến nghị

MVP đủ để chức năng có giá trị thật gồm:

- Progress duy nhất theo bài.
- Rương riêng theo từng nhóm, mỗi 10 bài.
- Claim chính xác một lần.
- Map có ba state rương.
- Rewards có quà chờ mở và tiến độ tất cả nhóm.
- Học lại không tăng tiến độ rương, không khóa lại trạm.
- Backfill an toàn cho dữ liệu hiện có.

Khu vườn trang trí, marketplace, random reward và hệ kinh tế phức tạp không nên nằm trong MVP. Trước hết cần làm cho mối liên hệ **học bài -> tiến gần rương -> mở rương -> thấy quà trong kho** trở nên rõ ràng, đúng dữ liệu và nhất quán.

## 15. Tham chiếu source đã rà soát

- `Controllers/KidsController.cs`: actions Skill, Answer, CompleteTracing, Summary, Rewards và `EvaluateAndAwardBadgesAsync`.
- `Controllers/AdminController.cs`: `ClearChildProgress`, `DeleteChildProfile`, `DeleteParentAccount` và phạm vi dữ liệu hiện đang xóa.
- `Application/Learning/TodayLessonService.cs`: session, tiến độ roadmap và kế hoạch 12 bài/ngày.
- `Views/Kids/Skill.cshtml`: cách suy diễn trạm và rương mỗi 5 bài.
- `Views/Kids/Rewards.cshtml`: collection grid và nội dung hard-code.
- `Views/Kids/Summary.cshtml`: hiển thị quà vừa mở.
- `Models/LearningAttempt.cs`, `ChildReward.cs`, `RewardDefinition.cs`, `GardenItem.cs`, `SkillProgress.cs`.
- `Data/ApplicationDbContext.cs`: indexes và quan hệ database hiện có.
- `Data/SeedDataInitializer.cs`: catalog phần thưởng mặc định.
