namespace HanhTrangLop1.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalActivities { get; set; }
    public int PublishedActivities { get; set; }
    public int DraftActivities { get; set; }
    public int ReviewActivities { get; set; }
    public int ArchivedActivities { get; set; }
    public int SkillGroups { get; set; }
    public int TotalChildren { get; set; }
    public int TotalAttempts { get; set; }
    public int TotalParents { get; set; }
    public IReadOnlyList<LearningItem> RecentItems { get; set; } = [];
    public IReadOnlyList<LearningAttempt> RecentAttempts { get; set; } = [];
}

public class AdminLearningItemListViewModel
{
    public string? Status { get; set; }
    public string? InteractionType { get; set; }
    public IReadOnlyList<LearningItem> Items { get; set; } = [];
}

public class EditLearningItemViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid SkillGroupId { get; set; }
    public Guid? TopicId { get; set; }
    public byte Level { get; set; } = 1;
    public int EstimatedMinutes { get; set; } = 5;
    public string InstructionText { get; set; } = string.Empty;
    public string PromptText { get; set; } = string.Empty;
    public string HintText { get; set; } = string.Empty;
    public string CorrectFeedback { get; set; } = string.Empty;
    public string RetryFeedback { get; set; } = string.Empty;
}

public class CreateTracingItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public Guid SkillGroupId { get; set; }
    public Guid? TopicId { get; set; }
    public string Symbol { get; set; } = "A";
    public string InstructionText { get; set; } = string.Empty;
    public string PromptText { get; set; } = string.Empty;
    public int MinPoints { get; set; } = 20;
    public byte Level { get; set; } = 1;
}
