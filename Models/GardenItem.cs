using System.ComponentModel.DataAnnotations;

namespace HanhTrangLop1.Models;

public class GardenItem
{
    public Guid Id { get; set; }

    public Guid ChildProfileId { get; set; }

    [Required, MaxLength(80)]
    public string ItemKey { get; set; } = string.Empty;

    public int SlotIndex { get; set; }

    public DateTimeOffset PlacedAt { get; set; } = DateTimeOffset.UtcNow;

    public ChildProfile? ChildProfile { get; set; }
}
