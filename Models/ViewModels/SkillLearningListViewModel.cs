namespace HanhTrangLop1.Models.ViewModels;

public class SkillLearningListViewModel
{
    public ChildProfile? ChildProfile { get; set; }
    public SkillGroup SkillGroup { get; set; } = new();
    public IReadOnlyList<SkillLearningItemViewModel> Items { get; set; } = [];
    public Guid? LastPracticedItemId { get; set; }
    public Guid? CurrentItemId { get; set; }
    public IReadOnlyList<SkillMilestoneViewModel> Milestones { get; set; } = [];
    public int CompletedCount => Items.Count(x => x.EverCompleted);
}

public class SkillLearningItemViewModel
{
    public LearningItem Item { get; set; } = new();
    public string? LatestStatus { get; set; }
    public int StarsEarned { get; set; }
    public bool EverCompleted { get; set; }
}

public class SkillMilestoneViewModel
{
    public int MilestoneValue { get; set; }
    public string State { get; set; } = "locked";
    public Guid? GrantId { get; set; }
    public string RewardName { get; set; } = string.Empty;
    public string RewardIconKey { get; set; } = "redeem";
}
