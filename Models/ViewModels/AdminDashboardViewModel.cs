namespace HanhTrangLop1.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalActivities { get; set; }
    public int PublishedActivities { get; set; }
    public int DraftActivities { get; set; }
    public int SkillGroups { get; set; }
    public IReadOnlyList<LearningItem> RecentItems { get; set; } = [];
}
