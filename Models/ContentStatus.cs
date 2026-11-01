namespace HanhTrangLop1.Models;

public static class ContentStatus
{
    public const string Draft = "draft";
    public const string Review = "review";
    public const string Published = "published";
    public const string Archived = "archived";

    /// <summary>Quản trị viên tạm ẩn với bé: bài và tiến độ giữ nguyên, chuyển lại Xuất bản là hiện lại. Bước seed không đổi trạng thái này.</summary>
    public const string Hidden = "hidden";

    public static string GetDisplayName(string? status) => status switch
    {
        Draft => "Nháp",
        Review => "Chờ duyệt",
        Published => "Xuất bản",
        Archived => "Lưu trữ",
        Hidden => "Đang ẩn",
        _ => "Không xác định"
    };
}
