using HanhTrangLop1.Data;
using HanhTrangLop1.Models;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HanhTrangLop1.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminController : Controller
{
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
            SkillGroups = await _db.SkillGroups.CountAsync(x => x.IsActive),
            RecentItems = await _db.LearningItems.Include(x => x.SkillGroup).OrderByDescending(x => x.UpdatedAt).Take(8).ToListAsync()
        };

        return View(model);
    }

    [HttpGet("learning-items")]
    public async Task<IActionResult> LearningItems()
    {
        var items = await _db.LearningItems.Include(x => x.SkillGroup).OrderBy(x => x.Title).ToListAsync();
        return View(items);
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

        var itemId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
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
            Status = ContentStatus.Published,
            PublishedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        item.Questions.Add(new Question
        {
            Id = Guid.NewGuid(),
            LearningItemId = itemId,
            PromptText = model.PromptText.Trim(),
            QuestionType = "choice",
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
        return RedirectToAction(nameof(LearningItems));
    }

    private async Task LoadContentListsAsync()
    {
        ViewBag.SkillGroups = await _db.SkillGroups.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync();
        ViewBag.Topics = await _db.Topics.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync();
    }
}
