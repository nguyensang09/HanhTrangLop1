using HanhTrangLop1.Application.Learning;
using HanhTrangLop1.Application.Voice;
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
    private readonly VoiceLibraryMaintenanceService _voiceLibraryService;

    public KidsController(
        ApplicationDbContext db,
        TodayLessonService todayLessonService,
        UserManager<ApplicationUser> userManager,
        VoiceLibraryMaintenanceService voiceLibraryService)
    {
        _db = db;
        _todayLessonService = todayLessonService;
        _userManager = userManager;
        _voiceLibraryService = voiceLibraryService;
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

        var totalStars = await _db.LearningAttempts
            .Where(x => x.ChildProfileId == child.Id)
            .SumAsync(x => (int?)x.StarsEarned) ?? 0;

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
                .AsNoTracking()
                .Where(x => x.ChildProfileId == child.Id && itemIds.Contains(x.LearningItemId))
                .OrderByDescending(x => x.StartedAt)
                .ToListAsync();

        var mostRecentAttempt = latestAttempts.OrderByDescending(x => x.CompletedAt ?? x.StartedAt).FirstOrDefault();
        var latestAttemptByItemId = latestAttempts
            .GroupBy(x => x.LearningItemId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.StartedAt).First());

        var model = new SkillLearningListViewModel
        {
            ChildProfile = child,
            SkillGroup = skillGroup,
            LastPracticedItemId = mostRecentAttempt?.LearningItemId,
            Items = items.Select(item =>
            {
                latestAttemptByItemId.TryGetValue(item.Id, out var latestAttempt);
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

        var attempts = await _db.LearningAttempts
            .AsNoTracking()
            .Where(x => x.ChildProfileId == child.Id)
            .ToListAsync();

        var attemptLookup = attempts
            .GroupBy(x => x.LearningItemId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(a => a.StartedAt).First()
            );

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

            var attempt = attemptLookup.GetValueOrDefault(item.Id);
            var isCompleted = attempt?.Status == "completed";
            var starsEarned = attempt?.StarsEarned ?? (isCompleted ? 2 : 0);

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
            else if (topicCode.Contains("net") || titleLower.Contains("nét"))
            {
                basicStrokes.Add(new KidsTracingItemViewModel
                {
                    Item = item,
                    Symbol = symbol,
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

        var audioUrl = await _voiceLibraryService.EnsureAudioFileAsync(text, lang, rate, cancellationToken);
        if (string.IsNullOrEmpty(audioUrl))
        {
            return NotFound(new { success = false, message = "Could not generate audio" });
        }

        return Json(new { success = true, audioUrl });
    }

    [HttpGet("learn/{id:guid}")]
    public async Task<IActionResult> Learn(Guid id, Guid? skillGroupId, bool fromTracing = false)
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

        return View(await BuildLearnViewModelAsync(
            item,
            question,
            child,
            skillGroupId,
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
    public async Task<IActionResult> CompleteTracing(Guid id, SubmitTracingViewModel tracing, Guid? skillGroupId, bool fromTracing = false)
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

        var session = await GetCurrentLearningSessionAsync(child, item.Id);
        HttpContext.Session.SetString(SessionKeys.CurrentLearningSessionId, session.Id.ToString());

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
            true,
            fromTracing: fromTracing));
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
            session = await _db.LearningSessions.FirstOrDefaultAsync(x => x.Id == sessionId && x.ChildProfileId == child.Id);
        }

        if (session is null)
        {
            session = await _db.LearningSessions
                .Where(x => x.ChildProfileId == child.Id)
                .OrderByDescending(x => x.StartedAt)
                .FirstOrDefaultAsync();
        }

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

        var completedCount = attempts.Count(x => x.Status == "completed");
        var starsEarned = attempts.Sum(x => x.StarsEarned);

        // Luồng cấp huy hiệu tự động khi hoàn thành bài học
        var newlyUnlocked = await EvaluateAndAwardBadgesAsync(child.Id, session, completedCount, starsEarned);
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

        await EnsureDefaultRewardsExistAsync();

        var allRewards = await _db.RewardDefinitions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .ToListAsync();

        var earnedRewards = await _db.ChildRewards
            .AsNoTracking()
            .Where(x => x.ChildProfileId == child.Id)
            .ToDictionaryAsync(x => x.RewardDefinitionId);

        var totalStars = await _db.LearningAttempts
            .Where(x => x.ChildProfileId == child.Id)
            .SumAsync(x => x.StarsEarned);

        var model = new KidsRewardsViewModel
        {
            ChildProfile = child,
            TotalStars = totalStars,
            Badges = allRewards.Select(r => new RewardItemViewModel
            {
                Definition = r,
                IsEarned = earnedRewards.ContainsKey(r.Id),
                EarnedAt = earnedRewards.TryGetValue(r.Id, out var cr) ? cr.EarnedAt : null
            }).ToList()
        };

        return View(model);
    }

    private async Task EnsureDefaultRewardsExistAsync()
    {
        var rewardSeeds = new (string Code, string Name, string Type, string Icon, string Rule)[]
        {
            // Huy hiệu tiến trình & chuyên cần
            ("badge-first-step", "Bước Chân Đầu Tiên", "badge", "hotel_class", "Hoàn thành bài học đầu tiên"),
            ("badge-daily-champion", "Chiến Binh Chăm Chỉ", "badge", "military_tech", "Hoàn thành trọn vẹn buổi học hôm nay"),
            ("badge-streak-3d", "Ong Vàng Siêng Năng", "badge", "local_fire_department", "Hoàn thành 3 ngày học liên tiếp"),
            ("badge-streak-7d", "Bậc Thầy Chuyên Cần", "badge", "workspace_premium", "Kiên trì học 7 ngày cùng Sóc Nâu"),
            ("badge-super-scholar", "Đại Sứ Sóc Nâu", "badge", "emoji_events", "Tích lũy trên 10 ngôi sao vàng"),
            ("badge-star-collector", "Nhà Sưu Tầm Sao", "badge", "stars", "Tích lũy trên 25 ngôi sao vàng"),

            // Huy hiệu nhóm kỹ năng
            ("badge-alphabet-star", "Ngôi Sao Chữ Cái", "badge", "menu_book", "Chinh phục các bài học chữ cái tiếng Việt"),
            ("badge-handwriting-hero", "Bàn Tay Khéo Léo", "badge", "edit", "Hoàn thành các bài luyện tô nét chữ chuẩn"),
            ("badge-math-whiz", "Nhà Toán Học Nhí", "badge", "calculate", "Làm quen các con số và đếm số lượng"),
            ("badge-logic-explorer", "Thám Tử Thông Minh", "badge", "psychology", "Vượt qua các câu đố tư duy logic"),
            ("badge-habit-hero", "Bé Ngoan Tự Lập", "badge", "volunteer_activism", "Học tốt các kỹ năng sống và thói quen"),
            ("badge-story-teller", "Nhà Kể Chuyện Nhí", "badge", "auto_stories", "Mở rộng vốn từ và nghe hiểu câu chuyện"),
            ("badge-shape-master", "Kiến Trúc Sư Tí Hon", "badge", "category", "Phân biệt thành thạo các hình khối và không gian"),

            // Vật phẩm trang trí khu vườn của bé
            ("item-golden-acorn", "Quả Sồi Hoàng Gia", "item", "nature", "Vật phẩm quý giá nhận khi chăm chỉ học tập"),
            ("item-magic-pencil", "Bút Chì Cầu Vồng", "item", "draw", "Bút chì thần kỳ tô điểm những nét chữ đẹp"),
            ("item-knowledge-tree", "Cây Tri Thức 3D", "item", "park", "Khu vườn nở hoa khi bé học thêm nhiều điều mới"),
            ("item-tiny-crown", "Vương Miện Tí Hon", "item", "royalty", "Vương miện vinh danh bạn nhỏ xuất sắc"),
            ("item-trophy-gold", "Cúp Sóc Nâu Danh Dự", "item", "trophy", "Cúp vàng cao quý nhất của trường mầm non Sóc Nâu")
        };

        var existing = await _db.RewardDefinitions.ToDictionaryAsync(x => x.Code);
        var added = false;
        foreach (var (code, name, type, icon, rule) in rewardSeeds)
        {
            if (!existing.TryGetValue(code, out var item))
            {
                item = new RewardDefinition
                {
                    Id = Guid.NewGuid(),
                    Code = code
                };
                _db.RewardDefinitions.Add(item);
                added = true;
            }
            item.Name = name;
            item.RewardType = type;
            item.IconKey = icon;
            item.RuleJson = rule;
            item.IsActive = true;
        }

        if (added || existing.Count < rewardSeeds.Length)
        {
            await _db.SaveChangesAsync();
        }
    }

    private async Task<List<RewardDefinition>> EvaluateAndAwardBadgesAsync(Guid childProfileId, LearningSession session, int completedCount, int starsEarned)
    {
        await EnsureDefaultRewardsExistAsync();

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

        // Lấy dữ liệu tổng hợp lịch sử của bé
        var totalAttempts = await _db.LearningAttempts
            .AsNoTracking()
            .Include(x => x.LearningItem)
            .ThenInclude(x => x!.SkillGroup)
            .Where(x => x.ChildProfileId == childProfileId && x.Status == "completed")
            .ToListAsync();

        var totalStars = totalAttempts.Sum(x => x.StarsEarned);
        var totalSessions = await _db.LearningSessions
            .CountAsync(x => x.ChildProfileId == childProfileId && x.Status == "completed");

        // 1. Bước chân đầu tiên: Hoàn thành bài học đầu tiên
        if (totalAttempts.Count >= 1)
        {
            TryAward("badge-first-step");
        }

        // 2. Chiến binh chăm chỉ: Hoàn thành buổi học hôm nay
        if (session.Status == "completed" && completedCount >= 1)
        {
            TryAward("badge-daily-champion");
        }

        // 3. Chuỗi học tập
        if (totalSessions >= 3) TryAward("badge-streak-3d");
        if (totalSessions >= 7) TryAward("badge-streak-7d");

        // 4. Mốc số sao
        if (totalStars >= 10) TryAward("badge-super-scholar");
        if (totalStars >= 25) TryAward("badge-star-collector");

        // 5. Huy hiệu kỹ năng theo nhóm bài
        var completedSkillCodes = totalAttempts
            .Where(x => x.LearningItem?.SkillGroup != null)
            .Select(x => x.LearningItem!.SkillGroup!.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (completedSkillCodes.Contains("chu-cai")) TryAward("badge-alphabet-star");
        if (totalAttempts.Any(x => x.LearningItem?.InteractionType == InteractionTypes.Tracing)) TryAward("badge-handwriting-hero");
        if (completedSkillCodes.Contains("chu-so") || completedSkillCodes.Contains("so-luong-toan")) TryAward("badge-math-whiz");
        if (completedSkillCodes.Contains("tu-duy-logic")) TryAward("badge-logic-explorer");
        if (completedSkillCodes.Contains("ky-nang-song")) TryAward("badge-habit-hero");
        if (completedSkillCodes.Contains("ngon-ngu")) TryAward("badge-story-teller");
        if (completedSkillCodes.Contains("hinh-dang-khong-gian")) TryAward("badge-shape-master");

        // 6. Vật phẩm trang trí khu vườn
        if (totalAttempts.Count >= 5) TryAward("item-golden-acorn");
        if (totalAttempts.Count(x => x.LearningItem?.InteractionType == InteractionTypes.Tracing) >= 3) TryAward("item-magic-pencil");
        if (totalAttempts.Count >= 10) TryAward("item-knowledge-tree");
        if (totalSessions >= 5) TryAward("item-tiny-crown");
        if (totalStars >= 50) TryAward("item-trophy-gold");

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

    private async Task<LearningSession> GetCurrentLearningSessionAsync(ChildProfile child, Guid? currentItemId = null)
    {
        var sessionRaw = HttpContext.Session.GetString(SessionKeys.CurrentLearningSessionId);
        if (Guid.TryParse(sessionRaw, out var sessionId))
        {
            var session = await _db.LearningSessions.FirstOrDefaultAsync(x => x.Id == sessionId && x.ChildProfileId == child.Id);
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
                .Where(x => x.ChildProfileId == child.Id && x.SessionPlanJson.Contains(currentItemId.Value.ToString()))
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

        var titleAudioUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "titleAudioUrl", string.Empty);
        var titleAudioUrlEn = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "titleAudioUrlEn", string.Empty);
        var questionAudioUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "questionAudioUrl", string.Empty);
        var questionAudioUrlEn = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "questionAudioUrlEn", string.Empty);
        var instructionAudioUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "instructionAudioUrl", string.Empty);
        var instructionAudioUrlEn = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "instructionAudioUrlEn", string.Empty);
        var tracingAudioUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "audioUrl", string.Empty);
        var tracingAudioUrlEn = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "audioUrlEn", string.Empty);
        var correctFeedbackAudioUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "correctAudioUrl", string.Empty);
        var correctFeedbackAudioUrlEn = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "correctAudioUrlEn", string.Empty);
        var retryFeedbackAudioUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "retryAudioUrl", string.Empty);
        var retryFeedbackAudioUrlEn = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "retryAudioUrlEn", string.Empty);

        if (string.IsNullOrWhiteSpace(questionAudioUrl)) questionAudioUrl = await ResolveActiveVoiceUrlAsync(question?.PromptText ?? item.Title);
        if (string.IsNullOrWhiteSpace(questionAudioUrlEn)) questionAudioUrlEn = await ResolveActiveVoiceUrlEnAsync(question?.PromptText ?? item.Title);
        if (string.IsNullOrWhiteSpace(titleAudioUrl)) titleAudioUrl = questionAudioUrl;
        if (string.IsNullOrWhiteSpace(titleAudioUrlEn)) titleAudioUrlEn = questionAudioUrlEn;
        if (string.IsNullOrWhiteSpace(instructionAudioUrl)) instructionAudioUrl = questionAudioUrl;
        if (string.IsNullOrWhiteSpace(instructionAudioUrlEn)) instructionAudioUrlEn = questionAudioUrlEn;
        if (string.IsNullOrWhiteSpace(tracingAudioUrl)) tracingAudioUrl = questionAudioUrl;
        if (string.IsNullOrWhiteSpace(tracingAudioUrlEn)) tracingAudioUrlEn = questionAudioUrlEn;
        if (string.IsNullOrWhiteSpace(correctFeedbackAudioUrl)) correctFeedbackAudioUrl = await ResolveActiveVoiceUrlAsync("Giỏi lắm, con làm đúng rồi!");
        if (string.IsNullOrWhiteSpace(correctFeedbackAudioUrlEn)) correctFeedbackAudioUrlEn = await ResolveActiveVoiceUrlEnAsync("Giỏi lắm, con làm đúng rồi!");
        if (string.IsNullOrWhiteSpace(retryFeedbackAudioUrl)) retryFeedbackAudioUrl = await ResolveActiveVoiceUrlAsync("Con thử lại nhé");
        if (string.IsNullOrWhiteSpace(retryFeedbackAudioUrlEn)) retryFeedbackAudioUrlEn = await ResolveActiveVoiceUrlEnAsync("Con thử lại nhé");

        if (question is not null)
        {
            question.PayloadJson = await EnrichPayloadOptionAudioAsync(question.PayloadJson);
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
        var clean = text.Trim();
        var entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
            x.Status == "ready" &&
            !string.IsNullOrEmpty(x.AudioUrl) &&
            (x.NormalizedText == clean || x.OriginalText == clean));
        return entry?.AudioUrl ?? string.Empty;
    }

    private async Task<string> ResolveActiveVoiceUrlEnAsync(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var clean = text.Trim();
        var entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
            x.StatusEn == "ready" &&
            !string.IsNullOrEmpty(x.AudioUrlEn) &&
            (x.NormalizedText == clean || x.OriginalText == clean));
        return entry?.AudioUrlEn ?? string.Empty;
    }

    private async Task<string> EnrichPayloadOptionAudioAsync(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson)) return "{}";
        try
        {
            var payload = System.Text.Json.Nodes.JsonNode.Parse(payloadJson)?.AsObject();
            if (payload is null) return payloadJson;

            var audioMap = payload.TryGetPropertyValue("optionAudio", out var optNode) && optNode is System.Text.Json.Nodes.JsonObject optObj ? optObj : new System.Text.Json.Nodes.JsonObject();
            var audioMapEn = payload.TryGetPropertyValue("optionAudioEn", out var optEnNode) && optEnNode is System.Text.Json.Nodes.JsonObject optEnObj ? optEnObj : new System.Text.Json.Nodes.JsonObject();

            var labels = CollectOptionLabelsFromPayload(payload).ToList();
            var changed = false;

            foreach (var label in labels)
            {
                var cleanLabel = label.Trim();
                var currentVi = audioMap.TryGetPropertyValue(cleanLabel, out var vNode) ? vNode?.ToString() : null;
                if (string.IsNullOrWhiteSpace(currentVi))
                {
                    var url = await ResolveActiveVoiceUrlAsync(label);
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        audioMap[cleanLabel] = url;
                        audioMap[label] = url;
                        changed = true;
                    }
                }

                var currentEn = audioMapEn.TryGetPropertyValue(cleanLabel, out var veNode) ? veNode?.ToString() : null;
                if (string.IsNullOrWhiteSpace(currentEn))
                {
                    var urlEn = await ResolveActiveVoiceUrlEnAsync(label);
                    if (!string.IsNullOrWhiteSpace(urlEn))
                    {
                        audioMapEn[cleanLabel] = urlEn;
                        audioMapEn[label] = urlEn;
                        changed = true;
                    }
                }
            }

            if (changed || !payload.ContainsKey("optionAudio") || !payload.ContainsKey("optionAudioEn"))
            {
                payload["optionAudio"] = audioMap;
                payload["optionAudioEn"] = audioMapEn;
                return payload.ToJsonString();
            }
            return payloadJson;
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
