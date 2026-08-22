namespace HanhTrangLop1.Models;

public class ContentReview
{
    public Guid Id { get; set; }

    public Guid LearningItemId { get; set; }

    public string Status { get; set; } = ContentStatus.Review;

    public string? Note { get; set; }

    public string? ReviewerUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ReviewedAt { get; set; }

    public LearningItem? LearningItem { get; set; }
}
