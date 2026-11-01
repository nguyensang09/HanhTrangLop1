using HanhTrangLop1.Application.MathWorld;
using HanhTrangLop1.Data;
using HanhTrangLop1.Models;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Application.Learning;

/// <summary>Mã các module không phải nhóm kỹ năng trên Góc của bé.</summary>
public static class KidsModuleKeys
{
    public const string Today = "today";
    public const string Tracing = "tracing";
    public const string Games = "games";
    public const string Bilingual = "bilingual";
    public const string Rewards = "rewards";
    public const string Numbers = "chu-so";
}

public sealed record KidsModule(string Key, string Name, string Icon, Guid? SkillGroupId, bool IsPinned, bool IsVisible);

/// <summary>Nội dung thẻ module, dùng chung cho trang của bé và trang xem trước của quản trị viên để hai bên luôn giống nhau.</summary>
public sealed record KidsModuleCard(string Title, string Emoji, string Theme, string Badge, string Description)
{
    public static KidsModuleCard For(KidsModule module, string? groupDescription, int stars, int dayNumber, string? todayTitle) => module.Key switch
    {
        KidsModuleKeys.Today => new("Bài Học Hôm Nay", "⭐", "kh-tone-amber", $"Ngày {dayNumber} • Mục Tiêu", todayTitle ?? "Bài học rèn luyện trong ngày"),
        KidsModuleKeys.Tracing => new("Bé Tập Tô Nét", "🎨", "kh-tone-coral", "Xưởng Vẽ Sáng Tạo", "Luyện nét chữ, tô chữ cái và vẽ hình"),
        KidsModuleKeys.Games => new("Trò Chơi Vui Nhộn", "🎮", "kh-tone-coral", "4 Mini-game Hấp Dẫn", "Bắn bóng, đào vàng, khinh khí cầu & tàu số"),
        KidsModuleKeys.Bilingual => new("Nghe Song Ngữ", "🎧", "kh-tone-purple", "Phát Âm Bản Xứ", "Luyện nghe chuẩn ABC & Số 0-20"),
        KidsModuleKeys.Rewards => new("Kho Quà Của Bé", "🏆", "kh-tone-gold", $"{stars} Sao Vàng ⭐", "Mở rương báu, đổi quà & huy hiệu"),
        "chu-cai" => new(module.Name, "📖", "kh-tone-coral", "33 Chữ Việt – Anh & Nét", "Mặt chữ, nét chuẩn & phát âm"),
        "chu-so" => new(module.Name, "🔢", "kh-tone-emerald", "Số 0 - 20 & Đếm", "Nhận biết số, đếm, so sánh & tách gộp"),
        "tu-duy-logic" => new(module.Name, "💡", "kh-tone-cyan", "Tư Duy & Ghép Bóng", "Quy luật, phân loại & tư duy"),
        "ky-nang-song" => new(module.Name, "💖", "kh-tone-rose", "Kỹ Năng & Tự Lập", "Tự phục vụ, an toàn & cảm xúc"),
        "ngon-ngu" => new(module.Name, "📚", "kh-tone-purple", "Vốn Từ & Âm Vần", "Nghe hiểu, kể chuyện & từ vựng"),
        "hinh-dang-khong-gian" => new(module.Name, "🔺", "kh-tone-teal", "Hình Khối & Không Gian", "Nhận biết hình học & định hướng"),
        "ghi-nho-tap-trung" => new(module.Name, "🧠", "kh-tone-amber", "Ghi Nhớ & Tập Trung", "Rèn trí nhớ & khả năng quan sát"),
        _ => new(module.Name, "🌟", "kh-tone-coral", module.Name, groupDescription ?? "Hành trình khám phá tri thức")
    };
}

/// <summary>
/// Danh sách module của Góc của bé: thứ tự và trạng thái ẩn/hiện do quản trị viên cấu hình.
/// Bài học hôm nay luôn đứng đầu; các module khác xếp theo cấu hình.
/// Module bị ẩn vẫn giữ nguyên dữ liệu, bật lại là bé học tiếp đúng chỗ cũ.
/// </summary>
public sealed class KidsModuleService
{
    private static readonly string[] PinnedKeys = [KidsModuleKeys.Today];

    /// <summary>Thứ tự mặc định của các module không ghim, khi quản trị viên chưa sắp xếp.</summary>
    private static readonly string[] DefaultLeading = [KidsModuleKeys.Numbers, MathCurriculum.SkillGroupCode, KidsModuleKeys.Tracing, "chu-cai", KidsModuleKeys.Games, KidsModuleKeys.Bilingual];

