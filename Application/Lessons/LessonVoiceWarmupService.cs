using System.Text.Json.Nodes;
using HanhTrangLop1.Application.MathWorld;

namespace HanhTrangLop1.Application.Lessons;

/// <summary>
/// Sinh sẵn Voice (tiếng Việt và tiếng Anh viết sẵn) cho kho bài học sau khi ứng dụng khởi động, theo thứ tự nhóm
/// trên trang chủ. Chạy nền, sau Đảo Toán Học; bài nào bé mở trước khi chạy xong thì trang làm bài tự xin sinh
/// bổ sung cho riêng bài đó. Tắt bằng cấu hình VoiceMaintenance:GenerateLessonVoicesOnStartup = false.
/// </summary>
public sealed class LessonVoiceWarmupService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    IHostApplicationLifetime lifetime,
    ILogger<LessonVoiceWarmupService> logger) : BackgroundService
{
    public const string UsageType = "lesson";

    /// <summary>Mọi câu cần đọc của kho bài, mỗi câu tiếng Việt một lần, theo thứ tự nhóm.</summary>
    public static IReadOnlyList<IReadOnlyList<MathVoiceLine>> LinesByGroup()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<IReadOnlyList<MathVoiceLine>>();
        foreach (var group in LessonBank.All.GroupBy(x => x.Group))
        {
            var lines = new List<MathVoiceLine>();
            foreach (var lesson in group)
            {
                if (lesson.Payload["speech"] is not JsonObject map) continue;
                foreach (var (_, value) in map)
                {
                    if (value is not JsonArray pair || pair.Count < 2) continue;
                    var vi = pair[0]!.GetValue<string>().Trim();
                    var en = pair[1]!.GetValue<string>().Trim();
                    if (vi.Length == 0 || en.Length == 0 || !seen.Add(vi)) continue;
                    lines.Add(new MathVoiceLine(lesson.Code, vi, en));
                }
            }
            result.Add(lines);
        }
        return result;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.GetValue("VoiceMaintenance:GenerateLessonVoicesOnStartup", true)) return;
        try
        {
            var started = new TaskCompletionSource();
            lifetime.ApplicationStarted.Register(() => started.TrySetResult());
            stoppingToken.Register(() => started.TrySetCanceled(stoppingToken));
            await started.Task;
            // Nhường Đảo Toán Học chạy trước.
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            var total = 0;
            foreach (var lines in LinesByGroup())
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    await scope.ServiceProvider.GetRequiredService<MathVoiceService>().EnsureRowsAsync(lines, UsageType, stoppingToken);
                }

                foreach (var chunk in lines.Chunk(120))
                {
                    using var scope = scopeFactory.CreateScope();
                    var result = await scope.ServiceProvider.GetRequiredService<MathVoiceService>()
                        .GenerateMissingAsync(chunk, englishOnly: null, TimeSpan.FromMinutes(15), stoppingToken);
                    total += result.Generated;
                    if (result.Failed > 0 && result.Generated == 0)
                    {
                        logger.LogWarning("Tạm dừng sinh Voice kho bài học: nhà cung cấp TTS không phản hồi. Sẽ thử lại ở lần khởi động sau hoặc khi bé mở bài.");
                        return;
                    }
                }
            }

            if (total > 0) logger.LogInformation("Kho bài học: đã sinh bổ sung {Count} file Voice (VI/EN).", total);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Bỏ qua bước sinh sẵn Voice kho bài học.");
        }
    }
}
