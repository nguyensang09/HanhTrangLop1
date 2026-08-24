using HanhTrangLop1.Application.Learning;
using HanhTrangLop1.Data;
using HanhTrangLop1.Infrastructure;
using HanhTrangLop1.Models;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HanhTrangLop1.Controllers;

[Route("kids")]
public class KidsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly TodayLessonService _todayLessonService;
    private readonly UserManager<ApplicationUser> _userManager;

    public KidsController(
        ApplicationDbContext db,
        TodayLessonService todayLessonService,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _todayLessonService = todayLessonService;
        _userManager = userManager;
    }

    [HttpGet("")]
    [HttpGet("home")]
    public async Task<IActionResult> Home(Guid? childProfileId)
    {
        if (childProfileId.HasValue)
        {
            var canSelect = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                canSelect = await _db.ChildProfiles.AnyAsync(x => x.Id == childProfileId.Value && (x.ParentUserId == userId || User.IsInRole("Admin")));
            }
            else
            {
                canSelect = await _db.ChildProfiles.AnyAsync(x => x.Id == childProfileId.Value && x.ParentUserId == null);
            }

            if (canSelect)
            {
                HttpContext.Session.SetString(SessionKeys.SelectedChildProfileId, childProfileId.Value.ToString());
                HttpContext.Session.Remove(SessionKeys.CurrentLearningSessionId);
            }
        }

        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("CreateProfile", "Parent");
            }
            return RedirectToAction("Index", "Profiles");
        }

        var todayItems = await _db.LearningItems
            .Include(x => x.Topic)
            .Include(x => x.SkillGroup)
            .Where(x => x.Status == ContentStatus.Published)
            .OrderBy(x => x.SkillGroup!.SortOrder)
            .ToListAsync();

        var model = new KidsHomeViewModel
        {
            ChildProfile = child,
            SkillGroups = await _db.SkillGroups.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync(),
            TodayItems = todayItems.Where(ActivityTemplateCatalog.IsItemAllowed).Take(5).ToList()
        };

        return View(model);
    }

    [HttpGet("today")]
    public async Task<IActionResult> Today()
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            return RedirectToAction("Index", "Profiles");
        }

        var session = await _todayLessonService.GetOrCreateActiveSessionAsync(child);
        HttpContext.Session.SetString(SessionKeys.CurrentLearningSessionId, session.Id.ToString());
        var model = await _todayLessonService.BuildTodayViewModelAsync(child, session);

        return View(model);
    }

    [HttpGet("skills/{id:guid}")]
    public async Task<IActionResult> Skill(Guid id)
    {
        var skillGroup = await _db.SkillGroups
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (skillGroup is null)
        {
            return NotFound();
        }

        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            return RedirectToAction("Index", "Profiles");
        }

        var items = await _db.LearningItems
            .Include(x => x.Topic)
            .Include(x => x.Questions)
            .Where(x => x.SkillGroupId == id && x.Status == ContentStatus.Published)
            .OrderBy(x => x.Topic!.SortOrder)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Level)
            .ThenBy(x => x.Title)
            .ToListAsync();
        items = items.Where(ActivityTemplateCatalog.IsItemAllowed).ToList();

        var itemIds = items.Select(x => x.Id).ToList();
        var latestAttempts = itemIds.Count == 0
            ? []
            : await _db.LearningAttempts
                .Where(x => x.ChildProfileId == child.Id && itemIds.Contains(x.LearningItemId))
                .OrderByDescending(x => x.StartedAt)
                .ToListAsync();

        var model = new SkillLearningListViewModel
        {
            ChildProfile = child,
            SkillGroup = skillGroup,
            Items = items.Select(item =>
            {
                var latestAttempt = latestAttempts.FirstOrDefault(x => x.LearningItemId == item.Id);
                return new SkillLearningItemViewModel
                {
                    Item = item,
                    LatestStatus = latestAttempt?.Status,
                    StarsEarned = latestAttempt?.StarsEarned ?? 0
                };
            }).ToList()
        };

        return View(model);
    }

    [HttpGet("learn/{id:guid}")]
    public async Task<IActionResult> Learn(Guid id, Guid? skillGroupId)
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            return RedirectToAction("Index", "Profiles");
        }

        var item = await _db.LearningItems
            .Include(x => x.SkillGroup)
            .Include(x => x.Topic)
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.Status == ContentStatus.Published &&
                (!skillGroupId.HasValue || x.SkillGroupId == skillGroupId.Value));

        if (item is null || !ActivityTemplateCatalog.IsItemAllowed(item))
        {
            return NotFound();
        }

        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        return View(await BuildLearnViewModelAsync(
            item,
            question,
            child,
            skillGroupId));
    }

    [HttpPost("learn/{id:guid}/answer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Answer(Guid id, SubmitAnswerViewModel answer, Guid? skillGroupId)
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            return RedirectToAction("Index", "Profiles");
        }

        var item = await _db.LearningItems
            .Include(x => x.SkillGroup)
            .Include(x => x.Topic)
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.Status == ContentStatus.Published &&
                (!skillGroupId.HasValue || x.SkillGroupId == skillGroupId.Value));

        var question = item?.Questions.FirstOrDefault(x => x.Id == answer.QuestionId);
        if (item is null || question is null || !ActivityTemplateCatalog.IsItemAllowed(item))
        {
            return NotFound();
        }

        var correctAnswer = LearningJsonReader.ReadCorrectAnswer(question.CorrectAnswerJson);
        var isCorrect = LearningAnswerEvaluator.IsCorrect(item.InteractionType, answer.AnswerValue, correctAnswer);
        var session = await _todayLessonService.GetOrCreateActiveSessionAsync(child);
        HttpContext.Session.SetString(SessionKeys.CurrentLearningSessionId, session.Id.ToString());

        var learningAttempt = new LearningAttempt
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            ChildProfileId = child.Id,
            LearningItemId = item.Id,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            Status = isCorrect ? "completed" : "needs_practice",
            ScoreInternal = isCorrect ? 3 : 1,
            StarsEarned = isCorrect ? 3 : 1,
            MistakeCount = isCorrect ? 0 : 1,
            DeviceInputType = "mouse"
        };

        _db.LearningAttempts.Add(learningAttempt);
        _db.QuestionAttempts.Add(new QuestionAttempt
        {
            Id = Guid.NewGuid(),
            LearningAttemptId = learningAttempt.Id,
            QuestionId = question.Id,
            AnswerJson = JsonSerializer.Serialize(new { value = answer.AnswerValue }),
            IsCorrect = isCorrect,
            AttemptCount = 1,
            MetricsJson = JsonSerializer.Serialize(new { source = "choice_engine_v1" })
        });

        await UpdateSkillProgressAsync(child.Id, item.SkillGroupId, isCorrect);
        await _db.SaveChangesAsync();

        return View("Learn", await BuildLearnViewModelAsync(
            item,
            question,
            child,
            skillGroupId,
            LearningJsonReader.ReadFeedback(question.FeedbackJson, isCorrect),
            isCorrect));
    }

    [HttpPost("learn/{id:guid}/complete-tracing")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteTracing(Guid id, SubmitTracingViewModel tracing, Guid? skillGroupId)
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            return RedirectToAction("Index", "Profiles");
        }

        var item = await _db.LearningItems
            .Include(x => x.Topic)
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.Status == ContentStatus.Published &&
                x.InteractionType == InteractionTypes.Tracing &&
                (!skillGroupId.HasValue || x.SkillGroupId == skillGroupId.Value));
        if (item is null || !ActivityTemplateCatalog.IsItemAllowed(item))
        {
            return NotFound();
        }

        var session = await _todayLessonService.GetOrCreateActiveSessionAsync(child);
        var attempt = new LearningAttempt
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            ChildProfileId = child.Id,
            LearningItemId = item.Id,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            Status = "completed",
            ScoreInternal = 2,
            StarsEarned = 2,
            DeviceInputType = "touch",
            DurationSeconds = 60
        };

        _db.LearningAttempts.Add(attempt);

        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        if (question is not null)
        {
            _db.QuestionAttempts.Add(new QuestionAttempt
            {
                Id = Guid.NewGuid(),
                LearningAttemptId = attempt.Id,
                QuestionId = question.Id,
                AnswerJson = string.IsNullOrWhiteSpace(tracing.StrokeDataJson) ? "[]" : tracing.StrokeDataJson,
                IsCorrect = true,
                AttemptCount = 1,
                MetricsJson = string.IsNullOrWhiteSpace(tracing.MetricsJson) ? "{}" : tracing.MetricsJson
            });
        }

        await UpdateSkillProgressAsync(child.Id, item.SkillGroupId, isCorrect: true);
        await _db.SaveChangesAsync();

        var feedback = question is null
            ? "Con đã hoàn thành bài tô nét!"
            : LearningJsonReader.ReadFeedback(question.FeedbackJson, true);
        return View("Learn", await BuildLearnViewModelAsync(
            item,
            question,
            child,
            skillGroupId,
            feedback,
            true));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            return RedirectToAction("Index", "Profiles");
        }

        var sessionRaw = HttpContext.Session.GetString(SessionKeys.CurrentLearningSessionId);
        if (!Guid.TryParse(sessionRaw, out var sessionId))
        {
            return RedirectToAction(nameof(Today));
        }

        var session = await _db.LearningSessions.FirstOrDefaultAsync(x => x.Id == sessionId && x.ChildProfileId == child.Id);
        if (session is null)
        {
            return RedirectToAction(nameof(Today));
        }

        if (session.Status == "active")
        {
            await _todayLessonService.CompleteSessionAsync(session);
        }

        var attempts = await _db.LearningAttempts
            .Include(x => x.LearningItem)
            .Where(x => x.SessionId == session.Id)
            .OrderBy(x => x.StartedAt)
            .ToListAsync();

        var model = new SessionSummaryViewModel
        {
            ChildProfile = child,
            Session = session,
            Attempts = attempts,
            CompletedItems = attempts.Count(x => x.Status == "completed"),
            NeedsPracticeItems = attempts.Count(x => x.Status == "needs_practice"),
            StarsEarned = attempts.Sum(x => x.StarsEarned)
        };

        return View(model);
    }

    [HttpGet("rewards")]
    public async Task<IActionResult> Rewards()
    {
        var rewards = await _db.RewardDefinitions.Where(x => x.IsActive).ToListAsync();
        return View(rewards);
    }

    private Guid? GetSelectedChildProfileId()
    {
        var rawValue = HttpContext.Session.GetString(SessionKeys.SelectedChildProfileId);
        return Guid.TryParse(rawValue, out var selectedProfileId) ? selectedProfileId : null;
    }

    private async Task<ChildProfile?> GetSelectedChildProfileAsync()
    {
        var selectedProfileId = GetSelectedChildProfileId();

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            if (selectedProfileId.HasValue)
            {
                var sessionChild = await _db.ChildProfiles
                    .FirstOrDefaultAsync(x => x.Id == selectedProfileId.Value && (x.ParentUserId == userId || User.IsInRole("Admin")));
                if (sessionChild is not null)
                {
                    return sessionChild;
                }
            }

            var firstChild = await _db.ChildProfiles
                .Where(x => x.ParentUserId == userId)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (firstChild is not null)
            {
                HttpContext.Session.SetString(SessionKeys.SelectedChildProfileId, firstChild.Id.ToString());
            }
            else
            {
                HttpContext.Session.Remove(SessionKeys.SelectedChildProfileId);
            }

            return firstChild;
        }
        else
        {
            if (selectedProfileId.HasValue)
            {
                var guestChild = await _db.ChildProfiles
                    .FirstOrDefaultAsync(x => x.Id == selectedProfileId.Value && x.ParentUserId == null);
                if (guestChild is not null)
                {
                    return guestChild;
                }
            }

            var defaultGuest = await _db.ChildProfiles
                .Where(x => x.ParentUserId == null)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (defaultGuest is not null)
            {
                HttpContext.Session.SetString(SessionKeys.SelectedChildProfileId, defaultGuest.Id.ToString());
            }

            return defaultGuest;
        }
    }

    private async Task UpdateSkillProgressAsync(Guid childProfileId, Guid skillGroupId, bool isCorrect)
    {
        var progress = await _db.SkillProgress.FirstOrDefaultAsync(x => x.ChildProfileId == childProfileId && x.SkillGroupId == skillGroupId);
        if (progress is null)
        {
            progress = new SkillProgress
            {
                Id = Guid.NewGuid(),
                ChildProfileId = childProfileId,
                SkillGroupId = skillGroupId
            };
            _db.SkillProgress.Add(progress);
        }

        progress.CompletedItems += isCorrect ? 1 : 0;
        progress.NeedsPracticeItems += isCorrect ? 0 : 1;
        progress.MasteryLevel = Math.Min(100, progress.MasteryLevel + (isCorrect ? 8 : 2));
        progress.LastPracticedAt = DateTimeOffset.UtcNow;
        progress.SummaryJson = JsonSerializer.Serialize(new
        {
            lastResult = isCorrect ? "Hoàn thành tốt" : "Cần luyện thêm",
            updatedAt = DateTimeOffset.UtcNow
        });
    }

    private async Task<Guid?> FindNextItemIdInCurrentSessionAsync(Guid currentItemId)
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            return null;
        }

        var session = await _todayLessonService.GetOrCreateActiveSessionAsync(child);
        return await _todayLessonService.FindNextItemIdAsync(session, currentItemId);
    }

    private async Task<LearnViewModel> BuildLearnViewModelAsync(
        LearningItem item,
        Question? question,
        ChildProfile? child,
        Guid? skillGroupId,
        string? feedbackMessage = null,
        bool? isCorrect = null)
    {
        var payloadSymbol = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "symbol", string.Empty);
        var tracingSymbol = ExtractTracingSymbol(payloadSymbol, item.Title, question?.PromptText);
        var questionImageUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "imageUrl", string.Empty);
        if (string.IsNullOrWhiteSpace(questionImageUrl) && item.InteractionType == InteractionTypes.Tracing)
        {
            questionImageUrl = ResolveTracingFlashcardUrl(tracingSymbol);
        }
        if (string.IsNullOrWhiteSpace(questionImageUrl) && question is not null)
        {
            questionImageUrl = ResolveQuestionImageFromItemMedia(question);
        }

        return new LearnViewModel
        {
            Item = item,
            ChildProfile = child,
            CurrentQuestion = question,
            Choices = question is null ? [] : LearningJsonReader.ReadChoices(question.PayloadJson),
            TracingSymbol = tracingSymbol,
            TracingMinPoints = question is null ? 20 : LearningJsonReader.ReadIntProperty(question.CorrectAnswerJson, "minPoints", 20),
            TracingGuideMode = question is null ? "outline" : LearningJsonReader.ReadStringProperty(question.PayloadJson, "guideMode", "outline"),
            TracingExpectedStrokeCount = question is null ? 1 : LearningJsonReader.ReadIntProperty(question.PayloadJson, "expectedStrokeCount", 1),
            TracingShowStartPoint = question is null || LearningJsonReader.ReadBoolProperty(question.PayloadJson, "showStartPoint", true),
            TracingAudioUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "audioUrl", string.Empty),
            QuestionImageUrl = questionImageUrl,
            QuestionImageAltText = question is null ? "Hình minh họa bài học" : LearningJsonReader.ReadStringProperty(question.PayloadJson, "imageAltText", "Hình minh họa bài học"),
            TitleAudioUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "titleAudioUrl", string.Empty),
            QuestionAudioUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "questionAudioUrl", string.Empty),
            InstructionAudioUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "instructionAudioUrl", string.Empty),
            CorrectFeedbackAudioUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "correctAudioUrl", string.Empty),
            RetryFeedbackAudioUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "retryAudioUrl", string.Empty),
            FeedbackMessage = feedbackMessage,
            IsCorrect = isCorrect,
            NextItemId = await FindNextItemIdAsync(item, skillGroupId),
            ReturnSkillGroupId = skillGroupId
        };
    }

    private static string ExtractTracingSymbol(string? payloadSymbol, string? itemTitle, string? promptText)
    {
        if (!string.IsNullOrWhiteSpace(payloadSymbol) && !string.Equals(payloadSymbol.Trim(), "A", StringComparison.OrdinalIgnoreCase))
        {
            return payloadSymbol.Trim();
        }

        if (!string.IsNullOrWhiteSpace(promptText))
        {
            var match = System.Text.RegularExpressions.Regex.Match(promptText, @"cách viết\s+([^\s!.,?]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        if (!string.IsNullOrWhiteSpace(itemTitle))
        {
            var match = System.Text.RegularExpressions.Regex.Match(itemTitle, @"(chữ số|chữ|số|nét)\s+([^\s!.,?]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var val = match.Groups[2].Value.Trim();
                if (!string.IsNullOrWhiteSpace(val))
                {
                    return val;
                }
            }
        }

        return string.IsNullOrWhiteSpace(payloadSymbol) ? "A" : payloadSymbol.Trim();
    }

    private static string ResolveLetterFlashcardUrl(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return string.Empty;
        }

        var trimmed = symbol.Trim().ToLowerInvariant();
        if (trimmed.Length == 1)
        {
            var ch = trimmed[0];
            if (ch is 'ă' or 'â' or 'đ' or 'ê' or 'ô' or 'ơ' or 'ư')
            {
                return $"/images/photos/flashcard-letter-{ch}.svg";
            }
            if (ch is >= 'a' and <= 'z')
            {
                return $"/images/photos/flashcard-letter-{ch}.jpg";
            }
        }

        return string.Empty;
    }

    private static string ResolveNumberFlashcardUrl(string symbol)
    {
        if (string.Equals(symbol?.Trim(), "0", StringComparison.OrdinalIgnoreCase))
        {
            return "/images/photos/flashcard-number-0.svg";
        }
        return int.TryParse(symbol, out var number) && number is >= 1 and <= 20
            ? $"/images/photos/flashcard-number-{number}.jpg"
            : string.Empty;
    }

    private static string ResolveTracingFlashcardUrl(string symbol)
    {
        var numberImageUrl = ResolveNumberFlashcardUrl(symbol);
        return string.IsNullOrWhiteSpace(numberImageUrl)
            ? ResolveLetterFlashcardUrl(symbol)
            : numberImageUrl;
    }

    private static string ResolveQuestionImageFromItemMedia(Question question)
    {
        var answer = LearningJsonReader.ReadCorrectAnswer(question.CorrectAnswerJson);
        if (string.IsNullOrWhiteSpace(answer) || answer.Contains('|', StringComparison.Ordinal))
        {
            return string.Empty;
        }

        var payload = JsonNode.Parse(question.PayloadJson)?.AsObject();
        if (payload?["itemMedia"] is not JsonObject itemMedia)
        {
            return string.Empty;
        }

        foreach (var key in ResolveMediaKeys(answer))
        {
            var match = itemMedia.FirstOrDefault(property =>
                string.Equals(property.Key, key, StringComparison.OrdinalIgnoreCase));
            if (match.Value is JsonValue value && value.TryGetValue<string>(out var imageUrl))
            {
                return imageUrl;
            }
        }

        return string.Empty;
    }

    private static IEnumerable<string> ResolveMediaKeys(string text)
    {
        var normalized = text.Trim();
        yield return normalized;

        foreach (var prefix in new[] { "Con ", "Chú ", "Cái ", "Quả " })
        {
            if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                yield return normalized[prefix.Length..];
            }
        }
    }

    private async Task<Guid?> FindNextItemIdAsync(LearningItem currentItem, Guid? skillGroupId)
    {
        if (!skillGroupId.HasValue)
        {
            return await FindNextItemIdInCurrentSessionAsync(currentItem.Id);
        }

        var items = await _db.LearningItems
            .Include(x => x.Topic)
            .Where(x => x.SkillGroupId == skillGroupId.Value && x.Status == ContentStatus.Published)
            .OrderBy(x => x.Topic!.SortOrder)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Level)
            .ThenBy(x => x.Title)
            .ToListAsync();
        var itemIds = items.Where(ActivityTemplateCatalog.IsItemAllowed).Select(x => x.Id).ToList();

        var currentIndex = itemIds.IndexOf(currentItem.Id);
        return currentIndex >= 0 && currentIndex + 1 < itemIds.Count
            ? itemIds[currentIndex + 1]
            : null;
    }
}
