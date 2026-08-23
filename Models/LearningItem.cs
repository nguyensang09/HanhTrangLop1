using System.ComponentModel.DataAnnotations;

namespace HanhTrangLop1.Models;

public class LearningItem
{
    public Guid Id { get; set; }

    [Required, MaxLength(80)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public Guid SkillGroupId { get; set; }

    public Guid? TopicId { get; set; }

    public byte Level { get; set; } = 1;

    public int SortOrder { get; set; }

    [Required, MaxLength(50)]
    public string InteractionType { get; set; } = InteractionTypes.SingleChoice;

    public int EstimatedMinutes { get; set; } = 5;

    [MaxLength(500)]
    public string InstructionText { get; set; } = string.Empty;

    public Guid? InstructionAudioAssetId { get; set; }

    public string ContentJson { get; set; } = "{}";

    [Required, MaxLength(30)]
    public string Status { get; set; } = ContentStatus.Draft;

    public int Version { get; set; } = 1;

    public DateTimeOffset? PublishedAt { get; set; }

    public string? CreatedByUserId { get; set; }

    public string? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public SkillGroup? SkillGroup { get; set; }

    public Topic? Topic { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
