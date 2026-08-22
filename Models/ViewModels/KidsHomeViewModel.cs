namespace HanhTrangLop1.Models.ViewModels;

public class KidsHomeViewModel
{
    public ChildProfile? ChildProfile { get; set; }
    public IReadOnlyList<SkillGroup> SkillGroups { get; set; } = [];
    public IReadOnlyList<LearningItem> TodayItems { get; set; } = [];
    public int Stars { get; set; } = 125;
}
