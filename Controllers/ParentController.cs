using HanhTrangLop1.Data;
using HanhTrangLop1.Infrastructure;
using HanhTrangLop1.Models;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace HanhTrangLop1.Controllers;

[Route("parent")]
public class ParentController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public ParentController(ApplicationDbContext db, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [Authorize]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = GetCurrentUserId();
        var children = await _db.ChildProfiles.AsNoTracking().Where(x => x.ParentUserId == userId).OrderBy(x => x.CreatedAt).ToListAsync();
        var childIds = children.Select(x => x.Id).ToList();
        var attempts = await _db.LearningAttempts
            .AsNoTracking()
            .Include(x => x.ChildProfile)
            .Include(x => x.LearningItem)
            .Where(x => childIds.Contains(x.ChildProfileId))
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync();
        var sessions = await _db.LearningSessions
            .AsNoTracking()
            .Where(x => childIds.Contains(x.ChildProfileId))
            .ToListAsync();
        var progressItems = await _db.SkillProgress
            .AsNoTracking()
            .Include(x => x.SkillGroup)
            .Where(x => childIds.Contains(x.ChildProfileId))
            .ToListAsync();

        var model = new ParentDashboardViewModel
        {
            Children = children,
            ChildSummaries = BuildChildSummaries(children, attempts, sessions, progressItems),
            ProgressItems = progressItems,
            DailyActivities = BuildDailyActivities(attempts, sessions, days: 7),
            TotalCompletedItems = attempts.Count(x => x.Status == "completed"),
            TotalNeedsPracticeItems = attempts.Count(x => x.Status == "needs_practice"),
            TotalLearningMinutes = sessions.Sum(x => x.ActualSeconds) / 60,
            TotalRewards = await _db.ChildRewards.CountAsync(x => childIds.Contains(x.ChildProfileId)),
            RecentAttempts = attempts.Take(8).ToList()
        };

        return View(model);
    }

    [Authorize]
    [HttpGet("reports/{childId:guid}")]
    public async Task<IActionResult> Report(Guid childId)
    {
        var child = await FindOwnedChildProfileAsync(childId);
        if (child is null)
        {
            return NotFound();
        }

        var attempts = await _db.LearningAttempts
            .AsNoTracking()
            .Include(x => x.LearningItem)
            .Where(x => x.ChildProfileId == child.Id)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync();
        var sessions = await _db.LearningSessions
            .AsNoTracking()
            .Where(x => x.ChildProfileId == child.Id)
            .ToListAsync();
        var progressItems = await _db.SkillProgress
            .AsNoTracking()
            .Include(x => x.SkillGroup)
            .Where(x => x.ChildProfileId == child.Id)
            .OrderBy(x => x.SkillGroup!.SortOrder)
            .ToListAsync();

        var model = new ParentReportViewModel
        {
            Child = child,
            SkillReports = BuildSkillReports(progressItems),
            DailyActivities = BuildDailyActivities(attempts, sessions, days: 14),
            RecentAttempts = attempts.Take(12).ToList(),
            TotalCompletedItems = attempts.Count(x => x.Status == "completed"),
            TotalNeedsPracticeItems = attempts.Count(x => x.Status == "needs_practice"),
            TotalLearningMinutes = sessions.Sum(x => x.ActualSeconds) / 60,
            TotalStars = attempts.Sum(x => x.StarsEarned),
            RecommendationText = BuildRecommendation(progressItems, attempts)
        };

        return View(model);
    }

    [Authorize]
    [HttpGet("reports/{childId:guid}/export")]
    public async Task<IActionResult> ExportReport(Guid childId)
    {
        var child = await FindOwnedChildProfileAsync(childId);
        if (child is null)
        {
            return NotFound();
        }

        var attempts = await _db.LearningAttempts
            .AsNoTracking()
            .Include(x => x.LearningItem)
            .Where(x => x.ChildProfileId == child.Id)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync();

        var builder = new StringBuilder();
        builder.AppendLine("Ngay,BaiHoc,TrangThai,Sao,SoLoi,ThoiLuongGiay");
        foreach (var attempt in attempts)
        {
            builder.AppendLine(string.Join(',',
                EscapeCsv(attempt.StartedAt.LocalDateTime.ToString("dd/MM/yyyy HH:mm")),
                EscapeCsv(attempt.LearningItem?.Title ?? string.Empty),
                EscapeCsv(GetAttemptStatusText(attempt.Status)),
                attempt.StarsEarned,
                attempt.MistakeCount,
                attempt.DurationSeconds));
        }

        var fileName = $"bao-cao-{child.Nickname}-{DateTime.Now:yyyyMMdd}.csv";
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    [Authorize]
    [HttpGet("profiles")]
    public async Task<IActionResult> Profiles()
    {
        var userId = GetCurrentUserId();
        var model = new ChildProfileListViewModel
        {
            Children = await _db.ChildProfiles
                .AsNoTracking()
                .Where(x => x.ParentUserId == userId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync()
        };

        return View(model);
    }

    [Authorize]
    [HttpGet("profiles/create")]
    public IActionResult CreateProfile()
    {
        return View("ProfileForm", new ChildProfileFormViewModel());
    }

    [Authorize]
    [HttpPost("profiles/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProfile(ChildProfileFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("ProfileForm", model);
        }

        var uploadedAvatar = await SaveUploadedAvatarAsync(model.AvatarFile);

        var child = new ChildProfile
        {
            ParentUserId = GetCurrentUserId(),
            Nickname = model.Nickname.Trim(),
            BirthYear = model.BirthYear,
            AvatarKey = uploadedAvatar ?? model.AvatarKey ?? "avatar-squirrel",
            DailyLearningMinutes = model.DailyLearningMinutes,
            SoundEnabled = model.SoundEnabled
        };

        _db.ChildProfiles.Add(child);
        await _db.SaveChangesAsync();

        HttpContext.Session.SetString(SessionKeys.SelectedChildProfileId, child.Id.ToString());
        HttpContext.Session.Remove(SessionKeys.CurrentLearningSessionId);

        return RedirectToAction(nameof(Profiles));
    }

    [Authorize]
    [HttpGet("profiles/{id:guid}/edit")]
    public async Task<IActionResult> EditProfile(Guid id)
    {
        var child = await FindOwnedChildProfileAsync(id);
        if (child is null)
        {
            return NotFound();
        }

        var model = new ChildProfileFormViewModel
        {
            Id = child.Id,
            Nickname = child.Nickname,
            BirthYear = child.BirthYear,
            AvatarKey = child.AvatarKey,
            DailyLearningMinutes = child.DailyLearningMinutes,
            SoundEnabled = child.SoundEnabled
        };

        return View("ProfileForm", model);
    }

    [Authorize]
    [HttpPost("profiles/{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(Guid id, ChildProfileFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View("ProfileForm", model);
        }

        var child = await FindOwnedChildProfileAsync(id);
        if (child is null)
        {
            return NotFound();
        }

        var uploadedAvatar = await SaveUploadedAvatarAsync(model.AvatarFile);

        child.Nickname = model.Nickname.Trim();
        child.BirthYear = model.BirthYear;
        if (!string.IsNullOrEmpty(uploadedAvatar))
        {
            child.AvatarKey = uploadedAvatar;
        }
        else if (!string.IsNullOrEmpty(model.AvatarKey))
        {
            child.AvatarKey = model.AvatarKey;
        }
        child.DailyLearningMinutes = model.DailyLearningMinutes;
        child.SoundEnabled = model.SoundEnabled;
        child.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Profiles));
    }

    private async Task<string?> SaveUploadedAvatarAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || ext is not (".jpg" or ".jpeg" or ".png" or ".webp" or ".gif"))
        {
            ext = ".png";
        }

        var fileName = $"child_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        return $"/uploads/avatars/{fileName}";
    }

    [Authorize]
    [HttpPost("profiles/{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProfile(Guid id)
    {
        var child = await FindOwnedChildProfileAsync(id);
        if (child is null)
        {
            return NotFound();
        }

        var selectedRaw = HttpContext.Session.GetString(SessionKeys.SelectedChildProfileId);
        if (Guid.TryParse(selectedRaw, out var selectedId) && selectedId == id)
        {
            HttpContext.Session.Remove(SessionKeys.SelectedChildProfileId);
            HttpContext.Session.Remove(SessionKeys.CurrentLearningSessionId);
        }

        _db.ChildProfiles.Remove(child);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Profiles));
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var username = model.Username.Trim();
        var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu chưa đúng.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            HttpContext.Session.Remove(SessionKeys.CurrentLearningSessionId);
            var child = await _db.ChildProfiles
                .Where(x => x.ParentUserId == user.Id)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (child is not null)
            {
                HttpContext.Session.SetString(SessionKeys.SelectedChildProfileId, child.Id.ToString());
            }
            else
            {
                HttpContext.Session.Remove(SessionKeys.SelectedChildProfileId);
            }

            return RedirectToAction(nameof(Dashboard));
        }

        ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu chưa đúng.");
        return View(model);
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var username = model.Username.Trim();
        var existingUser = await _userManager.FindByNameAsync(username);
        if (existingUser is not null)
        {
            ModelState.AddModelError(nameof(model.Username), "Tên tài khoản này đã được sử dụng. Vui lòng chọn tên khác hoặc đăng nhập.");
            return View(model);
        }

        var displayName = string.IsNullOrWhiteSpace(model.DisplayName) ? $"Phụ huynh {username}" : model.DisplayName.Trim();
        var email = username.Contains('@') ? username : $"{username.ToLowerInvariant().Replace(" ", "")}@parent.hanhtranglop1.local";

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true,
            DisplayName = displayName
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Parent");
        var child = new ChildProfile
        {
            ParentUserId = user.Id,
            Nickname = model.ChildNickname.Trim(),
            DailyLearningMinutes = 15
        };
        _db.ChildProfiles.Add(child);
        await _db.SaveChangesAsync();

        await _signInManager.SignInAsync(user, isPersistent: false);
        HttpContext.Session.Clear();
        HttpContext.Session.SetString(SessionKeys.SelectedChildProfileId, child.Id.ToString());

        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private string GetCurrentUserId()
    {
        return _userManager.GetUserId(User) ?? string.Empty;
    }

    private Task<ChildProfile?> FindOwnedChildProfileAsync(Guid id)
    {
        var userId = GetCurrentUserId();
        return _db.ChildProfiles.FirstOrDefaultAsync(x => x.Id == id && x.ParentUserId == userId);
    }

    private static IReadOnlyList<ParentChildSummaryViewModel> BuildChildSummaries(
        IReadOnlyList<ChildProfile> children,
        IReadOnlyList<LearningAttempt> attempts,
        IReadOnlyList<LearningSession> sessions,
        IReadOnlyList<SkillProgress> progressItems)
    {
        return children.Select(child =>
        {
            var childAttempts = attempts.Where(x => x.ChildProfileId == child.Id).ToList();
            var childProgress = progressItems.Where(x => x.ChildProfileId == child.Id).ToList();

            return new ParentChildSummaryViewModel
            {
                Child = child,
                CompletedItems = childAttempts.Count(x => x.Status == "completed"),
                NeedsPracticeItems = childAttempts.Count(x => x.Status == "needs_practice"),
                LearningMinutes = sessions.Where(x => x.ChildProfileId == child.Id).Sum(x => x.ActualSeconds) / 60,
                StarsEarned = childAttempts.Sum(x => x.StarsEarned),
                AverageMastery = childProgress.Count == 0 ? 0 : Math.Round(childProgress.Average(x => x.MasteryLevel), 1),
                LastLearnedAt = childAttempts.OrderByDescending(x => x.StartedAt).FirstOrDefault()?.StartedAt
            };
        }).ToList();
    }

    private static IReadOnlyList<ParentSkillReportItemViewModel> BuildSkillReports(IReadOnlyList<SkillProgress> progressItems)
    {
        return progressItems.Select(progress => new ParentSkillReportItemViewModel
        {
            SkillName = progress.SkillGroup?.Name ?? "Kỹ năng",
            IconKey = progress.SkillGroup?.IconKey ?? "auto_stories",
            Color = progress.SkillGroup?.Color ?? "#ff8542",
            MasteryLevel = progress.MasteryLevel,
            CompletedItems = progress.CompletedItems,
            NeedsPracticeItems = progress.NeedsPracticeItems,
            LastPracticedAt = progress.LastPracticedAt
        }).ToList();
    }

    private static IReadOnlyList<ParentDailyActivityViewModel> BuildDailyActivities(
        IReadOnlyList<LearningAttempt> attempts,
        IReadOnlyList<LearningSession> sessions,
        int days)
    {
        var today = DateTime.Today;
        return Enumerable.Range(0, days)
            .Select(offset => today.AddDays(offset - days + 1))
            .Select(date =>
            {
                var dayAttempts = attempts.Where(x => x.StartedAt.LocalDateTime.Date == date).ToList();
                var daySessions = sessions.Where(x => x.StartedAt.LocalDateTime.Date == date).ToList();
                return new ParentDailyActivityViewModel
                {
                    Date = date,
                    DateLabel = date.ToString("dd/MM"),
                    CompletedItems = dayAttempts.Count(x => x.Status == "completed"),
                    NeedsPracticeItems = dayAttempts.Count(x => x.Status == "needs_practice"),
                    LearningMinutes = daySessions.Sum(x => x.ActualSeconds) / 60
                };
            }).ToList();
    }

    private static string BuildRecommendation(IReadOnlyList<SkillProgress> progressItems, IReadOnlyList<LearningAttempt> attempts)
    {
        if (!attempts.Any())
        {
            return "Bé chưa có dữ liệu học tập. Phụ huynh có thể cho bé bắt đầu bằng 10-15 phút mỗi ngày.";
        }

        var weakestSkill = progressItems
            .OrderByDescending(x => x.NeedsPracticeItems)
            .ThenBy(x => x.MasteryLevel)
            .FirstOrDefault();
        if (weakestSkill is not null && weakestSkill.NeedsPracticeItems > 0)
        {
            return $"Bé nên luyện thêm nhóm {weakestSkill.SkillGroup?.Name?.ToLower() ?? "kỹ năng"} bằng các bài ngắn, ưu tiên nhắc lại nhẹ nhàng.";
        }

        var totalMinutes = attempts.Sum(x => x.DurationSeconds) / 60;
        return totalMinutes < 15
            ? "Bé đang làm tốt. Có thể tăng thêm vài phút luyện tập nếu bé còn hứng thú."
            : "Bé duy trì tiến độ tốt. Phụ huynh nên khen ngợi và cho bé nghỉ sau mỗi phiên học.";
    }

    private static string GetAttemptStatusText(string status)
    {
        return status == "completed" ? "Hoàn thành" : "Cần luyện thêm";
    }

    private static string EscapeCsv(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
