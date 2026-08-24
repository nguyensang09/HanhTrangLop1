using System.ComponentModel.DataAnnotations;

namespace HanhTrangLop1.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên tài khoản.")]
    [Display(Name = "Tên tài khoản")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên tài khoản.")]
    [MinLength(2, ErrorMessage = "Tên tài khoản cần có ít nhất 2 ký tự.")]
    [MaxLength(50, ErrorMessage = "Tên tài khoản tối đa 50 ký tự.")]
    [Display(Name = "Tên tài khoản")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Tên phụ huynh")]
    [MaxLength(100)]
    public string? DisplayName { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [MinLength(3, ErrorMessage = "Mật khẩu cần ít nhất 3 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên bé.")]
    [MaxLength(80, ErrorMessage = "Tên bé tối đa 80 ký tự.")]
    [Display(Name = "Tên bé")]
    public string ChildNickname { get; set; } = string.Empty;
}
