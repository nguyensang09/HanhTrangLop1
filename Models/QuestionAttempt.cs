namespace HanhTrangLop1.Models;

public class QuestionAttempt
{
    public Guid Id { get; set; }

    public Guid LearningAttemptId { get; set; }

    public Guid QuestionId { get; set; }

    public string AnswerJson { get; set; } = "{}";

    public bool? IsCorrect { get; set; }

    public int AttemptCount { get; set; }

    public int HintLevelUsed { get; set; }

    public string MetricsJson { get; set; } = "{}";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public LearningAttempt? LearningAttempt { get; set; }

    public Question? Question { get; set; }
}
