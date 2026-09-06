namespace HanhTrangLop1.Models;

public class ChildLessonProgress
{
    public Guid Id { get; set; }
    public Guid ChildProfileId { get; set; }
    public Guid LearningItemId { get; set; }
    public DateTimeOffset? FirstCompletedAt { get; set; }
    public DateTimeOffset LastAttemptedAt { get; set; } = DateTimeOffset.UtcNow;
    public string LatestStatus { get; set; } = "started";
    public int BestStars { get; set; }
    public int AttemptCount { get; set; }
    public int ReviewCount { get; set; }
    public int ProgressEpoch { get; set; }
    public ChildProfile? ChildProfile { get; set; }
    public LearningItem? LearningItem { get; set; }
}
