using System.Collections.Concurrent;
using System.Diagnostics;
using HanhTrangLop1.Application.Voice;
using HanhTrangLop1.Data;
using HanhTrangLop1.Models;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Application.MathWorld;

public sealed record MathVoiceClip(string Text, string? Url);

public sealed record MathVoicePack(string Lang, string SpeechLang, IReadOnlyDictionary<string, MathVoiceClip> Lines)
{
    public int MissingCount => Lines.Values.Count(x => string.IsNullOrEmpty(x.Url));
}

public sealed record MathVoiceSyncResult(int RowsCreated, int RowsUpdated, int Total);

public sealed record MathVoiceGenerateResult(int Generated, int Failed);

/// <summary>
/// Gắn kịch bản Voice của Đảo Toán Học vào kho Voice chung (TextToSpeechCache + Edge TTS) và phát đúng
/// ngôn ngữ theo cấu hình giọng đọc của bé (<see cref="ChildProfile.EnglishVoice"/>). File đã tạo được dùng lại
/// cho mọi bài có cùng câu; câu nào còn thiếu file thì được sinh bổ sung rồi lưu để lần sau dùng lại.
/// </summary>
public sealed class MathVoiceService
{
    // Câu đang được sinh file (theo khóa băm): tránh hai luồng cùng ghi một file mp3 tất định.
    private static readonly ConcurrentDictionary<string, byte> InFlight = new();
    private const int Parallelism = 4;

    private readonly ApplicationDbContext _db;
    private readonly VoiceLibraryMaintenanceService _voiceLibrary;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MathVoiceService> _logger;

    public MathVoiceService(
        ApplicationDbContext db,
        VoiceLibraryMaintenanceService voiceLibrary,
        IServiceScopeFactory scopeFactory,
        ILogger<MathVoiceService> logger)
    {
        _scopeFactory = scopeFactory;
        _db = db;
        _voiceLibrary = voiceLibrary;
        _logger = logger;
    }

    public static IReadOnlyList<MathVoiceLine> LinesFor(MathLessonDefinition lesson) =>
        MathVoiceCatalog.Common.Concat(MathVoiceCatalog.ForLesson(lesson)).ToList();

    /// <summary>
    /// Bảo đảm mọi câu của Đảo Toán Học có dòng trong kho Voice với đúng bản tiếng Anh viết sẵn.
    /// Chạy lại an toàn: chỉ thêm dòng thiếu và sửa bản tiếng Anh bị lệch (khi đó file EN cũ được tạo lại).
    /// </summary>
    public Task<MathVoiceSyncResult> EnsureCatalogRowsAsync(CancellationToken cancellationToken = default) =>
        EnsureRowsAsync(MathVoiceCatalog.AllDistinct(), MathVoiceCatalog.UsageType, cancellationToken);

