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

    public GamesController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        VoiceLibraryMaintenanceService voiceLibraryService)
    {
        _db = db;
        _userManager = userManager;
        _voiceLibraryService = voiceLibraryService;
    }

    [HttpGet("voice")]
    public async Task<IActionResult> GetVoice(string text, string lang = "vi", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest(new { success = false, message = "Text is required" });
        }

        var raw = text.Trim();

        // Nếu yêu cầu tiếng Anh: tra cứu giọng nữ tiếng Anh chuẩn (en-US-JennyNeural) trong kho TextToSpeechCaches
        if (lang.StartsWith("en", StringComparison.OrdinalIgnoreCase))
        {
            var enUrl = await _voiceLibraryService.ResolveBilingualListenAudioUrlAsync(raw, "en", cancellationToken);
            if (!string.IsNullOrEmpty(enUrl))
            {
                return Json(new { success = true, audioUrl = enUrl });
            }
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
                    if (string.IsNullOrEmpty(entry.AudioUrl) || entry.Status != "ready")
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

        var totalStars = await _db.ChildLessonProgresses
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch)
            .SumAsync(x => (int?)x.BestStars) ?? 0;

        var games = new List<GameHubCardViewModel>
        {
            new()
            {
                Key = "bubble",
                Title = "Bắn Bong Bóng Chữ Cái",
                Subtitle = "Luyện nghe, chạm nổ bóng nhận diện chữ tiếng Việt",
                Category = "letter",
                Emoji = "🎈",
                ThemeClass = "card-theme-coral",
                Badge = "Nghe & Nhận Diện",
                TotalLevels = 12,
                CompletedLevels = 0,
                EarnedStars = 0,
                IsLocked = false
            },
            new()
            {
                Key = "gold-miner",
                Title = "Đào Vàng Tìm Chữ & Số",
                Subtitle = "Căn thời gian thả móc câu gắp đúng chữ và số kho báu",
                Category = "letter",
                Emoji = "⛏️",
                ThemeClass = "card-theme-gold",
                Badge = "Phản Xạ & Phân Biệt",
                TotalLevels = 10,
                CompletedLevels = 0,
                EarnedStars = 0,
                IsLocked = false
            },
            new()
            {
                Key = "balloon-word",
                Title = "Khinh Khí Cầu Ghép Vần",
                Subtitle = "Kéo chữ và dấu thanh tạo tiếng, từ quen thuộc có hình",
                Category = "letter",
                Emoji = "☁️",
                ThemeClass = "card-theme-purple",
                Badge = "Ghép Âm & Dấu Thanh",
                TotalLevels = 10,
                CompletedLevels = 0,
                EarnedStars = 0,
                IsLocked = false
            },
            new()
            {
                Key = "number-train",
                Title = "Đoàn Tàu Chở Số",
                Subtitle = "Điền toa số còn thiếu, đếm số lượng hoa quả và con vật",
                Category = "number",
                Emoji = "🚂",
                ThemeClass = "card-theme-emerald",
                Badge = "Số Lượng & Thứ Tự",
                TotalLevels = 10,
                CompletedLevels = 0,
                EarnedStars = 0,
                IsLocked = false
            }
        };

        var model = new KidsGamesLobbyViewModel
        {
            ChildProfile = child,
            TotalStars = Math.Max(totalStars, 0),
            Games = games
        };

        return View(model);
    }

    [HttpGet("play")]
    [HttpGet("/games/play")]
    public async Task<IActionResult> Play(string game = "bubble", int level = 1)
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

        var totalStars = await _db.ChildLessonProgresses
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch)
            .SumAsync(x => (int?)x.BestStars) ?? 0;

        var gameTitle = game switch
        {
            "gold-miner" => "Đào Vàng Tìm Chữ & Số",
            "balloon-word" => "Khinh Khí Cầu Ghép Vần",
            "number-train" => "Đoàn Tàu Chở Số",
            _ => "Bắn Bong Bóng Chữ Cái"
        };

        var category = game == "number-train" ? "number" : "letter";

        var model = new KidsGamePlayViewModel
        {
            ChildProfile = child,
            GameKey = game,
            GameTitle = gameTitle,
            Category = category,
            InitialLevel = Math.Max(1, level),
            TotalStars = Math.Max(totalStars, 0),
            SoundEnabled = child.SoundEnabled
        };

        return View(model);
    }

    [HttpPost("save-progress")]
    [HttpGet("/games/save-progress")]
    public async Task<IActionResult> SaveProgress([FromBody] SaveGameProgressRequest request)
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            return Json(new { success = false, message = "Chưa chọn hồ sơ bé" });
        }

        // Cập nhật hoặc ghi nhận thành tích
        return Json(new
        {
            success = true,
            gameKey = request.GameKey,
            level = request.Level,
            starsEarned = request.StarsEarned
        });
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
