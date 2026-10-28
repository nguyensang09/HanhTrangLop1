using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using HanhTrangLop1.Application.Learning;
using HanhTrangLop1.Data;
using HanhTrangLop1.Models;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Application.MathWorld;

public sealed record MathCompletionResult(
    bool Success,
    string? Error,
    int Stars = 0,
    int BestStars = 0,
    int FirstTryCorrect = 0,
    int TotalQuestions = 0,
    bool IsFirstCompletion = false,
    string? MilestoneMessage = null,
    IReadOnlyList<MathBadgeViewModel>? NewBadges = null,
    MathLessonCardViewModel? NextLesson = null);

/// <summary>
/// Nghiệp vụ Đảo Toán Học: dựng bản đồ bài học, dữ liệu trình chơi và ghi nhận kết quả.
/// Tái sử dụng tiến trình chung (ChildLessonProgress, RewardProgressionService, SkillProgress)
/// nên bài toán được tính sao, mốc rương quà và báo cáo phụ huynh như mọi nhóm khác.
/// </summary>
public sealed class MathWorldService
{
    public const string SessionStatus = "math_practice";

    /// <summary>Huy hiệu hoàn thành theo phép toán (mã phép toán → mã phần thưởng).</summary>
    public static readonly IReadOnlyDictionary<string, string> OperationBadgeCodes = new Dictionary<string, string>
    {
        ["add"] = "badge-math-plus",
        ["sub"] = "badge-math-minus"
    };

    private const int LessonsToUnlockNextStage = 2;
    private static readonly JsonSerializerOptions ClientJson = new(JsonSerializerDefaults.Web);

    private readonly ApplicationDbContext _db;
    private readonly RewardProgressionService _rewardProgression;
    private readonly MathVoiceService _voices;

    public MathWorldService(ApplicationDbContext db, RewardProgressionService rewardProgression, MathVoiceService voices)
    {
        _db = db;
        _rewardProgression = rewardProgression;
        _voices = voices;
    }

    public async Task<(int Completed, int Total)> GetProgressSummaryAsync(ChildProfile child)
    {
        var lessonIds = await PublishedLessonsQuery().Select(x => x.Id).ToListAsync();
        var completed = await _db.ChildLessonProgresses.AsNoTracking()
            .CountAsync(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch &&
                             x.FirstCompletedAt != null && lessonIds.Contains(x.LearningItemId));
        return (completed, lessonIds.Count);
    }

    public async Task<MathHubViewModel> BuildHubAsync(ChildProfile child, string? selectedOperation)
    {
        var operations = await BuildOperationsAsync(child);
        var selected = operations.FirstOrDefault(x => x.Definition.Key == selectedOperation && x.HasLessons)
            ?? operations.FirstOrDefault(x => x.HasLessons && x.CompletedLessons < x.TotalLessons)
            ?? operations.First();

        var badgeCodes = OperationBadgeCodes.Values.ToList();
        var badgeDefinitions = await _db.RewardDefinitions.AsNoTracking()
            .Where(x => badgeCodes.Contains(x.Code))
            .ToListAsync();
        var earned = await _db.ChildRewards.AsNoTracking()
            .Where(x => x.ChildProfileId == child.Id)
            .Select(x => x.RewardDefinitionId)
            .ToListAsync();

        return new MathHubViewModel
        {
            ChildProfile = child,
            Operations = operations,
            SelectedOperation = selected.Definition.Key,
            TotalLessons = operations.Sum(x => x.TotalLessons),
            CompletedLessons = operations.Sum(x => x.CompletedLessons),
            TotalStars = operations.Sum(x => x.Stars),
            ContinueLesson = FindCurrent(operations, selected.Definition.Key),
            Styles = MathCurriculum.StyleCatalog,
            Badges = badgeCodes
                .Select(code => badgeDefinitions.FirstOrDefault(x => x.Code == code))
                .Where(x => x is not null)
                .Select(x => new MathBadgeViewModel
                {
                    Name = x!.Name,
                    IconKey = x.IconKey,
                    Rule = x.RuleJson,
                    IsEarned = earned.Contains(x.Id)
                }).ToList()
        };
    }

