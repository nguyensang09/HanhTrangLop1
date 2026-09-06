namespace HanhTrangLop1.Models;

public class ChildInventoryItem
{
    public Guid Id { get; set; }
    public Guid ChildProfileId { get; set; }
    public Guid RewardDefinitionId { get; set; }
    public int Quantity { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ChildProfile? ChildProfile { get; set; }
    public RewardDefinition? RewardDefinition { get; set; }
}
