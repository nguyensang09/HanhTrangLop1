using HanhTrangLop1.Application.Learning;
using HanhTrangLop1.Application.Voice;
using HanhTrangLop1.Data;
using HanhTrangLop1.Infrastructure;
using HanhTrangLop1.Models;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HanhTrangLop1.Controllers;

[Route("kids")]
public class KidsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly TodayLessonService _todayLessonService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly VoiceLibraryMaintenanceService _voiceLibraryService;
    private readonly RewardProgressionService _rewardProgressionService;

    public KidsController(
        ApplicationDbContext db,
        TodayLessonService todayLessonService,
        UserManager<ApplicationUser> userManager,
        VoiceLibraryMaintenanceService voiceLibraryService,
        RewardProgressionService rewardProgressionService)
    {
        _db = db;
        _todayLessonService = todayLessonService;
        _userManager = userManager;
        _voiceLibraryService = voiceLibraryService;
        _rewardProgressionService = rewardProgressionService;
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
            .AsNoTracking()
            .Include(x => x.Topic)
            .Include(x => x.SkillGroup)
            .Where(x => x.Status == ContentStatus.Published)
            .OrderBy(x => x.SkillGroup!.SortOrder)
            .ToListAsync();

        var totalStars = await _db.ChildLessonProgresses
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch)
            .SumAsync(x => (int?)x.BestStars) ?? 0;

        var model = new KidsHomeViewModel
        {
            ChildProfile = child,
            SkillGroups = await _db.SkillGroups.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync(),
            TodayItems = todayItems.Where(ActivityTemplateCatalog.IsItemAllowed).Take(10).ToList(),
            Stars = Math.Max(totalStars, 0)
        };

        return View(model);
    }

    [HttpGet("today")]
    public async Task<IActionResult> Today(int? day)
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            return RedirectToAction("Index", "Profiles");
        }

        var currentDay = await _todayLessonService.GetCurrentDayNumberAsync(child);
        var targetDay = day ?? currentDay;

        var session = await _todayLessonService.GetOrCreateActiveSessionAsync(child, targetDay);
        HttpContext.Session.SetString(SessionKeys.CurrentLearningSessionId, session.Id.ToString());
        var model = await _todayLessonService.BuildTodayViewModelAsync(child, session, targetDay);

        return View(model);
    }

    [HttpGet("skills")]
    [HttpGet("skill")]
    public async Task<IActionResult> Skills()
    {
        var firstSkill = await _db.SkillGroups
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .FirstOrDefaultAsync();

        if (firstSkill is not null)
        {
            return RedirectToAction(nameof(Skill), new { id = firstSkill.Id });
        }

        return RedirectToAction(nameof(Home));
    }

    [HttpGet("skills/{id:guid}")]
    public async Task<IActionResult> Skill(Guid id)
    {
        var skillGroup = await _db.SkillGroups
            .AsNoTracking()
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
            .AsNoTracking()
            .Include(x => x.SkillGroup)
            .Include(x => x.Topic)
            .Include(x => x.Questions)
            .Where(x => x.SkillGroupId == id && x.Status == ContentStatus.Published)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Level)
            .ThenBy(x => x.Title)
            .ToListAsync();
        items = items.Where(ActivityTemplateCatalog.IsItemAllowed).ToList();

        var itemIds = items.Select(x => x.Id).ToList();
        var lessonProgress = itemIds.Count == 0
            ? []
            : await _db.ChildLessonProgresses
                .AsNoTracking()
                .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch && itemIds.Contains(x.LearningItemId))
                .ToListAsync();
        var progressByItemId = lessonProgress.ToDictionary(x => x.LearningItemId);
        var currentItem = items.FirstOrDefault(x => !progressByItemId.TryGetValue(x.Id, out var p) || p.FirstCompletedAt is null);
        var grants = await _db.RewardGrants.AsNoTracking()
            .Include(x => x.RewardDefinition)
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch && x.SkillGroupId == id)
            .ToDictionaryAsync(x => x.MilestoneValue);
        var milestones = Enumerable.Range(1, items.Count / RewardProgressionService.LessonsPerMilestone)
            .Select(n => n * RewardProgressionService.LessonsPerMilestone)
            .Select(value =>
            {
                grants.TryGetValue(value, out var grant);
                return new SkillMilestoneViewModel
                {
                    MilestoneValue = value,
                    State = grant?.State ?? "locked",
                    GrantId = grant?.Id,
                    RewardName = grant?.RewardDefinition?.Name ?? string.Empty,
                    RewardIconKey = grant?.RewardDefinition?.IconKey ?? "redeem"
                };
            }).ToList();

        var model = new SkillLearningListViewModel
        {
            ChildProfile = child,
            SkillGroup = skillGroup,
            LastPracticedItemId = lessonProgress.OrderByDescending(x => x.LastAttemptedAt).FirstOrDefault()?.LearningItemId,
            CurrentItemId = currentItem?.Id,
            Milestones = milestones,
            Items = items.Select(item =>
            {
                progressByItemId.TryGetValue(item.Id, out var progress);
                return new SkillLearningItemViewModel
                {
                    Item = item,
                    LatestStatus = progress?.LatestStatus,
                    StarsEarned = progress?.BestStars ?? 0,
                    EverCompleted = progress?.FirstCompletedAt is not null
                };
            }).ToList()
        };

        return View(model);
    }

    [HttpGet("tracing")]
    [HttpGet("tap-to")]
    public async Task<IActionResult> Tracing(string? tab = "all")
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("CreateProfile", "Parent");
            }
            return RedirectToAction("Index", "Profiles");
        }

        var tracingItems = await _db.LearningItems
            .AsNoTracking()
            .Include(x => x.Topic)
            .Include(x => x.SkillGroup)
            .Include(x => x.Questions)
            .Where(x => x.InteractionType == InteractionTypes.Tracing && x.Status == ContentStatus.Published)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync();

        var tracingItemIds = tracingItems.Select(x => x.Id).ToList();
        var progressLookup = await _db.ChildLessonProgresses
            .AsNoTracking()
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch && tracingItemIds.Contains(x.LearningItemId))
            .ToDictionaryAsync(x => x.LearningItemId);

        var basicStrokes = new List<KidsTracingItemViewModel>();
        var pictureTraces = new List<KidsTracingItemViewModel>();
        var upperLetters = new List<KidsTracingItemViewModel>();
        var lowerLetters = new List<KidsTracingItemViewModel>();
        var numbers = new List<KidsTracingItemViewModel>();

        foreach (var item in tracingItems)
        {
            var question = item.Questions.FirstOrDefault();
            var payloadSymbol = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "symbol", string.Empty);
            var symbol = ExtractTracingSymbol(payloadSymbol, item.Title, question?.PromptText);

            var progress = progressLookup.GetValueOrDefault(item.Id);
            var isCompleted = progress?.FirstCompletedAt is not null;
            var starsEarned = progress?.BestStars ?? (isCompleted ? 2 : 0);

            var topicCode = item.Topic?.Code?.ToLowerInvariant() ?? string.Empty;
            var titleLower = item.Title.ToLowerInvariant();
            var symbolLower = symbol.ToLowerInvariant();

            var isPicture = topicCode.Contains("tao-hinh") || topicCode.Contains("tranh") ||
                            titleLower.Contains("tranh") || titleLower.Contains("tạo hình") ||
                            symbolLower.Contains("phong-canh") || symbolLower.Contains("do-dung") ||
                            symbolLower.Contains("meo-con") || symbolLower.Contains("ca-heo") ||
                            symbolLower.Contains("o-che-mua") || symbolLower.Contains("hinh-hoc") ||
                            symbolLower.Contains("trai-tao") || symbolLower.Contains("tau-hoa") ||
                            item.Code.Contains("picture");

            if (isPicture)
            {
                pictureTraces.Add(new KidsTracingItemViewModel
                {
                    Item = item,
                    Symbol = symbol,
                    Title = item.Title,
                    CategoryCode = "picture",
                    IsCompleted = isCompleted,
                    StarsEarned = starsEarned
                });
            }
            else if (string.Equals(item.SkillGroup?.Code, "van-dong-tinh", StringComparison.OrdinalIgnoreCase) &&
                     (item.Code.Contains("motor-", StringComparison.OrdinalIgnoreCase) ||
                      topicCode.Contains("net") || titleLower.Contains("nét")))
            {
                basicStrokes.Add(new KidsTracingItemViewModel
                {
                    Item = item,
                    Symbol = symbol,
                    DisplaySymbol = ResolveBasicStrokeDisplaySymbol(item.Code),
                    Title = item.Title,
                    CategoryCode = "basic",
                    IsCompleted = isCompleted,
                    StarsEarned = starsEarned
                });
            }
            else if (topicCode.Contains("viet-so") || topicCode.Contains("so") || titleLower.Contains("tô số") || int.TryParse(symbol, out _))
            {
                numbers.Add(new KidsTracingItemViewModel
                {
                    Item = item,
                    Symbol = symbol,
                    Title = item.Title,
                    CategoryCode = "number",
                    IsCompleted = isCompleted,
                    StarsEarned = starsEarned
                });
            }
            else if (topicCode.Contains("chu-in-thuong") || titleLower.Contains("in thường") || (symbol.Length == 1 && char.IsLower(symbol[0])))
            {
                lowerLetters.Add(new KidsTracingItemViewModel
                {
                    Item = item,
                    Symbol = symbol,
                    Title = item.Title,
                    CategoryCode = "lower",
                    IsCompleted = isCompleted,
                    StarsEarned = starsEarned
                });
            }
            else
            {
                upperLetters.Add(new KidsTracingItemViewModel
                {
                    Item = item,
                    Symbol = symbol,
                    Title = item.Title,
                    CategoryCode = "upper",
                    IsCompleted = isCompleted,
                    StarsEarned = starsEarned
                });
            }
        }

        var model = new KidsTracingHubViewModel
        {
            ChildProfile = child,
            BasicStrokes = basicStrokes,
            PictureTraces = pictureTraces,
            UppercaseLetters = upperLetters,
            LowercaseLetters = lowerLetters,
            Numbers = numbers,
            ActiveTab = string.IsNullOrWhiteSpace(tab) ? "all" : tab.ToLowerInvariant()
        };

        return View(model);
    }

    private static string ResolveBasicStrokeDisplaySymbol(string itemCode) => itemCode switch
    {
        var code when code.EndsWith("motor-vertical", StringComparison.OrdinalIgnoreCase) => "│",
        var code when code.EndsWith("motor-horizontal", StringComparison.OrdinalIgnoreCase) => "─",
        var code when code.EndsWith("motor-diagonal-left", StringComparison.OrdinalIgnoreCase) => "/",
        var code when code.EndsWith("motor-diagonal-right", StringComparison.OrdinalIgnoreCase) => "\\",
        var code when code.EndsWith("motor-hook-forward", StringComparison.OrdinalIgnoreCase) => "⤵",
        var code when code.EndsWith("motor-hook-reverse", StringComparison.OrdinalIgnoreCase) => "⤴",
        var code when code.EndsWith("motor-double-hook", StringComparison.OrdinalIgnoreCase) => "∪",
        var code when code.EndsWith("motor-open-curve-right", StringComparison.OrdinalIgnoreCase) => "(",
        var code when code.EndsWith("motor-open-curve-left", StringComparison.OrdinalIgnoreCase) => ")",
        var code when code.EndsWith("motor-closed-curve", StringComparison.OrdinalIgnoreCase) => "○",
        var code when code.EndsWith("motor-upper-loop", StringComparison.OrdinalIgnoreCase) => "ℓ",
        var code when code.EndsWith("motor-lower-loop", StringComparison.OrdinalIgnoreCase) => "ɟ",
        var code when code.EndsWith("motor-knot", StringComparison.OrdinalIgnoreCase) => "∞",
        var code when code.EndsWith("net-ngang", StringComparison.OrdinalIgnoreCase) => "─",
        var code when code.EndsWith("net-doc", StringComparison.OrdinalIgnoreCase) => "│",
        var code when code.EndsWith("net-xien-trai", StringComparison.OrdinalIgnoreCase) => "/",
        var code when code.EndsWith("net-xien-phai", StringComparison.OrdinalIgnoreCase) => "\\",
        var code when code.EndsWith("net-cong-trai", StringComparison.OrdinalIgnoreCase) => "(",
        var code when code.EndsWith("net-cong-phai", StringComparison.OrdinalIgnoreCase) => ")",
        var code when code.EndsWith("net-moc-xuoi", StringComparison.OrdinalIgnoreCase) => "⤵",
        var code when code.EndsWith("net-moc-nguoc", StringComparison.OrdinalIgnoreCase) => "⤴",
        var code when code.EndsWith("net-khuyet-tren", StringComparison.OrdinalIgnoreCase) => "ℓ",
        var code when code.EndsWith("net-khuyet-duoi", StringComparison.OrdinalIgnoreCase) => "ɟ",
        var code when code.EndsWith("net-that", StringComparison.OrdinalIgnoreCase) => "∞",
        var code when code.EndsWith("net-vong", StringComparison.OrdinalIgnoreCase) => "○",
        _ => "〰"
    };

    [HttpGet("bilingual-listen")]
    public async Task<IActionResult> BilingualListen()
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("CreateProfile", "Parent");
            }
            return RedirectToAction("Index", "Profiles");
        }

        return View(child);
    }

    [HttpGet("bilingual-audio")]
    public async Task<IActionResult> GetBilingualAudio(string text, string lang = "vi", string? rate = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest(new { success = false, message = "Text is required" });
        }

        var audioUrl = await _voiceLibraryService.ResolveBilingualListenAudioUrlAsync(text, lang, cancellationToken);
        if (string.IsNullOrEmpty(audioUrl))
        {
            return NotFound(new { success = false, message = "Audio is not available in TextToSpeechCaches" });
        }

        return Json(new { success = true, audioUrl });
    }

    [HttpGet("learn/{id:guid}")]
    public async Task<IActionResult> Learn(Guid id, Guid? skillGroupId, bool fromTracing = false, bool completed = false)
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
        if (!skillGroupId.HasValue && !fromTracing)
        {
            var activeSession = await GetCurrentLearningSessionAsync(child, id);
            HttpContext.Session.SetString(SessionKeys.CurrentLearningSessionId, activeSession.Id.ToString());
        }

        var completionFeedback = completed
            ? question is null
                ? fromTracing ? "Con đã hoàn thành bài tô nét!" : "Con đã hoàn thành bài học!"
                : LearningJsonReader.ReadFeedback(question.FeedbackJson, true)
            : null;

        return View(await BuildLearnViewModelAsync(
            item,
            question,
            child,
            skillGroupId,
            completionFeedback,
            completed ? true : null,
            fromTracing: fromTracing));
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
        var session = await GetCurrentLearningSessionAsync(child, item.Id);
        HttpContext.Session.SetString(SessionKeys.CurrentLearningSessionId, session.Id.ToString());
        await using var completionTransaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

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
            DeviceInputType = "mouse",
            ProgressEpoch = child.ProgressEpoch
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

        await UpdateSkillProgressAsync(child.Id, item.SkillGroupId, item.Id, isCorrect);
        var rewardProgress = await _rewardProgressionService.RecordAttemptAsync(child, item, isCorrect, learningAttempt.StarsEarned, learningAttempt.Status);
        await _db.SaveChangesAsync();
        await completionTransaction.CommitAsync();
        if (rewardProgress.NewlyUnlockedGrant is not null)
            TempData["MilestoneMessage"] = $"Bé đã mở khóa rương {rewardProgress.UniqueCompletedInSkill} bài!";

        var feedback = LearningJsonReader.ReadFeedback(question.FeedbackJson, isCorrect);
        if (isCorrect)
        {
            return RedirectToAction(nameof(Learn), new { id = item.Id, skillGroupId, completed = true });
        }

        return View("Learn", await BuildLearnViewModelAsync(
            item,
            question,
            child,
            skillGroupId,
            feedback,
            isCorrect));
    }

    [HttpPost("learn/{id:guid}/complete-tracing")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteTracing(Guid id, SubmitTracingViewModel tracing, Guid? skillGroupId, bool fromTracing = false)
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
                x.InteractionType == InteractionTypes.Tracing &&
                (!skillGroupId.HasValue || x.SkillGroupId == skillGroupId.Value));
        if (item is null || !ActivityTemplateCatalog.IsItemAllowed(item))
        {
            return NotFound();
        }

        var session = await GetCurrentLearningSessionAsync(child, item.Id);
        HttpContext.Session.SetString(SessionKeys.CurrentLearningSessionId, session.Id.ToString());
        await using var completionTransaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

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
            DurationSeconds = 60,
            ProgressEpoch = child.ProgressEpoch
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

        await UpdateSkillProgressAsync(child.Id, item.SkillGroupId, item.Id, isCorrect: true);
        var rewardProgress = await _rewardProgressionService.RecordAttemptAsync(child, item, true, attempt.StarsEarned, attempt.Status);
        await _db.SaveChangesAsync();
        await completionTransaction.CommitAsync();
        if (rewardProgress.NewlyUnlockedGrant is not null)
            TempData["MilestoneMessage"] = $"Bé đã mở khóa rương {rewardProgress.UniqueCompletedInSkill} bài!";

        return RedirectToAction(nameof(Learn), new { id = item.Id, skillGroupId, fromTracing, completed = true });
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
        LearningSession? session = null;
        if (Guid.TryParse(sessionRaw, out var sessionId))
        {
            session = await _db.LearningSessions.FirstOrDefaultAsync(x => x.Id == sessionId && x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch);
        }

        if (session is null)
        {
            session = await _db.LearningSessions
                .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch)
                .OrderByDescending(x => x.StartedAt)
                .FirstOrDefaultAsync();
        }

        if (session is null)
        {
            return RedirectToAction(nameof(Today));
        }

        var attempts = await _db.LearningAttempts
            .Include(x => x.LearningItem)
            .Where(x => x.SessionId == session.Id)
            .OrderBy(x => x.StartedAt)
            .ToListAsync();

        var completedCount = attempts.Where(x => x.Status == "completed").Select(x => x.LearningItemId).Distinct().Count();
        var starsEarned = attempts.GroupBy(x => x.LearningItemId).Sum(g => g.Max(x => x.StarsEarned));
        var plannedItemCount = ReadSessionPlanIds(session.SessionPlanJson).Distinct().Count();
        if (session.Status == "active" && plannedItemCount > 0 && completedCount >= plannedItemCount)
        {
            await _todayLessonService.CompleteSessionAsync(session);
        }

        // Luồng cấp huy hiệu tự động khi hoàn thành bài học
        var newlyUnlocked = await EvaluateAndAwardBadgesAsync(child.Id, session, completedCount);
        var totalBadges = await _db.ChildRewards.CountAsync(x => x.ChildProfileId == child.Id);

        var model = new SessionSummaryViewModel
        {
            ChildProfile = child,
            Session = session,
            Attempts = attempts,
            CompletedItems = completedCount,
            NeedsPracticeItems = attempts.Count(x => x.Status == "needs_practice"),
            StarsEarned = starsEarned,
            NewlyUnlockedRewards = newlyUnlocked,
            TotalEarnedBadgesCount = totalBadges
        };

        return View(model);
    }

    [HttpGet("rewards")]
    public async Task<IActionResult> Rewards()
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            return RedirectToAction("Index", "Profiles");
        }

        var allRewards = await _db.RewardDefinitions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .ToListAsync();

        var earnedRewards = await _db.ChildRewards
            .AsNoTracking()
            .Where(x => x.ChildProfileId == child.Id)
            .ToDictionaryAsync(x => x.RewardDefinitionId);

        var totalStars = await _db.ChildLessonProgresses
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch)
            .SumAsync(x => (int?)x.BestStars) ?? 0;

        var milestoneGrants = await _db.RewardGrants.AsNoTracking()
            .Include(x => x.RewardDefinition)
            .Include(x => x.SkillGroup)
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch)
            .OrderByDescending(x => x.ClaimedAt ?? x.UnlockedAt)
            .ToListAsync();
        var groups = await _db.SkillGroups.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync();
        var lessonCounts = await _db.LearningItems.AsNoTracking()
            .Where(x => x.Status == ContentStatus.Published)
            .GroupBy(x => x.SkillGroupId)
            .Select(g => new { SkillGroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SkillGroupId, x => x.Count);
        var completionCounts = await _db.ChildLessonProgresses.AsNoTracking()
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch && x.FirstCompletedAt != null)
            .GroupBy(x => x.LearningItem!.SkillGroupId)
            .Select(g => new { SkillGroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SkillGroupId, x => x.Count);

        var model = new KidsRewardsViewModel
        {
            ChildProfile = child,
            TotalStars = totalStars,
            ClaimableGrants = milestoneGrants
                .Where(x => x.State == RewardGrantStates.Claimable)
                .OrderBy(x => x.UnlockedAt)
                .Select(x => new RewardGrantItemViewModel
            {
                Grant = x,
                Reward = x.RewardDefinition!,
                SkillName = x.SkillGroup?.Name ?? "Hành trình học tập"
            }).ToList(),
            RecentClaimedGrants = milestoneGrants
                .Where(x => x.State == RewardGrantStates.Claimed)
                .Take(5)
                .Select(x => new RewardGrantItemViewModel
                {
                    Grant = x,
                    Reward = x.RewardDefinition!,
                    SkillName = x.SkillGroup?.Name ?? "Hành trình học tập"
                }).ToList(),
            SkillProgress = groups.Select(group =>
            {
                completionCounts.TryGetValue(group.Id, out var completed);
                lessonCounts.TryGetValue(group.Id, out var total);
                return new SkillRewardProgressViewModel
                {
                    SkillGroupId = group.Id,
                    SkillName = group.Name,
                    IconKey = group.IconKey,
                    Color = group.Color,
                    UniqueCompleted = completed,
                    TotalLessons = total,
                    CurrentSegmentProgress = completed % RewardProgressionService.LessonsPerMilestone,
                    NextMilestone = (completed / RewardProgressionService.LessonsPerMilestone + 1) * RewardProgressionService.LessonsPerMilestone
                };
            }).ToList(),
            Badges = allRewards.Select(r => new RewardItemViewModel
            {
                Definition = r,
                IsEarned = earnedRewards.ContainsKey(r.Id),
                EarnedAt = earnedRewards.TryGetValue(r.Id, out var cr) ? cr.EarnedAt : null
            }).ToList()
        };

        return View(model);
    }

    [HttpPost("rewards/grants/{grantId:guid}/claim")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClaimReward(Guid grantId)
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null) return RedirectToAction("Index", "Profiles");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var grant = await _db.RewardGrants
            .AsNoTracking()
            .Include(x => x.RewardDefinition)
            .FirstOrDefaultAsync(x => x.Id == grantId && x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch);
        if (grant is null) return NotFound();
        var claimedAt = DateTimeOffset.UtcNow;
        var claimedRows = await _db.RewardGrants
            .Where(x => x.Id == grantId && x.ChildProfileId == child.Id &&
                        x.ProgressEpoch == child.ProgressEpoch && x.State == RewardGrantStates.Claimable)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.State, RewardGrantStates.Claimed)
                .SetProperty(x => x.ClaimedAt, claimedAt));
        if (claimedRows == 0)
        {
            await transaction.RollbackAsync();
            return RedirectToAction(nameof(Rewards));
        }

        if (!await _db.ChildRewards.AnyAsync(x => x.ChildProfileId == child.Id && x.RewardDefinitionId == grant.RewardDefinitionId))
        {
            _db.ChildRewards.Add(new ChildReward
            {
                Id = Guid.NewGuid(),
                ChildProfileId = child.Id,
                RewardDefinitionId = grant.RewardDefinitionId,
                EarnedAt = claimedAt
            });
        }

        if (grant.RewardDefinition?.RewardType == "item")
        {
            var inventory = await _db.ChildInventoryItems.FirstOrDefaultAsync(x =>
                x.ChildProfileId == child.Id && x.RewardDefinitionId == grant.RewardDefinitionId);
            if (inventory is null)
            {
                inventory = new ChildInventoryItem
                {
                    Id = Guid.NewGuid(), ChildProfileId = child.Id,
                    RewardDefinitionId = grant.RewardDefinitionId, Quantity = 0
                };
                _db.ChildInventoryItems.Add(inventory);
            }
            inventory.Quantity++;
            inventory.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["RewardMessage"] = $"Đã nhận {grant.RewardDefinition?.Name ?? "quà tặng"}!";
        return RedirectToAction(nameof(Rewards));
    }

    private async Task<List<RewardDefinition>> EvaluateAndAwardBadgesAsync(Guid childProfileId, LearningSession session, int completedCount)
    {
        var earnedIds = await _db.ChildRewards
            .AsNoTracking()
            .Where(x => x.ChildProfileId == childProfileId)
            .Select(x => x.RewardDefinitionId)
            .ToHashSetAsync();

        var allRewards = await _db.RewardDefinitions
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Code);
        var newlyAwarded = new List<RewardDefinition>();

        void TryAward(string code)
        {
            if (allRewards.TryGetValue(code, out var reward) && !earnedIds.Contains(reward.Id))
            {
                _db.ChildRewards.Add(new ChildReward
                {
                    Id = Guid.NewGuid(),
                    ChildProfileId = childProfileId,
                    RewardDefinitionId = reward.Id,
                    EarnedAt = DateTimeOffset.UtcNow
                });
                earnedIds.Add(reward.Id);
                newlyAwarded.Add(reward);
            }
        }

        var uniqueProgress = await _db.ChildLessonProgresses
            .AsNoTracking()
            .Include(x => x.LearningItem)
            .Where(x => x.ChildProfileId == childProfileId && x.ProgressEpoch == session.ProgressEpoch && x.FirstCompletedAt != null)
            .ToListAsync();
        var totalStars = uniqueProgress.Sum(x => x.BestStars);
        var completedSessionDates = (await _db.LearningSessions.AsNoTracking()
            .Where(x => x.ChildProfileId == childProfileId && x.ProgressEpoch == session.ProgressEpoch &&
                        x.Status == "completed" && x.EndedAt != null)
            .Select(x => x.EndedAt!.Value)
            .ToListAsync())
            .Select(x => x.Date)
            .Distinct()
            .OrderByDescending(x => x)
            .ToList();
        var streakDays = 0;
        if (completedSessionDates.Count > 0)
        {
            var expectedDate = completedSessionDates[0];
            foreach (var date in completedSessionDates)
            {
                if (date != expectedDate) break;
                streakDays++;
                expectedDate = expectedDate.AddDays(-1);
            }
        }

        // 1. Bước chân đầu tiên: Hoàn thành bài học đầu tiên
        if (uniqueProgress.Count >= 1)
        {
            TryAward("badge-first-step");
        }

        // 2. Chiến binh chăm chỉ: Hoàn thành buổi học hôm nay
        var plannedItemCount = ReadSessionPlanIds(session.SessionPlanJson).Distinct().Count();
        if (session.Status == "completed" && plannedItemCount > 0 && completedCount >= plannedItemCount)
        {
            TryAward("badge-daily-champion");
        }

        // 3. Chuỗi học tập
        if (streakDays >= 3) TryAward("badge-streak-3d");
        if (streakDays >= 7) TryAward("badge-streak-7d");

        // 4. Mốc số sao
        if (totalStars >= 10) TryAward("badge-super-scholar");
        if (totalStars >= 25) TryAward("badge-star-collector");

        // 5. Huy hiệu chinh phục nhóm chỉ mở khi hoàn thành toàn bộ bài đang xuất bản.
        var publishedBySkill = await _db.LearningItems.AsNoTracking()
            .Where(x => x.Status == ContentStatus.Published)
            .GroupBy(x => x.SkillGroupId)
            .Select(g => new { SkillGroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SkillGroupId, x => x.Count);
        var completedBySkill = uniqueProgress
            .Where(x => x.LearningItem is not null)
            .GroupBy(x => x.LearningItem!.SkillGroupId)
            .ToDictionary(g => g.Key, g => g.Count());
        var groupIdsByCode = await _db.SkillGroups.AsNoTracking().ToDictionaryAsync(x => x.Code, x => x.Id);

        bool CompletedGroup(string code) =>
            groupIdsByCode.TryGetValue(code, out var groupId) &&
            publishedBySkill.TryGetValue(groupId, out var total) && total > 0 &&
            completedBySkill.GetValueOrDefault(groupId) >= total;

        if (CompletedGroup("chu-cai")) TryAward("badge-alphabet-star");
        if (CompletedGroup("chu-so")) TryAward("badge-number-explorer");
        if (CompletedGroup("so-luong-toan")) TryAward("badge-math-whiz");
        if (CompletedGroup("tu-duy-logic")) TryAward("badge-logic-explorer");
        if (CompletedGroup("ky-nang-song")) TryAward("badge-habit-hero");
        if (CompletedGroup("ngon-ngu")) TryAward("badge-story-teller");
        if (CompletedGroup("hinh-dang-khong-gian")) TryAward("badge-shape-master");
        if (CompletedGroup("ghi-nho-tap-trung")) TryAward("badge-focus-star");
        if (CompletedGroup("van-dong-tinh")) TryAward("badge-handwriting-hero");
        if (CompletedGroup("kham-pha")) TryAward("badge-world-explorer");

        if (newlyAwarded.Count > 0)
        {
            await _db.SaveChangesAsync();
        }

        return newlyAwarded;
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

    private async Task UpdateSkillProgressAsync(Guid childProfileId, Guid skillGroupId, Guid learningItemId, bool isCorrect)
    {
        var isFirstCompletion = isCorrect && !await _db.ChildLessonProgresses.AnyAsync(x =>
            x.ChildProfileId == childProfileId && x.LearningItemId == learningItemId && x.FirstCompletedAt != null);
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

        progress.CompletedItems += isFirstCompletion ? 1 : 0;
        progress.NeedsPracticeItems += isCorrect ? 0 : 1;
        progress.MasteryLevel = Math.Min(100, progress.MasteryLevel + (isCorrect ? 8 : 2));
        progress.LastPracticedAt = DateTimeOffset.UtcNow;
        progress.SummaryJson = JsonSerializer.Serialize(new
        {
            lastResult = isCorrect ? "Hoàn thành tốt" : "Cần luyện thêm",
            updatedAt = DateTimeOffset.UtcNow
        });
    }

    private static IReadOnlyList<Guid> ReadSessionPlanIds(string? sessionPlanJson)
    {
        if (string.IsNullOrWhiteSpace(sessionPlanJson)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(sessionPlanJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private async Task<LearningSession> GetCurrentLearningSessionAsync(ChildProfile child, Guid? currentItemId = null)
    {
        var sessionRaw = HttpContext.Session.GetString(SessionKeys.CurrentLearningSessionId);
        if (Guid.TryParse(sessionRaw, out var sessionId))
        {
            var session = await _db.LearningSessions.FirstOrDefaultAsync(x => x.Id == sessionId && x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch);
            if (session is not null)
            {
                if (!currentItemId.HasValue || session.SessionPlanJson.Contains(currentItemId.Value.ToString()))
                {
                    return session;
                }
            }
        }

        if (currentItemId.HasValue)
        {
            var matchingSession = await _db.LearningSessions
                .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch && x.SessionPlanJson.Contains(currentItemId.Value.ToString()))
                .OrderByDescending(x => x.StartedAt)
                .FirstOrDefaultAsync();

            if (matchingSession is not null)
            {
                HttpContext.Session.SetString(SessionKeys.CurrentLearningSessionId, matchingSession.Id.ToString());
                return matchingSession;
            }
        }

        var currentDay = await _todayLessonService.GetCurrentDayNumberAsync(child);
        var defaultSession = await _todayLessonService.GetOrCreateActiveSessionAsync(child, currentDay);
        HttpContext.Session.SetString(SessionKeys.CurrentLearningSessionId, defaultSession.Id.ToString());
        return defaultSession;
    }

    private async Task<Guid?> FindNextItemIdInCurrentSessionAsync(Guid currentItemId)
    {
        var child = await GetSelectedChildProfileAsync();
        if (child is null)
        {
            return null;
        }

        var session = await GetCurrentLearningSessionAsync(child, currentItemId);
        return await _todayLessonService.FindNextItemIdAsync(session, currentItemId);
    }

    private async Task<Guid?> FindNextTracingItemIdAsync(Guid currentItemId)
    {
        var currentItem = await _db.LearningItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == currentItemId);
        if (currentItem is null)
        {
            return null;
        }

        var allTracingItems = await _db.LearningItems
            .Where(x => x.InteractionType == InteractionTypes.Tracing && x.Status == ContentStatus.Published)
            .OrderBy(x => x.TopicId == currentItem.TopicId ? 0 : 1)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .Select(x => x.Id)
            .ToListAsync();

        var index = allTracingItems.IndexOf(currentItemId);
        if (index >= 0 && index + 1 < allTracingItems.Count)
        {
            return allTracingItems[index + 1];
        }

        return null;
    }

    private async Task<LearnViewModel> BuildLearnViewModelAsync(
        LearningItem item,
        Question? question,
        ChildProfile? child,
        Guid? skillGroupId,
        string? feedbackMessage = null,
        bool? isCorrect = null,
        bool fromTracing = false)
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

        var promptText = question?.PromptText;
        var speechText = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "speechText", string.Empty);
        var correctFeedbackText = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.FeedbackJson, "correct", string.Empty);
        if (string.IsNullOrWhiteSpace(correctFeedbackText)) correctFeedbackText = "Giỏi lắm";
        var retryFeedbackText = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.FeedbackJson, "retry", string.Empty);
        if (string.IsNullOrWhiteSpace(retryFeedbackText)) retryFeedbackText = "Con thử lại nhé";

        var questionAudioUrl = await ResolveActiveVoiceUrlAsync(promptText);
        if (string.IsNullOrWhiteSpace(questionAudioUrl)) questionAudioUrl = await ResolveActiveVoiceUrlAsync(item.Title);
        var questionAudioUrlEn = await ResolveActiveVoiceUrlEnAsync(promptText);
        if (string.IsNullOrWhiteSpace(questionAudioUrlEn)) questionAudioUrlEn = await ResolveActiveVoiceUrlEnAsync(item.Title);
        var contentAudioUrl = item.InteractionType == InteractionTypes.StoryChoice
            ? await ResolveActiveVoiceUrlAsync(speechText)
            : string.Empty;
        var contentAudioUrlEn = item.InteractionType == InteractionTypes.StoryChoice
            ? await ResolveActiveVoiceUrlEnAsync(speechText)
            : string.Empty;
        var titleAudioUrl = questionAudioUrl;
        var titleAudioUrlEn = questionAudioUrlEn;
        var instructionAudioUrl = questionAudioUrl;
        var instructionAudioUrlEn = questionAudioUrlEn;
        var tracingAudioUrl = questionAudioUrl;
        var tracingAudioUrlEn = questionAudioUrlEn;
        var correctFeedbackAudioUrl = await ResolveActiveVoiceUrlAsync(correctFeedbackText);
        var correctFeedbackAudioUrlEn = await ResolveActiveVoiceUrlEnAsync(correctFeedbackText);
        var retryFeedbackAudioUrl = await ResolveActiveVoiceUrlAsync(retryFeedbackText);
        var retryFeedbackAudioUrlEn = await ResolveActiveVoiceUrlEnAsync(retryFeedbackText);

        if (question is not null)
        {
            question.PayloadJson = await EnrichPayloadOptionAudioAsync(question.PayloadJson);
            var payload = System.Text.Json.Nodes.JsonNode.Parse(question.PayloadJson)?.AsObject() ?? new System.Text.Json.Nodes.JsonObject();
            payload["questionAudioUrl"] = questionAudioUrl;
            payload["questionAudioUrlEn"] = questionAudioUrlEn;
            payload["correctAudioUrl"] = correctFeedbackAudioUrl;
            payload["correctAudioUrlEn"] = correctFeedbackAudioUrlEn;
            payload["retryAudioUrl"] = retryFeedbackAudioUrl;
            payload["retryAudioUrlEn"] = retryFeedbackAudioUrlEn;
            if (item.InteractionType == InteractionTypes.Tracing)
            {
                payload["audioUrl"] = tracingAudioUrl;
                payload["audioUrlEn"] = tracingAudioUrlEn;
            }
            else if (item.InteractionType == InteractionTypes.ListenAndChoose)
            {
                payload["speechText"] = string.Empty;
                payload["speechTextEn"] = string.Empty;
                payload["audioUrl"] = string.Empty;
                payload["audioUrlEn"] = string.Empty;
            }
            else if (item.InteractionType == InteractionTypes.StoryChoice)
            {
                payload["audioUrl"] = !string.IsNullOrWhiteSpace(contentAudioUrl) ? contentAudioUrl : questionAudioUrl;
                payload["audioUrlEn"] = !string.IsNullOrWhiteSpace(contentAudioUrlEn) ? contentAudioUrlEn : questionAudioUrlEn;
            }
            question.PayloadJson = payload.ToJsonString();
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
            TracingAudioUrl = tracingAudioUrl,
            TracingAudioUrlEn = tracingAudioUrlEn,
            QuestionImageUrl = questionImageUrl,
            QuestionImageAltText = question is null ? "Hình minh họa bài học" : LearningJsonReader.ReadStringProperty(question.PayloadJson, "imageAltText", "Hình minh họa bài học"),
            TitleAudioUrl = titleAudioUrl,
            TitleAudioUrlEn = titleAudioUrlEn,
            QuestionAudioUrl = questionAudioUrl,
            QuestionAudioUrlEn = questionAudioUrlEn,
            InstructionAudioUrl = instructionAudioUrl,
            InstructionAudioUrlEn = instructionAudioUrlEn,
            CorrectFeedbackAudioUrl = correctFeedbackAudioUrl,
            CorrectFeedbackAudioUrlEn = correctFeedbackAudioUrlEn,
            RetryFeedbackAudioUrl = retryFeedbackAudioUrl,
            RetryFeedbackAudioUrlEn = retryFeedbackAudioUrlEn,
            EnglishVoiceEnabled = child?.EnglishVoice == true,
            FeedbackMessage = feedbackMessage,
            IsCorrect = isCorrect,
            NextItemId = fromTracing
                ? await FindNextTracingItemIdAsync(item.Id)
                : await FindNextItemIdAsync(item, skillGroupId),
            ReturnSkillGroupId = skillGroupId,
            FromTracing = fromTracing
        };
    }

    private async Task<string> ResolveActiveVoiceUrlAsync(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var url = await _voiceLibraryService.ResolveVoiceAudioUrlAsync(text, HttpContext.RequestAborted);
        return url ?? string.Empty;
    }

    private async Task<string> ResolveActiveVoiceUrlEnAsync(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var url = await _voiceLibraryService.ResolveVoiceAudioUrlEnAsync(text, HttpContext.RequestAborted);
        return url ?? string.Empty;
    }

    private async Task<string> EnrichPayloadOptionAudioAsync(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson)) return "{}";
        try
        {
            var payload = System.Text.Json.Nodes.JsonNode.Parse(payloadJson)?.AsObject();
            if (payload is null) return payloadJson;

            var audioMap = new System.Text.Json.Nodes.JsonObject();
            var audioMapEn = new System.Text.Json.Nodes.JsonObject();
            var labels = CollectOptionLabelsFromPayload(payload).ToList();

            foreach (var label in labels)
            {
                var cleanLabel = label.Trim();
                var url = await ResolveActiveVoiceUrlAsync(label);
                if (!string.IsNullOrWhiteSpace(url))
                {
                    audioMap[cleanLabel] = url;
                    if (!string.Equals(cleanLabel, label, StringComparison.Ordinal))
                    {
                        audioMap[label] = url;
                    }
                }

                var urlEn = await ResolveActiveVoiceUrlEnAsync(label);
                if (!string.IsNullOrWhiteSpace(urlEn))
                {
                    audioMapEn[cleanLabel] = urlEn;
                    if (!string.Equals(cleanLabel, label, StringComparison.Ordinal))
                    {
                        audioMapEn[label] = urlEn;
                    }
                }
            }

            payload["optionAudio"] = audioMap;
            payload["optionAudioEn"] = audioMapEn;
            return payload.ToJsonString();
        }
        catch
        {
            return payloadJson ?? "{}";
        }
    }

    private static IEnumerable<string> CollectOptionLabelsFromPayload(System.Text.Json.Nodes.JsonObject payload)
    {
        if (payload.TryGetPropertyValue("choices", out var chNode) && chNode is System.Text.Json.Nodes.JsonArray chArr)
        {
            foreach (var item in chArr) if (item != null) yield return item.ToString();
        }
        if (payload.TryGetPropertyValue("items", out var itNode) && itNode is System.Text.Json.Nodes.JsonArray itArr)
        {
            foreach (var item in itArr) if (item != null) yield return item.ToString();
        }
        if (payload.TryGetPropertyValue("categories", out var catNode) && catNode is System.Text.Json.Nodes.JsonArray catArr)
        {
            foreach (var item in catArr) if (item != null) yield return item.ToString();
        }
        if (payload.TryGetPropertyValue("pairs", out var pNode) && pNode is System.Text.Json.Nodes.JsonArray pArr)
        {
            foreach (var item in pArr)
            {
                if (item is System.Text.Json.Nodes.JsonObject obj)
                {
                    if (obj.TryGetPropertyValue("left", out var l) && l != null) yield return l.ToString();
                    if (obj.TryGetPropertyValue("right", out var r) && r != null) yield return r.ToString();
                }
            }
        }
        if (payload.TryGetPropertyValue("mappings", out var mNode) && mNode is System.Text.Json.Nodes.JsonArray mArr)
        {
            foreach (var item in mArr)
            {
                if (item is System.Text.Json.Nodes.JsonObject obj)
                {
                    if (obj.TryGetPropertyValue("left", out var l) && l != null) yield return l.ToString();
                    if (obj.TryGetPropertyValue("right", out var r) && r != null) yield return r.ToString();
                }
            }
        }
        if (payload.TryGetPropertyValue("targetLabel", out var tlNode) && tlNode != null && !string.IsNullOrWhiteSpace(tlNode.ToString()))
        {
            yield return tlNode.ToString();
        }
    }

    private static string ExtractTracingSymbol(string? payloadSymbol, string? itemTitle, string? promptText)
    {
        if (!string.IsNullOrWhiteSpace(payloadSymbol))
        {
            return payloadSymbol.Trim();
        }

        if (!string.IsNullOrWhiteSpace(itemTitle))
        {
            if (itemTitle.Contains("tranh", StringComparison.OrdinalIgnoreCase) ||
                itemTitle.Contains("phong cảnh", StringComparison.OrdinalIgnoreCase) ||
                itemTitle.Contains("đồ dùng", StringComparison.OrdinalIgnoreCase) ||
                itemTitle.Contains("mèo", StringComparison.OrdinalIgnoreCase) ||
                itemTitle.Contains("cá heo", StringComparison.OrdinalIgnoreCase) ||
                itemTitle.Contains("ô che mưa", StringComparison.OrdinalIgnoreCase) ||
                itemTitle.Contains("hình học", StringComparison.OrdinalIgnoreCase) ||
                itemTitle.Contains("táo", StringComparison.OrdinalIgnoreCase) ||
                itemTitle.Contains("tàu hỏa", StringComparison.OrdinalIgnoreCase))
            {
                return itemTitle.Trim();
            }

            var match = System.Text.RegularExpressions.Regex.Match(itemTitle, @"(?:chữ số|chữ|số|nét|hình)\s+([A-Za-zÀ-ỹ0-9\s]+?)(?:\s+in\s+hoa|\s+in\s+thường|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var val = match.Groups[1].Value.Trim();
                if (!string.IsNullOrWhiteSpace(val))
                {
                    return val;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(promptText))
        {
            var match = System.Text.RegularExpressions.Regex.Match(promptText, @"cách viết\s+([A-Za-zÀ-ỹ0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return "A";
    }

    private static string ResolveLetterFlashcardUrl(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return string.Empty;
        }

        return LearningContentSeed.ResolveLetterFlashcardUrl(symbol);
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
        return LearningContentSeed.ResolveTracingFlashcardUrl(symbol);
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
            .Include(x => x.SkillGroup)
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
