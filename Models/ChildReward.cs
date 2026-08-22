namespace HanhTrangLop1.Models;

public class ChildReward
{
    public Guid Id { get; set; }

    public Guid ChildProfileId { get; set; }

    public Guid RewardDefinitionId { get; set; }

    public DateTimeOffset EarnedAt { get; set; } = DateTimeOffset.UtcNow;

    public ChildProfile? ChildProfile { get; set; }

    public RewardDefinition? RewardDefinition { get; set; }
}
