namespace HanhTrangLop1.Models.ViewModels;

public class TodayLessonViewModel
{
    public ChildProfile ChildProfile { get; set; } = new();
    public LearningSession Session { get; set; } = new();
    public IReadOnlyList<TodayLessonStepViewModel> Steps { get; set; } = [];
    public int CompletedCount => Steps.Count(x => x.Status == TodayLessonStepStatus.Completed);
    public int TotalCount => Steps.Count;
}

public class TodayLessonStepViewModel
{
    public LearningItem Item { get; set; } = new();
    public TodayLessonStepStatus Status { get; set; }
    public int StarsEarned { get; set; }
}

public enum TodayLessonStepStatus
{
    Completed,
    Active,
    NeedsPractice,
    Locked
}
