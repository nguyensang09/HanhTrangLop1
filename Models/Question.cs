using System.ComponentModel.DataAnnotations;

namespace HanhTrangLop1.Models;

public class Question
{
    public Guid Id { get; set; }

    public Guid LearningItemId { get; set; }

    [MaxLength(500)]
    public string PromptText { get; set; } = string.Empty;

    public Guid? PromptAudioAssetId { get; set; }

    [Required, MaxLength(50)]
    public string QuestionType { get; set; } = "choice";

    public string PayloadJson { get; set; } = "{}";

    public string CorrectAnswerJson { get; set; } = "{}";

    public string HintJson { get; set; } = "{}";

    public string FeedbackJson { get; set; } = "{}";

    public int SortOrder { get; set; }

    public LearningItem? LearningItem { get; set; }
}
