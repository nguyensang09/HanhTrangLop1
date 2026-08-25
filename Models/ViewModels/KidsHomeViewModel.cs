namespace HanhTrangLop1.Models.ViewModels;

public class KidsHomeViewModel
{
    public ChildProfile? ChildProfile { get; set; }
    public IReadOnlyList<SkillGroup> SkillGroups { get; set; } = [];
    public IReadOnlyList<LearningItem> TodayItems { get; set; } = [];
    public int Stars { get; set; } = 125;
}

public class KidsTracingHubViewModel
{
    public ChildProfile? ChildProfile { get; set; }
    public int TotalCount => BasicStrokes.Count + UppercaseLetters.Count + LowercaseLetters.Count + Numbers.Count;
    public int CompletedCount => BasicStrokes.Count(x => x.IsCompleted) +
                                 UppercaseLetters.Count(x => x.IsCompleted) +
                                 LowercaseLetters.Count(x => x.IsCompleted) +
                                 Numbers.Count(x => x.IsCompleted);

    public IReadOnlyList<KidsTracingItemViewModel> BasicStrokes { get; set; } = [];
    public IReadOnlyList<KidsTracingItemViewModel> UppercaseLetters { get; set; } = [];
    public IReadOnlyList<KidsTracingItemViewModel> LowercaseLetters { get; set; } = [];
    public IReadOnlyList<KidsTracingItemViewModel> Numbers { get; set; } = [];
    public string ActiveTab { get; set; } = "all";
}

public class KidsTracingItemViewModel
{
    public LearningItem Item { get; set; } = null!;
    public string Symbol { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty; // basic, upper, lower, number
    public bool IsCompleted { get; set; }
    public int StarsEarned { get; set; }
}