    public async Task<MathPlayViewModel?> BuildPlayAsync(ChildProfile child, Guid lessonId, string completeUrl, string hubUrl, string voiceUrl)
    {
        var operations = await BuildOperationsAsync(child);
        var card = operations.SelectMany(x => x.Stages).SelectMany(x => x.Lessons).FirstOrDefault(x => x.Id == lessonId);
        if (card is null || card.IsLocked) return null;

        var item = await PublishedLessonsQuery()
            .Include(x => x.Questions)
            .FirstAsync(x => x.Id == lessonId);
        var operation = MathCurriculum.Operations.First(x => x.Key == card.OperationKey);

        // Voice gắn theo định nghĩa bài trong mã: câu thứ i của định nghĩa có các khóa "q{i}.ask", "q{i}.eq"...
        var definition = MathCurriculum.Lessons.FirstOrDefault(x => x.Id == item.Id);
        var voiceIndex = definition is null
            ? new Dictionary<Guid, int>()
            : Enumerable.Range(0, definition.Questions.Count).ToDictionary(i => definition.QuestionId(i + 1), i => i); // Seed đánh số câu từ 1.
        var english = child.EnglishVoice;
        var voice = definition is null ? null : await _voices.BuildPackAsync(definition, english);

        var questions = ActiveQuestions(item).Select(q => new
        {
            id = q.Id,
            prompt = q.PromptText,
            voiceKey = voiceIndex.TryGetValue(q.Id, out var vi) ? $"q{vi}." : null,
            answer = ReadAnswer(q.CorrectAnswerJson),
            payload = JsonNode.Parse(string.IsNullOrWhiteSpace(q.PayloadJson) ? "{}" : q.PayloadJson)
        }).ToList();

        var config = new
        {
            lesson = new
            {
                id = card.Id,
                title = card.Title,
                style = card.Style,
                styleName = card.StyleName,
                styleEmoji = card.StyleEmoji,
                instruction = item.InstructionText,
                operation = operation.Key,
                operationName = operation.Name,
                stageTitle = card.StageTitle,
                stageOrder = card.StageOrder,
                bestStars = card.Stars
            },
            child = new { name = child.Nickname, soundEnabled = child.SoundEnabled },
            voice = new
            {
                lang = english ? "en" : "vi",
                speechLang = english ? "en-US" : "vi-VN",
                lines = (object?)voice?.Lines.ToDictionary(x => x.Key, x => new { text = x.Value.Text, url = x.Value.Url }) ?? new Dictionary<string, object>()
            },
            urls = new { complete = completeUrl, hub = hubUrl, voice = voiceUrl },
            questions
        };

        return new MathPlayViewModel
        {
            ChildProfile = child,
            Lesson = card,
            OperationName = operation.Name,
            Instruction = item.InstructionText,
            ClientConfigJson = JsonSerializer.Serialize(config, ClientJson)
        };
    }

    public async Task<MathCompletionResult> CompleteAsync(ChildProfile child, Guid lessonId, MathCompleteRequest request)
    {
        var item = await PublishedLessonsQuery()
            .Include(x => x.Questions)
            .Include(x => x.SkillGroup)
            .Include(x => x.Topic)
            .FirstOrDefaultAsync(x => x.Id == lessonId);
        if (item is null) return new MathCompletionResult(false, "Không tìm thấy bài học.");

        // Bé chỉ hoàn thành bài khi đã trả lời đúng mọi câu (sai thì được gợi ý và thử lại),
        // nên máy chủ kiểm tra lại toàn bộ đáp án cuối cùng thay vì tin vào trình duyệt.
        var questions = ActiveQuestions(item).ToList();
        var answers = (request.Answers ?? [])
            .GroupBy(x => x.QuestionId)
            .ToDictionary(g => g.Key, g => g.First());
        foreach (var question in questions)
        {
            if (!answers.TryGetValue(question.Id, out var answer) ||
                !MathAnswerEvaluator.IsCorrect(ReadAnswer(question.CorrectAnswerJson), answer.Answer))
            {
                return new MathCompletionResult(false, "Bài làm chưa đầy đủ hoặc có câu chưa đúng.");
            }
        }

        var total = questions.Count;
        var attemptsByQuestion = questions.ToDictionary(q => q.Id, q => Math.Clamp(answers[q.Id].Attempts, 1, 50));
        var firstTry = attemptsByQuestion.Values.Count(x => x == 1);
        var mistakes = attemptsByQuestion.Values.Sum(x => x - 1);
        var stars = StarsFor(firstTry, total);
        var duration = Math.Clamp(request.DurationSeconds, 0, 3600);
        var now = DateTimeOffset.UtcNow;

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var session = await GetOrCreatePracticeSessionAsync(child, now);
        session.ActualSeconds += duration;
        session.EndedAt = now;

        var attempt = new LearningAttempt
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            ChildProfileId = child.Id,
            LearningItemId = item.Id,
            StartedAt = now.AddSeconds(-duration),
            CompletedAt = now,
            Status = "completed",
            ScoreInternal = firstTry,
            StarsEarned = stars,
            HintsUsed = questions.Sum(q => Math.Clamp(answers[q.Id].HintsUsed, 0, 10)),
            MistakeCount = mistakes,
            DurationSeconds = duration,
            DeviceInputType = "touch",
            ProgressEpoch = child.ProgressEpoch
        };
        _db.LearningAttempts.Add(attempt);
        foreach (var question in questions)
        {
            var answer = answers[question.Id];
            _db.QuestionAttempts.Add(new QuestionAttempt
            {
                Id = Guid.NewGuid(),
                LearningAttemptId = attempt.Id,
                QuestionId = question.Id,
                AnswerJson = JsonSerializer.Serialize(new { value = answer.Answer }),
                IsCorrect = attemptsByQuestion[question.Id] == 1,
                AttemptCount = attemptsByQuestion[question.Id],
                HintLevelUsed = Math.Clamp(answer.HintsUsed, 0, 2),
                MetricsJson = JsonSerializer.Serialize(new { source = MathCurriculum.Engine, style = question.QuestionType })
            });
        }

