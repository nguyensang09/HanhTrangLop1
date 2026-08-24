namespace HanhTrangLop1.Models.ViewModels;

public class TodayLessonViewModel
{
    public ChildProfile ChildProfile { get; set; } = new();
    public LearningSession Session { get; set; } = new();
    public int CurrentDayNumber { get; set; } = 1;
    public int SelectedDayNumber { get; set; } = 1;
    public string DayThemeTitle { get; set; } = "Khởi đầu hành trình";
    public IReadOnlyList<TodayLessonStepViewModel> Steps { get; set; } = [];
    public IReadOnlyList<DailyRoadmapItemViewModel> RoadmapDays { get; set; } = [];
    public int CompletedCount => Steps.Count(x => x.Status == TodayLessonStepStatus.Completed);
    public int TotalCount => Steps.Count;
}

public class DailyRoadmapItemViewModel
{
    public int DayNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconKey { get; set; } = "school";
    public string ColorHex { get; set; } = "#ff7a00";
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsLocked { get; set; }
    public int StarsEarned { get; set; }
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
