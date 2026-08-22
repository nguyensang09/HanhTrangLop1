using System.ComponentModel.DataAnnotations;

namespace HanhTrangLop1.Models;

public class Topic
{
    public Guid Id { get; set; }

    public Guid SkillGroupId { get; set; }

    [Required, MaxLength(80)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public SkillGroup? SkillGroup { get; set; }
}
