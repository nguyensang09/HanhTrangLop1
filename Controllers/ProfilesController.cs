using HanhTrangLop1.Data;
using HanhTrangLop1.Infrastructure;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Controllers;

[Route("profiles")]
public class ProfilesController : Controller
{
    private readonly ApplicationDbContext _db;

    public ProfilesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var selectedRaw = HttpContext.Session.GetString(SessionKeys.SelectedChildProfileId);
        Guid.TryParse(selectedRaw, out var selectedId);

        var model = new ChildProfileListViewModel
        {
            Children = await _db.ChildProfiles.OrderBy(x => x.CreatedAt).ToListAsync(),
            SelectedChildProfileId = selectedId == Guid.Empty ? null : selectedId
        };

        return View(model);
    }

    [HttpPost("select")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Select(Guid childProfileId)
    {
        var exists = await _db.ChildProfiles.AnyAsync(x => x.Id == childProfileId);
        if (!exists)
        {
            return NotFound();
        }

        HttpContext.Session.SetString(SessionKeys.SelectedChildProfileId, childProfileId.ToString());
        return RedirectToAction("Home", "Kids");
    }
}
