namespace HanhTrangLop1.Models;

public class LearningSession
{
    public Guid Id { get; set; }

    public Guid ChildProfileId { get; set; }

    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? EndedAt { get; set; }

    public int PlannedMinutes { get; set; } = 15;

    public int ActualSeconds { get; set; }

    public string Status { get; set; } = "active";

    public string SessionPlanJson { get; set; } = "[]";

    public ChildProfile? ChildProfile { get; set; }
}
