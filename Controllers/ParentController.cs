using HanhTrangLop1.Data;
using HanhTrangLop1.Models;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var children = await _db.ChildProfiles.Where(x => x.ParentUserId == userId).ToListAsync();
        var childIds = children.Select(x => x.Id).ToList();

        var model = new ParentDashboardViewModel
        {
            Children = children,
            ProgressItems = await _db.SkillProgress
                .Include(x => x.SkillGroup)
                .Where(x => childIds.Contains(x.ChildProfileId))
                .ToListAsync(),
            TotalCompletedItems = await _db.LearningAttempts.CountAsync(x => childIds.Contains(x.ChildProfileId) && x.Status == "completed"),
            TotalNeedsPracticeItems = await _db.LearningAttempts.CountAsync(x => childIds.Contains(x.ChildProfileId) && x.Status == "needs_practice"),
            TotalLearningMinutes = await _db.LearningSessions.Where(x => childIds.Contains(x.ChildProfileId)).SumAsync(x => x.ActualSeconds) / 60,
            TotalRewards = await _db.ChildRewards.CountAsync(x => childIds.Contains(x.ChildProfileId)),
            RecentAttempts = await _db.LearningAttempts
                .Include(x => x.LearningItem)
                .Where(x => childIds.Contains(x.ChildProfileId))
                .OrderByDescending(x => x.StartedAt)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }

    [Authorize]
    [HttpGet("profiles")]
    public async Task<IActionResult> Profiles()
    {
        var userId = GetCurrentUserId();
        var model = new ChildProfileListViewModel
        {
            Children = await _db.ChildProfiles
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

        _db.ChildProfiles.Add(new ChildProfile
        {
            ParentUserId = GetCurrentUserId(),
            Nickname = model.Nickname.Trim(),
            BirthYear = model.BirthYear,
            AvatarKey = model.AvatarKey,
            DailyLearningMinutes = model.DailyLearningMinutes,
            SoundEnabled = model.SoundEnabled
        });

        await _db.SaveChangesAsync();
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

        child.Nickname = model.Nickname.Trim();
        child.BirthYear = model.BirthYear;
        child.AvatarKey = model.AvatarKey;
        child.DailyLearningMinutes = model.DailyLearningMinutes;
        child.SoundEnabled = model.SoundEnabled;
        child.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Profiles));
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

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Dashboard));
        }

        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu chưa đúng.");
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

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            DisplayName = "Phụ huynh"
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
        _db.ChildProfiles.Add(new ChildProfile
        {
            ParentUserId = user.Id,
            Nickname = model.ChildNickname,
            DailyLearningMinutes = 15
        });
        await _db.SaveChangesAsync();

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
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
}
