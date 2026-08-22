namespace HanhTrangLop1.Models.ViewModels;

public class SkillLearningListViewModel
{
    public ChildProfile? ChildProfile { get; set; }
    public SkillGroup SkillGroup { get; set; } = new();
    public IReadOnlyList<SkillLearningItemViewModel> Items { get; set; } = [];
    public int CompletedCount => Items.Count(x => x.LatestStatus == "completed");
}

public class SkillLearningItemViewModel
{
    public LearningItem Item { get; set; } = new();
    public string? LatestStatus { get; set; }
    public int StarsEarned { get; set; }
}
