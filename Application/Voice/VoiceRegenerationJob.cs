using HanhTrangLop1.Application.Lessons;
using HanhTrangLop1.Application.MathWorld;
using HanhTrangLop1.Data;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Application.Voice;

public sealed record VoiceRegenerationStatus(
    bool Running,
    string Stage,
    int Total,
    int Done,
    int Generated,
    int Failed,
    string? LastError,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt);

/// <summary>
/// Sinh lại toàn bộ kho Voice bằng đúng một giọng trong cấu hình để mọi bài (kể cả Đảo Toán Học) đọc giống nhau:
/// dựng lại chỉ mục theo nội dung hiện tại, đọc lại từng câu, liên kết lại vào bài học rồi xóa file không còn dùng.
/// File mới chỉ thay file cũ sau khi sinh thành công, nên lỗi mạng giữa chừng không làm bé mất âm thanh. Chạy lại sau khi
/// bị dừng thì tiếp tục từ chỗ còn thiếu (mốc thời gian lưu ở App_Data). Voice phụ huynh tự tải lên (custom) không bị đụng tới.
/// </summary>
public sealed class VoiceRegenerationJob(
    IServiceScopeFactory scopeFactory,
    IWebHostEnvironment environment,
    ILogger<VoiceRegenerationJob> logger)
{
    private const int Workers = 6;
    private const int MaxFailuresInRow = 12;

    private readonly object _gate = new();
    private VoiceRegenerationStatus _status = new(false, "idle", 0, 0, 0, 0, null, null, null);
    private int _running;

    public VoiceRegenerationStatus Status
    {
        get { lock (_gate) return _status; }
    }

    private void Update(Func<VoiceRegenerationStatus, VoiceRegenerationStatus> change)
    {
        lock (_gate) _status = change(_status);
    }

    /// <summary>Chạy nền cho trang quản trị. Trả về false nếu đang có một lượt chạy.</summary>
    public bool TryStart(CancellationToken stoppingToken)
    {
        if (Interlocked.CompareExchange(ref _running, 1, 0) != 0) return false;
        _ = Task.Run(async () =>
        {
            try
            {
                await RunCoreAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                if (ex is not OperationCanceledException) logger.LogError(ex, "Sinh lại kho Voice thất bại.");
                Update(s => s with { Running = false, Stage = "stopped", LastError = ex.Message, FinishedAt = DateTimeOffset.UtcNow });
            }
            finally
            {
                Interlocked.Exchange(ref _running, 0);
            }
        }, CancellationToken.None);
        return true;
    }

    /// <summary>Chạy trực tiếp từ dòng lệnh.</summary>
    public async Task<VoiceRegenerationStatus> RunAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref _running, 1, 0) != 0) return Status;
        try
        {
            await RunCoreAsync(cancellationToken);
            return Status;
        }
        finally
        {
            Interlocked.Exchange(ref _running, 0);
        }
    }

    private async Task RunCoreAsync(CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        Update(_ => new VoiceRegenerationStatus(true, "index", 0, 0, 0, 0, null, startedAt, null));

        var marker = Path.Combine(environment.ContentRootPath, "App_Data", "backups", "voice-regeneration.marker");
        var cutoff = ReadOrCreateCutoff(marker);

        // 1. Dựng lại chỉ mục theo đúng nội dung bài hiện tại và xóa file không còn dùng.
        using (var scope = scopeFactory.CreateScope())
        {
            var library = scope.ServiceProvider.GetRequiredService<VoiceLibraryMaintenanceService>();
            var math = scope.ServiceProvider.GetRequiredService<MathVoiceService>();
            await library.CleanRebuildVoiceIndexAsync(async ct =>
            {
                await math.EnsureCatalogRowsAsync(ct);
                foreach (var lines in LessonVoiceWarmupService.LinesByGroup())
                {
                    await math.EnsureRowsAsync(lines, LessonVoiceWarmupService.UsageType, ct);
                }
            }, apply: true, cancellationToken);
        }

        // 2. Đọc lại từng câu, mỗi luồng một DbContext riêng.
        List<Guid> ids;
        using (var scope = scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            ids = await db.TextToSpeechCaches.AsNoTracking()
                .Where(x => x.UsageType != "custom")
                .OrderBy(x => x.UsageType == MathVoiceCatalog.UsageType ? 0 : 1)
                .ThenBy(x => x.Id)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
        }
        Update(s => s with { Stage = "generate", Total = ids.Count });

        var next = -1;
        var stop = false;
        await Task.WhenAll(Enumerable.Range(0, Workers).Select(_ => Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var library = scope.ServiceProvider.GetRequiredService<VoiceLibraryMaintenanceService>();
            var failuresInRow = 0;
            while (!Volatile.Read(ref stop))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var index = Interlocked.Increment(ref next);
                if (index >= ids.Count) return;
                var id = ids[index];
                var row = await db.TextToSpeechCaches.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (row is null) continue;

                var generated = 0;
                var failed = 0;
                string? error = null;
                if (!IsFresh(row.AudioUrl, cutoff))
                {
                    try
                    {
                        row.AudioUrl = await library.GenerateVoiceCacheFileAsync(row, cancellationToken, overwrite: true);
                        row.Status = "ready";
                        row.LastError = null;
                        generated++;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        row.LastError = error = Compact(ex.Message);
                        failed++;
                    }
                }

                if (!IsFresh(row.AudioUrlEn, cutoff))
                {
                    try
                    {
                        row.AudioUrlEn = await library.GenerateVoiceCacheFileEnAsync(row, cancellationToken, overwrite: true);
                        row.StatusEn = "ready";
                        row.LastErrorEn = null;
                        generated++;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        row.LastErrorEn = error = Compact(ex.Message);
                        failed++;
                    }
                }

                row.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
                db.ChangeTracker.Clear();

                failuresInRow = failed > 0 ? failuresInRow + failed : 0;
                var (generatedNow, failedNow, errorNow) = (generated, failed, error);
                Update(s => s with
                {
                    Done = s.Done + 1,
                    Generated = s.Generated + generatedNow,
                    Failed = s.Failed + failedNow,
                    LastError = errorNow ?? s.LastError
                });

                // Nhà cung cấp TTS không phản hồi: dừng để lần sau chạy tiếp, file cũ vẫn dùng được.
                if (failuresInRow >= MaxFailuresInRow) Volatile.Write(ref stop, true);
            }
        }, cancellationToken)));

        // 3. Liên kết lại vào bài học, kể cả khi dừng sớm, để bài luôn trỏ đúng file đang có.
        Update(s => s with { Stage = "link" });
        using (var scope = scopeFactory.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<VoiceLibraryMaintenanceService>().EnsureVoiceRowsAndRelinkAsync(cancellationToken);
        }

        var completed = !stop && Status.Failed == 0;

        // 4. Đã đọc lại đủ thì xóa file giọng cũ còn sót, để kho chỉ còn đúng một giọng.
        if (completed)
        {
            using var scope = scopeFactory.CreateScope();
            var cleanup = await scope.ServiceProvider.GetRequiredService<VoiceLibraryMaintenanceService>().RemoveUnreferencedVoiceFilesAsync(cancellationToken);
            logger.LogInformation("Đã xóa {Deleted}/{OnDisk} file Voice thừa.", cleanup.Deleted, cleanup.OnDisk);
        }
        if (completed && File.Exists(marker)) File.Delete(marker);
        Update(s => s with { Running = false, Stage = completed ? "done" : "partial", FinishedAt = DateTimeOffset.UtcNow });
        logger.LogInformation("Sinh lại kho Voice: {Done}/{Total} dòng, {Generated} file mới, {Failed} lỗi.", Status.Done, Status.Total, Status.Generated, Status.Failed);
    }

    private static DateTime ReadOrCreateCutoff(string marker)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(marker)!);
        if (File.Exists(marker) && long.TryParse(File.ReadAllText(marker).Trim(), out var ticks))
        {
            return new DateTime(ticks, DateTimeKind.Utc);
        }

        var now = DateTime.UtcNow;
        File.WriteAllText(marker, now.Ticks.ToString());
        return now;
    }

    /// <summary>File đã được sinh trong lượt này (hoặc lượt đang dở) thì không đọc lại.</summary>
    private bool IsFresh(string? url, DateTime cutoff)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        var path = Path.Combine(environment.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        var info = new FileInfo(path);
        return info.Exists && info.Length > 0 && info.LastWriteTimeUtc >= cutoff;
    }

    private static string Compact(string message) => message.Length > 900 ? message[..900] : message;
}
