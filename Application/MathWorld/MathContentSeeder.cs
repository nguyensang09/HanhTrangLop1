using System.Text.Json;
using HanhTrangLop1.Data;
using HanhTrangLop1.Models;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Application.MathWorld;

public sealed record MathSeedResult(int Created, int Updated, int Kept, int QuestionsTotal);

/// <summary>
/// Khởi tạo toàn bộ bài học Đảo Toán Học. Chạy lại an toàn:
/// - Bài và câu hỏi dùng Id tất định sinh từ mã bài, nên không bao giờ tạo trùng.
/// - Bài đã có chỉ được nâng cấp khi còn đúng định dạng của trình chơi toán và có phiên bản cũ hơn.
/// - Không bao giờ đổi trạng thái xuất bản (quản trị viên có thể ẩn/hiện bài tùy ý).
/// Phải chạy SAU LearningContentSeed vì seed nền đó chỉ chạy khi bảng bài học còn trống.
/// </summary>
public static class MathContentSeeder
{
    public const string RetiredQuestionType = "math_retired";

    public static async Task<MathSeedResult> SeedAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        var group = await db.SkillGroups.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == MathCurriculum.SkillGroupCode, cancellationToken);
        if (group is null) return new MathSeedResult(0, 0, 0, 0);

        var topics = await db.Topics.AsNoTracking()
            .Where(x => x.SkillGroupId == group.Id)
            .ToDictionaryAsync(x => x.Code, cancellationToken);
        var existing = await db.LearningItems
            .Include(x => x.Questions)
            .Where(x => x.Code.StartsWith(MathCurriculum.LessonCodePrefix))
            .ToDictionaryAsync(x => x.Code, cancellationToken);

        var lessons = MathCurriculum.Lessons;
        var now = DateTimeOffset.UtcNow;
        int created = 0, updated = 0, kept = 0;

        foreach (var lesson in lessons)
        {
            topics.TryGetValue(lesson.Operation.TopicCode, out var topic);

            if (!existing.TryGetValue(lesson.Code, out var item))
            {
                item = new LearningItem
                {
                    Id = lesson.Id,
                    Code = lesson.Code,
                    Status = ContentStatus.Published,
                    PublishedAt = now,
                    CreatedAt = now
                };
                ApplyLesson(item, lesson, group.Id, topic?.Id, now);
                foreach (var question in BuildQuestions(lesson))
                {
                    item.Questions.Add(question);
                }
                db.LearningItems.Add(item);
                created++;
                continue;
            }

            if (!IsUpgradable(item.ContentJson))
            {
                kept++;
                continue;
            }

            ApplyLesson(item, lesson, group.Id, topic?.Id, now);
            item.Version++;
            await SyncQuestionsAsync(db, item, lesson, cancellationToken);
            updated++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new MathSeedResult(created, updated, kept, lessons.Sum(x => x.Questions.Count));
    }

    private static void ApplyLesson(LearningItem item, MathLessonDefinition lesson, Guid groupId, Guid? topicId, DateTimeOffset now)
    {
        item.Title = lesson.Title;
        item.SkillGroupId = groupId;
        item.TopicId = topicId;
        item.Level = lesson.Level;
        item.SortOrder = lesson.SortOrder;
        item.InteractionType = InteractionTypes.MathPlay;
        item.EstimatedMinutes = Math.Clamp(lesson.Questions.Count / 2 + 1, 3, 6);
        item.InstructionText = lesson.Instruction;
        item.ContentJson = MathCurriculum.BuildLessonContentJson(lesson);
        item.UpdatedAt = now;
    }

    private static IEnumerable<Question> BuildQuestions(MathLessonDefinition lesson)
    {
        for (var i = 0; i < lesson.Questions.Count; i++)
        {
            var question = new Question { Id = lesson.QuestionId(i + 1), LearningItemId = lesson.Id };
            ApplyQuestion(question, lesson, lesson.Questions[i], i + 1);
            yield return question;
        }
    }

    private static void ApplyQuestion(Question question, MathLessonDefinition lesson, MathQuestionDefinition definition, int sortOrder)
    {
        var (hint1, hint2) = MathCurriculum.BuildHints(definition);
        question.SortOrder = sortOrder;
        question.QuestionType = "math_" + lesson.Style;
        question.PromptText = MathCurriculum.BuildPrompt(lesson, definition);
        question.PayloadJson = MathCurriculum.BuildQuestionPayloadJson(lesson, definition);
        question.CorrectAnswerJson = JsonSerializer.Serialize(new { value = definition.AnswerValue });
        question.HintJson = JsonSerializer.Serialize(new { level1 = hint1, level2 = hint2 });
        question.FeedbackJson = JsonSerializer.Serialize(new { correct = "Đúng rồi! Con giỏi quá!", retry = hint1 });
    }

    private static async Task SyncQuestionsAsync(ApplicationDbContext db, LearningItem item, MathLessonDefinition lesson, CancellationToken cancellationToken)
    {
        var expectedIds = new HashSet<Guid>();
        for (var i = 0; i < lesson.Questions.Count; i++)
        {
            var id = lesson.QuestionId(i + 1);
            expectedIds.Add(id);
            var question = item.Questions.FirstOrDefault(x => x.Id == id);
            if (question is null)
            {
                question = new Question { Id = id, LearningItemId = item.Id };
                item.Questions.Add(question);
                db.Questions.Add(question);
            }
            ApplyQuestion(question, lesson, lesson.Questions[i], i + 1);
        }

        var extras = item.Questions.Where(x => !expectedIds.Contains(x.Id)).ToList();
        if (extras.Count == 0) return;

        // Câu hỏi đã có lịch sử làm bài thì không xóa (khóa ngoại), chỉ đánh dấu ngừng dùng.
        var extraIds = extras.Select(x => x.Id).ToList();
        var referenced = await db.QuestionAttempts
            .Where(x => extraIds.Contains(x.QuestionId))
            .Select(x => x.QuestionId)
            .Distinct()
            .ToListAsync(cancellationToken);
        foreach (var extra in extras)
        {
            if (referenced.Contains(extra.Id))
            {
                extra.QuestionType = RetiredQuestionType;
                extra.SortOrder = 1000 + extra.SortOrder;
            }
            else
            {
                db.Questions.Remove(extra);
            }
        }
    }

    private static bool IsUpgradable(string? contentJson)
    {
        if (string.IsNullOrWhiteSpace(contentJson)) return false;
        try
        {
            using var doc = JsonDocument.Parse(contentJson);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return false;
            if (!root.TryGetProperty("engine", out var engine) || engine.GetString() != MathCurriculum.Engine) return false;
            var version = root.TryGetProperty("version", out var v) && v.TryGetInt32(out var n) ? n : 0;
            return version < MathCurriculum.ContentVersion;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
