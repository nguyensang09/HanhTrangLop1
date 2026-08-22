namespace HanhTrangLop1.Models.ViewModels;

public class LearnViewModel
{
    public LearningItem Item { get; set; } = new();
    public ChildProfile? ChildProfile { get; set; }
    public Question? CurrentQuestion { get; set; }
    public IReadOnlyList<ChoiceOptionViewModel> Choices { get; set; } = [];
    public string TracingSymbol { get; set; } = "A";
    public int TracingMinPoints { get; set; } = 20;
    public string? FeedbackMessage { get; set; }
    public bool? IsCorrect { get; set; }
    public Guid? NextItemId { get; set; }
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
    public string Title { get; set; } = string.Empty;
    public Guid SkillGroupId { get; set; }
    public Guid? TopicId { get; set; }
    public string InteractionType { get; set; } = InteractionTypes.SingleChoice;
    public string InstructionText { get; set; } = string.Empty;
    public string PromptText { get; set; } = string.Empty;
    public string ChoiceA { get; set; } = string.Empty;
    public string ChoiceB { get; set; } = string.Empty;
    public string ChoiceC { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public byte Level { get; set; } = 1;
}
