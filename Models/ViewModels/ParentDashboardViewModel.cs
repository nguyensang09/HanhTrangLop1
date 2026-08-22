namespace HanhTrangLop1.Models.ViewModels;

public class ParentDashboardViewModel
{
    public IReadOnlyList<ChildProfile> Children { get; set; } = [];
    public IReadOnlyList<ParentChildSummaryViewModel> ChildSummaries { get; set; } = [];
    public IReadOnlyList<SkillProgress> ProgressItems { get; set; } = [];
    public IReadOnlyList<LearningAttempt> RecentAttempts { get; set; } = [];
    public IReadOnlyList<ParentDailyActivityViewModel> DailyActivities { get; set; } = [];
    public int TotalCompletedItems { get; set; }
    public int TotalNeedsPracticeItems { get; set; }
    public int TotalLearningMinutes { get; set; }
    public int TotalRewards { get; set; }
}

public class ParentChildSummaryViewModel
{
    public ChildProfile Child { get; set; } = new();
    public int CompletedItems { get; set; }
    public int NeedsPracticeItems { get; set; }
    public int LearningMinutes { get; set; }
    public int StarsEarned { get; set; }
    public decimal AverageMastery { get; set; }
    public DateTimeOffset? LastLearnedAt { get; set; }
}

public class ParentSkillReportItemViewModel
{
    public string SkillName { get; set; } = string.Empty;
    public string IconKey { get; set; } = "auto_stories";
    public string Color { get; set; } = "#ff8542";
    public decimal MasteryLevel { get; set; }
    public int CompletedItems { get; set; }
    public int NeedsPracticeItems { get; set; }
    public DateTimeOffset? LastPracticedAt { get; set; }
}

public class ParentDailyActivityViewModel
{
    public DateTime Date { get; set; }
    public string DateLabel { get; set; } = string.Empty;
    public int CompletedItems { get; set; }
    public int NeedsPracticeItems { get; set; }
    public int LearningMinutes { get; set; }
}

public class ParentReportViewModel
{
    public ChildProfile Child { get; set; } = new();
    public IReadOnlyList<ParentSkillReportItemViewModel> SkillReports { get; set; } = [];
    public IReadOnlyList<ParentDailyActivityViewModel> DailyActivities { get; set; } = [];
    public IReadOnlyList<LearningAttempt> RecentAttempts { get; set; } = [];
    public int TotalCompletedItems { get; set; }
    public int TotalNeedsPracticeItems { get; set; }
    public int TotalLearningMinutes { get; set; }
    public int TotalStars { get; set; }
    public string RecommendationText { get; set; } = string.Empty;
}
