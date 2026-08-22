using HanhTrangLop1.Data;
using HanhTrangLop1.Models;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
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

    public AdminController(ApplicationDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
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
    public async Task<IActionResult> LearningItems(string? status, string? interactionType, Guid? skillGroupId)
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

        var model = new AdminLearningItemListViewModel
        {
            Status = status,
            InteractionType = interactionType,
            SkillGroupId = skillGroupId,
            SkillGroups = await _db.SkillGroups.OrderBy(x => x.SortOrder).ToListAsync(),
            Items = await query.OrderByDescending(x => x.UpdatedAt).ToListAsync()
        };

        return View(model);
    }

    [HttpGet("learning-items/{id:guid}")]
    public async Task<IActionResult> LearningItemDetail(Guid id)
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

        return View(item);
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
        var assets = await _db.MediaAssets.OrderByDescending(x => x.CreatedAt).ToListAsync();
        return View(new AdminMediaLibraryViewModel
        {
            Images = assets.Where(x => x.AssetType == "image").ToList(),
            AudioFiles = assets.Where(x => x.AssetType == "audio").ToList()
        });
    }

    [HttpGet("learning-items/create-choice")]
    public async Task<IActionResult> CreateChoice(Guid? skillGroupId, Guid? topicId, Guid? editId, string? interactionType)
    {
        await LoadContentListsAsync();

        if (editId.HasValue)
        {
            var item = await _db.LearningItems
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
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(LearningItemDetail), new { id = item.Id });
    }

    [HttpGet("learning-items/create-tracing")]
    public async Task<IActionResult> CreateTracing(Guid? skillGroupId, Guid? topicId, Guid? editId)
    {
        await LoadTracingListsAsync();
        if (editId.HasValue)
        {
            var item = await _db.LearningItems
                .Include(x => x.Questions.OrderBy(q => q.SortOrder))
                .FirstOrDefaultAsync(x => x.Id == editId.Value && x.InteractionType == InteractionTypes.Tracing);
            if (item is null) return NotFound();
            var question = item.Questions.FirstOrDefault();
            return View(new CreateTracingItemViewModel
            {
                Id = item.Id,
                Title = item.Title,
                SkillGroupId = item.SkillGroupId,
                TopicId = item.TopicId,
                Symbol = ReadJsonString(question?.PayloadJson, "symbol"),
                GuideMode = ReadJsonString(question?.PayloadJson, "guideMode") is { Length: > 0 } guide ? guide : "outline",
                ExpectedStrokeCount = ReadJsonInt(question?.PayloadJson, "expectedStrokeCount", 1),
                ShowStartPoint = ReadJsonBool(question?.PayloadJson, "showStartPoint", true),
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
            showStartPoint = model.ShowStartPoint,
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
            showStartPoint = model.ShowStartPoint
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
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(LearningItemDetail), new { id = item.Id });
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

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(LearningItemDetail), new { id = item.Id });
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
            return RedirectToAction(nameof(LearningItemDetail), new { id });
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
        return RedirectToAction(nameof(LearningItemDetail), new { id });
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
        ViewBag.MediaAssets = await _db.MediaAssets.OrderByDescending(x => x.CreatedAt).ToListAsync();
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

        ValidateMediaFile(model.ImageFile, "image", 5 * 1024 * 1024, nameof(model.ImageFile));
        ValidateMediaFile(model.AudioFile, "audio", 10 * 1024 * 1024, nameof(model.AudioFile));
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

    private static CreateChoiceItemViewModel BuildActivityEditorModel(LearningItem item)
    {
        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        var payloadJson = question?.PayloadJson ?? "{}";
        var choices = ReadJsonStringArray(payloadJson, "choices");
        var correctAnswer = ReadJsonString(question?.CorrectAnswerJson, "value");

        return new CreateChoiceItemViewModel
        {
            Id = item.Id,
            Title = item.Title,
            SkillGroupId = item.SkillGroupId,
            TopicId = item.TopicId,
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
            TargetLabel = ReadJsonString(payloadJson, "targetLabel"),
            ObjectSymbol = ReadJsonString(payloadJson, "objectSymbol"),
            TargetCount = ReadJsonInt(payloadJson, "targetCount", 4),
            SecondaryCount = ReadJsonInt(payloadJson, "rightCount", 2),
            ImageUrl = ReadJsonString(payloadJson, "imageUrl"),
            AudioUrl = ReadJsonString(payloadJson, "audioUrl"),
            Level = item.Level,
            EstimatedMinutes = item.EstimatedMinutes,
            HintText = ReadJsonString(question?.HintJson, "level1"),
            CorrectFeedback = ReadJsonString(question?.FeedbackJson, "correct"),
            RetryFeedback = ReadJsonString(question?.FeedbackJson, "retry")
        };
    }

    private ActivityConfiguration? BuildActivityConfiguration(CreateChoiceItemViewModel model)
    {
        var template = ActivityTemplateCatalog.Find(model.InteractionType);
        if (template?.RequiresAudio == true && string.IsNullOrWhiteSpace(model.AudioUrl) && model.AudioFile is null)
        {
            ModelState.AddModelError(nameof(model.AudioUrl), "Dạng bài này cần một tệp âm thanh hoặc âm thanh trong thư viện.");
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
            ["imageUrl"] = Clean(model.ImageUrl),
            ["audioUrl"] = Clean(model.AudioUrl)
        };

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
                var comparisonAnswer = model.TargetCount == model.SecondaryCount
                    ? "equal"
                    : model.TargetCount > model.SecondaryCount ? "left" : "right";
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

    private bool ValidateChoiceSet(string[] choices, string correctAnswer)
    {
        if (choices.Length >= 2 && choices.Contains(Clean(correctAnswer), StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        ModelState.AddModelError(nameof(CreateChoiceItemViewModel.CorrectAnswer), "Bài cần ít nhất hai lựa chọn và một đáp án đúng nằm trong danh sách.");
        return false;
    }

    private List<ActivityMapping> ParseMappings(string value, string fieldName, string errorMessage)
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

    private static string[] BuildChoices(params string[] values)
    {
        return values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
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

    private readonly record struct ActivityConfiguration(string PayloadJson, string CorrectAnswer);
    private readonly record struct ActivityMapping(string Left, string Right);
}
