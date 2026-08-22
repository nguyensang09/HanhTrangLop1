namespace HanhTrangLop1.Models.ViewModels;

public class SystemHealthViewModel
{
    public string Status { get; set; } = "ok";
    public bool DatabaseCanConnect { get; set; }
    public int PendingMigrations { get; set; }
    public int SkillGroups { get; set; }
    public int PublishedLearningItems { get; set; }
    public int ChildProfiles { get; set; }
    public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;
}
