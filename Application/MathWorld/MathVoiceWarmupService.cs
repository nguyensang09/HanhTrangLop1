namespace HanhTrangLop1.Application.MathWorld;

/// <summary>
/// Sinh sẵn file Voice (cả tiếng Việt và tiếng Anh) cho Đảo Toán Học sau khi ứng dụng khởi động, theo thứ tự
/// bé sẽ học: câu dùng chung trước, rồi từng bài theo lộ trình. Chạy nền nên không làm chậm khởi động; bài nào
/// bé mở trước khi chạy xong thì trình chơi tự yêu cầu sinh bổ sung cho riêng bài đó.
/// Tắt bằng cấu hình VoiceMaintenance:GenerateMathVoicesOnStartup = false.
/// </summary>
public sealed class MathVoiceWarmupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<MathVoiceWarmupService> _logger;

    public MathVoiceWarmupService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IHostApplicationLifetime lifetime,
        ILogger<MathVoiceWarmupService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_configuration.GetValue("VoiceMaintenance:GenerateMathVoicesOnStartup", true)) return;

        try
        {
            await WaitForStartAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            var batches = new List<IReadOnlyList<MathVoiceLine>> { MathVoiceCatalog.Common };
            batches.AddRange(MathCurriculum.Lessons.OrderBy(x => x.Operation.Key == "add" ? 0 : 1).ThenBy(x => x.SortOrder).Select(MathVoiceCatalog.ForLesson));

            var total = 0;
            foreach (var batch in batches)
            {
                using var scope = _scopeFactory.CreateScope();
                var voices = scope.ServiceProvider.GetRequiredService<MathVoiceService>();
                var result = await voices.GenerateMissingAsync(batch, englishOnly: null, TimeSpan.FromMinutes(10), stoppingToken);
                total += result.Generated;
                if (result.Failed > 0 && result.Generated == 0)
                {
                    _logger.LogWarning("Tạm dừng sinh Voice Đảo Toán Học: nhà cung cấp TTS không phản hồi. Sẽ thử lại ở lần khởi động sau hoặc khi bé mở bài.");
                    return;
                }
            }

            if (total > 0)
            {
                _logger.LogInformation("Đảo Toán Học: đã sinh bổ sung {Count} file Voice (VI/EN).", total);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Bỏ qua bước sinh sẵn Voice Đảo Toán Học.");
        }
    }

    private Task WaitForStartAsync(CancellationToken token)
    {
        var started = new TaskCompletionSource();
        _lifetime.ApplicationStarted.Register(() => started.TrySetResult());
        token.Register(() => started.TrySetCanceled(token));
        return started.Task;
    }
}
