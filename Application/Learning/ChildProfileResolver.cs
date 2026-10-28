using HanhTrangLop1.Data;
using HanhTrangLop1.Infrastructure;
using HanhTrangLop1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Application.Learning;

/// <summary>
/// Xác định bé đang học theo cùng quy tắc với KidsController: phụ huynh đăng nhập thấy hồ sơ của mình,
/// khách dùng hồ sơ không gắn phụ huynh. Dùng cho các khu học mới để không phải lặp lại logic.
/// </summary>
public sealed class ChildProfileResolver
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChildProfileResolver(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<ChildProfile?> GetSelectedChildAsync(HttpContext httpContext)
    {
        var user = httpContext.User;
        var rawValue = httpContext.Session.GetString(SessionKeys.SelectedChildProfileId);
        Guid? selectedId = Guid.TryParse(rawValue, out var parsed) ? parsed : null;

        ChildProfile? child;
        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(user);
            var isAdmin = user.IsInRole("Admin");
            child = selectedId.HasValue
                ? await _db.ChildProfiles.FirstOrDefaultAsync(x => x.Id == selectedId.Value && (x.ParentUserId == userId || isAdmin))
                : null;
            child ??= await _db.ChildProfiles
                .Where(x => x.ParentUserId == userId)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }
        else
        {
            child = selectedId.HasValue
                ? await _db.ChildProfiles.FirstOrDefaultAsync(x => x.Id == selectedId.Value && x.ParentUserId == null)
                : null;
            child ??= await _db.ChildProfiles
                .Where(x => x.ParentUserId == null)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        if (child is not null)
        {
            httpContext.Session.SetString(SessionKeys.SelectedChildProfileId, child.Id.ToString());
        }

        return child;
    }
}
