using HanhTrangLop1.Application.Learning;
using HanhTrangLop1.Application.MathWorld;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HanhTrangLop1.Controllers;

/// <summary>Đảo Toán Học — khu học toán độc lập trong góc của bé.</summary>
[Route("kids/math")]
public class KidsMathController : Controller
{
    private readonly ChildProfileResolver _childResolver;
    private readonly MathWorldService _mathWorld;
    private readonly MathVoiceService _voices;
    private readonly KidsModuleService _modules;
    private readonly ILogger<KidsMathController> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public KidsMathController(ChildProfileResolver childResolver, MathWorldService mathWorld, MathVoiceService voices, KidsModuleService modules, ILogger<KidsMathController> logger, IHostApplicationLifetime lifetime)
    {
        _modules = modules;
        _voices = voices;
        _childResolver = childResolver;
        _mathWorld = mathWorld;
        _logger = logger;
        _lifetime = lifetime;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? op)
    {
        if (!await _modules.IsVisibleAsync(MathCurriculum.SkillGroupCode)) return RedirectToAction("Home", "Kids");

        var child = await _childResolver.GetSelectedChildAsync(HttpContext);
        if (child is null) return RedirectToChildPicker();

        return View(await _mathWorld.BuildHubAsync(child, op));
    }

    [HttpGet("lesson/{id:guid}")]
    public async Task<IActionResult> Play(Guid id)
    {
        if (!await _modules.IsVisibleAsync(MathCurriculum.SkillGroupCode)) return RedirectToAction("Home", "Kids");

        var child = await _childResolver.GetSelectedChildAsync(HttpContext);
        if (child is null) return RedirectToChildPicker();

        var model = await _mathWorld.BuildPlayAsync(
            child,
            id,
            Url.Action(nameof(Complete), new { id })!,
            Url.Action(nameof(Index))!,
            Url.Action(nameof(Voice), new { id })!);
        if (model is null) return RedirectToAction(nameof(Index));

        return View(model);
    }

    [HttpPost("lesson/{id:guid}/complete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id, [FromBody] MathCompleteRequest request)
    {
        var child = await _childResolver.GetSelectedChildAsync(HttpContext);
        if (child is null) return Unauthorized(new { success = false, message = "Chưa chọn hồ sơ bé." });

        try
        {
            var result = await _mathWorld.CompleteAsync(child, id, request ?? new MathCompleteRequest());
            if (!result.Success) return BadRequest(new { success = false, message = result.Error });

            return Json(new
            {
                success = true,
                stars = result.Stars,
                bestStars = result.BestStars,
                firstTryCorrect = result.FirstTryCorrect,
                totalQuestions = result.TotalQuestions,
                isFirstCompletion = result.IsFirstCompletion,
                milestoneMessage = result.MilestoneMessage,
                newBadges = result.NewBadges?.Select(x => new { name = x.Name, icon = x.IconKey }) ?? [],
                nextLesson = result.NextLesson is null ? null : new
                {
                    title = result.NextLesson.Title,
                    styleEmoji = result.NextLesson.StyleEmoji,
                    url = Url.Action(nameof(Play), new { id = result.NextLesson.Id })
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            // Ví dụ: tiến độ vừa bị quản trị viên đặt lại trong lúc bé đang làm bài.
            _logger.LogWarning(ex, "Không ghi nhận được kết quả bài toán {LessonId}.", id);
            return Conflict(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Sinh bổ sung Voice còn thiếu của một bài theo ngôn ngữ giọng đọc của bé rồi trả về gói Voice mới.
    /// Trình chơi gọi khi mở bài mà kho chưa có đủ file (ví dụ bài mới, hoặc phụ huynh vừa đổi ngôn ngữ).
    /// </summary>
    [HttpPost("lesson/{id:guid}/voice")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Voice(Guid id)
    {
        var child = await _childResolver.GetSelectedChildAsync(HttpContext);
        if (child is null) return Unauthorized(new { success = false });

        var lesson = MathCurriculum.Lessons.FirstOrDefault(x => x.Id == id);
        if (lesson is null) return NotFound(new { success = false });

        var english = child.EnglishVoice;
        // Sinh nốt Voice kể cả khi bé đã rời trang (trình duyệt hủy yêu cầu): file được lưu vào kho để lần sau dùng
        // ngay, thay vì bị hủy giữa chừng và ném OperationCanceledException. Chỉ dừng khi máy chủ tắt.
        MathVoiceGenerateResult generated;
        MathVoicePack pack;
        try
        {
            generated = await _voices.GenerateMissingAsync(
                MathVoiceService.LinesFor(lesson), englishOnly: english, TimeSpan.FromSeconds(25), _lifetime.ApplicationStopping);
            pack = await _voices.BuildPackAsync(lesson, english, _lifetime.ApplicationStopping);
        }
        catch (OperationCanceledException) when (_lifetime.ApplicationStopping.IsCancellationRequested)
        {
            _logger.LogInformation("Dừng sinh Voice bài toán {LessonId} vì máy chủ đang tắt.", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { success = false });
        }
        if (HttpContext.RequestAborted.IsCancellationRequested) return new EmptyResult();
        return Json(new
        {
            success = true,
            lang = pack.Lang,
            generated = generated.Generated,
            failed = generated.Failed,
            missing = pack.MissingCount,
            lines = pack.Lines.ToDictionary(x => x.Key, x => new { text = x.Value.Text, url = x.Value.Url })
        });
    }

    private IActionResult RedirectToChildPicker() =>
        User.Identity?.IsAuthenticated == true
            ? RedirectToAction("CreateProfile", "Parent")
            : RedirectToAction("Index", "Profiles");
}
