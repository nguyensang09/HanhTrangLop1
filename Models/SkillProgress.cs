namespace HanhTrangLop1.Models;

public class SkillProgress
{
    public Guid Id { get; set; }

    public Guid ChildProfileId { get; set; }

    public Guid SkillGroupId { get; set; }

    public decimal MasteryLevel { get; set; }

    public int CompletedItems { get; set; }

    public int NeedsPracticeItems { get; set; }

    public DateTimeOffset? LastPracticedAt { get; set; }

    public string SummaryJson { get; set; } = "{}";

    public ChildProfile? ChildProfile { get; set; }

    public SkillGroup? SkillGroup { get; set; }
}
