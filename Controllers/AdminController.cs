using HanhTrangLop1.Data;
using HanhTrangLop1.Infrastructure;
using HanhTrangLop1.Models;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HanhTrangLop1.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminController : Controller
{
    private static readonly string[] AllowedStatuses =
    [
        ContentStatus.Draft,
        ContentStatus.Review,
        ContentStatus.Published,
        ContentStatus.Archived
    ];

    private static readonly string[] SupportedInteractionTypes =
    [
        InteractionTypes.SingleChoice,
        InteractionTypes.MultiSelect,
        InteractionTypes.ListenAndChoose,
        InteractionTypes.DragDrop,
        InteractionTypes.Matching,
        InteractionTypes.Ordering,
        InteractionTypes.Counting,
        InteractionTypes.QuantityBuilder,
        InteractionTypes.Comparison,
        InteractionTypes.Classification,
        InteractionTypes.StoryChoice
    ];

    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

    public AdminController(
        ApplicationDbContext db,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _environment = environment;
        _configuration = configuration;
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            TotalActivities = await _db.LearningItems.CountAsync(),
            PublishedActivities = await _db.LearningItems.CountAsync(x => x.Status == ContentStatus.Published),
            DraftActivities = await _db.LearningItems.CountAsync(x => x.Status == ContentStatus.Draft),
            ReviewActivities = await _db.LearningItems.CountAsync(x => x.Status == ContentStatus.Review),
            ArchivedActivities = await _db.LearningItems.CountAsync(x => x.Status == ContentStatus.Archived),
            SkillGroups = await _db.SkillGroups.CountAsync(x => x.IsActive),
            TotalChildren = await _db.ChildProfiles.CountAsync(),
            TotalAttempts = await _db.LearningAttempts.CountAsync(),
            TotalParents = await _db.Users.CountAsync(),
            RecentItems = await _db.LearningItems
                .Include(x => x.SkillGroup)
                .OrderByDescending(x => x.UpdatedAt)
                .Take(8)
                .ToListAsync(),
            RecentAttempts = await _db.LearningAttempts
                .Include(x => x.ChildProfile)
                .Include(x => x.LearningItem)
                .OrderByDescending(x => x.StartedAt)
                .Take(8)
                .ToListAsync()
        };

        return View(model);
    }

    [HttpGet("learning-items")]
    public async Task<IActionResult> LearningItems(string? status, string? interactionType, Guid? skillGroupId, Guid? topicId, string? search)
    {
        var query = _db.LearningItems
            .Include(x => x.SkillGroup)
            .Include(x => x.Topic)
            .Include(x => x.Questions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(interactionType))
        {
            query = query.Where(x => x.InteractionType == interactionType);
        }

        if (skillGroupId.HasValue)
        {
            query = query.Where(x => x.SkillGroupId == skillGroupId.Value);
        }

        if (topicId.HasValue)
        {
            query = query.Where(x => x.TopicId == topicId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x => x.Title.Contains(keyword) ||
                                     (x.Topic != null && x.Topic.Name.Contains(keyword)) ||
                                     (x.Topic != null && x.Topic.Code.Contains(keyword)) ||
                                     (x.SkillGroup != null && x.SkillGroup.Name.Contains(keyword)));
        }

        var allSkillGroups = await _db.SkillGroups.OrderBy(x => x.SortOrder).ToListAsync();
        var allTopics = await _db.Topics.OrderBy(x => x.SortOrder).ToListAsync();

        var items = await query
            .OrderBy(x => x.SkillGroup!.SortOrder)
            .ThenBy(x => x.Topic!.SortOrder)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync();

        var hasFilter = !string.IsNullOrWhiteSpace(status) ||
                        !string.IsNullOrWhiteSpace(interactionType) ||
                        skillGroupId.HasValue ||
                        topicId.HasValue ||
                        !string.IsNullOrWhiteSpace(search);

        var filteredGroups = allSkillGroups
            .Where(g => !skillGroupId.HasValue || g.Id == skillGroupId.Value)
            .ToList();

        var filteredTopics = allTopics
            .Where(t => (!skillGroupId.HasValue || t.SkillGroupId == skillGroupId.Value) &&
                        (!topicId.HasValue || t.Id == topicId.Value))
            .ToList();

        var treeGroups = new List<AdminLearningGroupTreeItem>();
        foreach (var group in filteredGroups)
        {
            var groupItems = items.Where(x => x.SkillGroupId == group.Id).ToList();
            var groupTopics = filteredTopics.Where(t => t.SkillGroupId == group.Id).ToList();

            // When a filter is active and specific group was not locked, hide empty group branches
            if (hasFilter && !skillGroupId.HasValue && groupItems.Count == 0)
            {
                continue;
            }

            var topicTreeItems = new List<AdminLearningTopicTreeItem>();
            foreach (var topic in groupTopics)
            {
                var topicItems = groupItems.Where(x => x.TopicId == topic.Id).ToList();
                if (hasFilter && !topicId.HasValue && topicItems.Count == 0)
                {
                    continue;
                }

                var allowedTemplates = ActivityTemplateCatalog.ForTopic(topic.Code).InteractionTypes
                    .Select(ActivityTemplateCatalog.Find)
                    .OfType<ActivityTemplateDefinition>()
                    .ToList();
                var allowsTracing = ActivityTemplateCatalog.ForTopic(topic.Code).AllowsTracing;

                topicTreeItems.Add(new AdminLearningTopicTreeItem
                {
                    Topic = topic,
                    LearningItemCount = topicItems.Count,
                    Items = topicItems,
                    AllowedTemplates = allowedTemplates,
                    AllowsTracing = allowsTracing
                });
            }

            var directItems = groupItems.Where(x => !x.TopicId.HasValue).ToList();

            treeGroups.Add(new AdminLearningGroupTreeItem
            {
                SkillGroup = group,
                LearningItemCount = groupItems.Count,
                Topics = topicTreeItems,
                DirectItems = directItems
            });
        }

        var model = new AdminLearningItemListViewModel
        {
            Search = search,
            Status = status,
            InteractionType = interactionType,
            SkillGroupId = skillGroupId,
            TopicId = topicId,
            SkillGroups = allSkillGroups,
            Topics = allTopics,
            Items = items,
            TreeGroups = treeGroups,
            TotalGroups = treeGroups.Count,
            TotalTopics = treeGroups.Sum(g => g.Topics.Count),
            TotalItems = items.Count
        };

        return View(model);
    }

    [HttpGet("learning-items/{id:guid}")]
    public async Task<IActionResult> LearningItemDetail(Guid id)
    {
        var item = await _db.LearningItems.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        return RedirectToEditor(item);
    }

    [HttpGet("learning-items/{id:guid}/preview")]
    public async Task<IActionResult> PreviewLearningItem(Guid id)
    {
        var item = await _db.LearningItems
            .Include(x => x.SkillGroup)
            .Include(x => x.Topic)
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        var model = await BuildAdminPreviewLearnViewModelAsync(item, question);
        return View("PreviewLearningItem", model);
    }

    [HttpPost("learning-items/{id:guid}/preview-answer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PreviewAnswer(Guid id, SubmitAnswerViewModel answer)
    {
        var item = await _db.LearningItems
            .Include(x => x.SkillGroup)
            .Include(x => x.Topic)
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id);

        var question = item?.Questions.FirstOrDefault(x => x.Id == answer.QuestionId);
        if (item is null || question is null)
        {
            return NotFound();
        }

        var correctAnswer = LearningJsonReader.ReadCorrectAnswer(question.CorrectAnswerJson);
        var isCorrect = LearningAnswerEvaluator.IsCorrect(item.InteractionType, answer.AnswerValue, correctAnswer);
        var feedbackMessage = LearningJsonReader.ReadFeedback(question.FeedbackJson, isCorrect);

        var model = await BuildAdminPreviewLearnViewModelAsync(item, question, feedbackMessage, isCorrect);
        return View("PreviewLearningItem", model);
    }

    [HttpPost("learning-items/{id:guid}/preview-tracing")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PreviewTracing(Guid id, SubmitTracingViewModel tracing)
    {
        var item = await _db.LearningItems
            .Include(x => x.SkillGroup)
            .Include(x => x.Topic)
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        var model = await BuildAdminPreviewLearnViewModelAsync(item, question, "Bé đã hoàn thành bài tô nét xuất sắc!", true);
        return View("PreviewLearningItem", model);
    }

    private async Task<LearnViewModel> BuildAdminPreviewLearnViewModelAsync(
        LearningItem item,
        Question? question,
        string? feedbackMessage = null,
        bool? isCorrect = null)
    {
        var payloadSymbol = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "symbol", string.Empty);
        var tracingSymbol = ExtractAdminTracingSymbol(payloadSymbol, item.Title, question?.PromptText);
        var questionImageUrl = question is null ? string.Empty : LearningJsonReader.ReadStringProperty(question.PayloadJson, "imageUrl", string.Empty);
        if (string.IsNullOrWhiteSpace(questionImageUrl) && item.InteractionType == InteractionTypes.Tracing)
        {
            questionImageUrl = ResolveAdminTracingFlashcardUrl(tracingSymbol);
        }
        if (string.IsNullOrWhiteSpace(questionImageUrl) && question is not null)
        {
            questionImageUrl = ResolveAdminQuestionImageFromItemMedia(question);
        }

        var nextItem = await _db.LearningItems
            .Where(x => x.SkillGroupId == item.SkillGroupId && x.SortOrder > item.SortOrder)
            .OrderBy(x => x.SortOrder)
            .FirstOrDefaultAsync();

        return new LearnViewModel
        {
            Item = item,
            ChildProfile = new ChildProfile { Nickname = "Bé Xem Thử (Admin)", SoundEnabled = true },
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
            NextItemId = nextItem?.Id,
            ReturnSkillGroupId = item.SkillGroupId
        };
    }

    private static string ExtractAdminTracingSymbol(string? payloadSymbol, string? itemTitle, string? promptText)
    {
        if (!string.IsNullOrWhiteSpace(payloadSymbol) && !string.Equals(payloadSymbol.Trim(), "A", StringComparison.OrdinalIgnoreCase))
        {
            return payloadSymbol.Trim();
        }

        if (!string.IsNullOrWhiteSpace(promptText))
        {
            var match = System.Text.RegularExpressions.Regex.Match(promptText, @"cách viết\s+([^!.,?]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        if (!string.IsNullOrWhiteSpace(itemTitle))
        {
            var match = System.Text.RegularExpressions.Regex.Match(itemTitle, @"(chữ số|chữ|số|nét)\s+([^!.,?]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
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

    private static string ResolveAdminTracingFlashcardUrl(string symbol)
    {
        if (string.Equals(symbol?.Trim(), "0", StringComparison.OrdinalIgnoreCase))
        {
            return "/images/photos/flashcard-number-0.svg";
        }
        if (int.TryParse(symbol, out var number) && number is >= 1 and <= 20)
        {
            return $"/images/photos/flashcard-number-{number}.jpg";
        }

        var trimmed = symbol?.Trim().ToLowerInvariant() ?? string.Empty;
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

    private static string ResolveAdminQuestionImageFromItemMedia(Question question)
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

        var match = itemMedia.FirstOrDefault(property =>
            string.Equals(property.Key, answer.Trim(), StringComparison.OrdinalIgnoreCase));
        if (match.Value is JsonValue value && value.TryGetValue<string>(out var imageUrl))
        {
            return imageUrl;
        }

        return string.Empty;
    }

    [HttpPost("learning-items/generate-missing-audio")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateMissingLearningItemsAudio(string? status, string? interactionType, Guid? skillGroupId, Guid? topicId, string? search)
    {
        var query = _db.LearningItems
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(interactionType))
        {
            query = query.Where(x => x.InteractionType == interactionType);
        }

        if (skillGroupId.HasValue)
        {
            query = query.Where(x => x.SkillGroupId == skillGroupId.Value);
        }

        if (topicId.HasValue)
        {
            query = query.Where(x => x.TopicId == topicId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x => x.Title.Contains(keyword) ||
                                     (x.Topic != null && x.Topic.Name.Contains(keyword)) ||
                                     (x.Topic != null && x.Topic.Code.Contains(keyword)) ||
                                     (x.SkillGroup != null && x.SkillGroup.Name.Contains(keyword)));
        }

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync();

        var linkedCount = 0;
        var updatedItems = 0;
        foreach (var item in items)
        {
            var linkedForItem = await SyncVoiceForLearningItemAsync(item, onlyMissing: true);
            if (linkedForItem > 0)
            {
                linkedCount += linkedForItem;
                updatedItems += 1;
            }
        }

        var hasChanges = _db.ChangeTracker.HasChanges();
        if (hasChanges)
        {
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                TempData["AdminMessage"] = $"Không thể lưu rà soát voice: {GetInnermostMessage(ex)}";
                return RedirectToAction(nameof(LearningItems), new { status, interactionType, skillGroupId, topicId, search });
            }
        }

        if (linkedCount > 0)
        {
            TempData["AdminMessage"] = $"Đã đồng bộ {linkedCount} voice có file cho {updatedItems} bài học. Các voice thiếu file đã nằm trong bảng kiểm soát.";
        }
        else
        {
            TempData["AdminMessage"] = "Đã rà soát voice. Chưa có file nào để gắn thêm; hãy vào Kiểm soát voice để tải file cho mục còn thiếu.";
        }

        return RedirectToAction(nameof(LearningItems), new { status, interactionType, skillGroupId, topicId, search });
    }

    [HttpPost("learning-items/{id:guid}/generate-audio")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateLearningItemAudio(Guid id)
    {
        var item = await _db.LearningItems
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var linkedCount = await SyncVoiceForLearningItemAsync(item, onlyMissing: false);
        var hasChanges = _db.ChangeTracker.HasChanges();
        if (hasChanges)
        {
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                TempData["AdminMessage"] = $"Không thể lưu voice cũ: {GetInnermostMessage(ex)}";
                return RedirectToAction(nameof(VoiceCache));
            }
        }

        if (linkedCount > 0)
        {
            TempData["AdminMessage"] = $"Đã đồng bộ {linkedCount} voice có file cho bài “{item.Title}”.";
        }
        else
        {
            TempData["AdminMessage"] = "Đã rà soát voice cho bài này. Mục thiếu file đã nằm trong Kiểm soát voice để tải lên.";
        }

        return RedirectToEditor(item);
    }

    [HttpGet("catalogs")]
    public async Task<IActionResult> Catalogs()
    {
        var groups = await _db.SkillGroups
            .AsSplitQuery()
            .Include(x => x.Topics.OrderBy(topic => topic.SortOrder))
            .Include(x => x.LearningItems)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        return View(new AdminCatalogViewModel
        {
            TotalLearningItems = groups.Sum(x => x.LearningItems.Count),
            SkillGroups = groups.Select(group => new AdminCatalogGroupViewModel
            {
                SkillGroup = group,
                LearningItemCount = group.LearningItems.Count,
                Topics = group.Topics.Select(topic => new AdminCatalogTopicViewModel
                {
                    Topic = topic,
                    LearningItemCount = group.LearningItems.Count(item => item.TopicId == topic.Id),
                    AllowedTemplates = ActivityTemplateCatalog.ForTopic(topic.Code).InteractionTypes
                        .Select(ActivityTemplateCatalog.Find)
                        .OfType<ActivityTemplateDefinition>()
                        .ToList(),
                    AllowsTracing = ActivityTemplateCatalog.ForTopic(topic.Code).AllowsTracing
                }).ToList()
            }).ToList()
        });
    }

    [HttpGet("media")]
    public async Task<IActionResult> MediaLibrary()
    {
        var assets = await _db.MediaAssets
            .Where(x => x.AssetType == "image")
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return View(new AdminMediaLibraryViewModel
        {
            Images = assets
        });
    }

    [HttpGet("voice-cache")]
    public async Task<IActionResult> VoiceCache(string? status, string? usageType, string? q, int page = 1, int pageSize = 50)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 20, 100);
        var query = _db.TextToSpeechCaches
            .Where(x => x.UsageType != "legacy")
            .AsQueryable();
        if (status == "ready")
        {
            query = query.Where(x => !string.IsNullOrWhiteSpace(x.AudioUrl) && x.Status == "ready");
        }
        else if (status == "missing")
        {
            query = query.Where(x => string.IsNullOrWhiteSpace(x.AudioUrl) || x.Status != "ready");
        }

        if (!string.IsNullOrWhiteSpace(usageType))
        {
            query = query.Where(x => x.UsageType == usageType);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var keyword = q.Trim();
            query = query.Where(x =>
                x.Name.Contains(keyword) ||
                x.NormalizedText.Contains(keyword) ||
                x.OriginalText.Contains(keyword));
        }

        ViewBag.Status = status;
        ViewBag.UsageType = usageType;
        ViewBag.Keyword = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalVoiceCount = await _db.TextToSpeechCaches.CountAsync(x => x.UsageType != "legacy");
        ViewBag.MissingVoiceCount = await _db.TextToSpeechCaches.CountAsync(x => x.UsageType != "legacy" && (string.IsNullOrWhiteSpace(x.AudioUrl) || x.Status != "ready"));
        ViewBag.LegacyVoiceCount = await _db.TextToSpeechCaches.CountAsync(x => x.UsageType == "legacy");
        ViewBag.UsageTypes = await _db.TextToSpeechCaches
            .Where(x => !string.IsNullOrWhiteSpace(x.UsageType) && x.UsageType != "legacy")
            .Select(x => x.UsageType)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var filteredCount = await query.CountAsync();
        ViewBag.FilteredVoiceCount = filteredCount;
        ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(filteredCount / (double)pageSize));

        var entries = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return View(entries);
    }

    [HttpPost("voice-cache/{id:guid}/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateVoiceCache(Guid id, string? name, string? text, string? usageType, bool generateFile = true)
    {
        var entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x => x.Id == id);
        if (entry is null)
        {
            return NotFound();
        }

        var normalizedText = NormalizeSpeechText(text ?? string.Empty);
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            TempData["AdminMessage"] = "Vui lòng nhập nội dung voice.";
            return RedirectToAction(nameof(VoiceCache), new { q = entry.NormalizedText });
        }

        var cacheKey = BuildTextToSpeechCacheKey(normalizedText);
        var duplicated = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
            x.Id != id &&
            x.Provider == cacheKey.Provider &&
            x.Voice == cacheKey.Voice &&
            x.ModelId == cacheKey.ModelId &&
            x.Format == cacheKey.Format &&
            x.TextHash == cacheKey.TextHash);
        if (duplicated is not null)
        {
            TempData["AdminMessage"] = "Text này đã có trong kho voice. Không thể sửa trùng với một dòng khác.";
            return RedirectToAction(nameof(VoiceCache), new { q = normalizedText });
        }

        var textChanged = !string.Equals(entry.TextHash, cacheKey.TextHash, StringComparison.OrdinalIgnoreCase);
        entry.Provider = cacheKey.Provider;
        entry.Voice = cacheKey.Voice;
        entry.ModelId = cacheKey.ModelId;
        entry.Format = cacheKey.Format;
        entry.TextHash = cacheKey.TextHash;
        entry.Name = string.IsNullOrWhiteSpace(name) ? BuildVoiceName(usageType ?? entry.UsageType, null, normalizedText) : AudioAltText(name.Trim());
        entry.UsageType = string.IsNullOrWhiteSpace(usageType) ? "custom" : usageType.Trim();
        entry.NormalizedText = AudioAltText(normalizedText);
        entry.OriginalText = AudioOriginalText(normalizedText);
        entry.LastError = null;
        entry.UpdatedAt = DateTimeOffset.UtcNow;

        if (textChanged || generateFile)
        {
            try
            {
                entry.AudioUrl = await GenerateVoiceCacheFileAsync(entry);
                entry.Status = "ready";
            }
            catch (Exception ex)
            {
                entry.AudioUrl = string.Empty;
                entry.Status = "missing";
                entry.LastError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
            }
        }

        await _db.SaveChangesAsync();
        TempData["AdminMessage"] = (textChanged || generateFile)
            ? entry.Status == "ready"
                ? "Đã cập nhật nội dung và tạo lại file voice cho dòng này."
                : "Đã cập nhật nội dung cho dòng này, nhưng chưa tạo được file. Có thể tải file lên hoặc bấm Tạo file."
            : "Đã cập nhật tên/loại voice.";
        return RedirectToAction(nameof(VoiceCache), new { q = entry.NormalizedText });
    }

    [HttpPost("voice-cache/{id:guid}/generate-file")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateVoiceCacheFile(Guid id)
    {
        var entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x => x.Id == id);
        if (entry is null)
        {
            return NotFound();
        }

        try
        {
            entry.AudioUrl = await GenerateVoiceCacheFileAsync(entry);
            entry.Status = "ready";
            entry.LastError = null;
            entry.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            TempData["AdminMessage"] = $"Đã tạo file voice cho “{entry.Name}”.";
        }
        catch (Exception ex)
        {
            entry.Status = "missing";
            entry.LastError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
            entry.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            TempData["AdminMessage"] = $"Không thể tạo file voice: {ex.Message}";
        }

        return RedirectToAction(nameof(VoiceCache), new { q = entry.NormalizedText });
    }

    [HttpPost("voice-cache/{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVoiceCache(Guid id)
    {
        var entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x => x.Id == id);
        if (entry is null)
        {
            return NotFound();
        }

        _db.TextToSpeechCaches.Remove(entry);
        await _db.SaveChangesAsync();
        TempData["AdminMessage"] = $"Đã xóa voice “{entry.Name}”.";
        return RedirectToAction(nameof(VoiceCache));
    }

    [HttpPost("voice-cache/delete-legacy")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLegacyVoiceCache()
    {
        var deleted = await _db.TextToSpeechCaches
            .Where(x => x.UsageType == "legacy")
            .ExecuteDeleteAsync();
        TempData["AdminMessage"] = $"Đã xóa {deleted} voice cũ khỏi bảng kiểm soát.";
        return RedirectToAction(nameof(VoiceCache));
    }

    [HttpPost("voice-cache/backfill")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BackfillVoiceCache()
    {
        var audioAssets = await _db.MediaAssets
            .Where(x => x.AssetType == "audio" && !string.IsNullOrWhiteSpace(x.AltText))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
        var existingHashes = await _db.TextToSpeechCaches
            .Select(x => x.TextHash)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);

        var added = 0;
        foreach (var asset in audioAssets)
        {
            var normalizedText = NormalizeSpeechText(ExtractVoiceTextFromAltText(asset.AltText!));
            if (string.IsNullOrWhiteSpace(normalizedText))
            {
                continue;
            }

            var cacheKey = BuildTextToSpeechCacheKey(normalizedText);
            if (existingHashes.Contains(cacheKey.TextHash))
            {
                continue;
            }

            _db.TextToSpeechCaches.Add(new TextToSpeechCache
            {
                Id = Guid.NewGuid(),
                Provider = cacheKey.Provider,
                Voice = cacheKey.Voice,
                ModelId = cacheKey.ModelId,
                Format = cacheKey.Format,
                TextHash = cacheKey.TextHash,
                Name = BuildVoiceName("legacy", null, normalizedText),
                UsageType = "legacy",
                NormalizedText = AudioAltText(normalizedText),
                OriginalText = AudioOriginalText(ExtractVoiceTextFromAltText(asset.AltText!)),
                AudioUrl = asset.StoragePath,
                Status = "ready",
                CreatedAt = asset.CreatedAt,
                UpdatedAt = DateTimeOffset.UtcNow
            });
            existingHashes.Add(cacheKey.TextHash);
            added += 1;
        }

        if (added > 0)
        {
            await _db.SaveChangesAsync();
        }

        TempData["AdminMessage"] = $"Đã chuẩn hóa {added} voice cũ vào bảng kiểm soát.";
        return RedirectToAction(nameof(VoiceCache));
    }

    [HttpPost("voice-cache/{id:guid}/upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadVoiceCacheFile(Guid id, IFormFile? audioFile)
    {
        var entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x => x.Id == id);
        if (entry is null)
        {
            return NotFound();
        }

        if (audioFile is null || audioFile.Length == 0)
        {
            TempData["AdminMessage"] = "Vui lòng chọn file âm thanh để tải lên.";
            return RedirectToAction(nameof(VoiceCache));
        }

        ValidateMediaFile(audioFile, "audio", 10 * 1024 * 1024, nameof(audioFile));
        if (!ModelState.IsValid)
        {
            TempData["AdminMessage"] = "File âm thanh chưa hợp lệ. Vui lòng chọn MP3/WAV/M4A.";
            return RedirectToAction(nameof(VoiceCache));
        }

        await ApplyVoiceCacheUploadAsync(entry, audioFile);
        await _db.SaveChangesAsync();

        TempData["AdminMessage"] = $"Đã cập nhật file cho voice “{entry.Name}”.";
        return RedirectToAction(nameof(VoiceCache));
    }

    [HttpPost("voice-cache/{id:guid}/upload-inline")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadVoiceCacheFileInline(Guid id, IFormFile? audioFile)
    {
        var entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x => x.Id == id);
        if (entry is null)
        {
            return NotFound(new { message = "Không tìm thấy voice." });
        }

        if (audioFile is null || audioFile.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng chọn file âm thanh." });
        }

        ValidateMediaFile(audioFile, "audio", 10 * 1024 * 1024, nameof(audioFile));
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "File âm thanh chưa hợp lệ. Vui lòng chọn MP3/WAV/M4A." });
        }

        await ApplyVoiceCacheUploadAsync(entry, audioFile);
        await _db.SaveChangesAsync();
        return Json(new
        {
            id = entry.Id,
            audioUrl = entry.AudioUrl,
            status = entry.Status,
            updatedAt = entry.UpdatedAt
        });
    }

    [HttpPost("voice-cache/{id:guid}/copy-inline")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CopyVoiceCacheFileInline(Guid id, Guid sourceId)
    {
        var entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x => x.Id == id);
        if (entry is null)
        {
            return NotFound(new { message = "Không tìm thấy voice cần đổi." });
        }

        var source = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
            x.Id == sourceId &&
            x.Status == "ready" &&
            !string.IsNullOrWhiteSpace(x.AudioUrl));
        if (source is null)
        {
            return BadRequest(new { message = "Voice trong kho chưa có file để dùng." });
        }

        entry.AudioUrl = source.AudioUrl;
        entry.Status = "ready";
        entry.LastError = null;
        entry.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();

        return Json(new
        {
            id = entry.Id,
            audioUrl = entry.AudioUrl,
            status = entry.Status,
            updatedAt = entry.UpdatedAt
        });
    }

    [HttpPost("voice-cache/generate-missing")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateMissingVoiceFiles()
    {
        var entries = await _db.TextToSpeechCaches
            .Where(x => string.IsNullOrWhiteSpace(x.AudioUrl) || x.Status != "ready")
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        var generated = 0;
        var failed = 0;
        foreach (var entry in entries)
        {
            try
            {
                entry.AudioUrl = await GenerateVoiceCacheFileAsync(entry);
                entry.Status = "ready";
                entry.LastError = null;
                entry.UpdatedAt = DateTimeOffset.UtcNow;
                generated += 1;
            }
            catch (Exception ex)
            {
                entry.Status = "missing";
                entry.LastError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                entry.UpdatedAt = DateTimeOffset.UtcNow;
                failed += 1;
            }
        }

        if (entries.Count > 0)
        {
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                TempData["AdminMessage"] = $"Không thể lưu file voice tự tạo: {GetInnermostMessage(ex)}";
                return RedirectToAction(nameof(VoiceCache));
            }
        }

        TempData["AdminMessage"] = $"Đã tự tạo {generated} file voice. Lỗi {failed} mục.";
        return RedirectToAction(nameof(VoiceCache));
    }

    [HttpPost("voice-cache/generate-text")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateVoiceFromText(string text, string? name, string? usageType)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            TempData["AdminMessage"] = "Vui lòng nhập nội dung cần tạo voice.";
            return RedirectToAction(nameof(VoiceCache));
        }

        var entry = await EnsureVoiceCacheEntryAsync(text, string.IsNullOrWhiteSpace(usageType) ? "custom" : usageType.Trim(), name);
        if (entry is null)
        {
            TempData["AdminMessage"] = "Không thể tạo voice vì nội dung trống.";
            return RedirectToAction(nameof(VoiceCache));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            entry.Name = AudioAltText(name.Trim());
        }

        try
        {
            entry.AudioUrl = await GenerateVoiceCacheFileAsync(entry);
            entry.Status = "ready";
            entry.LastError = null;
            entry.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            TempData["AdminMessage"] = $"Đã tạo voice cho “{entry.NormalizedText}”.";
            return RedirectToAction(nameof(VoiceCache), new { q = entry.NormalizedText });
        }
        catch (Exception ex)
        {
            entry.Status = "missing";
            entry.LastError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
            entry.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            TempData["AdminMessage"] = $"Không thể tạo file voice: {ex.Message}";
            return RedirectToAction(nameof(VoiceCache), new { q = entry.NormalizedText });
        }
    }

    [HttpGet("learning-items/create-choice")]
    public async Task<IActionResult> CreateChoice(Guid? skillGroupId, Guid? topicId, Guid? editId, string? interactionType)
    {
        await LoadContentListsAsync();

        if (editId.HasValue)
        {
            var item = await _db.LearningItems
                .Include(x => x.Topic)
                .Include(x => x.Questions.OrderBy(question => question.SortOrder))
                .FirstOrDefaultAsync(x => x.Id == editId.Value);
            if (item is null)
            {
                return NotFound();
            }
            if (item.InteractionType == InteractionTypes.Tracing)
            {
                return RedirectToAction(nameof(EditLearningItem), new { id = item.Id });
            }
            if (NormalizeLegacyActivityPayload(item))
            {
                await _db.SaveChangesAsync();
            }
            return View(BuildActivityEditorModel(item));
        }

        var firstGroupId = skillGroupId ?? await _db.SkillGroups
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync() ?? Guid.Empty;
        var selectedTopic = topicId.HasValue
            ? await _db.Topics.FirstOrDefaultAsync(x => x.Id == topicId.Value && x.SkillGroupId == firstGroupId && x.IsActive)
            : null;
        selectedTopic ??= await _db.Topics
            .Where(x => x.SkillGroupId == firstGroupId && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .FirstOrDefaultAsync();

        var rule = ActivityTemplateCatalog.ForTopic(selectedTopic?.Code);
        if (rule.InteractionTypes.Count == 0 && rule.AllowsTracing && selectedTopic is not null)
        {
            return RedirectToAction(nameof(CreateTracing), new { skillGroupId = firstGroupId, topicId = selectedTopic.Id });
        }

        var selectedInteractionType = !string.IsNullOrWhiteSpace(interactionType) &&
                                      rule.InteractionTypes.Contains(interactionType, StringComparer.OrdinalIgnoreCase)
            ? interactionType
            : rule.InteractionTypes.FirstOrDefault() ?? InteractionTypes.SingleChoice;
        var template = ActivityTemplateCatalog.Find(selectedInteractionType);

        return View(new CreateChoiceItemViewModel
        {
            SkillGroupId = firstGroupId,
            TopicId = selectedTopic?.Id,
            InteractionType = selectedInteractionType,
            InstructionText = template?.DefaultInstruction ?? "Con hãy thực hiện hoạt động.",
            PromptText = template?.DefaultPrompt ?? "Con trả lời câu hỏi nhé."
        });
    }

    [HttpPost("learning-items/create-choice")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateChoice(CreateChoiceItemViewModel model)
    {
        await ValidateClassificationAsync(model.SkillGroupId, model.TopicId, requireActive: !model.Id.HasValue);
        await ValidateActivityTemplateAsync(model.TopicId, model.InteractionType);
        await PrepareMediaSelectionAsync(model);
        if (!ModelState.IsValid)
        {
            await LoadContentListsAsync();
            return View(model);
        }

        if (!SupportedInteractionTypes.Contains(model.InteractionType))
        {
            ModelState.AddModelError(nameof(model.InteractionType), "Dạng tương tác chưa được hỗ trợ.");
            await LoadContentListsAsync();
            return View(model);
        }

        var configuration = BuildActivityConfiguration(model);
        if (!ModelState.IsValid || configuration is null)
        {
            await LoadContentListsAsync();
            return View(model);
        }

        await SaveUploadedMediaAsync(model);
        await PopulateVoiceUrlsFromCacheAsync(model);
        configuration = BuildActivityConfiguration(model);
        if (configuration is null)
        {
            await LoadContentListsAsync();
            return View(model);
        }

        var now = DateTimeOffset.UtcNow;
        var item = model.Id.HasValue
            ? await _db.LearningItems
                .Include(x => x.Questions.OrderBy(question => question.SortOrder))
                .FirstOrDefaultAsync(x => x.Id == model.Id.Value)
            : null;
        if (model.Id.HasValue && item is null)
        {
            return NotFound();
        }

        item ??= new LearningItem
        {
            Id = Guid.NewGuid(),
            Code = CreateLearningItemCode("bai"),
            Status = ContentStatus.Draft,
            CreatedAt = now,
            Questions = new List<Question>()
        };

        item.Title = Clean(model.Title);
        item.SkillGroupId = model.SkillGroupId;
        item.TopicId = model.TopicId;
        item.Level = model.Level;
        item.SortOrder = model.SortOrder > 0
            ? model.SortOrder
            : item.SortOrder > 0 ? item.SortOrder : await GetNextSortOrderAsync(model.TopicId);
        item.InteractionType = model.InteractionType;
        item.EstimatedMinutes = model.EstimatedMinutes;
        item.InstructionText = Clean(model.InstructionText);
        item.ContentJson = configuration.Value.PayloadJson;
        item.UpdatedAt = now;

        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        if (question is null)
        {
            question = new Question
            {
                Id = Guid.NewGuid(),
                LearningItemId = item.Id,
                SortOrder = 1
            };
            item.Questions.Add(question);
        }

        question.PromptText = Clean(model.PromptText);
        question.QuestionType = model.InteractionType;
        question.PayloadJson = configuration.Value.PayloadJson;
        question.CorrectAnswerJson = JsonSerializer.Serialize(new { value = configuration.Value.CorrectAnswer });
        question.HintJson = JsonSerializer.Serialize(new { level1 = Clean(model.HintText) });
        question.FeedbackJson = JsonSerializer.Serialize(new
        {
            correct = Clean(model.CorrectFeedback),
            retry = Clean(model.RetryFeedback)
        });

        if (!model.Id.HasValue)
        {
            _db.LearningItems.Add(item);
        }
        await SyncVoiceForLearningItemAsync(item, onlyMissing: true);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(CreateChoice), new { editId = item.Id });
    }

    [HttpGet("learning-items/create-tracing")]
    public async Task<IActionResult> CreateTracing(Guid? skillGroupId, Guid? topicId, Guid? editId)
    {
        await LoadTracingListsAsync();
        if (editId.HasValue)
        {
            var item = await _db.LearningItems
                .Include(x => x.Topic)
                .Include(x => x.Questions.OrderBy(q => q.SortOrder))
                .FirstOrDefaultAsync(x => x.Id == editId.Value && x.InteractionType == InteractionTypes.Tracing);
            if (item is null) return NotFound();
            var question = item.Questions.FirstOrDefault();
            return View(new CreateTracingItemViewModel
            {
                Id = item.Id,
                Status = item.Status,
                IsCompatible = ActivityTemplateCatalog.IsItemAllowed(item),
                Title = item.Title,
                SkillGroupId = item.SkillGroupId,
                TopicId = item.TopicId,
                SortOrder = item.SortOrder,
                Symbol = ReadJsonString(question?.PayloadJson, "symbol"),
                GuideMode = ReadJsonString(question?.PayloadJson, "guideMode") is { Length: > 0 } guide ? guide : "outline",
                ExpectedStrokeCount = ReadJsonInt(question?.PayloadJson, "expectedStrokeCount", 1),
                ShowStartPoint = false,
                AudioUrl = ReadJsonString(question?.PayloadJson, "audioUrl"),
                InstructionText = item.InstructionText,
                PromptText = question?.PromptText ?? string.Empty,
                MinPoints = ReadJsonInt(question?.CorrectAnswerJson, "minPoints", 20),
                Level = item.Level
            });
        }

        var tracingTopics = ViewBag.Topics as IReadOnlyList<Topic> ?? [];
        var selectedTopic = tracingTopics.FirstOrDefault(x => x.Id == topicId)
            ?? tracingTopics.FirstOrDefault(x => !skillGroupId.HasValue || x.SkillGroupId == skillGroupId.Value)
            ?? tracingTopics.FirstOrDefault();
        return View(new CreateTracingItemViewModel
        {
            SkillGroupId = selectedTopic?.SkillGroupId ?? Guid.Empty,
            TopicId = selectedTopic?.Id,
            InstructionText = "Con tô theo nét gợi ý nhé.",
            PromptText = "Con tô ký tự theo đường viền."
        });
    }

    [HttpPost("learning-items/create-tracing")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTracing(CreateTracingItemViewModel model)
    {
        await ValidateClassificationAsync(model.SkillGroupId, model.TopicId);
        await ValidateTracingTopicAsync(model.TopicId);
        await PrepareTracingMediaAsync(model);
        if (!ModelState.IsValid)
        {
            await LoadTracingListsAsync();
            return View(model);
        }

        if (model.AudioFile is not null)
        {
            model.AudioUrl = await SaveMediaFileAsync(model.AudioFile, "audio");
        }
        if (string.IsNullOrWhiteSpace(model.AudioUrl))
        {
            model.AudioUrl = await ResolveVoiceAudioAsync(model.PromptText, "tracing-prompt") ?? string.Empty;
        }

        var now = DateTimeOffset.UtcNow;
        var symbol = string.IsNullOrWhiteSpace(model.Symbol) ? "A" : model.Symbol.Trim();
        var item = model.Id.HasValue
            ? await _db.LearningItems.Include(x => x.Questions).FirstOrDefaultAsync(x => x.Id == model.Id.Value)
            : null;
        if (model.Id.HasValue && (item is null || item.InteractionType != InteractionTypes.Tracing))
        {
            return NotFound();
        }

        var question = item?.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        var templateId = ReadJsonGuid(question?.PayloadJson, "templateId") ?? Guid.NewGuid();
        var guideMode = model.GuideMode is "outline" or "free" ? model.GuideMode : "outline";
        var tracingPayload = JsonSerializer.Serialize(new
        {
            symbol,
            templateId,
            guideMode,
            expectedStrokeCount = model.ExpectedStrokeCount,
            showStartPoint = true,
            audioUrl = Clean(model.AudioUrl)
        });

        var tracingTemplate = await _db.TracingTemplates.FirstOrDefaultAsync(x => x.Id == templateId);
        if (tracingTemplate is null)
        {
            tracingTemplate = new TracingTemplate { Id = templateId, CreatedAt = now };
            _db.TracingTemplates.Add(tracingTemplate);
        }
        tracingTemplate.SymbolType = await ResolveTracingSymbolTypeAsync(model.TopicId);
        tracingTemplate.Symbol = symbol;
        tracingTemplate.DisplayName = model.Title.Trim();
        tracingTemplate.CanvasWidth = 720;
        tracingTemplate.CanvasHeight = 720;
        tracingTemplate.GuideJson = JsonSerializer.Serialize(new
        {
            guideMode,
            expectedStrokeCount = model.ExpectedStrokeCount,
            showStartPoint = true
        });

        item ??= new LearningItem
        {
            Id = Guid.NewGuid(),
            Code = CreateLearningItemCode($"to-net-{symbol}"),
            Status = ContentStatus.Draft,
            CreatedAt = now,
            Questions = new List<Question>()
        };
        item.Title = model.Title.Trim();
        item.SkillGroupId = model.SkillGroupId;
        item.TopicId = model.TopicId;
        item.Level = model.Level;
        item.SortOrder = model.SortOrder > 0
            ? model.SortOrder
            : item.SortOrder > 0 ? item.SortOrder : await GetNextSortOrderAsync(model.TopicId);
        item.InteractionType = InteractionTypes.Tracing;
        item.EstimatedMinutes = 5;
        item.InstructionText = model.InstructionText.Trim();
        item.ContentJson = tracingPayload;
        item.UpdatedAt = now;

        if (question is null)
        {
            question = new Question { Id = Guid.NewGuid(), LearningItemId = item.Id, SortOrder = 1 };
            item.Questions.Add(question);
        }
        question.PromptText = model.PromptText.Trim();
        question.QuestionType = InteractionTypes.Tracing;
        question.PayloadJson = tracingPayload;
        question.CorrectAnswerJson = JsonSerializer.Serialize(new
        {
            minPoints = Math.Max(5, model.MinPoints),
            expectedStrokeCount = model.ExpectedStrokeCount
        });
        question.HintJson = JsonSerializer.Serialize(new { level1 = "Con bắt đầu từ điểm màu cam nhé." });
        question.FeedbackJson = JsonSerializer.Serialize(new
        {
            correct = $"Tốt lắm, con đã tô xong {symbol}!",
            retry = "Mình thử tô lại một nét nhé."
        });

        if (!model.Id.HasValue) _db.LearningItems.Add(item);
        await SyncVoiceForLearningItemAsync(item, onlyMissing: true);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(CreateTracing), new { editId = item.Id });
    }

    [HttpGet("learning-items/{id:guid}/edit")]
    public async Task<IActionResult> EditLearningItem(Guid id)
    {
        var item = await _db.LearningItems
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        if (item.InteractionType == InteractionTypes.Tracing)
        {
            return RedirectToAction(nameof(CreateTracing), new { editId = item.Id });
        }
        return RedirectToAction(nameof(CreateChoice), new { editId = item.Id });
    }

    [HttpPost("learning-items/{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLearningItem(Guid id, EditLearningItemViewModel model)
    {
        model.InteractionType = InteractionTypes.Tracing;
        await ValidateClassificationAsync(model.SkillGroupId, model.TopicId, requireActive: false);
        if (string.IsNullOrWhiteSpace(model.Symbol))
        {
            ModelState.AddModelError(nameof(model.Symbol), "Vui lòng nhập ký tự cần tô.");
        }

        if (!ModelState.IsValid)
        {
            model.Id = id;
            await LoadContentListsAsync(model.SkillGroupId, model.TopicId);
            return View(model);
        }

        var item = await _db.LearningItems
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.Title = model.Title.Trim();
        item.SkillGroupId = model.SkillGroupId;
        item.TopicId = model.TopicId;
        item.Level = model.Level;
        item.EstimatedMinutes = Math.Max(1, model.EstimatedMinutes);
        item.InstructionText = model.InstructionText.Trim();
        item.InteractionType = model.InteractionType;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        if (question is not null)
        {
            question.PromptText = model.PromptText.Trim();
            question.QuestionType = model.InteractionType;
            question.HintJson = JsonSerializer.Serialize(new { level1 = Clean(model.HintText) });
            question.FeedbackJson = JsonSerializer.Serialize(new
            {
                correct = Clean(model.CorrectFeedback),
                retry = Clean(model.RetryFeedback)
            });

            var symbol = model.Symbol.Trim().ToUpperInvariant();
            item.ContentJson = JsonSerializer.Serialize(new { symbol });
            question.PayloadJson = JsonSerializer.Serialize(new { symbol });
            question.CorrectAnswerJson = JsonSerializer.Serialize(new { minPoints = Math.Clamp(model.MinPoints, 5, 300) });
        }

        await SyncVoiceForLearningItemAsync(item, onlyMissing: true);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(CreateTracing), new { editId = item.Id });
    }

    [HttpPost("learning-items/{id:guid}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(Guid id, string status)
    {
        var item = await _db.LearningItems.Include(x => x.Topic).FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        if (!AllowedStatuses.Contains(status))
        {
            return BadRequest();
        }

        if (status == ContentStatus.Published && !ActivityTemplateCatalog.IsItemAllowed(item))
        {
            TempData["AdminMessage"] = "Bài học chưa phù hợp với chủ đề. Vui lòng sửa mẫu hoạt động trước khi xuất bản.";
            return RedirectToEditor(item);
        }

        item.Status = status;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        item.PublishedAt = status == ContentStatus.Published ? DateTimeOffset.UtcNow : item.PublishedAt;

        if (status == ContentStatus.Review)
        {
            _db.ContentReviews.Add(new ContentReview
            {
                Id = Guid.NewGuid(),
                LearningItemId = item.Id,
                Status = ContentStatus.Review,
                Note = "Gửi duyệt từ admin MVP.",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        return RedirectToEditor(item);
    }

    [HttpPost("learning-items/{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLearningItem(Guid id)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        var item = await _db.LearningItems
            .Include(x => x.Questions)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        Guid? tracingTemplateId = null;
        if (item.InteractionType == InteractionTypes.Tracing)
        {
            tracingTemplateId = ReadJsonGuid(
                item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault()?.PayloadJson,
                "templateId");
        }

        await _db.QuestionAttempts
            .Where(x => x.Question != null && x.Question.LearningItemId == item.Id)
            .ExecuteDeleteAsync();

        await _db.LearningAttempts
            .Where(x => x.LearningItemId == item.Id)
            .ExecuteDeleteAsync();

        _db.LearningItems.Remove(item);
        if (tracingTemplateId.HasValue)
        {
            var tracingTemplate = await _db.TracingTemplates.FirstOrDefaultAsync(x => x.Id == tracingTemplateId.Value);
            if (tracingTemplate is not null)
            {
                _db.TracingTemplates.Remove(tracingTemplate);
            }
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["AdminMessage"] = $"Đã xóa bài học “{item.Title}”.";
        return RedirectToAction(nameof(LearningItems));
    }

    private IActionResult RedirectToEditor(LearningItem item)
    {
        return item.InteractionType == InteractionTypes.Tracing
            ? RedirectToAction(nameof(CreateTracing), new { editId = item.Id })
            : RedirectToAction(nameof(CreateChoice), new { editId = item.Id });
    }

    private async Task LoadContentListsAsync(Guid? includeSkillGroupId = null, Guid? includeTopicId = null)
    {
        ViewBag.SkillGroups = await _db.SkillGroups
            .Where(x => x.IsActive || x.Id == includeSkillGroupId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();
        ViewBag.Topics = await _db.Topics
            .Where(x => x.IsActive || x.Id == includeTopicId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();
        ViewBag.ActivityTemplates = ActivityTemplateCatalog.Templates;
        var mediaAssets = await _db.MediaAssets.OrderByDescending(x => x.CreatedAt).ToListAsync();
        ViewBag.MediaAssets = mediaAssets;
        ViewBag.ImageAssetsJson = JsonSerializer.Serialize(mediaAssets
            .Where(x => x.AssetType == "image")
            .Take(1000)
            .Select(x => new
            {
                x.Id,
                x.FileName,
                x.StoragePath,
                x.AltText
            }));
        var voiceEntries = await _db.TextToSpeechCaches
            .OrderByDescending(x => x.UpdatedAt)
            .Take(1000)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.UsageType,
                x.NormalizedText,
                x.AudioUrl,
                x.Status
            })
            .ToListAsync();
        ViewBag.VoiceCacheJson = JsonSerializer.Serialize(voiceEntries);
    }

    private async Task LoadTracingListsAsync()
    {
        var topics = await _db.Topics.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync();
        var tracingTopics = topics.Where(x => ActivityTemplateCatalog.ForTopic(x.Code).AllowsTracing).ToList();
        var groupIds = tracingTopics.Select(x => x.SkillGroupId).Distinct().ToList();
        ViewBag.SkillGroups = await _db.SkillGroups
            .Where(x => x.IsActive && groupIds.Contains(x.Id))
            .OrderBy(x => x.SortOrder)
            .ToListAsync();
        ViewBag.Topics = tracingTopics;
        ViewBag.MediaAssets = await _db.MediaAssets
            .Where(x => x.AssetType == "audio")
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    private async Task ValidateTracingTopicAsync(Guid? topicId)
    {
        if (!topicId.HasValue)
        {
            ModelState.AddModelError(nameof(CreateTracingItemViewModel.TopicId), "Vui lòng chọn chủ đề tô nét.");
            return;
        }

        var topicCode = await _db.Topics.Where(x => x.Id == topicId.Value).Select(x => x.Code).FirstOrDefaultAsync();
        if (topicCode is null || !ActivityTemplateCatalog.ForTopic(topicCode).AllowsTracing)
        {
            ModelState.AddModelError(nameof(CreateTracingItemViewModel.TopicId), "Chủ đề này không hỗ trợ bài tô theo nét.");
        }
    }

    private async Task PrepareTracingMediaAsync(CreateTracingItemViewModel model)
    {
        if (model.ExistingAudioAssetId.HasValue)
        {
            var audio = await _db.MediaAssets.FirstOrDefaultAsync(x =>
                x.Id == model.ExistingAudioAssetId.Value && x.AssetType == "audio");
            if (audio is null)
            {
                ModelState.AddModelError(nameof(model.ExistingAudioAssetId), "Âm thanh trong thư viện không còn tồn tại.");
            }
            else
            {
                model.AudioUrl = audio.StoragePath;
            }
        }
        ValidateMediaFile(model.AudioFile, "audio", 10 * 1024 * 1024, nameof(model.AudioFile));
    }

    private async Task<string> ResolveTracingSymbolTypeAsync(Guid? topicId)
    {
        var code = topicId.HasValue
            ? await _db.Topics.Where(x => x.Id == topicId.Value).Select(x => x.Code).FirstOrDefaultAsync()
            : null;
        return code switch
        {
            "chu-in-hoa" => "uppercase",
            "chu-in-thuong" => "lowercase",
            "viet-so" => "number",
            "net-co-ban" => "stroke",
            "noi-diem" => "connect-dots",
            _ => "symbol"
        };
    }

    private async Task ValidateActivityTemplateAsync(Guid? topicId, string interactionType)
    {
        if (!topicId.HasValue)
        {
            ModelState.AddModelError(nameof(CreateChoiceItemViewModel.TopicId), "Vui lòng chọn chủ đề trước khi chọn mẫu hoạt động.");
            return;
        }

        var topicCode = await _db.Topics
            .Where(x => x.Id == topicId.Value)
            .Select(x => x.Code)
            .FirstOrDefaultAsync();
        if (topicCode is null || !ActivityTemplateCatalog.IsAllowed(topicCode, interactionType))
        {
            ModelState.AddModelError(nameof(CreateChoiceItemViewModel.InteractionType), "Mẫu hoạt động không phù hợp với chủ đề đã chọn.");
        }
    }

    private async Task PrepareMediaSelectionAsync(CreateChoiceItemViewModel model)
    {
        if (model.ExistingImageAssetId.HasValue)
        {
            var image = await _db.MediaAssets.FirstOrDefaultAsync(x =>
                x.Id == model.ExistingImageAssetId.Value && x.AssetType == "image");
            if (image is null)
            {
                ModelState.AddModelError(nameof(model.ExistingImageAssetId), "Hình trong thư viện không còn tồn tại.");
            }
            else
            {
                model.ImageUrl = image.StoragePath;
            }
        }

        if (model.ExistingAudioAssetId.HasValue)
        {
            var audio = await _db.MediaAssets.FirstOrDefaultAsync(x =>
                x.Id == model.ExistingAudioAssetId.Value && x.AssetType == "audio");
            if (audio is null)
            {
                ModelState.AddModelError(nameof(model.ExistingAudioAssetId), "Âm thanh trong thư viện không còn tồn tại.");
            }
            else
            {
                model.AudioUrl = audio.StoragePath;
            }
        }

        if (model.ExistingQuestionAudioAssetId.HasValue)
        {
            var questionAudio = await _db.MediaAssets.FirstOrDefaultAsync(x =>
                x.Id == model.ExistingQuestionAudioAssetId.Value && x.AssetType == "audio");
            if (questionAudio is null)
            {
                ModelState.AddModelError(nameof(model.ExistingQuestionAudioAssetId), "Âm thanh câu hỏi trong thư viện không còn tồn tại.");
            }
            else
            {
                model.QuestionAudioUrl = questionAudio.StoragePath;
            }
        }

        ValidateMediaFile(model.ImageFile, "image", 5 * 1024 * 1024, nameof(model.ImageFile));
        ValidateMediaFile(model.AudioFile, "audio", 10 * 1024 * 1024, nameof(model.AudioFile));
        ValidateMediaFile(model.QuestionAudioFile, "audio", 10 * 1024 * 1024, nameof(model.QuestionAudioFile));
    }

    private void ValidateMediaFile(IFormFile? file, string assetType, long maxBytes, string fieldName)
    {
        if (file is null)
        {
            return;
        }

        var allowedExtensions = assetType == "image"
            ? new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }
            : new[] { ".mp3", ".wav", ".ogg", ".m4a" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (file.Length == 0 || file.Length > maxBytes || !allowedExtensions.Contains(extension))
        {
            var limit = maxBytes / 1024 / 1024;
            ModelState.AddModelError(fieldName, $"Tệp {assetType} không hợp lệ hoặc vượt quá {limit} MB.");
        }
    }

    private async Task SaveUploadedMediaAsync(CreateChoiceItemViewModel model)
    {
        if (model.ImageFile is not null)
        {
            model.ImageUrl = await SaveMediaFileAsync(model.ImageFile, "image");
        }
        if (model.AudioFile is not null)
        {
            model.AudioUrl = await SaveMediaFileAsync(model.AudioFile, "audio");
        }
        if (model.QuestionAudioFile is not null)
        {
            model.QuestionAudioUrl = await SaveMediaFileAsync(model.QuestionAudioFile, "audio");
        }
    }

    private async Task PopulateVoiceUrlsFromCacheAsync(CreateChoiceItemViewModel model)
    {
        model.TitleAudioUrl = await ResolveVoiceAudioAsync(model.Title, "title", model.Title) ?? model.TitleAudioUrl;
        model.QuestionAudioUrl = await ResolveVoiceAudioAsync(model.PromptText, "question", model.Title) ?? model.QuestionAudioUrl;
        model.InstructionAudioUrl = await ResolveVoiceAudioAsync(model.InstructionText, "instruction", model.Title) ?? model.InstructionAudioUrl;
        model.CorrectFeedbackAudioUrl = await ResolveVoiceAudioAsync(model.CorrectFeedback, "correct-feedback", model.Title) ?? model.CorrectFeedbackAudioUrl;
        model.RetryFeedbackAudioUrl = await ResolveVoiceAudioAsync(model.RetryFeedback, "retry-feedback", model.Title) ?? model.RetryFeedbackAudioUrl;

        if ((model.InteractionType == InteractionTypes.ListenAndChoose ||
             model.InteractionType == InteractionTypes.StoryChoice) &&
            string.IsNullOrWhiteSpace(model.AudioUrl) &&
            !string.IsNullOrWhiteSpace(model.SpeechText))
        {
            model.AudioUrl = await ResolveVoiceAudioAsync(model.SpeechText, "content", model.Title) ?? string.Empty;
        }
    }

    private async Task<int> SyncVoiceForLearningItemAsync(LearningItem item, bool onlyMissing)
    {
        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        if (question is null)
        {
            return 0;
        }

        var payload = ParsePayloadObject(question.PayloadJson);
        var linkedCount = 0;
        linkedCount += await SyncPayloadVoiceAsync(payload, "titleAudioUrl", item.Title, "title", item.Title, onlyMissing);
        linkedCount += await SyncPayloadVoiceAsync(payload, "instructionAudioUrl", item.InstructionText, "instruction", item.Title, onlyMissing);
        linkedCount += await SyncPayloadVoiceAsync(payload, "correctAudioUrl", ReadJsonString(question.FeedbackJson, "correct"), "correct-feedback", item.Title, onlyMissing);
        linkedCount += await SyncPayloadVoiceAsync(payload, "retryAudioUrl", ReadJsonString(question.FeedbackJson, "retry"), "retry-feedback", item.Title, onlyMissing);

        if (item.InteractionType == InteractionTypes.Tracing)
        {
            linkedCount += await SyncPayloadVoiceAsync(payload, "audioUrl", question.PromptText, "tracing-prompt", item.Title, onlyMissing);
            linkedCount += await SyncPayloadVoiceAsync(payload, "questionAudioUrl", question.PromptText, "question", item.Title, onlyMissing);
        }
        else
        {
            linkedCount += await SyncPayloadVoiceAsync(payload, "questionAudioUrl", question.PromptText, "question", item.Title, onlyMissing);
            if (item.InteractionType == InteractionTypes.ListenAndChoose ||
                item.InteractionType == InteractionTypes.StoryChoice)
            {
                linkedCount += await SyncPayloadVoiceAsync(payload, "audioUrl", ReadJsonString(payload, "speechText"), "content", item.Title, onlyMissing);
            }
        }

        linkedCount += await SyncOptionVoiceMapAsync(payload, item.Title, onlyMissing);

        var payloadJson = payload.ToJsonString();
        question.PayloadJson = payloadJson;
        item.ContentJson = payloadJson;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        return linkedCount;
    }

    private async Task<int> SyncPayloadVoiceAsync(JsonObject payload, string propertyName, string text, string usageType, string? lessonTitle, bool onlyMissing)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        if (onlyMissing && !string.IsNullOrWhiteSpace(ReadJsonString(payload, propertyName)))
        {
            await EnsureVoiceCacheEntryAsync(text, usageType, lessonTitle);
            return 0;
        }

        var audioUrl = await ResolveVoiceAudioAsync(text, usageType, lessonTitle);
        if (string.IsNullOrWhiteSpace(audioUrl))
        {
            payload[propertyName] = string.Empty;
            return 0;
        }

        payload[propertyName] = audioUrl;
        return 1;
    }

    private async Task<int> SyncOptionVoiceMapAsync(JsonObject payload, string? lessonTitle, bool onlyMissing)
    {
        var labels = CollectOptionSpeechLabels(payload)
            .Select(Clean)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (labels.Count == 0)
        {
            return 0;
        }

        var audioMap = payload.TryGetPropertyValue("optionAudio", out var existingNode) && existingNode is JsonObject existingObject
            ? existingObject
            : new JsonObject();
        var linkedCount = 0;
        foreach (var label in labels)
        {
            if (onlyMissing && !string.IsNullOrWhiteSpace(ReadJsonString(audioMap, label)))
            {
                await EnsureVoiceCacheEntryAsync(label, "option", lessonTitle);
                continue;
            }

            var audioUrl = await ResolveVoiceAudioAsync(label, "option", lessonTitle);
            if (string.IsNullOrWhiteSpace(audioUrl))
            {
                audioMap[label] = string.Empty;
                continue;
            }

            audioMap[label] = audioUrl;
            linkedCount += 1;
        }

        payload["optionAudio"] = audioMap;
        return linkedCount;
    }
    private static IEnumerable<string> CollectOptionSpeechLabels(JsonObject payload)
    {
        foreach (var value in ReadJsonStringArray(payload, "choices"))
        {
            yield return value;
        }

        foreach (var value in ReadJsonStringArray(payload, "items"))
        {
            yield return value;
        }

        foreach (var value in ReadJsonStringArray(payload, "categories"))
        {
            yield return value;
        }

        foreach (var value in ReadJsonMappingLabels(payload, "pairs"))
        {
            yield return value;
        }

        foreach (var value in ReadJsonMappingLabels(payload, "mappings"))
        {
            yield return value;
        }

        var targetLabel = ReadJsonString(payload, "targetLabel");
        if (!string.IsNullOrWhiteSpace(targetLabel))
        {
            yield return targetLabel;
        }

        var leftLabel = ReadJsonString(payload, "leftLabel");
        if (!string.IsNullOrWhiteSpace(leftLabel))
        {
            yield return leftLabel;
        }

        var rightLabel = ReadJsonString(payload, "rightLabel");
        if (!string.IsNullOrWhiteSpace(rightLabel))
        {
            yield return rightLabel;
        }
    }

    private async Task<string> SaveMediaFileAsync(IFormFile file, string assetType)
    {
        var folderName = assetType == "image" ? "images" : "audio";
        var folder = Path.Combine(_environment.WebRootPath, "uploads", folderName);
        Directory.CreateDirectory(folder);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var storedName = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}{extension}";
        var diskPath = Path.Combine(folder, storedName);
        await using (var stream = System.IO.File.Create(diskPath))
        {
            await file.CopyToAsync(stream);
        }

        var storagePath = $"/uploads/{folderName}/{storedName}";
        _db.MediaAssets.Add(new MediaAsset
        {
            Id = Guid.NewGuid(),
            AssetType = assetType,
            FileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            StoragePath = storagePath,
            CreatedAt = DateTimeOffset.UtcNow
        });
        return storagePath;
    }

    private async Task<string?> ResolveVoiceAudioAsync(string text, string usageType, string? lessonTitle = null)
    {
        var entry = await EnsureVoiceCacheEntryAsync(text, usageType, lessonTitle);
        return entry is { Status: "ready", AudioUrl.Length: > 0 } ? entry.AudioUrl : null;
    }

    private async Task<TextToSpeechCache?> EnsureVoiceCacheEntryAsync(string text, string usageType, string? lessonTitle = null)
    {
        var normalizedText = NormalizeSpeechText(text);
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return null;
        }

        var cacheKey = BuildTextToSpeechCacheKey(normalizedText);
        var pendingEntry = _db.ChangeTracker.Entries<TextToSpeechCache>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Unchanged)
            .Select(x => x.Entity)
            .FirstOrDefault(x =>
                x.Provider == cacheKey.Provider &&
                x.Voice == cacheKey.Voice &&
                x.ModelId == cacheKey.ModelId &&
                x.Format == cacheKey.Format &&
                x.TextHash == cacheKey.TextHash);
        if (pendingEntry is not null)
        {
            if (string.IsNullOrWhiteSpace(pendingEntry.Name))
            {
                pendingEntry.Name = BuildVoiceName(usageType, lessonTitle, normalizedText);
            }
            if (string.IsNullOrWhiteSpace(pendingEntry.UsageType))
            {
                pendingEntry.UsageType = usageType;
            }
            pendingEntry.ReuseCount += 1;
            pendingEntry.UpdatedAt = DateTimeOffset.UtcNow;
            return pendingEntry;
        }

        var entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
            x.Provider == cacheKey.Provider &&
            x.Voice == cacheKey.Voice &&
            x.ModelId == cacheKey.ModelId &&
            x.Format == cacheKey.Format &&
            x.TextHash == cacheKey.TextHash);
        if (entry is not null)
        {
            if (string.IsNullOrWhiteSpace(entry.Name))
            {
                entry.Name = BuildVoiceName(usageType, lessonTitle, normalizedText);
            }
            if (string.IsNullOrWhiteSpace(entry.UsageType))
            {
                entry.UsageType = usageType;
            }
            entry.ReuseCount += 1;
            entry.UpdatedAt = DateTimeOffset.UtcNow;
            return entry;
        }

        entry = new TextToSpeechCache
        {
            Id = Guid.NewGuid(),
            Provider = cacheKey.Provider,
            Voice = cacheKey.Voice,
            ModelId = cacheKey.ModelId,
            Format = cacheKey.Format,
            TextHash = cacheKey.TextHash,
            Name = BuildVoiceName(usageType, lessonTitle, normalizedText),
            UsageType = usageType,
            NormalizedText = AudioAltText(normalizedText),
            OriginalText = AudioOriginalText(text),
            AudioUrl = string.Empty,
            Status = "missing",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _db.TextToSpeechCaches.Add(entry);
        return entry;
    }

    private async Task<string> SaveVoiceCacheFileAsync(IFormFile file, TextToSpeechCache entry)
    {
        var folder = Path.Combine(_environment.WebRootPath, "uploads", "audio");
        Directory.CreateDirectory(folder);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".mp3";
        }
        var storedName = $"voice-{NormalizeCode(entry.Name)}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}{extension}";
        var diskPath = Path.Combine(folder, storedName);
        await using (var stream = System.IO.File.Create(diskPath))
        {
            await file.CopyToAsync(stream);
        }

        var storagePath = $"/uploads/audio/{storedName}";
        _db.MediaAssets.Add(new MediaAsset
        {
            Id = Guid.NewGuid(),
            AssetType = "audio",
            FileName = storedName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "audio/mpeg" : file.ContentType,
            StoragePath = storagePath,
            AltText = AudioCacheKey(entry.NormalizedText),
            CreatedAt = DateTimeOffset.UtcNow
        });
        return storagePath;
    }

    private async Task ApplyVoiceCacheUploadAsync(TextToSpeechCache entry, IFormFile audioFile)
    {
        entry.AudioUrl = await SaveVoiceCacheFileAsync(audioFile, entry);
        entry.Status = "ready";
        entry.LastError = null;
        entry.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private async Task<string> GenerateVoiceCacheFileAsync(TextToSpeechCache entry)
    {
        var text = ResolveTextForSpeechSynthesis(NormalizeSpeechText(string.IsNullOrWhiteSpace(entry.OriginalText)
            ? entry.NormalizedText
            : entry.OriginalText));
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Voice không có nội dung text để tạo file.");
        }

        var folder = Path.Combine(_environment.WebRootPath, "uploads", "audio");
        Directory.CreateDirectory(folder);

        var storedName = $"voice-{NormalizeCode(entry.Name)}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.mp3";
        var diskPath = Path.Combine(folder, storedName);
        var voice = _configuration["VoiceLibrary:Voice"]?.Trim();
        if (string.IsNullOrWhiteSpace(voice))
        {
            voice = "vi-VN-HoaiMyNeural";
        }
        var rate = _configuration["VoiceLibrary:Rate"]?.Trim();
        if (string.IsNullOrWhiteSpace(rate))
        {
            rate = "-10%";
        }

        await RunEdgeTextToSpeechAsync(text, voice, rate, diskPath);

        var storagePath = $"/uploads/audio/{storedName}";
        _db.MediaAssets.Add(new MediaAsset
        {
            Id = Guid.NewGuid(),
            AssetType = "audio",
            FileName = storedName,
            ContentType = "audio/mpeg",
            StoragePath = storagePath,
            AltText = AudioCacheKey(entry.NormalizedText),
            CreatedAt = DateTimeOffset.UtcNow
        });
        return storagePath;
    }

    private static async Task RunEdgeTextToSpeechAsync(string text, string voice, string rate, string outputPath)
    {
        var candidates = new[]
        {
            ("python", new[] { "-m", "edge_tts" }),
            ("py", new[] { "-m", "edge_tts" })
        };
        var errors = new List<string>();
        foreach (var (fileName, prefixArgs) in candidates)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            foreach (var arg in prefixArgs)
            {
                startInfo.ArgumentList.Add(arg);
            }
            startInfo.ArgumentList.Add("--voice");
            startInfo.ArgumentList.Add(voice);
            startInfo.ArgumentList.Add("--rate");
            startInfo.ArgumentList.Add(rate);
            startInfo.ArgumentList.Add("--text");
            startInfo.ArgumentList.Add(text);
            startInfo.ArgumentList.Add("--write-media");
            startInfo.ArgumentList.Add(outputPath);

            try
            {
                using var process = Process.Start(startInfo);
                if (process is null)
                {
                    errors.Add($"{fileName}: không khởi động được process.");
                    continue;
                }

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();
                var exitTask = process.WaitForExitAsync();
                var completedTask = await Task.WhenAny(exitTask, Task.Delay(TimeSpan.FromSeconds(45)));
                if (completedTask != exitTask)
                {
                    try
                    {
                        process.Kill(entireProcessTree: true);
                    }
                    catch
                    {
                        // Ignore kill errors; the failure message below is enough for admin.
                    }
                    errors.Add($"{fileName}: quá thời gian tạo voice.");
                    continue;
                }

                var stdout = await outputTask;
                var stderr = await errorTask;
                if (process.ExitCode == 0 && System.IO.File.Exists(outputPath) && new FileInfo(outputPath).Length > 0)
                {
                    return;
                }

                errors.Add($"{fileName}: {stderr} {stdout}".Trim());
            }
            catch (Exception ex)
            {
                errors.Add($"{fileName}: {ex.Message}");
            }
        }

        throw new InvalidOperationException(string.Join(" | ", errors.Where(x => !string.IsNullOrWhiteSpace(x))));
    }

    private static string AudioAltText(string text) => text.Length > 180 ? text[..180] : text;

    private static string AudioOriginalText(string text) => text.Length > 1000 ? text[..1000] : text;

    private static string GetInnermostMessage(Exception exception)
    {
        var current = exception;
        while (current.InnerException is not null)
        {
            current = current.InnerException;
        }

        return current.Message;
    }

    private static string ExtractVoiceTextFromAltText(string altText)
    {
        const string prefix = "tts:v1:";
        var cleaned = Clean(altText);
        return cleaned.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? cleaned[prefix.Length..]
            : cleaned;
    }

    private static string NormalizeSpeechText(string text)
    {
        return string.Join(' ', Clean(text).Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string ResolveTextForSpeechSynthesis(string text) => text switch
    {
        "△" or "▲" => "hình tam giác",
        "□" or "■" => "hình vuông",
        "○" or "●" => "hình tròn",
        "◇" or "◆" => "hình thoi",
        "☆" or "★" => "ngôi sao",
        _ => text
    };

    private static string AudioCacheKey(string normalizedText)
    {
        var key = $"tts:v1:{normalizedText.ToLowerInvariant()}";
        return key.Length > 500 ? key[..500] : key;
    }

    private TextToSpeechCacheKey BuildTextToSpeechCacheKey(string normalizedText)
    {
        var provider = _configuration["VoiceLibrary:Provider"]?.Trim();
        var voice = _configuration["VoiceLibrary:Voice"]?.Trim();
        var modelId = _configuration["VoiceLibrary:ModelId"]?.Trim();
        var format = _configuration["VoiceLibrary:Format"]?.Trim();
        provider = string.IsNullOrWhiteSpace(provider) ? "Manual" : provider;
        voice = string.IsNullOrWhiteSpace(voice) ? "vi-VN-HoaiMyNeural" : voice;
        modelId = string.IsNullOrWhiteSpace(modelId) ? "manual-upload" : modelId;
        format = string.IsNullOrWhiteSpace(format) ? "mp3" : format;
        var hashSource = $"{provider}|{voice}|{modelId}|{format}|{normalizedText.ToLowerInvariant()}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hashSource))).ToLowerInvariant();
        return new TextToSpeechCacheKey(provider, voice, modelId, format, hash);
    }

    private static string BuildVoiceName(string usageType, string? lessonTitle, string normalizedText)
    {
        var prefix = usageType switch
        {
            "title" => "Tiêu đề",
            "instruction" => "Hướng dẫn",
            "question" => "Câu hỏi",
            "correct-feedback" => "Phản hồi đúng",
            "retry-feedback" => "Phản hồi sai",
            "option" => "Đáp án",
            "content" => "Nội dung",
            "tracing-prompt" => "Tô nét",
            _ => "Voice"
        };
        var scope = string.IsNullOrWhiteSpace(lessonTitle) ? normalizedText : lessonTitle;
        return AudioAltText($"{prefix} - {scope}");
    }

    private sealed record TextToSpeechCacheKey(
        string Provider,
        string Voice,
        string ModelId,
        string Format,
        string TextHash);
    private static CreateChoiceItemViewModel BuildActivityEditorModel(LearningItem item)
    {
        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        var payloadJson = question?.PayloadJson ?? "{}";
        var choices = ReadJsonStringArray(payloadJson, "choices");
        var correctAnswer = ReadJsonString(question?.CorrectAnswerJson, "value");
        var hintText = ReadJsonString(question?.HintJson, "level1");
        var correctFeedback = ReadJsonString(question?.FeedbackJson, "correct");
        var retryFeedback = ReadJsonString(question?.FeedbackJson, "retry");

        return new CreateChoiceItemViewModel
        {
            Id = item.Id,
            Status = item.Status,
            IsCompatible = ActivityTemplateCatalog.IsItemAllowed(item),
            Title = item.Title,
            SkillGroupId = item.SkillGroupId,
            TopicId = item.TopicId,
            SortOrder = item.SortOrder,
            InteractionType = item.InteractionType,
            InstructionText = item.InstructionText,
            PromptText = question?.PromptText ?? string.Empty,
            ChoiceA = choices.ElementAtOrDefault(0) ?? string.Empty,
            ChoiceB = choices.ElementAtOrDefault(1) ?? string.Empty,
            ChoiceC = choices.ElementAtOrDefault(2) ?? string.Empty,
            ChoiceD = choices.ElementAtOrDefault(3) ?? string.Empty,
            ChoiceE = choices.ElementAtOrDefault(4) ?? string.Empty,
            CorrectAnswer = correctAnswer,
            CorrectAnswersText = item.InteractionType == InteractionTypes.MultiSelect
                ? string.Join(Environment.NewLine, correctAnswer.Split('|', StringSplitOptions.RemoveEmptyEntries))
                : string.Empty,
            SequenceItemsText = string.Join(Environment.NewLine, ReadJsonStringArray(payloadJson, "items")),
            PairsText = ReadJsonMappingLines(payloadJson, "pairs"),
            ClassificationText = ReadJsonMappingLines(payloadJson, "mappings"),
            ItemMediaText = ReadJsonObjectLines(payloadJson, "itemMedia"),
            TargetLabel = ReadJsonString(payloadJson, "targetLabel"),
            ObjectSymbol = ReadJsonString(payloadJson, "objectSymbol"),
            TargetCount = ReadJsonInt(payloadJson, "targetCount", 4),
            SecondaryCount = ReadJsonInt(payloadJson, "rightCount", 2),
            ComparisonMode = ReadJsonString(payloadJson, "comparisonMode") is { Length: > 0 } comparisonMode ? comparisonMode : "more",
            ImageUrl = ReadJsonString(payloadJson, "imageUrl"),
            ImageAltText = ReadJsonString(payloadJson, "imageAltText"),
            AudioUrl = ReadJsonString(payloadJson, "audioUrl"),
            TitleAudioUrl = ReadJsonString(payloadJson, "titleAudioUrl"),
            QuestionAudioUrl = ReadJsonString(payloadJson, "questionAudioUrl"),
            InstructionAudioUrl = ReadJsonString(payloadJson, "instructionAudioUrl"),
            CorrectFeedbackAudioUrl = ReadJsonString(payloadJson, "correctAudioUrl"),
            RetryFeedbackAudioUrl = ReadJsonString(payloadJson, "retryAudioUrl"),
            SpeechText = ReadJsonString(payloadJson, "speechText"),
            LeftLabel = ReadJsonString(payloadJson, "leftLabel") is { Length: > 0 } leftLabel ? leftLabel : "Nhóm A",
            RightLabel = ReadJsonString(payloadJson, "rightLabel") is { Length: > 0 } rightLabel ? rightLabel : "Nhóm B",
            Level = item.Level,
            EstimatedMinutes = item.EstimatedMinutes,
            HintText = string.IsNullOrWhiteSpace(hintText) ? "Con nhìn kỹ từng lựa chọn nhé." : hintText,
            CorrectFeedback = string.IsNullOrWhiteSpace(correctFeedback) ? "Giỏi lắm, con chọn đúng rồi!" : correctFeedback,
            RetryFeedback = string.IsNullOrWhiteSpace(retryFeedback) ? "Không sao, mình thử lại nhẹ nhàng nhé." : retryFeedback
        };
    }

    private static bool NormalizeLegacyActivityPayload(LearningItem item)
    {
        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        if (question is null)
        {
            return false;
        }

        var payload = ParsePayloadObject(question.PayloadJson);
        var changed = false;

        EnsureNumber("schemaVersion", 2);
        EnsureString("activityType", item.InteractionType);
        EnsureString("imageUrl");
        EnsureString("imageAltText");
        EnsureString("audioUrl");
        EnsureString("titleAudioUrl");
        EnsureString("questionAudioUrl");
        EnsureString("instructionAudioUrl");
        EnsureString("correctAudioUrl");
        EnsureString("retryAudioUrl");
        EnsureString("speechText");
        EnsureString("instructionSpeechText", item.InstructionText);
        EnsureString("questionSpeechText", question.PromptText);
        EnsureString("correctSpeechText", ReadJsonString(question.FeedbackJson, "correct"));
        EnsureString("retrySpeechText", ReadJsonString(question.FeedbackJson, "retry"));
        EnsureObject("itemMedia");
        EnsureObject("optionAudio");

        if (!changed)
        {
            return false;
        }

        var payloadJson = payload.ToJsonString();
        question.PayloadJson = payloadJson;
        item.ContentJson = payloadJson;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        return true;

        void EnsureString(string propertyName, string? value = null)
        {
            if (payload.TryGetPropertyValue(propertyName, out var node) && node is not null)
            {
                return;
            }

            payload[propertyName] = Clean(value);
            changed = true;
        }

        void EnsureNumber(string propertyName, int value)
        {
            if (payload.TryGetPropertyValue(propertyName, out var node) && node is not null)
            {
                return;
            }

            payload[propertyName] = value;
            changed = true;
        }

        void EnsureObject(string propertyName)
        {
            if (payload.TryGetPropertyValue(propertyName, out var node) && node is JsonObject)
            {
                return;
            }

            payload[propertyName] = new JsonObject();
            changed = true;
        }
    }

    private ActivityConfiguration? BuildActivityConfiguration(CreateChoiceItemViewModel model)
    {
        var template = ActivityTemplateCatalog.Find(model.InteractionType);
        if (template?.RequiresAudio == true && string.IsNullOrWhiteSpace(model.AudioUrl) &&
            string.IsNullOrWhiteSpace(model.SpeechText) && model.AudioFile is null)
        {
            ModelState.AddModelError(nameof(model.AudioUrl), "Dạng bài này cần nội dung đọc trong bảng Kiểm soát voice hoặc file âm thanh đã gắn.");
        }
        if (template?.RequiresImage == true && string.IsNullOrWhiteSpace(model.ImageUrl) && model.ImageFile is null)
        {
            ModelState.AddModelError(nameof(model.ImageUrl), "Dạng bài này cần một hình minh họa hoặc hình trong thư viện.");
        }
        if (!ModelState.IsValid)
        {
            return null;
        }

        var choices = BuildChoices(model.ChoiceA, model.ChoiceB, model.ChoiceC, model.ChoiceD, model.ChoiceE);
        var payload = new JsonObject
        {
            ["schemaVersion"] = 2,
            ["activityType"] = model.InteractionType,
            ["imageUrl"] = Clean(model.ImageUrl),
            ["imageAltText"] = Clean(model.ImageAltText),
            ["audioUrl"] = Clean(model.AudioUrl),
            ["titleAudioUrl"] = Clean(model.TitleAudioUrl),
            ["questionAudioUrl"] = Clean(model.QuestionAudioUrl),
            ["instructionAudioUrl"] = Clean(model.InstructionAudioUrl),
            ["correctAudioUrl"] = Clean(model.CorrectFeedbackAudioUrl),
            ["retryAudioUrl"] = Clean(model.RetryFeedbackAudioUrl),
            ["speechText"] = Clean(model.SpeechText),
            ["instructionSpeechText"] = Clean(model.InstructionText),
            ["questionSpeechText"] = Clean(model.PromptText),
            ["correctSpeechText"] = "Giỏi lắm, con đã làm đúng!",
            ["retrySpeechText"] = "Con quan sát kỹ rồi thử lại nhé."
        };

        payload["correctSpeechText"] = Clean(model.CorrectFeedback);
        payload["retrySpeechText"] = Clean(model.RetryFeedback);

        var itemMedia = ParseMappings(
            model.ItemMediaText,
            nameof(model.ItemMediaText),
            "Mỗi ảnh riêng cần có dạng Tên nội dung = Đường dẫn ảnh.");
        if (itemMedia.Count > 30)
        {
            ModelState.AddModelError(nameof(model.ItemMediaText), "Mỗi bài dùng tối đa 30 ảnh riêng.");
            return null;
        }
        foreach (var media in itemMedia)
        {
            if (!Uri.TryCreate(media.Right, UriKind.RelativeOrAbsolute, out var uri) ||
                (!media.Right.StartsWith('/') && !uri.IsAbsoluteUri))
            {
                ModelState.AddModelError(nameof(model.ItemMediaText), $"Đường dẫn ảnh của '{media.Left}' chưa hợp lệ.");
            }
        }
        if (!ModelState.IsValid)
        {
            return null;
        }
        payload["itemMedia"] = new JsonObject(itemMedia
            .Select(media => new KeyValuePair<string, JsonNode?>(media.Left, JsonValue.Create(media.Right))));

        switch (model.InteractionType)
        {
            case InteractionTypes.SingleChoice:
            case InteractionTypes.ListenAndChoose:
            case InteractionTypes.StoryChoice:
            case InteractionTypes.DragDrop:
                if (!ValidateChoiceSet(choices, model.CorrectAnswer))
                {
                    return null;
                }
                payload["choices"] = ToJsonArray(choices);
                payload["targetLabel"] = Clean(model.TargetLabel);
                return new(payload.ToJsonString(), Clean(model.CorrectAnswer));

            case InteractionTypes.MultiSelect:
                var correctValues = SplitLines(model.CorrectAnswersText)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                if (choices.Length < 2 || correctValues.Length == 0 ||
                    correctValues.Any(value => !choices.Contains(value, StringComparer.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError(nameof(model.CorrectAnswersText), "Các đáp án đúng phải nằm trong danh sách lựa chọn.");
                    return null;
                }
                payload["choices"] = ToJsonArray(choices);
                payload["correctCount"] = correctValues.Length;
                return new(payload.ToJsonString(), CanonicalList(correctValues));

            case InteractionTypes.Matching:
                var pairs = ParseMappings(model.PairsText, nameof(model.PairsText), "Mỗi cặp cần có dạng Bên trái = Bên phải.");
                if (pairs.Count < 2)
                {
                    ModelState.AddModelError(nameof(model.PairsText), "Bài nối cặp cần ít nhất hai cặp.");
                    return null;
                }
                payload["pairs"] = ToMappingArray(pairs);
                return new(payload.ToJsonString(), CanonicalMappings(pairs));

            case InteractionTypes.Ordering:
                var sequence = SplitLines(model.SequenceItemsText);
                if (sequence.Length < 2)
                {
                    ModelState.AddModelError(nameof(model.SequenceItemsText), "Bài sắp xếp cần ít nhất hai mục.");
                    return null;
                }
                payload["items"] = ToJsonArray(sequence);
                return new(payload.ToJsonString(), string.Join('|', sequence));

            case InteractionTypes.Counting:
                if (choices.Length < 2)
                {
                    choices =
                    [
                        Math.Max(0, model.TargetCount - 1).ToString(),
                        model.TargetCount.ToString(),
                        (model.TargetCount + 1).ToString()
                    ];
                }
                payload["choices"] = ToJsonArray(choices.Distinct().ToArray());
                payload["objectSymbol"] = Clean(model.ObjectSymbol);
                payload["targetCount"] = model.TargetCount;
                return new(payload.ToJsonString(), model.TargetCount.ToString());

            case InteractionTypes.QuantityBuilder:
                payload["objectSymbol"] = Clean(model.ObjectSymbol);
                payload["targetCount"] = model.TargetCount;
                payload["maxItems"] = Math.Min(20, model.TargetCount + 3);
                payload["targetLabel"] = Clean(model.TargetLabel);
                return new(payload.ToJsonString(), model.TargetCount.ToString());

            case InteractionTypes.Comparison:
                payload["objectSymbol"] = Clean(model.ObjectSymbol);
                payload["leftCount"] = model.TargetCount;
                payload["rightCount"] = model.SecondaryCount;
                payload["leftLabel"] = Clean(model.LeftLabel);
                payload["rightLabel"] = Clean(model.RightLabel);
                var comparisonMode = model.ComparisonMode is "less" or "equal" ? model.ComparisonMode : "more";
                if (comparisonMode == "equal" && model.TargetCount != model.SecondaryCount)
                {
                    ModelState.AddModelError(nameof(model.SecondaryCount), "Khi chọn kiểm tra bằng nhau, hai nhóm phải có cùng số lượng.");
                    return null;
                }
                payload["comparisonMode"] = comparisonMode;
                var comparisonAnswer = comparisonMode switch
                {
                    "equal" => "equal",
                    "less" => model.TargetCount == model.SecondaryCount
                        ? "equal"
                        : model.TargetCount < model.SecondaryCount ? "left" : "right",
                    _ => model.TargetCount == model.SecondaryCount
                        ? "equal"
                        : model.TargetCount > model.SecondaryCount ? "left" : "right"
                };
                return new(payload.ToJsonString(), comparisonAnswer);

            case InteractionTypes.Classification:
                var mappings = ParseMappings(
                    model.ClassificationText,
                    nameof(model.ClassificationText),
                    "Mỗi vật cần có dạng Tên vật = Nhóm phân loại.");
                if (mappings.Count < 2 || mappings.Select(x => x.Right).Distinct(StringComparer.OrdinalIgnoreCase).Count() < 2)
                {
                    ModelState.AddModelError(nameof(model.ClassificationText), "Bài phân loại cần ít nhất hai vật và hai nhóm.");
                    return null;
                }
                payload["mappings"] = ToMappingArray(mappings);
                payload["categories"] = ToJsonArray(mappings.Select(x => x.Right).Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
                return new(payload.ToJsonString(), CanonicalMappings(mappings));

            default:
                return null;
        }
    }

    private bool ValidateChoiceSet(string[] choices, string? correctAnswer)
    {
        if (choices.Length >= 2 && choices.Contains(Clean(correctAnswer), StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        ModelState.AddModelError(nameof(CreateChoiceItemViewModel.CorrectAnswer), "Bài cần ít nhất hai lựa chọn và một đáp án đúng nằm trong danh sách.");
        return false;
    }

    private List<ActivityMapping> ParseMappings(string? value, string fieldName, string errorMessage)
    {
        var mappings = new List<ActivityMapping>();
        foreach (var line in SplitLines(value))
        {
            var parts = line.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            {
                ModelState.AddModelError(fieldName, errorMessage);
                continue;
            }
            mappings.Add(new ActivityMapping(parts[0], parts[1]));
        }
        return mappings;
    }

    private static string[] SplitLines(string? value)
    {
        return (value ?? string.Empty)
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static string CanonicalList(IEnumerable<string> values)
    {
        return string.Join('|', values.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
    }

    private static string CanonicalMappings(IEnumerable<ActivityMapping> mappings)
    {
        return string.Join('|', mappings
            .OrderBy(x => x.Left, StringComparer.OrdinalIgnoreCase)
            .Select(x => $"{x.Left}=>{x.Right}"));
    }

    private static JsonArray ToJsonArray(IEnumerable<string> values)
    {
        return new JsonArray(values.Select(value => (JsonNode?)JsonValue.Create(value)).ToArray());
    }

    private static JsonArray ToMappingArray(IEnumerable<ActivityMapping> mappings)
    {
        return new JsonArray(mappings.Select(mapping => (JsonNode?)new JsonObject
        {
            ["left"] = mapping.Left,
            ["right"] = mapping.Right
        }).ToArray());
    }

    private async Task ValidateClassificationAsync(Guid skillGroupId, Guid? topicId, bool requireActive = true)
    {
        if (skillGroupId == Guid.Empty || !await _db.SkillGroups.AnyAsync(x =>
                x.Id == skillGroupId && (!requireActive || x.IsActive)))
        {
            ModelState.AddModelError(nameof(CreateChoiceItemViewModel.SkillGroupId), "Nhóm kỹ năng không hợp lệ hoặc đang tạm ẩn.");
        }

        if (topicId.HasValue && !await _db.Topics.AnyAsync(x =>
                x.Id == topicId.Value && x.SkillGroupId == skillGroupId && (!requireActive || x.IsActive)))
        {
            ModelState.AddModelError(nameof(CreateChoiceItemViewModel.TopicId), "Chủ đề không thuộc nhóm kỹ năng đã chọn hoặc đang tạm ẩn.");
        }
    }

    private void ValidateChoices(string choiceA, string choiceB, string choiceC, string correctAnswer)
    {
        var choices = BuildChoices(choiceA, choiceB, choiceC);
        if (choices.Length < 2 || !choices.Contains(Clean(correctAnswer), StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(CreateChoiceItemViewModel.CorrectAnswer), "Đáp án đúng cần nằm trong các lựa chọn đã nhập.");
        }
    }

    private static string[] BuildChoices(params string?[] values)
    {
        return values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(Clean)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string Clean(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static string CreateLearningItemCode(string prefix)
    {
        var normalizedPrefix = NormalizeCode(prefix);
        var code = $"{normalizedPrefix}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
        return code[..Math.Min(80, code.Length)];
    }

    private async Task<int> GetNextSortOrderAsync(Guid? topicId)
    {
        var currentMax = await _db.LearningItems
            .Where(x => x.TopicId == topicId)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync() ?? 0;
        return currentMax + 10;
    }

    private static string NormalizeCode(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        var separatorPending = false;

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            var current = character == 'đ' ? 'd' : character;
            if (char.IsLetterOrDigit(current))
            {
                if (separatorPending && builder.Length > 0)
                {
                    builder.Append('-');
                }
                builder.Append(current);
                separatorPending = false;
            }
            else
            {
                separatorPending = true;
            }
        }

        return builder.Length == 0 ? $"muc-{Guid.NewGuid():N}"[..12] : builder.ToString();
    }

    private static string ReadJsonString(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        var node = JsonNode.Parse(json);
        return node?[propertyName]?.GetValue<string>() ?? string.Empty;
    }

    private static string ReadJsonString(JsonObject payload, string propertyName)
    {
        return payload.TryGetPropertyValue(propertyName, out var node)
            ? node?.GetValue<string>() ?? string.Empty
            : string.Empty;
    }

    private static IReadOnlyList<string> ReadJsonStringArray(JsonObject payload, string propertyName)
    {
        return payload.TryGetPropertyValue(propertyName, out var node) && node is JsonArray array
            ? array.Select(x => x?.GetValue<string>() ?? string.Empty)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList()
            : [];
    }

    private static IEnumerable<string> ReadJsonMappingLabels(JsonObject payload, string propertyName)
    {
        if (!payload.TryGetPropertyValue(propertyName, out var node) || node is not JsonArray array)
        {
            yield break;
        }

        foreach (var item in array.OfType<JsonObject>())
        {
            var left = ReadJsonString(item, "left");
            if (!string.IsNullOrWhiteSpace(left))
            {
                yield return left;
            }

            var right = ReadJsonString(item, "right");
            if (!string.IsNullOrWhiteSpace(right))
            {
                yield return right;
            }
        }
    }

    private static JsonObject ParsePayloadObject(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new JsonObject();
        }

        try
        {
            return JsonNode.Parse(json)?.AsObject() ?? new JsonObject();
        }
        catch
        {
            return new JsonObject();
        }
    }

    private static IReadOnlyList<string> ReadJsonStringArray(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonNode.Parse(json)?[propertyName]?.AsArray()
            .Select(x => x?.GetValue<string>() ?? string.Empty)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList() ?? [];
    }

    private static int ReadJsonInt(string? json, string propertyName, int fallback)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return fallback;
        }

        return JsonNode.Parse(json)?[propertyName]?.GetValue<int>() ?? fallback;
    }

    private static bool ReadJsonBool(string? json, string propertyName, bool fallback)
    {
        if (string.IsNullOrWhiteSpace(json)) return fallback;
        return JsonNode.Parse(json)?[propertyName]?.GetValue<bool>() ?? fallback;
    }

    private static Guid? ReadJsonGuid(string? json, string propertyName)
    {
        var value = ReadJsonString(json, propertyName);
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static string ReadJsonMappingLines(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        var mappings = JsonNode.Parse(json)?[propertyName]?.AsArray();
        if (mappings is null)
        {
            return string.Empty;
        }

        return string.Join(Environment.NewLine, mappings.Select(mapping =>
            $"{mapping?["left"]?.GetValue<string>()} = {mapping?["right"]?.GetValue<string>()}"));
    }

    private static string ReadJsonObjectLines(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        var values = JsonNode.Parse(json)?[propertyName] as JsonObject;
        return values is null
            ? string.Empty
            : string.Join(Environment.NewLine, values.Select(value => $"{value.Key} = {value.Value?.GetValue<string>()}"));
    }

    [HttpGet("parents-and-kids")]
    public async Task<IActionResult> ParentsAndKids(string? q)
    {
        var usersQuery = _db.Users.AsQueryable();
        var childrenQuery = _db.ChildProfiles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLower();
            usersQuery = usersQuery.Where(u => u.Email!.ToLower().Contains(search) || (u.DisplayName != null && u.DisplayName.ToLower().Contains(search)));
            childrenQuery = childrenQuery.Where(c => c.Nickname.ToLower().Contains(search));
        }

        var users = await usersQuery.OrderByDescending(x => x.Email).ToListAsync();
        var children = await childrenQuery.ToListAsync();

        var childIds = children.Select(x => x.Id).ToList();
        var attempts = await _db.LearningAttempts
            .Where(x => childIds.Contains(x.ChildProfileId))
            .ToListAsync();

        var sessions = await _db.LearningSessions
            .Where(x => childIds.Contains(x.ChildProfileId))
            .ToListAsync();

        var rewards = await _db.ChildRewards
            .Where(x => childIds.Contains(x.ChildProfileId))
            .ToListAsync();

        AdminChildItemViewModel MapChild(ChildProfile child, ApplicationUser? parent)
        {
            var cAttempts = attempts.Where(x => x.ChildProfileId == child.Id).ToList();
            var cSessions = sessions.Where(x => x.ChildProfileId == child.Id).ToList();
            var cRewards = rewards.Where(x => x.ChildProfileId == child.Id).ToList();
            var lastAttempt = cAttempts.OrderByDescending(x => x.StartedAt).FirstOrDefault();

            return new AdminChildItemViewModel
            {
                Id = child.Id,
                Nickname = child.Nickname,
                AvatarKey = child.AvatarKey,
                BirthYear = child.BirthYear,
                DailyLearningMinutes = child.DailyLearningMinutes,
                CreatedAt = child.CreatedAt,
                ParentEmail = parent?.Email,
                ParentDisplayName = parent?.DisplayName,
                TotalStars = cAttempts.Sum(x => x.StarsEarned),
                CompletedLessonsCount = cAttempts.Count(x => x.Status == "completed"),
                TotalSessionsCount = cSessions.Count(x => x.Status == "completed"),
                BadgesCount = cRewards.Count,
                LastActiveAt = lastAttempt?.StartedAt
            };
        }

        var parentItems = new List<AdminParentItemViewModel>();
        foreach (var user in users)
        {
            var userChildren = children.Where(c => c.ParentUserId == user.Id).ToList();
            parentItems.Add(new AdminParentItemViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName,
                Children = userChildren.Select(c => MapChild(c, user)).ToList()
            });
        }

        var guestChildren = children
            .Where(c => string.IsNullOrEmpty(c.ParentUserId))
            .Select(c => MapChild(c, null))
            .ToList();

        var model = new AdminParentsAndKidsViewModel
        {
            SearchQuery = q ?? string.Empty,
            Parents = parentItems,
            GuestChildren = guestChildren,
            TotalParentsCount = users.Count,
            TotalChildrenCount = children.Count,
            TotalCompletedSessions = sessions.Count(x => x.Status == "completed")
        };

        return View(model);
    }

    [HttpGet("kids/{id:guid}")]
    public async Task<IActionResult> ChildDetail(Guid id)
    {
        var child = await _db.ChildProfiles.FirstOrDefaultAsync(x => x.Id == id);
        if (child is null) return NotFound();

        ApplicationUser? parent = null;
        if (!string.IsNullOrEmpty(child.ParentUserId))
        {
            parent = await _db.Users.FirstOrDefaultAsync(x => x.Id == child.ParentUserId);
        }

        var attempts = await _db.LearningAttempts
            .Include(x => x.LearningItem)
            .ThenInclude(x => x!.SkillGroup)
            .Where(x => x.ChildProfileId == id)
            .OrderByDescending(x => x.StartedAt)
            .Take(50)
            .ToListAsync();

        var sessions = await _db.LearningSessions
            .Where(x => x.ChildProfileId == id)
            .OrderByDescending(x => x.StartedAt)
            .Take(20)
            .ToListAsync();

        var rewards = await _db.ChildRewards
            .Include(x => x.RewardDefinition)
            .Where(x => x.ChildProfileId == id)
            .OrderByDescending(x => x.EarnedAt)
            .ToListAsync();

        var skillProgresses = await _db.SkillProgress
            .Include(x => x.SkillGroup)
            .Where(x => x.ChildProfileId == id)
            .ToListAsync();

        var model = new AdminChildDetailViewModel
        {
            Child = child,
            Parent = parent,
            TotalStars = attempts.Sum(x => x.StarsEarned),
            CompletedLessonsCount = attempts.Count(x => x.Status == "completed"),
            NeedsPracticeCount = attempts.Count(x => x.Status == "needs_practice"),
            TotalSessionsCount = sessions.Count(x => x.Status == "completed"),
            EarnedRewards = rewards,
            RecentAttempts = attempts,
            RecentSessions = sessions,
            SkillProgresses = skillProgresses
        };

        return View(model);
    }

    [HttpPost("kids/{id:guid}/clear-progress")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearChildProgress(Guid id)
    {
        var child = await _db.ChildProfiles.FirstOrDefaultAsync(x => x.Id == id);
        if (child is null) return NotFound();

        var attempts = await _db.LearningAttempts.Where(x => x.ChildProfileId == id).ToListAsync();
        var attemptAnswers = await _db.QuestionAttempts
            .Where(x => attempts.Select(a => a.Id).Contains(x.LearningAttemptId))
            .ToListAsync();
        var sessions = await _db.LearningSessions.Where(x => x.ChildProfileId == id).ToListAsync();
        var skillProgresses = await _db.SkillProgress.Where(x => x.ChildProfileId == id).ToListAsync();
        var rewards = await _db.ChildRewards.Where(x => x.ChildProfileId == id).ToListAsync();

        _db.QuestionAttempts.RemoveRange(attemptAnswers);
        _db.LearningAttempts.RemoveRange(attempts);
        _db.LearningSessions.RemoveRange(sessions);
        _db.SkillProgress.RemoveRange(skillProgresses);
        _db.ChildRewards.RemoveRange(rewards);

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Đã xóa sạch tiến độ học tập của bé {child.Nickname} thành công! Bé có thể bắt đầu học lại từ đầu.";
        return RedirectToAction(nameof(ChildDetail), new { id });
    }

    [HttpPost("kids/{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteChildProfile(Guid id)
    {
        var child = await _db.ChildProfiles.FirstOrDefaultAsync(x => x.Id == id);
        if (child is null) return NotFound();

        var attempts = await _db.LearningAttempts.Where(x => x.ChildProfileId == id).ToListAsync();
        var attemptAnswers = await _db.QuestionAttempts
            .Where(x => attempts.Select(a => a.Id).Contains(x.LearningAttemptId))
            .ToListAsync();
        var sessions = await _db.LearningSessions.Where(x => x.ChildProfileId == id).ToListAsync();
        var skillProgresses = await _db.SkillProgress.Where(x => x.ChildProfileId == id).ToListAsync();
        var rewards = await _db.ChildRewards.Where(x => x.ChildProfileId == id).ToListAsync();

        _db.QuestionAttempts.RemoveRange(attemptAnswers);
        _db.LearningAttempts.RemoveRange(attempts);
        _db.LearningSessions.RemoveRange(sessions);
        _db.SkillProgress.RemoveRange(skillProgresses);
        _db.ChildRewards.RemoveRange(rewards);
        _db.ChildProfiles.Remove(child);

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Đã xóa hồ sơ bé {child.Nickname} và dữ liệu liên quan thành công.";
        return RedirectToAction(nameof(ParentsAndKids));
    }

    [HttpPost("parents/{userId}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteParentAccount(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        // Không cho phép xóa admin chính
        if (await _userManager.IsInRoleAsync(user, "Admin") && user.Email == "admin@hanhtranglop1.local")
        {
            TempData["ErrorMessage"] = "Không thể xóa tài khoản quản trị viên hệ thống!";
            return RedirectToAction(nameof(ParentsAndKids));
        }

        var children = await _db.ChildProfiles.Where(x => x.ParentUserId == userId).ToListAsync();
        var childIds = children.Select(x => x.Id).ToList();

        var attempts = await _db.LearningAttempts.Where(x => childIds.Contains(x.ChildProfileId)).ToListAsync();
        var attemptAnswers = await _db.QuestionAttempts
            .Where(x => attempts.Select(a => a.Id).Contains(x.LearningAttemptId))
            .ToListAsync();
        var sessions = await _db.LearningSessions.Where(x => childIds.Contains(x.ChildProfileId)).ToListAsync();
        var skillProgresses = await _db.SkillProgress.Where(x => childIds.Contains(x.ChildProfileId)).ToListAsync();
        var rewards = await _db.ChildRewards.Where(x => childIds.Contains(x.ChildProfileId)).ToListAsync();

        _db.QuestionAttempts.RemoveRange(attemptAnswers);
        _db.LearningAttempts.RemoveRange(attempts);
        _db.LearningSessions.RemoveRange(sessions);
        _db.SkillProgress.RemoveRange(skillProgresses);
        _db.ChildRewards.RemoveRange(rewards);
        _db.ChildProfiles.RemoveRange(children);

        await _userManager.DeleteAsync(user);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Đã xóa tài khoản phụ huynh {user.Email} và các hồ sơ bé liên quan thành công.";
        return RedirectToAction(nameof(ParentsAndKids));
    }

    private readonly record struct ActivityConfiguration(string PayloadJson, string CorrectAnswer);
    private readonly record struct ActivityMapping(string Left, string Right);
}

