using System.ComponentModel.DataAnnotations;

namespace HanhTrangLop1.Models;

public class TextToSpeechCache
{
    public Guid Id { get; set; }

    [Required, MaxLength(30)]
    public string Provider { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Voice { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ModelId { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Format { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    public string TextHash { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string UsageType { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string NormalizedText { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string OriginalText { get; set; } = string.Empty;

    [MaxLength(500)]
    public string AudioUrl { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Status { get; set; } = "ready";

    [MaxLength(1000)]
    public string? LastError { get; set; }

    public int ReuseCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
