using HanhTrangLop1.Data;
using HanhTrangLop1.Infrastructure;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Controllers;

[Route("profiles")]
public class ProfilesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfilesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var selectedRaw = HttpContext.Session.GetString(SessionKeys.SelectedChildProfileId);
        Guid.TryParse(selectedRaw, out var selectedId);

        List<HanhTrangLop1.Models.ChildProfile> children;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            children = await _db.ChildProfiles
                .Where(x => x.ParentUserId == userId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }
        else
        {
            children = await _db.ChildProfiles
                .Where(x => x.ParentUserId == null)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        var validSelectedId = children.Any(x => x.Id == selectedId)
            ? selectedId
            : children.FirstOrDefault()?.Id;

        if (validSelectedId.HasValue && validSelectedId.Value != Guid.Empty)
        {
            HttpContext.Session.SetString(SessionKeys.SelectedChildProfileId, validSelectedId.Value.ToString());
        }

        var model = new ChildProfileListViewModel
        {
            Children = children,
            SelectedChildProfileId = validSelectedId == Guid.Empty ? null : validSelectedId
        };

        return View(model);
    }

    [HttpPost("select")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Select(Guid childProfileId)
    {
        HanhTrangLop1.Models.ChildProfile? child;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            child = await _db.ChildProfiles
                .FirstOrDefaultAsync(x => x.Id == childProfileId && (x.ParentUserId == userId || User.IsInRole("Admin")));
        }
        else
        {
            child = await _db.ChildProfiles
                .FirstOrDefaultAsync(x => x.Id == childProfileId && x.ParentUserId == null);
        }

        if (child is null)
        {
            return NotFound();
        }

        HttpContext.Session.SetString(SessionKeys.SelectedChildProfileId, child.Id.ToString());
        HttpContext.Session.Remove(SessionKeys.CurrentLearningSessionId);

        return RedirectToAction("Home", "Kids");
    }
}