    private static readonly (string Key, string Name, string Icon)[] Features =
    [
        (KidsModuleKeys.Today, "Bài Học Hôm Nay", "play_circle"),
        (KidsModuleKeys.Tracing, "Bé Tập Tô Nét", "draw"),
        (KidsModuleKeys.Games, "Trò Chơi Vui Nhộn", "sports_esports"),
        (KidsModuleKeys.Bilingual, "Nghe Song Ngữ", "record_voice_over"),
        (KidsModuleKeys.Rewards, "Kho Quà Của Bé", "emoji_events")
    ];

    private readonly ApplicationDbContext _db;
    private IReadOnlyList<KidsModule>? _modules;

    public KidsModuleService(ApplicationDbContext db) => _db = db;

    /// <summary>Toàn bộ module (kể cả đang ẩn) theo thứ tự hiển thị. Lưu lại trong phạm vi một yêu cầu.</summary>
    public async Task<IReadOnlyList<KidsModule>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_modules is not null) return _modules;

        var settings = await _db.KidsModuleSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, cancellationToken);
        var groups = await _db.SkillGroups.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);

        var catalog = Features.Select(x => (x.Key, x.Name, x.Icon, SkillGroupId: (Guid?)null))
            .Concat(groups.Select(x => (Key: x.Code, x.Name, Icon: x.IconKey, SkillGroupId: (Guid?)x.Id)))
            .ToList();

        // Mặc định: các mục đầu theo danh sách cố định, rồi các nhóm kỹ năng còn lại, Kho quà luôn ở cuối.
        int DefaultRank(string key)
        {
            var leading = Array.IndexOf(DefaultLeading, key);
            if (leading >= 0) return leading;
            if (key == KidsModuleKeys.Rewards) return int.MaxValue;
            return DefaultLeading.Length + groups.FindIndex(x => x.Code == key);
        }

        _modules = catalog
            .Select(x => new KidsModule(x.Key, x.Name, x.Icon, x.SkillGroupId, PinnedKeys.Contains(x.Key),
                !settings.TryGetValue(x.Key, out var setting) || setting.IsVisible))
            .OrderBy(x => x.IsPinned ? Array.IndexOf(PinnedKeys, x.Key) : PinnedKeys.Length)
            .ThenBy(x => settings.TryGetValue(x.Key, out var setting) ? setting.SortOrder : int.MaxValue)
            .ThenBy(x => DefaultRank(x.Key))
            .ToList();
        return _modules;
    }

    public async Task<IReadOnlyList<KidsModule>> GetVisibleAsync(CancellationToken cancellationToken = default) =>
        (await GetAllAsync(cancellationToken)).Where(x => x.IsVisible).ToList();

    /// <summary>Mã không có trong danh mục (ví dụ nhóm đã ngừng dùng) được coi là hiển thị để không chặn nhầm.</summary>
    public async Task<bool> IsVisibleAsync(string key, CancellationToken cancellationToken = default)
    {
        var module = (await GetAllAsync(cancellationToken)).FirstOrDefault(x => x.Key == key);
        return module is null || module.IsVisible;
    }

    public async Task<bool> IsGroupVisibleAsync(Guid skillGroupId, CancellationToken cancellationToken = default)
    {
        var module = (await GetAllAsync(cancellationToken)).FirstOrDefault(x => x.SkillGroupId == skillGroupId);
        return module is null || module.IsVisible;
    }

    public async Task<IReadOnlySet<Guid>> GetHiddenGroupIdsAsync(CancellationToken cancellationToken = default) =>
        (await GetAllAsync(cancellationToken))
            .Where(x => !x.IsVisible && x.SkillGroupId.HasValue)
            .Select(x => x.SkillGroupId!.Value)
            .ToHashSet();

    /// <summary>Lưu cấu hình từ trang quản trị. Mã lạ bị bỏ qua; module không có trong danh sách hiển thị thì ẩn.</summary>
    public async Task SaveAsync(IReadOnlyList<string> orderedKeys, IReadOnlySet<string> visibleKeys, CancellationToken cancellationToken = default)
    {
        var known = (await GetAllAsync(cancellationToken)).Select(x => x.Key).ToList();
        var ordered = orderedKeys.Where(known.Contains).Distinct().ToList();
        ordered.AddRange(known.Where(x => !ordered.Contains(x)));

        var existing = await _db.KidsModuleSettings.ToDictionaryAsync(x => x.Key, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        for (var i = 0; i < ordered.Count; i++)
        {
            if (!existing.TryGetValue(ordered[i], out var setting))
            {
                setting = new KidsModuleSetting { Key = ordered[i] };
                _db.KidsModuleSettings.Add(setting);
            }
            setting.IsVisible = visibleKeys.Contains(ordered[i]);
            setting.SortOrder = (i + 1) * 10;
            setting.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _modules = null;
    }
}
