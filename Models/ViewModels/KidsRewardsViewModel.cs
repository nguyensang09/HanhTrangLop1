namespace HanhTrangLop1.Models.ViewModels;

public class KidsRewardsViewModel
{
    public ChildProfile? ChildProfile { get; set; }
    public int TotalStars { get; set; }
    public List<RewardItemViewModel> Badges { get; set; } = [];
}

public class RewardItemViewModel
{
    public RewardDefinition Definition { get; set; } = null!;
    public bool IsEarned { get; set; }
    public DateTimeOffset? EarnedAt { get; set; }
}
