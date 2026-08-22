using HanhTrangLop1.Data;
using HanhTrangLop1.Models;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Controllers;

[AllowAnonymous]
[Route("health")]
public class HealthController : Controller
{
    private readonly ApplicationDbContext _db;

    public HealthController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var model = new SystemHealthViewModel
        {
            DatabaseCanConnect = await _db.Database.CanConnectAsync(),
            PendingMigrations = (await _db.Database.GetPendingMigrationsAsync()).Count(),
            SkillGroups = await _db.SkillGroups.CountAsync(x => x.IsActive),
            PublishedLearningItems = await _db.LearningItems.CountAsync(x => x.Status == ContentStatus.Published),
            ChildProfiles = await _db.ChildProfiles.CountAsync()
        };

        if (!model.DatabaseCanConnect || model.PendingMigrations > 0 || model.SkillGroups == 0 || model.PublishedLearningItems == 0)
        {
            model.Status = "warning";
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        }

        return Json(model);
    }
}
