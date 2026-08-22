namespace HanhTrangLop1.Models.ViewModels;

public class SessionSummaryViewModel
{
    public ChildProfile? ChildProfile { get; set; }
    public LearningSession? Session { get; set; }
    public int CompletedItems { get; set; }
    public int StarsEarned { get; set; }
    public int NeedsPracticeItems { get; set; }
    public IReadOnlyList<LearningAttempt> Attempts { get; set; } = [];
}
