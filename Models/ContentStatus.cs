namespace HanhTrangLop1.Models;

public static class ContentStatus
{
    public const string Draft = "draft";
    public const string Review = "review";
    public const string Published = "published";
    public const string Archived = "archived";

    public static string GetDisplayName(string? status) => status switch
    {
        Draft => "Nháp",
        Review => "Chờ duyệt",
        Published => "Xuất bản",
        Archived => "Lưu trữ",
        _ => "Không xác định"
    };
}
