using System.Text.Json;
using System.Text.Json.Nodes;
using HanhTrangLop1.Application.MathWorld;
using HanhTrangLop1.Data;
using HanhTrangLop1.Models;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Application.Lessons;

public sealed record LessonBankSeedResult(int Created, int Updated, int Kept, int Archived, int Total);

/// <summary>
/// Đưa kho bài học (<see cref="LessonBank"/>) vào CSDL. Chạy lại an toàn:
/// - Ghép theo mã bài: bài cũ cùng mã được làm mới tại chỗ (giữ Id, nên tiến độ và lịch sử làm bài của bé còn nguyên).
/// - Bài đã ở phiên bản kho hiện tại thì giữ nguyên.
/// - Bài nền cũ (mã curriculum-v4-, chưa từng thuộc kho mới) không còn trong kho thì chuyển sang Lưu trữ, không xóa.
/// - Bài do quản trị viên tự tạo (mã khác) không bao giờ bị động tới.
/// - Bài tô nét chỉ được làm mới tên và lời hướng dẫn, giữ nguyên mẫu nét.
/// </summary>
public static class LessonBankSeeder
{
    public static async Task<LessonBankSeedResult> SeedAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        var lessons = LessonBank.All;
        var groups = await db.SkillGroups.AsNoTracking().ToDictionaryAsync(x => x.Code, cancellationToken);
        var topics = await db.Topics.AsNoTracking().ToListAsync(cancellationToken);
        var existing = await db.LearningItems
            .Include(x => x.Questions)
            .Where(x => x.Code.StartsWith(LessonBank.CodePrefix))
            .ToDictionaryAsync(x => x.Code, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        int created = 0, updated = 0, kept = 0, archived = 0;
        var bankCodes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var lesson in lessons)
        {
            bankCodes.Add(lesson.Code);
            if (!groups.TryGetValue(lesson.Group, out var group)) continue;
            var topicId = lesson.Topic is null ? null : topics.FirstOrDefault(t => t.SkillGroupId == group.Id && t.Code == lesson.Topic)?.Id;

            if (!existing.TryGetValue(lesson.Code, out var item))
            {
                // Bài tô nét cần mẫu nét có sẵn; kho chỉ làm mới bài tô đã có chứ không tạo bài tô mới.
                if (lesson.Type == InteractionTypes.Tracing) continue;
                item = new LearningItem
                {
                    Id = StableGuid(lesson.Code),
                    Code = lesson.Code,
                    Status = ContentStatus.Published,
                    PublishedAt = now,
                    CreatedAt = now,
                    Questions = []
                };
                db.LearningItems.Add(item);
                Apply(item, lesson, group.Id, topicId, now, null);
                created++;
                continue;
            }

            if (ReadBankVersion(item.ContentJson) >= LessonBank.Version)
            {
                kept++;
                continue;
            }

            var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
            Apply(item, lesson, group.Id, topicId, now, question);
            if (item.Status == ContentStatus.Archived) item.Status = ContentStatus.Published;
            item.Version++;
            updated++;
        }

        foreach (var (code, item) in existing)
        {
            if (bankCodes.Contains(code) || item.InteractionType == InteractionTypes.MathPlay || code.StartsWith(MathCurriculum.LessonCodePrefix)) continue;
            if (item.Status != ContentStatus.Published || ReadBankVersion(item.ContentJson) > 0) continue;
            item.Status = ContentStatus.Archived;
            item.UpdatedAt = now;
            archived++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new LessonBankSeedResult(created, updated, kept, archived, lessons.Count);
    }

    private static void Apply(LearningItem item, BankLesson lesson, Guid groupId, Guid? topicId, DateTimeOffset now, Question? question)
    {
        item.Title = lesson.Title;
        item.SkillGroupId = groupId;
        item.TopicId = topicId;
        item.Level = 1;
        item.SortOrder = lesson.SortOrder;
        item.InteractionType = lesson.Type;
        item.EstimatedMinutes = lesson.Type == InteractionTypes.StoryChoice ? 5 : 3;
        item.InstructionText = lesson.Instruction;
        item.UpdatedAt = now;

        if (question is null)
        {
            question = new Question { Id = StableGuid(lesson.Code + "#q1"), LearningItemId = item.Id, SortOrder = 1 };
            item.Questions.Add(question);
        }

        JsonObject payload;
        if (lesson.Type == InteractionTypes.Tracing)
        {
            // Giữ nguyên mẫu nét (templateId, nét chuẩn...), chỉ thay lời đọc và thông tin bản đồ.
            payload = Parse(question.PayloadJson);
            foreach (var key in new[] { "speech", "mapIcon", "stage", "bankVersion" }) payload[key] = lesson.Payload[key]?.DeepClone();
            foreach (var key in new[] { "audioUrl", "audioUrlEn", "questionAudioUrl", "questionAudioUrlEn" }) payload[key] = string.Empty;
        }
        else
        {
            payload = (JsonObject)lesson.Payload.DeepClone();
            question.QuestionType = lesson.Type;
            question.CorrectAnswerJson = JsonSerializer.Serialize(new { value = lesson.Answer });
        }

        question.PromptText = lesson.Prompt;
        question.HintJson = JsonSerializer.Serialize(new { level1 = lesson.Hint });
        question.FeedbackJson = JsonSerializer.Serialize(new { correct = lesson.Correct, retry = lesson.Retry });
        question.PayloadJson = payload.ToJsonString();
        item.ContentJson = question.PayloadJson;
    }

    private static int ReadBankVersion(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return 0;
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Object &&
                   doc.RootElement.TryGetProperty("bankVersion", out var v) && v.TryGetInt32(out var n) ? n : 0;
        }
        catch (JsonException)
        {
            return 0;
        }
    }

    private static JsonObject Parse(string? json)
    {
        try
        {
            return JsonNode.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json) as JsonObject ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static Guid StableGuid(string value) => new(System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes("lesson-bank:" + value)));
}
