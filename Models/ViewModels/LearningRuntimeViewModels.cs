using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HanhTrangLop1.Models.ViewModels;

public class LearnViewModel
{
    public LearningItem Item { get; set; } = new();
    public ChildProfile? ChildProfile { get; set; }
    public Question? CurrentQuestion { get; set; }
    public IReadOnlyList<ChoiceOptionViewModel> Choices { get; set; } = [];
    public string TracingSymbol { get; set; } = "A";
    public int TracingMinPoints { get; set; } = 20;
    public string TracingGuideMode { get; set; } = "outline";
    public int TracingExpectedStrokeCount { get; set; } = 1;
    public bool TracingShowStartPoint { get; set; } = true;
    public string TracingAudioUrl { get; set; } = string.Empty;
    public string QuestionImageUrl { get; set; } = string.Empty;
    public string QuestionImageAltText { get; set; } = "Hình minh họa bài học";
    public string TitleAudioUrl { get; set; } = string.Empty;
    public string QuestionAudioUrl { get; set; } = string.Empty;
    public string InstructionAudioUrl { get; set; } = string.Empty;
    public string CorrectFeedbackAudioUrl { get; set; } = string.Empty;
    public string RetryFeedbackAudioUrl { get; set; } = string.Empty;
    public string? FeedbackMessage { get; set; }
    public bool? IsCorrect { get; set; }
    public Guid? NextItemId { get; set; }
    public Guid? ReturnSkillGroupId { get; set; }
}

public class ChoiceOptionViewModel
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class SubmitAnswerViewModel
{
    public Guid QuestionId { get; set; }
    public string AnswerValue { get; set; } = string.Empty;
}

public class SubmitTracingViewModel
{
    public string StrokeDataJson { get; set; } = "[]";
    public string MetricsJson { get; set; } = "{}";
}

public class CreateChoiceItemViewModel
{
    public Guid? Id { get; set; }
    public string Status { get; set; } = ContentStatus.Draft;
    public bool IsCompatible { get; set; } = true;

    [Required(ErrorMessage = "Vui lòng nhập tên bài."), MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn nhóm kỹ năng.")]
    public Guid SkillGroupId { get; set; }

    public Guid? TopicId { get; set; }

    [Range(0, 100000, ErrorMessage = "Thứ tự bài học phải từ 0 đến 100000.")]
    public int SortOrder { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn dạng tương tác.")]
    public string InteractionType { get; set; } = InteractionTypes.SingleChoice;

    [Required(ErrorMessage = "Vui lòng nhập lời hướng dẫn."), MaxLength(500)]
    public string InstructionText { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập câu hỏi."), MaxLength(500)]
    public string PromptText { get; set; } = string.Empty;

    public string? ChoiceA { get; set; }

    public string? ChoiceB { get; set; }

    public string? ChoiceC { get; set; }
    public string? ChoiceD { get; set; }
    public string? ChoiceE { get; set; }

    public string? CorrectAnswer { get; set; }

    public string? CorrectAnswersText { get; set; }
    public string? SequenceItemsText { get; set; }
    public string? PairsText { get; set; }
    public string? ClassificationText { get; set; }

    [MaxLength(8000)]
    public string? ItemMediaText { get; set; }

    public string TargetLabel { get; set; } = "Vùng đích";
    public string ObjectSymbol { get; set; } = "🍎";

    [Range(0, 20, ErrorMessage = "Số lượng mục tiêu phải từ 0 đến 20.")]
    public int TargetCount { get; set; } = 4;

    [Range(0, 20, ErrorMessage = "Số lượng thứ hai phải từ 0 đến 20.")]
    public int SecondaryCount { get; set; } = 2;

    [Required(ErrorMessage = "Vui lòng chọn yêu cầu so sánh.")]
    public string ComparisonMode { get; set; } = "more";

    [MaxLength(1000)]
    public string? ImageUrl { get; set; }

    [MaxLength(250)]
    public string? ImageAltText { get; set; }

    [MaxLength(1000)]
    public string? TitleAudioUrl { get; set; }

    [MaxLength(1000)]
    public string? QuestionAudioUrl { get; set; }

    [MaxLength(1000)]
    public string? InstructionAudioUrl { get; set; }

    [MaxLength(1000)]
    public string? CorrectFeedbackAudioUrl { get; set; }

    [MaxLength(1000)]
    public string? RetryFeedbackAudioUrl { get; set; }

    [MaxLength(1000)]
    public string? AudioUrl { get; set; }

    [MaxLength(500)]
    public string? SpeechText { get; set; }

    [MaxLength(100)]
    public string LeftLabel { get; set; } = "Nhóm A";

    [MaxLength(100)]
    public string RightLabel { get; set; } = "Nhóm B";

    public Guid? ExistingImageAssetId { get; set; }
    public Guid? ExistingAudioAssetId { get; set; }
    public Guid? ExistingQuestionAudioAssetId { get; set; }
    public IFormFile? ImageFile { get; set; }
    public IFormFile? AudioFile { get; set; }
    public IFormFile? QuestionAudioFile { get; set; }

    [Range(1, 3, ErrorMessage = "Độ khó phải từ 1 đến 3.")]
    public byte Level { get; set; } = 1;

    [Range(1, 30, ErrorMessage = "Thời lượng phải từ 1 đến 30 phút.")]
    public int EstimatedMinutes { get; set; } = 5;

    [MaxLength(500)]
    public string HintText { get; set; } = "Con nhìn kỹ từng lựa chọn nhé.";

    [MaxLength(500)]
    public string CorrectFeedback { get; set; } = "Giỏi lắm, con chọn đúng rồi!";

    [MaxLength(500)]
    public string RetryFeedback { get; set; } = "Không sao, mình thử lại nhẹ nhàng nhé.";
}
