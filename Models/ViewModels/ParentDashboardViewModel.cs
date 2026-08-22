namespace HanhTrangLop1.Models.ViewModels;

public class ParentDashboardViewModel
{
    public IReadOnlyList<ChildProfile> Children { get; set; } = [];
    public IReadOnlyList<SkillProgress> ProgressItems { get; set; } = [];
    public IReadOnlyList<LearningAttempt> RecentAttempts { get; set; } = [];
    public int TotalCompletedItems { get; set; }
    public int TotalNeedsPracticeItems { get; set; }
    public int TotalLearningMinutes { get; set; }
    public int TotalRewards { get; set; }
}
