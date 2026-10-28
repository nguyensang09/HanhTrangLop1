using System.Text;
using System.Text.Json.Nodes;
using HanhTrangLop1.Data;
using HanhTrangLop1.Models;

namespace HanhTrangLop1.Application.Lessons;

/// <summary>Báo cáo kiểm tra kho bài (chạy: dotnet run -- --lesson-report). Dựng kho cũng là bước kiểm tra lỗi dữ liệu.</summary>
public static class LessonBankReport
{
    public static string Build()
    {
        var lessons = LessonBank.All;
        var sb = new StringBuilder();
        sb.AppendLine($"Kho bài học v{LessonBank.Version}: {lessons.Count} bài");
        foreach (var group in lessons.GroupBy(x => x.Group))
        {
            var list = group.ToList();
            var visible = list.Count(x => x.Type != InteractionTypes.Tracing || group.Key == "van-dong-tinh");
            var disallowed = list.Where(x => !ActivityTemplateCatalog.IsAllowedForSkillGroup(group.Key, x.Type)).Select(x => x.Code).ToList();
            sb.AppendLine($"- {group.Key}: {list.Count} bài ({visible} hiện trên bản đồ), {list.Select(x => x.Payload["stage"]?.GetValue<string>()).Distinct().Count()} chặng");
            foreach (var type in list.GroupBy(x => x.Type)) sb.AppendLine($"    {type.Key}: {type.Count()}");
            if (disallowed.Count > 0) sb.AppendLine($"    !! dạng không được phép trong nhóm: {string.Join(", ", disallowed.Take(5))}");
        }

        var noVisual = lessons
            .Where(x => x.Type is not InteractionTypes.Tracing and not InteractionTypes.Counting and not InteractionTypes.QuantityBuilder and not InteractionTypes.Comparison)
            .Where(x => x.Payload["visuals"] is JsonObject v && v.Count == 0 && x.Payload["scene"] is null)
            .Select(x => x.Code).ToList();
        sb.AppendLine($"Bài không có hình cho đáp án và không có cảnh minh họa: {noVisual.Count} (chủ yếu là bài chữ cái/chữ số, chữ chính là hình)");

        var longPrompts = lessons.Where(x => x.Prompt.Length > 110).Select(x => $"{x.Code}: {x.Prompt}").ToList();
        if (longPrompts.Count > 0)
        {
            sb.AppendLine($"Câu hỏi quá dài (> 110 ký tự): {longPrompts.Count}");
            foreach (var line in longPrompts.Take(10)) sb.AppendLine("  " + line);
        }

        sb.AppendLine("Ví dụ lời đọc:");
        foreach (var lesson in lessons.Where((_, i) => i % 97 == 0).Take(12))
        {
            var pair = lesson.Payload["speech"]?[lesson.Prompt] as JsonArray;
            sb.AppendLine($"  [{lesson.Group}] {lesson.Title}");
            sb.AppendLine($"     VI: {pair?[0]}");
            sb.AppendLine($"     EN: {pair?[1]}");
        }
        return sb.ToString();
    }
}
