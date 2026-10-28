using System.Text;

namespace HanhTrangLop1.Application.MathWorld;

/// <summary>Báo cáo chương trình Đảo Toán Học (chạy: dotnet run -- --math-report). Không cần CSDL.</summary>
public static class MathCurriculumReport
{
    public static string Build()
    {
        var lessons = MathCurriculum.Lessons; // Truy cập lần đầu sẽ chạy kiểm tra toàn vẹn, lỗi thì ném ngoại lệ.
        var sb = new StringBuilder();
        sb.AppendLine("ĐẢO TOÁN HỌC — BÁO CÁO CHƯƠNG TRÌNH");
        sb.AppendLine($"Tổng: {lessons.Count} bài, {lessons.Sum(x => x.Questions.Count)} câu hỏi, phiên bản nội dung {MathCurriculum.ContentVersion}.");

        var resultFacts = lessons.SelectMany(l => l.Questions)
            .SelectMany(q => q.IsBoard ? q.Pairs : [q.Problem!])
            .Where(p => p.Missing == "result")
            .Select(p => p.ToString())
            .ToHashSet();
        var addCovered = MathCurriculum.AdditionFacts().Count(p => resultFacts.Contains(p.ToString()));
        var subCovered = MathCurriculum.SubtractionFacts().Count(p => resultFacts.Contains(p.ToString()));
        sb.AppendLine($"Bao phủ phép cộng 0–10: {addCovered}/{MathCurriculum.AdditionFacts().Count}; phép trừ 0–10: {subCovered}/{MathCurriculum.SubtractionFacts().Count}.");
        sb.AppendLine("Phong cách: " + string.Join(", ", lessons.GroupBy(x => x.Style).Select(g => $"{MathCurriculum.GetStyle(g.Key).Name}={g.Count()}")));

        foreach (var operation in MathCurriculum.Operations)
        {
            var opLessons = lessons.Where(x => x.Operation == operation).ToList();
            sb.AppendLine();
            sb.AppendLine($"== {operation.Name} ({operation.DisplaySymbol}) — {(operation.IsAvailable ? $"{opLessons.Count} bài" : "chưa triển khai")}");
            foreach (var stage in opLessons.GroupBy(x => x.Stage).OrderBy(x => x.Key.Order))
            {
                sb.AppendLine($"  Chặng {stage.Key.Order}: {stage.Key.Title}");
                foreach (var lesson in stage)
                {
                    var sample = string.Join("  ", lesson.Questions.Take(4).Select(q => q.IsBoard
                        ? "[" + string.Join(" | ", q.Pairs.Select(p => p.ToDisplay())) + "]"
                        : q.Problem!.ToDisplay()));
                    sb.AppendLine($"    {lesson.Code,-20} {MathCurriculum.GetStyle(lesson.Style).Name,-18} {lesson.Questions.Count,2} câu  {lesson.Title}");
                    sb.AppendLine($"      vd: {sample}");
                }
            }
        }

        var voiceLines = MathVoiceCatalog.AllDistinct();
        var conflicts = MathVoiceCatalog.FindConflicts();
        var lessonsWithoutVoice = lessons.Count(l => MathVoiceCatalog.ForLesson(l).Count(x => x.Key.EndsWith(".ask")) != l.Questions.Count);
        sb.AppendLine();
        sb.AppendLine("== Voice (VI/EN viết sẵn)");
        sb.AppendLine($"  Câu cần đọc (không trùng): {voiceLines.Count}; dùng chung: {MathVoiceCatalog.Common.Count}; " +
                      $"xung đột VI→EN: {conflicts.Count}; bài thiếu câu hỏi đọc: {lessonsWithoutVoice}.");
        foreach (var c in conflicts.Take(10)) sb.AppendLine($"  ! {c}");
        foreach (var lesson in lessons.Where(x => x.Code.EndsWith("s1-01") || x.Code.EndsWith("s4-01") || x.Code.EndsWith("s5-01")))
        {
            sb.AppendLine($"  {lesson.Code}:");
            foreach (var line in MathVoiceCatalog.ForLesson(lesson).Where(x => x.Key == "intro" || x.Key.StartsWith("q0.") || x.Key.StartsWith("x:")).Take(8))
            {
                sb.AppendLine($"    {line.Key,-10} VI: {line.Vi}");
                sb.AppendLine($"    {"",-10} EN: {line.En}");
            }
        }

        return sb.ToString();
    }
}