    /// <summary>Như trên cho một danh sách câu bất kỳ (kho bài học các nhóm khác cũng dùng).</summary>
    public async Task<MathVoiceSyncResult> EnsureRowsAsync(IReadOnlyList<MathVoiceLine> lines, string usageType, CancellationToken cancellationToken = default)
    {
        var rows = await LoadRowsAsync(lines, tracked: true, cancellationToken);
        var created = 0;
        var updated = 0;

        foreach (var line in lines)
        {
            var hash = VoiceLibraryMaintenanceService.ComputeTextHash(line.Vi);
            var isNew = false;
            if (!rows.TryGetValue(hash, out var row))
            {
                row = await _voiceLibrary.EnsureVoiceEntryAsync(line.Vi, usageType, cancellationToken: cancellationToken);
                if (row is null) continue;
                rows[hash] = row;
                created++;
                isNew = true;
            }

            if (!SameText(row.TextEn, line.En))
            {
                if (!isNew) updated++;
                row.TextEn = line.En;
                row.AudioUrlEn = string.Empty;
                row.StatusEn = "missing";
                row.LastErrorEn = null;
                row.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new MathVoiceSyncResult(created, updated, lines.Count);
    }

    /// <summary>Gói Voice của một bài theo ngôn ngữ đã cấu hình: văn bản (để dự phòng) và URL file nếu đã có.</summary>
    public async Task<MathVoicePack> BuildPackAsync(MathLessonDefinition lesson, bool english, CancellationToken cancellationToken = default)
    {
        var lines = LinesFor(lesson);
        var rows = await LoadRowsAsync(lines, tracked: false, cancellationToken);
        var result = new Dictionary<string, MathVoiceClip>(StringComparer.Ordinal);
        foreach (var line in lines)
        {
            rows.TryGetValue(VoiceLibraryMaintenanceService.ComputeTextHash(line.Vi), out var row);
            result[line.Key] = new MathVoiceClip(line.TextFor(english), ReadyUrl(row, line, english));
        }

        return new MathVoicePack(english ? "en" : "vi", english ? "en-US" : "vi-VN", result);
    }

    /// <summary>
    /// Sinh file còn thiếu cho các câu đã cho (một ngôn ngữ hoặc cả hai) trong giới hạn thời gian.
    /// Trả về số file mới và số lần lỗi; dừng sớm khi nhà cung cấp TTS lỗi liên tiếp để không treo yêu cầu.
    /// </summary>
    public async Task<MathVoiceGenerateResult> GenerateMissingAsync(
        IReadOnlyList<MathVoiceLine> lines,
        bool? englishOnly,
        TimeSpan budget,
        CancellationToken cancellationToken = default)
    {
        var distinct = lines.GroupBy(x => x.Vi.Trim().ToLowerInvariant()).Select(g => g.First()).ToList();
        var rows = await LoadRowsAsync(distinct, tracked: false, cancellationToken);
        var pending = distinct.Where(line =>
        {
            rows.TryGetValue(VoiceLibraryMaintenanceService.ComputeTextHash(line.Vi), out var row);
            return row is not null &&
                   ((englishOnly != true && ReadyUrl(row, line, false) is null) ||
                    (englishOnly != false && ReadyUrl(row, line, true) is null));
        }).ToList();
        if (pending.Count == 0) return new MathVoiceGenerateResult(0, 0);

        // Mỗi câu mất vài giây ở nhà cung cấp TTS nên chia cho vài luồng, mỗi luồng một DbContext riêng.
        var workers = Math.Min(Parallelism, pending.Count);
        var deadline = DateTime.UtcNow + budget;
        var results = await Task.WhenAll(Enumerable.Range(0, workers).Select(async w =>
        {
            using var scope = _scopeFactory.CreateScope();
            var worker = scope.ServiceProvider.GetRequiredService<MathVoiceService>();
            var bucket = pending.Where((_, i) => i % workers == w).ToList();
            return await worker.GenerateSequentialAsync(bucket, englishOnly, deadline - DateTime.UtcNow, cancellationToken);
        }));
        return new MathVoiceGenerateResult(results.Sum(x => x.Generated), results.Sum(x => x.Failed));
    }

    private async Task<MathVoiceGenerateResult> GenerateSequentialAsync(
        IReadOnlyList<MathVoiceLine> lines,
        bool? englishOnly,
        TimeSpan budget,
        CancellationToken cancellationToken)
    {
        var watch = Stopwatch.StartNew();
        var generated = 0;
        var failuresInRow = 0;
        var failed = 0;

        foreach (var line in lines)
        {
            if (watch.Elapsed > budget || failuresInRow >= 3) break;
            cancellationToken.ThrowIfCancellationRequested();

            var hash = VoiceLibraryMaintenanceService.ComputeTextHash(line.Vi);
            var needVi = englishOnly != true;
            var needEn = englishOnly != false;
            var row = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x => x.TextHash == hash, cancellationToken);
            if (row is null) continue; // Dòng được tạo bởi EnsureCatalogRowsAsync khi khởi động.
            if ((!needVi || ReadyUrl(row, line, false) is not null) && (!needEn || ReadyUrl(row, line, true) is not null)) continue;

            if (!InFlight.TryAdd(hash, 0)) continue; // Luồng khác đang sinh câu này.
            try
            {
                if (needVi && ReadyUrl(row, line, false) is null)
                {
                    try
                    {
                        row.AudioUrl = await _voiceLibrary.GenerateVoiceCacheFileAsync(row, cancellationToken);
                        row.Status = "ready";
                        row.LastError = null;
                        generated++;
                        failuresInRow = 0;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        row.LastError = Compact(ex.Message);
                        failuresInRow++;
                        failed++;
                        _logger.LogWarning("Không tạo được Voice VI cho \"{Text}\": {Error}", line.Vi, row.LastError);
                    }
                }

                if (needEn && ReadyUrl(row, line, true) is null)
                {
                    try
                    {
                        row.TextEn = line.En;
                        row.AudioUrlEn = await _voiceLibrary.GenerateVoiceCacheFileEnAsync(row, cancellationToken);
                        row.StatusEn = "ready";
                        row.LastErrorEn = null;
                        generated++;
                        failuresInRow = 0;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        row.LastErrorEn = Compact(ex.Message);
                        failuresInRow++;
                        failed++;
                        _logger.LogWarning("Không tạo được Voice EN cho \"{Text}\": {Error}", line.En, row.LastErrorEn);
                    }
                }

                row.UpdatedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                InFlight.TryRemove(hash, out _);
            }
        }

        return new MathVoiceGenerateResult(generated, failed);
    }

    /// <summary>Số câu còn thiếu file theo từng ngôn ngữ (dùng cho báo cáo và kiểm thử).</summary>
    public async Task<(int Total, int MissingVi, int MissingEn)> GetCoverageAsync(CancellationToken cancellationToken = default)
    {
        var lines = MathVoiceCatalog.AllDistinct();
        var rows = await LoadRowsAsync(lines, tracked: false, cancellationToken);
        var missingVi = 0;
        var missingEn = 0;
        foreach (var line in lines)
        {
            rows.TryGetValue(VoiceLibraryMaintenanceService.ComputeTextHash(line.Vi), out var row);
            if (ReadyUrl(row, line, false) is null) missingVi++;
            if (ReadyUrl(row, line, true) is null) missingEn++;
        }
        return (lines.Count, missingVi, missingEn);
    }

    private string? ReadyUrl(TextToSpeechCache? row, MathVoiceLine line, bool english)
    {
        if (row is null) return null;
        if (english)
        {
            // Chỉ phát file EN khi nó được tạo từ đúng câu tiếng Anh viết sẵn, tránh phát nhầm bản dịch máy cũ.
            return row.StatusEn == "ready" && SameText(row.TextEn, line.En) && _voiceLibrary.VoiceFileExists(row.AudioUrlEn)
                ? row.AudioUrlEn
                : null;
        }
        return row.Status == "ready" && _voiceLibrary.VoiceFileExists(row.AudioUrl) ? row.AudioUrl : null;
    }

    private async Task<Dictionary<string, TextToSpeechCache>> LoadRowsAsync(
        IEnumerable<MathVoiceLine> lines,
        bool tracked,
        CancellationToken cancellationToken)
    {
        var hashes = lines.Select(x => VoiceLibraryMaintenanceService.ComputeTextHash(x.Vi)).Distinct().ToList();
        var result = new Dictionary<string, TextToSpeechCache>(StringComparer.Ordinal);
        // Truy vấn theo lô để danh sách tham số không quá dài.
        foreach (var chunk in hashes.Chunk(400))
        {
            var query = _db.TextToSpeechCaches.Where(x => x.UsageType != "bilingual-listen" && chunk.Contains(x.TextHash));
            var rows = tracked ? await query.ToListAsync(cancellationToken) : await query.AsNoTracking().ToListAsync(cancellationToken);
            foreach (var row in rows.OrderByDescending(x => x.Status == "ready"))
            {
                result.TryAdd(row.TextHash, row);
            }
        }
        return result;
    }

    private static bool SameText(string? a, string? b) =>
        string.Equals((a ?? string.Empty).Trim(), (b ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);

    private static string Compact(string message) => message.Length > 900 ? message[..900] : message;
}
