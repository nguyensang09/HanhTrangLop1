using System.ComponentModel.DataAnnotations;

namespace HanhTrangLop1.Models;

public class MediaAsset
{
    public Guid Id { get; set; }

    [Required, MaxLength(30)]
    public string AssetType { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string StoragePath { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AltText { get; set; }

    public int? DurationMs { get; set; }

    public string? UploadedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
