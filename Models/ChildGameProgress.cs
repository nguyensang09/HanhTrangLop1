namespace HanhTrangLop1.Models;

/// <summary>Màn trò chơi bé đã chơi qua: mỗi bé, mỗi trò, mỗi màn một dòng (lưu theo bé thay cho localStorage của trình duyệt).</summary>
public class ChildGameProgress
{
    public Guid Id { get; set; }
    public Guid ChildProfileId { get; set; }
    public string GameKey { get; set; } = string.Empty;
    public int Level { get; set; }
    public int BestStars { get; set; }
    public int PlayCount { get; set; }
    public DateTimeOffset FirstCompletedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastPlayedAt { get; set; } = DateTimeOffset.UtcNow;
    public int ProgressEpoch { get; set; }
    public ChildProfile? ChildProfile { get; set; }
}
