namespace HanhTrangLop1.Models.ViewModels;

public class KidsRewardsViewModel
{
    public ChildProfile? ChildProfile { get; set; }
    public int TotalStars { get; set; }
    public List<RewardItemViewModel> Badges { get; set; } = [];
    public List<RewardGrantItemViewModel> ClaimableGrants { get; set; } = [];
    public List<RewardGrantItemViewModel> RecentClaimedGrants { get; set; } = [];
    public List<SkillRewardProgressViewModel> SkillProgress { get; set; } = [];
}

public class RewardGrantItemViewModel
{
    public RewardGrant Grant { get; set; } = null!;
    public RewardDefinition Reward { get; set; } = null!;
    public string SkillName { get; set; } = string.Empty;
}

public class SkillRewardProgressViewModel
{
    public Guid SkillGroupId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string IconKey { get; set; } = "school";
    public string Color { get; set; } = "#ff8542";
    public int UniqueCompleted { get; set; }
    public int TotalLessons { get; set; }
    public int CurrentSegmentProgress { get; set; }
    public int NextMilestone { get; set; }
}

public class RewardItemViewModel
{
    public RewardDefinition Definition { get; set; } = null!;
    public bool IsEarned { get; set; }
    public DateTimeOffset? EarnedAt { get; set; }
}
