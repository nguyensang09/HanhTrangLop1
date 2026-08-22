using HanhTrangLop1.Data;
using HanhTrangLop1.Models;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
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
    public async Task<IActionResult> LearningItems(string? status, string? interactionType)
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

        var model = new AdminLearningItemListViewModel
        {
            Status = status,
            InteractionType = interactionType,
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

    [HttpGet("learning-items/create-choice")]
    public async Task<IActionResult> CreateChoice()
    {
        await LoadContentListsAsync();
        return View(new CreateChoiceItemViewModel());
    }

    [HttpPost("learning-items/create-choice")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateChoice(CreateChoiceItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadContentListsAsync();
            return View(model);
        }

        var choices = new[] { model.ChoiceA, model.ChoiceB, model.ChoiceC }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToArray();
        var supportedTypes = new[]
        {
            InteractionTypes.SingleChoice,
            InteractionTypes.ListenAndChoose,
            InteractionTypes.DragDrop,
            InteractionTypes.Matching,
            InteractionTypes.Ordering
        };

        if (!supportedTypes.Contains(model.InteractionType))
        {
            ModelState.AddModelError(string.Empty, "Dạng tương tác chưa được hỗ trợ trong MVP.");
            await LoadContentListsAsync();
            return View(model);
        }

        if (choices.Length < 2 || !choices.Contains(model.CorrectAnswer.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Đáp án đúng cần nằm trong các lựa chọn đã nhập.");
            await LoadContentListsAsync();
            return View(model);
        }

        var now = DateTimeOffset.UtcNow;
        var itemId = Guid.NewGuid();
        var item = new LearningItem
        {
            Id = itemId,
            Code = $"bai-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            Title = model.Title.Trim(),
            SkillGroupId = model.SkillGroupId,
            TopicId = model.TopicId,
            Level = model.Level,
            InteractionType = model.InteractionType,
            EstimatedMinutes = 4,
            InstructionText = model.InstructionText.Trim(),
            ContentJson = JsonSerializer.Serialize(new { choices, answer = model.CorrectAnswer.Trim() }),
            Status = ContentStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };

        item.Questions.Add(new Question
        {
            Id = Guid.NewGuid(),
            LearningItemId = itemId,
            PromptText = model.PromptText.Trim(),
            QuestionType = model.InteractionType,
            PayloadJson = JsonSerializer.Serialize(new { choices }),
            CorrectAnswerJson = JsonSerializer.Serialize(new { value = model.CorrectAnswer.Trim() }),
            HintJson = JsonSerializer.Serialize(new { level1 = "Con nhìn kỹ từng lựa chọn nhé." }),
            FeedbackJson = JsonSerializer.Serialize(new
            {
                correct = "Giỏi lắm, con chọn đúng rồi!",
                retry = "Không sao, mình thử lại nhẹ nhàng nhé."
            }),
            SortOrder = 1
        });

        _db.LearningItems.Add(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(LearningItemDetail), new { id = item.Id });
    }

    [HttpGet("learning-items/create-tracing")]
    public async Task<IActionResult> CreateTracing()
    {
        await LoadContentListsAsync();
        return View(new CreateTracingItemViewModel
        {
            InstructionText = "Con tô theo nét gợi ý nhé.",
            PromptText = "Con tô chữ theo nét gợi ý."
        });
    }

    [HttpPost("learning-items/create-tracing")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTracing(CreateTracingItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadContentListsAsync();
            return View(model);
        }

        var now = DateTimeOffset.UtcNow;
        var itemId = Guid.NewGuid();
        var symbol = string.IsNullOrWhiteSpace(model.Symbol) ? "A" : model.Symbol.Trim().ToUpperInvariant();
        var item = new LearningItem
        {
            Id = itemId,
            Code = $"to-net-{symbol.ToLowerInvariant()}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            Title = model.Title.Trim(),
            SkillGroupId = model.SkillGroupId,
            TopicId = model.TopicId,
            Level = model.Level,
            InteractionType = InteractionTypes.Tracing,
            EstimatedMinutes = 5,
            InstructionText = model.InstructionText.Trim(),
            ContentJson = JsonSerializer.Serialize(new { symbol }),
            Status = ContentStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };

        item.Questions.Add(new Question
        {
            Id = Guid.NewGuid(),
            LearningItemId = itemId,
            PromptText = model.PromptText.Trim(),
            QuestionType = InteractionTypes.Tracing,
            PayloadJson = JsonSerializer.Serialize(new { symbol }),
            CorrectAnswerJson = JsonSerializer.Serialize(new { minPoints = Math.Max(5, model.MinPoints) }),
            HintJson = JsonSerializer.Serialize(new { level1 = "Con bắt đầu từ chấm màu cam nhé." }),
            FeedbackJson = JsonSerializer.Serialize(new
            {
                correct = $"Tốt lắm, con đã tô xong chữ {symbol}!",
                retry = "Mình thử tô lại một nét nhé."
            }),
            SortOrder = 1
        });

        _db.LearningItems.Add(item);
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

        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        var model = new EditLearningItemViewModel
        {
            Id = item.Id,
            Title = item.Title,
            SkillGroupId = item.SkillGroupId,
            TopicId = item.TopicId,
            Level = item.Level,
            EstimatedMinutes = item.EstimatedMinutes,
            InstructionText = item.InstructionText,
            PromptText = question?.PromptText ?? string.Empty,
            HintText = ReadJsonString(question?.HintJson, "level1"),
            CorrectFeedback = ReadJsonString(question?.FeedbackJson, "correct"),
            RetryFeedback = ReadJsonString(question?.FeedbackJson, "retry")
        };

        await LoadContentListsAsync();
        return View(model);
    }

    [HttpPost("learning-items/{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLearningItem(Guid id, EditLearningItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            await LoadContentListsAsync();
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
        item.UpdatedAt = DateTimeOffset.UtcNow;

        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        if (question is not null)
        {
            question.PromptText = model.PromptText.Trim();
            question.HintJson = JsonSerializer.Serialize(new { level1 = model.HintText.Trim() });
            question.FeedbackJson = JsonSerializer.Serialize(new
            {
                correct = model.CorrectFeedback.Trim(),
                retry = model.RetryFeedback.Trim()
            });
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(LearningItemDetail), new { id = item.Id });
    }

    [HttpPost("learning-items/{id:guid}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(Guid id, string status)
    {
        var item = await _db.LearningItems.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        if (!AllowedStatuses.Contains(status))
        {
            return BadRequest();
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

    private async Task LoadContentListsAsync()
    {
        ViewBag.SkillGroups = await _db.SkillGroups.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync();
        ViewBag.Topics = await _db.Topics.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync();
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
}
