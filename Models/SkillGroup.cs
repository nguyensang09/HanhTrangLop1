using System.ComponentModel.DataAnnotations;

namespace HanhTrangLop1.Models;

public class SkillGroup
{
    public Guid Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(80)]
    public string IconKey { get; set; } = "auto_stories";

    [MaxLength(20)]
    public string Color { get; set; } = "#ff8542";

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Topic> Topics { get; set; } = new List<Topic>();

    public ICollection<LearningItem> LearningItems { get; set; } = new List<LearningItem>();
}
