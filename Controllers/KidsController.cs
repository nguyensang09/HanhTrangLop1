using HanhTrangLop1.Application.Learning;
using HanhTrangLop1.Data;
using HanhTrangLop1.Infrastructure;
using HanhTrangLop1.Models;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HanhTrangLop1.Controllers;

[Route("kids")]
public class KidsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly TodayLessonService _todayLessonService;

    public KidsController(ApplicationDbContext db, TodayLessonService todayLessonService)
    {
        _db = db;
        _todayLessonService = todayLessonService;
    }

    [HttpGet("")]
    [HttpGet("home")]
    public async Task<IActionResult> Home(Guid? childProfileId)
    {
        if (childProfileId.HasValue)
        {
            HttpContext.Session.SetString(SessionKeys.SelectedChildProfileId, childProfileId.Value.ToString());
        }

        var selectedProfileId = GetSelectedChildProfileId();
        var child = selectedProfileId.HasValue
            ? await _db.ChildProfiles.FirstOrDefaultAsync(x => x.Id == selectedProfileId.Value)
            : await _db.ChildProfiles.OrderBy(x => x.CreatedAt).FirstOrDefaultAsync();

        if (child is not null)
        {
            HttpContext.Session.SetString(SessionKeys.SelectedChildProfileId, child.Id.ToString());
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
        var items = await _db.LearningItems
            .Include(x => x.Topic)
            .Include(x => x.Questions)
            .Where(x => x.SkillGroupId == id && x.Status == ContentStatus.Published)
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Title)
            .ToListAsync();
        items = items.Where(ActivityTemplateCatalog.IsItemAllowed).ToList();

        var itemIds = items.Select(x => x.Id).ToList();
        var latestAttempts = child is null || itemIds.Count == 0
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
        var model = new LearnViewModel
        {
            Item = item,
            ChildProfile = await GetSelectedChildProfileAsync(),
            CurrentQuestion = question,
            Choices = question is null ? [] : LearningJsonReader.ReadChoices(question.PayloadJson),
            TracingSymbol = question is null ? "A" : LearningJsonReader.ReadStringProperty(question.PayloadJson, "symbol", "A"),
            TracingMinPoints = question is null ? 20 : LearningJsonReader.ReadIntProperty(question.CorrectAnswerJson, "minPoints", 20),
            TracingGuideMode = question is null ? "outline" : LearningJsonReader.ReadStringProperty(question.PayloadJson, "guideMode", "outline"),
            TracingExpectedStrokeCount = question is null ? 1 : LearningJsonReader.ReadIntProperty(question.PayloadJson, "expectedStrokeCount", 1),
            TracingShowStartPoint = question is null || LearningJsonReader.ReadBoolProperty(question.PayloadJson, "showStartPoint", true),
            TracingAudioUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "audioUrl", string.Empty),
            QuestionImageUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "imageUrl", string.Empty),
            NextItemId = await FindNextItemIdAsync(item, skillGroupId),
            ReturnSkillGroupId = skillGroupId
        };

        return View(model);
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
        var isCorrect = string.Equals(answer.AnswerValue, correctAnswer, StringComparison.OrdinalIgnoreCase);
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

        var model = new LearnViewModel
        {
            Item = item,
            ChildProfile = child,
            CurrentQuestion = question,
            Choices = LearningJsonReader.ReadChoices(question.PayloadJson),
            TracingSymbol = LearningJsonReader.ReadStringProperty(question.PayloadJson, "symbol", "A"),
            TracingMinPoints = LearningJsonReader.ReadIntProperty(question.CorrectAnswerJson, "minPoints", 20),
            QuestionImageUrl = LearningJsonReader.ReadStringProperty(question.PayloadJson, "imageUrl", string.Empty),
            FeedbackMessage = LearningJsonReader.ReadFeedback(question.FeedbackJson, isCorrect),
            IsCorrect = isCorrect,
            NextItemId = await FindNextItemIdAsync(item, skillGroupId),
            ReturnSkillGroupId = skillGroupId
        };

        return View("Learn", model);
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

        var nextItemId = await FindNextItemIdAsync(item, skillGroupId);
        if (nextItemId.HasValue)
        {
            return RedirectToAction(nameof(Learn), new { id = nextItemId.Value, skillGroupId });
        }

        if (skillGroupId.HasValue)
        {
            return RedirectToAction(nameof(Skill), new { id = skillGroupId.Value });
        }

        return RedirectToAction(nameof(Summary));
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
        if (selectedProfileId.HasValue)
        {
            return await _db.ChildProfiles.FirstOrDefaultAsync(x => x.Id == selectedProfileId.Value);
        }

        var firstChild = await _db.ChildProfiles.OrderBy(x => x.CreatedAt).FirstOrDefaultAsync();
        if (firstChild is not null)
        {
            HttpContext.Session.SetString(SessionKeys.SelectedChildProfileId, firstChild.Id.ToString());
        }

        return firstChild;
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

    private async Task<Guid?> FindNextItemIdAsync(LearningItem currentItem, Guid? skillGroupId)
    {
        if (!skillGroupId.HasValue)
        {
            return await FindNextItemIdInCurrentSessionAsync(currentItem.Id);
        }

        var items = await _db.LearningItems
            .Include(x => x.Topic)
            .Where(x => x.SkillGroupId == skillGroupId.Value && x.Status == ContentStatus.Published)
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Title)
            .ToListAsync();
        var itemIds = items.Where(ActivityTemplateCatalog.IsItemAllowed).Select(x => x.Id).ToList();

        var currentIndex = itemIds.IndexOf(currentItem.Id);
        return currentIndex >= 0 && currentIndex + 1 < itemIds.Count
            ? itemIds[currentIndex + 1]
            : null;
    }
}
