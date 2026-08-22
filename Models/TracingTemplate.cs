using System.ComponentModel.DataAnnotations;

namespace HanhTrangLop1.Models;

public class TracingTemplate
{
    public Guid Id { get; set; }

    [Required, MaxLength(30)]
    public string SymbolType { get; set; } = "uppercase";

    [Required, MaxLength(10)]
    public string Symbol { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    public int CanvasWidth { get; set; } = 720;

    public int CanvasHeight { get; set; } = 720;

    public string GuideJson { get; set; } = "{}";

    public Guid? PreviewAssetId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
