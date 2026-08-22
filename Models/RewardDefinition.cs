using System.ComponentModel.DataAnnotations;

namespace HanhTrangLop1.Models;

public class RewardDefinition
{
    public Guid Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string RewardType { get; set; } = "badge";

    [MaxLength(80)]
    public string IconKey { get; set; } = "emoji_events";

    public string RuleJson { get; set; } = "{}";

    public bool IsActive { get; set; } = true;
}
