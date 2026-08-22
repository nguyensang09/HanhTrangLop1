namespace HanhTrangLop1.Models;

public class LearningAttempt
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public Guid ChildProfileId { get; set; }

    public Guid LearningItemId { get; set; }

    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; set; }

    public string Status { get; set; } = "started";

    public int ScoreInternal { get; set; }

    public int StarsEarned { get; set; }

    public int HintsUsed { get; set; }

    public int MistakeCount { get; set; }

    public int DurationSeconds { get; set; }

    public string DeviceInputType { get; set; } = "touch";

    public LearningSession? Session { get; set; }

    public ChildProfile? ChildProfile { get; set; }

    public LearningItem? LearningItem { get; set; }
}
