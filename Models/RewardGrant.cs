using System.ComponentModel.DataAnnotations;

namespace HanhTrangLop1.Models;

public class RewardGrant
{
    public Guid Id { get; set; }
    public Guid ChildProfileId { get; set; }
    public Guid RewardDefinitionId { get; set; }
    [Required, MaxLength(40)] public string SourceType { get; set; } = "skill_milestone";
    [Required, MaxLength(160)] public string SourceKey { get; set; } = string.Empty;
    public Guid? SkillGroupId { get; set; }
    public int MilestoneValue { get; set; }
    [Required, MaxLength(20)] public string State { get; set; } = RewardGrantStates.Claimable;
    public DateTimeOffset UnlockedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ClaimedAt { get; set; }
    public int ProgressEpoch { get; set; }
    public ChildProfile? ChildProfile { get; set; }
    public RewardDefinition? RewardDefinition { get; set; }
    public SkillGroup? SkillGroup { get; set; }
}

public static class RewardGrantStates
{
    public const string Claimable = "claimable";
    public const string Claimed = "claimed";
}
