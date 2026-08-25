using System.ComponentModel.DataAnnotations;
using HanhTrangLop1.Data;
using Microsoft.AspNetCore.Http;

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
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? InteractionType { get; set; }
    public Guid? SkillGroupId { get; set; }
    public Guid? TopicId { get; set; }
    public IReadOnlyList<SkillGroup> SkillGroups { get; set; } = [];
    public IReadOnlyList<Topic> Topics { get; set; } = [];
    public IReadOnlyList<LearningItem> Items { get; set; } = [];
    public IReadOnlyList<AdminLearningGroupTreeItem> TreeGroups { get; set; } = [];
    public int TotalGroups { get; set; }
    public int TotalTopics { get; set; }
    public int TotalItems { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
    public bool HasActiveFilter => !string.IsNullOrWhiteSpace(Search) || !string.IsNullOrWhiteSpace(Status) || !string.IsNullOrWhiteSpace(InteractionType) || SkillGroupId.HasValue || TopicId.HasValue;
}

public class AdminLearningGroupTreeItem
{
    public SkillGroup SkillGroup { get; set; } = new();
    public int LearningItemCount { get; set; }
    public IReadOnlyList<AdminLearningTopicTreeItem> Topics { get; set; } = [];
    public IReadOnlyList<LearningItem> DirectItems { get; set; } = [];
}

public class AdminLearningTopicTreeItem
{
    public Topic Topic { get; set; } = new();
    public int LearningItemCount { get; set; }
    public IReadOnlyList<LearningItem> Items { get; set; } = [];
    public IReadOnlyList<ActivityTemplateDefinition> AllowedTemplates { get; set; } = [];
    public bool AllowsTracing { get; set; }
}

public class EditLearningItemViewModel
{
    public Guid Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên bài."), MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    public Guid SkillGroupId { get; set; }
    public Guid? TopicId { get; set; }

    [Range(0, 100000, ErrorMessage = "Thứ tự bài học phải từ 0 đến 100000.")]
    public int SortOrder { get; set; }
    [Range(1, 3, ErrorMessage = "Độ khó phải từ 1 đến 3.")]
    public byte Level { get; set; } = 1;
    [Range(1, 30, ErrorMessage = "Thời lượng phải từ 1 đến 30 phút.")]
    public int EstimatedMinutes { get; set; } = 5;
    [Required(ErrorMessage = "Vui lòng nhập lời hướng dẫn."), MaxLength(500)]
    public string InstructionText { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập câu hỏi."), MaxLength(500)]
    public string PromptText { get; set; } = string.Empty;
    public string HintText { get; set; } = string.Empty;
    public string CorrectFeedback { get; set; } = string.Empty;
    public string RetryFeedback { get; set; } = string.Empty;
    public string InteractionType { get; set; } = InteractionTypes.SingleChoice;
    public string ChoiceA { get; set; } = string.Empty;
    public string ChoiceB { get; set; } = string.Empty;
    public string ChoiceC { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Symbol { get; set; } = "A";
    public int MinPoints { get; set; } = 20;
}

public class CreateTracingItemViewModel
{
    public Guid? Id { get; set; }
    public string Status { get; set; } = ContentStatus.Draft;
    public bool IsCompatible { get; set; } = true;

    [Range(0, 100000, ErrorMessage = "Thứ tự bài học phải từ 0 đến 100000.")]
    public int SortOrder { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên bài."), MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn nhóm kỹ năng.")]
    public Guid SkillGroupId { get; set; }
    public Guid? TopicId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập ký tự cần tô."), MaxLength(2)]
    public string Symbol { get; set; } = "A";

    [Required(ErrorMessage = "Vui lòng nhập lời hướng dẫn."), MaxLength(500)]
    public string InstructionText { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập câu hỏi."), MaxLength(500)]
    public string PromptText { get; set; } = string.Empty;

    [Range(5, 300, ErrorMessage = "Số điểm nét phải từ 5 đến 300.")]
    public int MinPoints { get; set; } = 20;

    [Required(ErrorMessage = "Vui lòng chọn kiểu hướng dẫn nét.")]
    public string GuideMode { get; set; } = "outline";

    [Range(1, 10, ErrorMessage = "Số nét dự kiến phải từ 1 đến 10.")]
    public int ExpectedStrokeCount { get; set; } = 1;

    public bool ShowStartPoint { get; set; }

    [MaxLength(1000)]
    public string AudioUrl { get; set; } = string.Empty;

    public Guid? ExistingAudioAssetId { get; set; }
    public IFormFile? AudioFile { get; set; }

    [Range(1, 3, ErrorMessage = "Độ khó phải từ 1 đến 3.")]
    public byte Level { get; set; } = 1;
}

public class LearningItemWorkflowViewModel
{
    public Guid Id { get; set; }
    public string Status { get; set; } = ContentStatus.Draft;
    public bool IsCompatible { get; set; } = true;
}

public class AdminCatalogViewModel
{
    public int TotalLearningItems { get; set; }
    public IReadOnlyList<AdminCatalogGroupViewModel> SkillGroups { get; set; } = [];
}

public class AdminCatalogGroupViewModel
{
    public SkillGroup SkillGroup { get; set; } = new();
    public int LearningItemCount { get; set; }
    public IReadOnlyList<AdminCatalogTopicViewModel> Topics { get; set; } = [];
}

public class AdminCatalogTopicViewModel
{
    public Topic Topic { get; set; } = new();
    public int LearningItemCount { get; set; }
    public IReadOnlyList<ActivityTemplateDefinition> AllowedTemplates { get; set; } = [];
    public bool AllowsTracing { get; set; }
}

public class AdminMediaLibraryViewModel
{
    public IReadOnlyList<MediaAsset> Images { get; set; } = [];
    public IReadOnlyList<MediaAsset> AudioFiles { get; set; } = [];
}
