using HanhTrangLop1.Application.MathWorld;

namespace HanhTrangLop1.Models.ViewModels;

public class MathHubViewModel
{
    public ChildProfile ChildProfile { get; set; } = null!;
    public int TotalStars { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public string SelectedOperation { get; set; } = "add";
    public IReadOnlyList<MathOperationViewModel> Operations { get; set; } = [];
    public MathLessonCardViewModel? ContinueLesson { get; set; }
    public IReadOnlyList<MathBadgeViewModel> Badges { get; set; } = [];
    public IReadOnlyList<MathStyleDefinition> Styles { get; set; } = [];
}

public class MathOperationViewModel
{
    public MathOperationDefinition Definition { get; set; } = null!;
    public IReadOnlyList<MathStageViewModel> Stages { get; set; } = [];
    public int CompletedLessons => Stages.Sum(x => x.Lessons.Count(l => l.IsCompleted));
    public int TotalLessons => Stages.Sum(x => x.Lessons.Count);
    public int Stars => Stages.Sum(x => x.Lessons.Sum(l => l.Stars));
    public bool HasLessons => TotalLessons > 0;
}

public class MathStageViewModel
{
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Emoji { get; set; } = "⭐";
    public bool IsUnlocked { get; set; }
    public int LessonsNeededToUnlock { get; set; }
    public List<MathLessonCardViewModel> Lessons { get; set; } = [];
}

public class MathLessonCardViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OperationKey { get; set; } = string.Empty;
    public int StageOrder { get; set; }
    public string StageTitle { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;
    public string StyleName { get; set; } = string.Empty;
    public string StyleEmoji { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public int SortOrder { get; set; }
    public int Stars { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsLocked { get; set; }
    public bool IsCurrent { get; set; }
}

public class MathBadgeViewModel
{
    public string Name { get; set; } = string.Empty;
    public string IconKey { get; set; } = "emoji_events";
    public string Rule { get; set; } = string.Empty;
    public bool IsEarned { get; set; }
}

public class MathPlayViewModel
{
    public ChildProfile ChildProfile { get; set; } = null!;
    public MathLessonCardViewModel Lesson { get; set; } = null!;
    public string OperationName { get; set; } = string.Empty;
    public string Instruction { get; set; } = string.Empty;
    public string ClientConfigJson { get; set; } = "{}";
}

public class MathCompleteRequest
{
    public List<MathAnswerRequest> Answers { get; set; } = [];
    public int DurationSeconds { get; set; }
}

public class MathAnswerRequest
{
    public Guid QuestionId { get; set; }
    public string? Answer { get; set; }
    public int Attempts { get; set; }
    public int HintsUsed { get; set; }
}
