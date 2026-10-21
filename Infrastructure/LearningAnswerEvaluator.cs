using System.Text.RegularExpressions;
using HanhTrangLop1.Models;

namespace HanhTrangLop1.Infrastructure;

public static class LearningAnswerEvaluator
{
    // Các dạng bài mà thứ tự lựa chọn không quan trọng: chỉ cần đúng tập hợp đáp án.
    private static readonly string[] UnorderedTypes =
    [
        InteractionTypes.MultiSelect,
        InteractionTypes.Matching,
        InteractionTypes.Classification
    ];

    private static readonly Regex WhitespaceRuns = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex MappingArrow = new(@"\s*=>\s*", RegexOptions.Compiled);

    public static bool IsCorrect(string interactionType, string? submittedAnswer, string? correctAnswer)
    {
        var submitted = Normalize(submittedAnswer);
        var expected = Normalize(correctAnswer);

        // Không có đáp án chuẩn thì không thể kết luận là đúng; tránh việc tự động
        // hoàn thành bài do dữ liệu đáp án bị thiếu hoặc sai định dạng.
        if (expected.Length == 0)
        {
            return false;
        }

        if (UnorderedTypes.Contains(interactionType, StringComparer.OrdinalIgnoreCase))
        {
            return AsSet(submitted).SetEquals(AsSet(expected));
        }

        // Dạng có thứ tự (ordering...) so sánh theo đúng trình tự nhưng đã được chuẩn hóa
        // khoảng trắng để không bị chấm sai chỉ vì thừa/thiếu dấu cách.
        return string.Equals(
            string.Join('|', AsList(submitted)),
            string.Join('|', AsList(expected)),
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Chuẩn hóa chuỗi đáp án: bỏ khoảng trắng đầu/cuối và gộp các khoảng trắng liên tiếp
    /// (kể cả khoảng trắng do bàn phím cảm ứng hoặc dữ liệu soạn bài sinh ra) thành một dấu cách.
    /// </summary>
    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : WhitespaceRuns.Replace(value, " ").Trim();

    private static List<string> AsList(string value) => value
        .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(segment => MappingArrow.Replace(WhitespaceRuns.Replace(segment, " "), "=>").Trim())
        .ToList();

    private static HashSet<string> AsSet(string value) =>
        AsList(value).ToHashSet(StringComparer.OrdinalIgnoreCase);
}
