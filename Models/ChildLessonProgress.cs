namespace HanhTrangLop1.Models;

public class ChildLessonProgress
{
    public Guid Id { get; set; }
    public Guid ChildProfileId { get; set; }
    public Guid LearningItemId { get; set; }
    /// <summary>Lần hoàn thành đầu tiên (lịch sử; dùng cho sao, quà, thống kê). Không bao giờ bị xóa khi học lại.</summary>
    public DateTimeOffset? FirstCompletedAt { get; set; }
    /// <summary>
    /// Hoàn thành trong lộ trình hiện tại: quyết định bài nào đã xong / đang học / còn khóa. Khi bé chọn học lại một
    /// bài cũ, giá trị này được xóa cho bài đó và mọi bài phía sau để bé học lại lần lượt từ vị trí mới.
    /// </summary>
    public DateTimeOffset? PathCompletedAt { get; set; }
    public DateTimeOffset LastAttemptedAt { get; set; } = DateTimeOffset.UtcNow;
    public string LatestStatus { get; set; } = "started";
    public int BestStars { get; set; }
    public int AttemptCount { get; set; }
    public int ReviewCount { get; set; }
    public int ProgressEpoch { get; set; }
    public ChildProfile? ChildProfile { get; set; }
    public LearningItem? LearningItem { get; set; }
}