        await UpdateSkillProgressAsync(child, item, mistakes == 0, now);
        var reward = await _rewardProgression.RecordAttemptAsync(child, item, true, stars, "completed");
        await _db.SaveChangesAsync();

        var newBadges = await AwardOperationBadgeAsync(child, item, now);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        var operations = await BuildOperationsAsync(child);
        var operationKey = MathCurriculum.FindOperationByTopic(item.Topic?.Code)?.Key ?? "add";
        var next = FindNext(operations, operationKey, item.SortOrder);

        return new MathCompletionResult(
            true,
            null,
            stars,
            reward.BestStars,
            firstTry,
            total,
            reward.IsFirstCompletion,
            reward.NewlyUnlockedGrant is not null ? $"Bé đã mở khóa rương quà {reward.UniqueCompletedInSkill} bài toán!" : null,
            newBadges,
            next);
    }

    public static int StarsFor(int firstTryCorrect, int total)
    {
        if (total <= 0) return 1;
        var ratio = (double)firstTryCorrect / total;
        return ratio >= 0.85 ? 3 : ratio >= 0.5 ? 2 : 1;
    }

    // ---------- Dựng bản đồ bài học ----------

    private IQueryable<LearningItem> PublishedLessonsQuery() =>
        _db.LearningItems.AsNoTracking()
            .Where(x => x.SkillGroup!.Code == MathCurriculum.SkillGroupCode &&
                        x.InteractionType == InteractionTypes.MathPlay &&
                        x.Status == ContentStatus.Published);

    private static IEnumerable<Question> ActiveQuestions(LearningItem item) =>
        item.Questions
            .Where(q => q.QuestionType != MathContentSeeder.RetiredQuestionType)
            .OrderBy(q => q.SortOrder);

    private async Task<List<MathOperationViewModel>> BuildOperationsAsync(ChildProfile child)
    {
        var items = await PublishedLessonsQuery()
            .Include(x => x.Topic)
            .OrderBy(x => x.SortOrder)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.SortOrder,
                x.Level,
                TopicCode = x.Topic != null ? x.Topic.Code : null,
                x.ContentJson,
                QuestionCount = x.Questions.Count(q => q.QuestionType != MathContentSeeder.RetiredQuestionType)
            })
            .ToListAsync();
        var itemIds = items.Select(x => x.Id).ToList();
        var progress = await _db.ChildLessonProgresses.AsNoTracking()
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch && itemIds.Contains(x.LearningItemId))
            .ToDictionaryAsync(x => x.LearningItemId);

        var result = new List<MathOperationViewModel>();
        foreach (var operation in MathCurriculum.Operations)
        {
            var cards = new List<MathLessonCardViewModel>();
            foreach (var item in items.Where(x => x.TopicCode == operation.TopicCode))
            {
                var meta = ReadMeta(item.ContentJson, item.Level);
                var style = MathCurriculum.GetStyle(meta.Style);
                progress.TryGetValue(item.Id, out var p);
                cards.Add(new MathLessonCardViewModel
                {
                    Id = item.Id,
                    Title = item.Title,
                    OperationKey = operation.Key,
                    StageOrder = meta.StageOrder,
                    StageTitle = meta.StageTitle,
                    Style = style.Key,
                    StyleName = style.Name,
                    StyleEmoji = style.Emoji,
                    QuestionCount = item.QuestionCount,
                    SortOrder = item.SortOrder,
                    Stars = p?.BestStars ?? 0,
                    IsCompleted = p?.FirstCompletedAt is not null
                });
            }

            var stages = new List<MathStageViewModel>();
            var definitions = MathCurriculum.StagesFor(operation);
            foreach (var group in cards.GroupBy(x => x.StageOrder).OrderBy(x => x.Key))
            {
                var definition = definitions.FirstOrDefault(x => x.Order == group.Key);
                stages.Add(new MathStageViewModel
                {
                    Order = group.Key,
                    Title = definition?.Title ?? group.First().StageTitle,
                    Subtitle = definition?.Subtitle ?? string.Empty,
                    Emoji = definition?.Emoji ?? "⭐",
                    Lessons = group.OrderBy(x => x.SortOrder).ToList()
                });
            }

            // Chặng đầu luôn mở. Chặng sau mở khi bé xong ít nhất 2 bài ở chặng trước
            // (hoặc đã từng học chặng đó) — đủ để giữ thứ tự dễ → khó mà không bắt bé làm hết.
            for (var i = 0; i < stages.Count; i++)
            {
                var stage = stages[i];
                if (i == 0 || stage.Lessons.Any(x => x.IsCompleted))
                {
                    stage.IsUnlocked = true;
                }
                else
                {
                    var previous = stages[i - 1];
                    var needed = Math.Min(LessonsToUnlockNextStage, previous.Lessons.Count);
                    var done = previous.Lessons.Count(x => x.IsCompleted);
                    stage.IsUnlocked = previous.IsUnlocked && done >= needed;
                    stage.LessonsNeededToUnlock = Math.Max(0, needed - done);
                }
                foreach (var lesson in stage.Lessons) lesson.IsLocked = !stage.IsUnlocked;
            }

            var current = stages.Where(x => x.IsUnlocked).SelectMany(x => x.Lessons).FirstOrDefault(x => !x.IsCompleted);
            if (current is not null) current.IsCurrent = true;

            result.Add(new MathOperationViewModel { Definition = operation, Stages = stages });
        }

        return result;
    }

    private static MathLessonCardViewModel? FindCurrent(IEnumerable<MathOperationViewModel> operations, string preferredOperation) =>
        operations.OrderBy(x => x.Definition.Key == preferredOperation ? 0 : 1)
            .SelectMany(x => x.Stages).SelectMany(x => x.Lessons)
            .FirstOrDefault(x => x.IsCurrent);

    private static MathLessonCardViewModel? FindNext(IEnumerable<MathOperationViewModel> operations, string operationKey, int afterSortOrder)
    {
        var all = operations.OrderBy(x => x.Definition.Key == operationKey ? 0 : 1)
            .SelectMany(x => x.Stages).SelectMany(x => x.Lessons)
            .Where(x => !x.IsLocked)
            .ToList();
        return all.FirstOrDefault(x => x.OperationKey == operationKey && x.SortOrder > afterSortOrder && !x.IsCompleted)
            ?? all.FirstOrDefault(x => !x.IsCompleted)
            ?? all.FirstOrDefault(x => x.OperationKey == operationKey && x.SortOrder > afterSortOrder);
    }

    private static (int StageOrder, string StageTitle, string Style) ReadMeta(string? contentJson, byte level)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(contentJson) ? "{}" : contentJson);
            var root = doc.RootElement;
            var order = root.TryGetProperty("stageOrder", out var o) && o.TryGetInt32(out var n) ? n : level;
            var title = root.TryGetProperty("stageTitle", out var t) ? t.GetString() ?? string.Empty : string.Empty;
            var style = root.TryGetProperty("style", out var s) ? s.GetString() ?? string.Empty : string.Empty;
            return (order, title, style);
        }
        catch (JsonException)
        {
            return (level, string.Empty, string.Empty);
        }
    }

    private static string ReadAnswer(string? correctAnswerJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(correctAnswerJson) ? "{}" : correctAnswerJson);
            return doc.RootElement.TryGetProperty("value", out var value) ? value.ToString() : string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    // ---------- Ghi nhận tiến trình ----------

    private async Task<LearningSession> GetOrCreatePracticeSessionAsync(ChildProfile child, DateTimeOffset now)
    {
        // Phiên luyện toán riêng mỗi ngày: PlannedMinutes = 0 nên không bao giờ trùng với phiên
        // "Bài học hôm nay" (dùng PlannedMinutes làm số ngày) và không làm tăng chuỗi ngày học.
        var dayStart = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);
        var session = await _db.LearningSessions
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch &&
                        x.Status == SessionStatus && x.StartedAt >= dayStart)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync();
        if (session is not null) return session;

        session = new LearningSession
        {
            Id = Guid.NewGuid(),
            ChildProfileId = child.Id,
            StartedAt = now,
            PlannedMinutes = 0,
            Status = SessionStatus,
            SessionPlanJson = "[]",
            ProgressEpoch = child.ProgressEpoch
        };
        _db.LearningSessions.Add(session);
        return session;
    }

    private async Task UpdateSkillProgressAsync(ChildProfile child, LearningItem item, bool perfect, DateTimeOffset now)
    {
        var isFirstCompletion = !await _db.ChildLessonProgresses.AnyAsync(x =>
            x.ChildProfileId == child.Id && x.LearningItemId == item.Id && x.FirstCompletedAt != null);
        var progress = await _db.SkillProgress.FirstOrDefaultAsync(x => x.ChildProfileId == child.Id && x.SkillGroupId == item.SkillGroupId);
        if (progress is null)
        {
            progress = new SkillProgress { Id = Guid.NewGuid(), ChildProfileId = child.Id, SkillGroupId = item.SkillGroupId };
            _db.SkillProgress.Add(progress);
        }

        progress.CompletedItems += isFirstCompletion ? 1 : 0;
        progress.NeedsPracticeItems += perfect ? 0 : 1;
        progress.MasteryLevel = Math.Min(100, progress.MasteryLevel + (perfect ? 8 : 4));
        progress.LastPracticedAt = now;
        progress.SummaryJson = JsonSerializer.Serialize(new
        {
            lastResult = perfect ? "Hoàn thành tốt" : "Hoàn thành, cần luyện thêm",
            lastLesson = item.Title,
            updatedAt = now
        });
    }

    private async Task<List<MathBadgeViewModel>> AwardOperationBadgeAsync(ChildProfile child, LearningItem item, DateTimeOffset now)
    {
        var operation = MathCurriculum.FindOperationByTopic(item.Topic?.Code);
        if (operation is null || !OperationBadgeCodes.TryGetValue(operation.Key, out var badgeCode)) return [];

        var reward = await _db.RewardDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.Code == badgeCode && x.IsActive);
        if (reward is null) return [];
        if (await _db.ChildRewards.AnyAsync(x => x.ChildProfileId == child.Id && x.RewardDefinitionId == reward.Id)) return [];

        var topicLessonIds = await PublishedLessonsQuery()
            .Where(x => x.TopicId == item.TopicId)
            .Select(x => x.Id)
            .ToListAsync();
        var completed = await _db.ChildLessonProgresses.AsNoTracking()
            .CountAsync(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch &&
                             x.FirstCompletedAt != null && topicLessonIds.Contains(x.LearningItemId));
        if (topicLessonIds.Count == 0 || completed < topicLessonIds.Count) return [];

        _db.ChildRewards.Add(new ChildReward
        {
            Id = Guid.NewGuid(),
            ChildProfileId = child.Id,
            RewardDefinitionId = reward.Id,
            EarnedAt = now
        });
        return [new MathBadgeViewModel { Name = reward.Name, IconKey = reward.IconKey, Rule = reward.RuleJson, IsEarned = true }];
    }
}

/// <summary>So khớp đáp án toán: bỏ khoảng trắng, số không phân biệt định dạng ("05" = "5").</summary>
public static class MathAnswerEvaluator
{
    public static bool IsCorrect(string? expected, string? actual)
    {
        var e = Normalize(expected);
        var a = Normalize(actual);
        return e.Length > 0 && e == a;
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var parts = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x, out var n) ? n.ToString() : x.Replace(" ", string.Empty));
        return string.Join(",", parts);
    }
}
