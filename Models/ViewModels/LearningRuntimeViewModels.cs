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

    [Required(ErrorMessage = "Vui lòng nhập tên bài."), MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn nhóm kỹ năng.")]
    public Guid SkillGroupId { get; set; }

    public Guid? TopicId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn dạng tương tác.")]
    public string InteractionType { get; set; } = InteractionTypes.SingleChoice;

    [Required(ErrorMessage = "Vui lòng nhập lời hướng dẫn."), MaxLength(500)]
    public string InstructionText { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập câu hỏi."), MaxLength(500)]
    public string PromptText { get; set; } = string.Empty;

    public string ChoiceA { get; set; } = string.Empty;

    public string ChoiceB { get; set; } = string.Empty;

    public string ChoiceC { get; set; } = string.Empty;
    public string ChoiceD { get; set; } = string.Empty;
    public string ChoiceE { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = string.Empty;

    public string CorrectAnswersText { get; set; } = string.Empty;
    public string SequenceItemsText { get; set; } = string.Empty;
    public string PairsText { get; set; } = string.Empty;
    public string ClassificationText { get; set; } = string.Empty;
    public string TargetLabel { get; set; } = "Vùng đích";
    public string ObjectSymbol { get; set; } = "🍎";

    [Range(1, 20, ErrorMessage = "Số lượng mục tiêu phải từ 1 đến 20.")]
    public int TargetCount { get; set; } = 4;

    [Range(0, 20, ErrorMessage = "Số lượng thứ hai phải từ 0 đến 20.")]
    public int SecondaryCount { get; set; } = 2;

    [MaxLength(1000)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string AudioUrl { get; set; } = string.Empty;

    public Guid? ExistingImageAssetId { get; set; }
    public Guid? ExistingAudioAssetId { get; set; }
    public IFormFile? ImageFile { get; set; }
    public IFormFile? AudioFile { get; set; }

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
