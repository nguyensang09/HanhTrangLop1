using System.ComponentModel.DataAnnotations;
using HanhTrangLop1.Data;

namespace HanhTrangLop1.Models;

public class ChildProfile
{
    public Guid Id { get; set; }

    [Required]
    public string ParentUserId { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Nickname { get; set; } = string.Empty;

    public int? BirthYear { get; set; }

    [MaxLength(100)]
    public string AvatarKey { get; set; } = "soc-nau";

    public int DailyLearningMinutes { get; set; } = 15;

    public bool SoundEnabled { get; set; } = true;

    public string PreferredSkillGroupIdsJson { get; set; } = "[]";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ApplicationUser? ParentUser { get; set; }
}
