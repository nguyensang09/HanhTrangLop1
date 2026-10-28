using System.Text.Json;
using HanhTrangLop1.Application.Voice;
using HanhTrangLop1.Data;
using HanhTrangLop1.Infrastructure;
using HanhTrangLop1.Models;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Controllers;

[Route("kids/games")]
public class GamesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly VoiceLibraryMaintenanceService _voiceLibraryService;

    /// <summary>
    /// Danh mục trò chơi. Số màn khớp với wwwroot/js/games/game-levels.js. Mỗi trò gắn với một nhóm bài học
    /// để bé đi qua lại giữa "học" và "chơi" (học mà chơi, chơi mà học).
    /// </summary>
    private sealed record GameInfo(string Key, string Title, string Subtitle, string Category, string Emoji, string Badge,
        string Image, string ButtonClass, int Levels, string LinkedGroupCode);

    private static readonly GameInfo[] Catalog =
    [
        new("bubble", "Bắn Bong Bóng Chữ Cái", "Nghe cô đọc rồi bắn đúng bong bóng chữ và số. Ôn 29 chữ cái và các số từ 0 đến 20.",
            "letter number", "🎈", "Nghe và nhận diện", "/images/games/bg-bubble.png", "btn-3d-green", 50, "chu-cai"),
        new("gold-miner", "Đào Vàng Tìm Chữ & Số", "Canh thời gian thả móc để gắp đúng khối vàng mang chữ cái và chữ số.",
            "letter number", "⛏️", "Phản xạ và tinh mắt", "/images/games/bg-miner.png", "btn-3d-orange", 39, "chu-so"),
        new("balloon-word", "Khinh Khí Cầu Ghép Vần", "Kéo chữ và dấu thanh để ghép thành tiếng, thành từ có hình minh họa.",
            "letter", "☁️", "Ghép âm và dấu thanh", "/images/games/bg-balloon.png", "btn-3d-blue", 40, "ngon-ngu"),
        new("number-train", "Đoàn Tàu Chở Số", "Tìm toa số còn thiếu, xếp toa theo thứ tự và đếm hàng hóa trên tàu.",
            "number", "🚂", "Số lượng và thứ tự", "/images/games/bg-train.png", "btn-3d-brown", 40, "so-luong-toan")
    ];

    /// <summary>Thứ tự 29 chữ cái trong trò Bắn bong bóng (màn 1–29), sau đó là số 0–20 (màn 30–50).</summary>
    private static readonly string[] BubbleLetters = ["A", "Ă", "Â", "B", "C", "D", "Đ", "E", "Ê", "G", "H", "I", "K", "L", "M", "N", "O", "Ô", "Ơ", "P", "Q", "R", "S", "T", "U", "Ư", "V", "X", "Y"];

    public GamesController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        VoiceLibraryMaintenanceService voiceLibraryService)
    {
        _db = db;
        _userManager = userManager;
        _voiceLibraryService = voiceLibraryService;
    }

    /// <summary>
    /// Voice cho trò chơi. Tiếng Việt: tìm trong kho Voice chung, chưa có thì sinh và lưu lại.
    /// Tiếng Anh: khi trò chơi gửi kèm câu tiếng Việt gốc (vi) thì lưu bản tiếng Anh vào đúng dòng đó và sinh file,
    /// nhờ vậy bé dùng giọng tiếng Anh nghe đúng câu viết sẵn chứ không phải bản dịch máy.
    /// </summary>
    [HttpGet("voice")]
    public async Task<IActionResult> GetVoice(string text, string lang = "vi", string? vi = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest(new { success = false, message = "Text is required" });
        }

        var raw = text.Trim();

        if (lang.StartsWith("en", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(vi))
            {
                var authored = await EnsureEnglishGameVoiceAsync(vi.Trim(), raw, cancellationToken);
                return Json(new { success = !string.IsNullOrEmpty(authored), audioUrl = authored ?? string.Empty });
            }

            var enUrl = await _voiceLibraryService.ResolveBilingualListenAudioUrlAsync(raw, "en", cancellationToken);
            return Json(new { success = !string.IsNullOrEmpty(enUrl), audioUrl = enUrl ?? string.Empty });
        }

        var audioUrl = await _voiceLibraryService.ResolveVoiceAudioUrlAsync(raw, cancellationToken);

        if (string.IsNullOrEmpty(audioUrl) && raw.Length <= 3)
        {
            audioUrl = await _voiceLibraryService.ResolveVoiceAudioUrlAsync($"chữ {raw}", cancellationToken);
        }
        if (string.IsNullOrEmpty(audioUrl) && raw.Length <= 3)
        {
            audioUrl = await _voiceLibraryService.ResolveVoiceAudioUrlAsync($"số {raw}", cancellationToken);
        }
        if (string.IsNullOrEmpty(audioUrl) && (raw.StartsWith("chữ ", StringComparison.OrdinalIgnoreCase) || raw.StartsWith("số ", StringComparison.OrdinalIgnoreCase)))
        {
            var stripped = raw.Substring(raw.IndexOf(' ') + 1).Trim();
            audioUrl = await _voiceLibraryService.ResolveVoiceAudioUrlAsync(stripped, cancellationToken);
        }

        if (string.IsNullOrEmpty(audioUrl))
        {
            try
            {
                var entry = await _voiceLibraryService.EnsureVoiceEntryAsync(raw, "game-voice", "Trò Chơi", cancellationToken);
                if (entry != null)
                {
                    if (string.IsNullOrEmpty(entry.AudioUrl) || entry.Status != "ready" || !_voiceLibraryService.VoiceFileExists(entry.AudioUrl))
                    {
                        entry.AudioUrl = await _voiceLibraryService.GenerateVoiceCacheFileAsync(entry, cancellationToken);
                        entry.Status = "ready";
                        await _db.SaveChangesAsync(cancellationToken);
                    }
                    audioUrl = entry.AudioUrl;
                }
            }
            catch
            {
                // Bỏ qua lỗi sinh voice để không ảnh hưởng luồng chơi
            }
        }

        return Json(new { success = !string.IsNullOrEmpty(audioUrl), audioUrl = audioUrl ?? string.Empty });
    }

    private async Task<string?> EnsureEnglishGameVoiceAsync(string viText, string enText, CancellationToken cancellationToken)
    {
        try
        {
            var entry = await _voiceLibraryService.EnsureVoiceEntryAsync(viText, "game-voice", "Trò Chơi", cancellationToken);
            if (entry is null) return null;
            if (!string.Equals(entry.TextEn, enText, StringComparison.Ordinal))
            {
                entry.TextEn = enText;
                entry.AudioUrlEn = string.Empty;
                entry.StatusEn = "missing";
            }
            if (string.IsNullOrEmpty(entry.AudioUrlEn) || entry.StatusEn != "ready" || !_voiceLibraryService.VoiceFileExists(entry.AudioUrlEn))
            {
                entry.AudioUrlEn = await _voiceLibraryService.GenerateVoiceCacheFileEnAsync(entry, cancellationToken);
                entry.StatusEn = "ready";
                entry.UpdatedAt = DateTimeOffset.UtcNow;
            }
            await _db.SaveChangesAsync(cancellationToken);
            return entry.AudioUrlEn;
        }
        catch
        {
            return null;
        }
    }

    [HttpGet("")]
    [HttpGet("/games")]
    public async Task<IActionResult> Index()
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("CreateProfile", "Parent");
            }
            return RedirectToAction("Index", "Profiles");
        }

        var progress = await LoadGameProgressAsync(child);
        var groups = await _db.SkillGroups.AsNoTracking().Select(x => new { x.Id, x.Code, x.Name }).ToListAsync();

        var games = Catalog.Select(game =>
        {
            progress.TryGetValue(game.Key, out var levels);
            levels ??= [];
            var group = groups.FirstOrDefault(x => x.Code == game.LinkedGroupCode);
            return new GameHubCardViewModel
            {
                Key = game.Key,
                Title = game.Title,
                Subtitle = game.Subtitle,
                Category = game.Category,
                Emoji = game.Emoji,
                Badge = game.Badge,
                Image = game.Image,
                ButtonClass = game.ButtonClass,
                TotalLevels = game.Levels,
                CompletedLevels = levels.Count(x => x.Key <= game.Levels),
                EarnedStars = levels.Values.Sum(),
                NextLevel = NextLevel(levels, game.Levels),
                LinkedGroupName = group?.Name ?? string.Empty,
                LinkedGroupId = group?.Id
            };
        }).ToList();

        var model = new KidsGamesLobbyViewModel
        {
            ChildProfile = child,
            TotalStars = await LessonStarsAsync(child) + games.Sum(x => x.EarnedStars),
            Games = games,
            Suggestion = await BuildSuggestionAsync(child, progress)
        };

        return View(model);
    }

    [HttpGet("play")]
    [HttpGet("/games/play")]
    public async Task<IActionResult> Play(string game = "bubble", int? level = null)
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("CreateProfile", "Parent");
            }
            return RedirectToAction("Index", "Profiles");
        }

        var info = Catalog.FirstOrDefault(x => x.Key == game) ?? Catalog[0];
        var progress = await LoadGameProgressAsync(child);
        progress.TryGetValue(info.Key, out var levels);
        levels ??= [];

        var model = new KidsGamePlayViewModel
        {
            ChildProfile = child,
            GameKey = info.Key,
            GameTitle = info.Title,
            Category = info.Key == "number-train" ? "number" : "letter",
            // Không chọn màn cụ thể thì chơi tiếp màn đầu tiên con chưa qua.
            InitialLevel = Math.Clamp(level ?? NextLevel(levels, info.Levels), 1, info.Levels),
            TotalStars = await LessonStarsAsync(child) + progress.Values.Sum(x => x.Values.Sum()),
            SoundEnabled = child.SoundEnabled,
            EnglishVoice = child.EnglishVoice,
            LevelStars = levels
        };

        return View(model);
    }

    /// <summary>Lưu màn bé vừa qua (theo bé, theo lượt học hiện tại). Gọi lại nhiều lần vẫn chỉ giữ số sao tốt nhất.</summary>
    [HttpPost("save-progress")]
    public async Task<IActionResult> SaveProgress([FromBody] SaveGameProgressRequest request)
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            return Json(new { success = false, message = "Chưa chọn hồ sơ bé" });
        }

        var info = Catalog.FirstOrDefault(x => x.Key == request.GameKey);
        if (info is null || request.Level < 1 || request.Level > info.Levels)
        {
            return BadRequest(new { success = false, message = "Màn chơi không hợp lệ" });
        }

        var stars = Math.Clamp(request.StarsEarned, 1, 3);
        var now = DateTimeOffset.UtcNow;
        var row = await _db.ChildGameProgresses.FirstOrDefaultAsync(x =>
            x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch && x.GameKey == info.Key && x.Level == request.Level);
        if (row is null)
        {
            row = new ChildGameProgress
            {
                Id = Guid.NewGuid(),
                ChildProfileId = child.Id,
                ProgressEpoch = child.ProgressEpoch,
                GameKey = info.Key,
                Level = request.Level,
                FirstCompletedAt = now
            };
            _db.ChildGameProgresses.Add(row);
        }
        row.BestStars = Math.Max(row.BestStars, stars);
        row.PlayCount++;
        row.LastPlayedAt = now;
        await _db.SaveChangesAsync();

        var completed = await _db.ChildGameProgresses.CountAsync(x =>
            x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch && x.GameKey == info.Key);
        return Json(new
        {
            success = true,
            gameKey = info.Key,
            level = row.Level,
            bestStars = row.BestStars,
            completedLevels = completed,
            totalLevels = info.Levels
        });
    }

    private async Task<Dictionary<string, Dictionary<int, int>>> LoadGameProgressAsync(ChildProfile child)
    {
        var rows = await _db.ChildGameProgresses.AsNoTracking()
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch)
            .Select(x => new { x.GameKey, x.Level, x.BestStars })
            .ToListAsync();
        return rows.GroupBy(x => x.GameKey)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.Level, x => x.BestStars));
    }

    private static int NextLevel(Dictionary<int, int> levels, int total)
    {
        for (var level = 1; level <= total; level++)
        {
            if (!levels.ContainsKey(level)) return level;
        }
        return total;
    }

    private Task<int> LessonStarsAsync(ChildProfile child) =>
        _db.ChildLessonProgresses
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch)
            .SumAsync(x => (int?)x.BestStars)
            .ContinueWith(t => Math.Max(t.Result ?? 0, 0));

    /// <summary>
    /// Học mà chơi: lấy bài bé vừa học gần nhất rồi gợi ý đúng màn chơi ôn lại điều đó
    /// (vừa học chữ B → bắn bong bóng chữ B; vừa học số 7 → bắn bong bóng số 7; đếm/so sánh → đoàn tàu chở số...).
    /// </summary>
    private async Task<GameSuggestionViewModel?> BuildSuggestionAsync(ChildProfile child, Dictionary<string, Dictionary<int, int>> progress)
    {
        var recent = await _db.ChildLessonProgresses.AsNoTracking()
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch && x.FirstCompletedAt != null)
            .OrderByDescending(x => x.LastAttemptedAt)
            .Select(x => new { x.LearningItem!.Title, x.LearningItem.ContentJson, GroupCode = x.LearningItem.SkillGroup!.Code })
            .FirstOrDefaultAsync();
        if (recent is null) return null;

        int Next(string key) => NextLevel(progress.TryGetValue(key, out var levels) ? levels : [], Catalog.First(x => x.Key == key).Levels);
        var icon = ReadMapIcon(recent.ContentJson);
        var (key, level, reason) = recent.GroupCode switch
        {
            "chu-cai" when Array.IndexOf(BubbleLetters, icon.ToUpperInvariant()) is var i && i >= 0 =>
                ("bubble", i + 1, $"Con vừa học chữ {icon.ToUpperInvariant()}. Mình bắn bong bóng chữ {icon.ToUpperInvariant()} để nhớ thật lâu nhé!"),
            "chu-so" when int.TryParse(icon, out var n) && n is >= 0 and <= 20 =>
                ("bubble", 30 + n, $"Con vừa học số {n}. Mình bắn bong bóng số {n} cho vui nhé!"),
            "so-luong-toan" or "toan-hoc" =>
                ("number-train", Next("number-train"), "Con vừa luyện đếm và so sánh. Lên đoàn tàu chở số đếm hàng nào!"),
            "ngon-ngu" =>
                ("balloon-word", Next("balloon-word"), "Con vừa học từ mới. Mình ghép vần trên khinh khí cầu nhé!"),
            "chu-so" =>
                ("gold-miner", Next("gold-miner"), "Con vừa học chữ số. Mình đi đào vàng tìm số nhé!"),
            _ =>
                ("gold-miner", Next("gold-miner"), $"Con vừa học xong bài \"{recent.Title}\". Nghỉ tay chơi đào vàng một chút nhé!")
        };

        var game = Catalog.First(x => x.Key == key);
        return new GameSuggestionViewModel { GameKey = key, GameTitle = game.Title, Emoji = game.Emoji, Level = level, Message = reason };
    }

    private static string ReadMapIcon(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return string.Empty;
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("mapIcon", out var v) && v.ValueKind == JsonValueKind.String
                ? v.GetString() ?? string.Empty
                : string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }


    private async Task<ChildProfile?> GetSelectedChildProfileAsync()
    {
        var rawId = HttpContext.Session.GetString(SessionKeys.SelectedChildProfileId);
        if (Guid.TryParse(rawId, out var parsedId))
        {
            var profile = await _db.ChildProfiles.FirstOrDefaultAsync(x => x.Id == parsedId);
            if (profile is not null)
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = _userManager.GetUserId(User);
                    if (profile.ParentUserId == userId || User.IsInRole("Admin"))
                    {
                        return profile;
                    }
                }
                else if (profile.ParentUserId == null)
                {
                    return profile;
                }
            }
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            return await _db.ChildProfiles
                .Where(x => x.ParentUserId == userId)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        return await _db.ChildProfiles
            .Where(x => x.ParentUserId == null)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
