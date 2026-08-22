using Microsoft.AspNetCore.Identity;

namespace HanhTrangLop1.Data;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
