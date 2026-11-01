using System.ComponentModel.DataAnnotations;

namespace HanhTrangLop1.Models;

/// <summary>
/// Cấu hình hiển thị một module trên Góc của bé. Không có dòng nào nghĩa là đang hiển thị theo thứ tự mặc định.
/// Tách riêng khỏi SkillGroup.IsActive vì seed danh mục đặt lại cờ đó mỗi lần khởi động và cờ đó còn chi phối
/// bài học, phần thưởng; ẩn ở đây chỉ đổi giao diện của bé, dữ liệu bài học và tiến độ giữ nguyên.
/// </summary>
public class KidsModuleSetting
{
    /// <summary>Mã module: mã nhóm kỹ năng (SkillGroup.Code) hoặc mã tính năng trong KidsModuleKeys.</summary>
    [Key, MaxLength(50)]
    public string Key { get; set; } = string.Empty;

    public bool IsVisible { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
