using System.Diagnostics;
using System.Net.WebSockets;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using HanhTrangLop1.Data;
using HanhTrangLop1.Models;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Application.Voice;

public sealed record VoiceLibraryResetResult(
    int DeletedVoiceRows,
    int DeletedAudioRows,
    int DeletedAudioFiles,
    int LearningItemsScanned,
    int VoiceRowsCreated,
    int VoiceFilesCreated,
    int VoiceFilesFailed,
    int LearningItemsUpdated);

public sealed record VoiceLibraryRelinkResult(
    int LegacyAudioRowsBackfilled,
    int LearningItemsScanned,
    int LearningItemsUpdated);

public sealed record VoiceLibrarySyncBatchResult(
    int ScannedItems,
    int TotalEntries,
    int MissingVi,
    int MissingEn,
    int ProcessedInBatch,
    int CreatedVi,
    int CreatedEn,
    int Failed,
    int UpdatedItems,
    int RemainingMissing,
    bool IsCompleted,
    IReadOnlyList<string> ErrorMessages,
    bool CanContinue = true);

public sealed record VoiceAuditStatsResult(
    int TotalVoices,
    int ReadyVoicesVi,
    int MissingVoicesVi,
    int ReadyVoicesEn,
    int MissingVoicesEn,
    int TotalLessons,
    int FullySyncedLessons,
    int TotalReuses = 0);

public sealed record VoiceLibraryRebuildResult(
    int TotalVoices,
    int GeneratedVi,
    int GeneratedEn,
    int Failed,
    int UpdatedLessons);

public sealed class VoiceLibraryMaintenanceService
{
    private const string ManualUsageType = "custom";
    private const string BilingualListenUsageType = "bilingual-listen";

    private sealed record BilingualListenVoicePair(string TextVi, string TextEn, string UsageType);

    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VoiceLibraryMaintenanceService> _logger;

    public VoiceLibraryMaintenanceService(
        ApplicationDbContext db,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<VoiceLibraryMaintenanceService> logger)
    {
        _db = db;
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    private List<TextToSpeechCache>? _cachedGeneralVoices;

    private async Task<List<TextToSpeechCache>> GetCachedGeneralVoicesAsync(CancellationToken cancellationToken)
    {
        if (_cachedGeneralVoices is not null) return _cachedGeneralVoices;
        _cachedGeneralVoices = await _db.TextToSpeechCaches
            .AsNoTracking()
            .Where(x => x.UsageType != BilingualListenUsageType)
            .ToListAsync(cancellationToken);
        return _cachedGeneralVoices;
    }

    private void InvalidateVoiceLookupCache()
    {
        _cachedGeneralVoices = null;
    }

    public async Task<(int Rows, int Files)> PurgeAllVoiceDataAsync(CancellationToken cancellationToken = default)
    {
        var folder = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "uploads", "audio"));
        var webRoot = Path.GetFullPath(_environment.WebRootPath);
        if (!folder.StartsWith(webRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Thư mục voice nằm ngoài wwwroot.");
        }

        var deletedFiles = 0;
        if (Directory.Exists(folder))
        {
            foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly))
            {
                File.Delete(file);
                deletedFiles++;
            }
        }

        var voiceRows = await _db.TextToSpeechCaches.ToListAsync(cancellationToken);
        var audioRows = await _db.MediaAssets.Where(x => x.AssetType == "audio").ToListAsync(cancellationToken);
        _db.TextToSpeechCaches.RemoveRange(voiceRows);
        _db.MediaAssets.RemoveRange(audioRows);
        await _db.SaveChangesAsync(cancellationToken);
        return (voiceRows.Count + audioRows.Count, deletedFiles);
    }

    public async Task<VoiceAuditStatsResult> GetVoiceAuditStatsAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.MigrateAsync(cancellationToken);

        var totalVoices = await _db.TextToSpeechCaches.CountAsync(cancellationToken);
        var readyVi = await _db.TextToSpeechCaches.CountAsync(x => x.Status == "ready" && x.AudioUrl != null && x.AudioUrl != "", cancellationToken);
        var missingVi = await _db.TextToSpeechCaches.CountAsync(x => x.UsageType != ManualUsageType && x.UsageType != "legacy" && x.UsageType != "title" && x.UsageType != "instruction" && (x.Status == null || x.Status != "ready" || x.AudioUrl == null || x.AudioUrl == ""), cancellationToken);
        var readyEn = await _db.TextToSpeechCaches.CountAsync(x => x.StatusEn == "ready" && x.AudioUrlEn != null && x.AudioUrlEn != "", cancellationToken);
        var missingEn = await _db.TextToSpeechCaches.CountAsync(x => x.UsageType != ManualUsageType && x.UsageType != "legacy" && x.UsageType != "title" && x.UsageType != "instruction" && (x.StatusEn == null || x.StatusEn != "ready" || x.AudioUrlEn == null || x.AudioUrlEn == ""), cancellationToken);

        var totalLessons = await _db.LearningItems.CountAsync(cancellationToken);
        var fullySyncedLessons = await _db.LearningItems
            .CountAsync(x => x.ContentJson.Contains("questionAudioUrlEn") && x.ContentJson.Contains("questionAudioUrl"), cancellationToken);
        var totalReuses = totalVoices > 0 ? await _db.TextToSpeechCaches.SumAsync(x => x.ReuseCount, cancellationToken) : 0;

        return new VoiceAuditStatsResult(
            totalVoices,
            readyVi,
            missingVi,
            readyEn,
            missingEn,
            totalLessons,
            fullySyncedLessons,
            totalReuses);
    }

    public async Task<string> BuildReportAsync(CancellationToken cancellationToken = default)
    {
        var stats = await GetVoiceAuditStatsAsync(cancellationToken);
        var duplicates = await _db.TextToSpeechCaches
            .GroupBy(x => x.NormalizedText.ToLower())
            .Where(g => g.Count() > 1)
            .CountAsync(cancellationToken);
        var topReused = await _db.TextToSpeechCaches
            .OrderByDescending(x => x.ReuseCount)
            .Take(5)
            .Select(x => $"{x.Name} (Dùng lại {x.ReuseCount} lần)")
            .ToListAsync(cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine($"TotalVoices: {stats.TotalVoices}");
        builder.AppendLine($"DuplicatesCount: {duplicates}");
        builder.AppendLine($"TotalReuses: {stats.TotalReuses}");
        builder.AppendLine($"ReadyVoicesVi: {stats.ReadyVoicesVi}");
        builder.AppendLine($"MissingVoicesVi: {stats.MissingVoicesVi}");
        builder.AppendLine($"ReadyVoicesEn: {stats.ReadyVoicesEn}");
        builder.AppendLine($"MissingVoicesEn: {stats.MissingVoicesEn}");
        builder.AppendLine($"TotalLessons: {stats.TotalLessons}");
        builder.AppendLine($"FullySyncedLessons: {stats.FullySyncedLessons}");
        builder.AppendLine("TopReused: " + string.Join(" | ", topReused));
        return builder.ToString();
    }

    private static readonly SemaphoreSlim SyncGate = new(1, 1);

    public async Task<VoiceLibrarySyncBatchResult> SyncAndGenerateBatchAsync(int batchSize = 1, CancellationToken cancellationToken = default, bool initialize = true)
    {
        await SyncGate.WaitAsync(cancellationToken);
        try
        {
            return await SyncOneVoiceAsync(cancellationToken, initialize);
        }
        finally
        {
            SyncGate.Release();
        }
    }

    private async Task<VoiceLibrarySyncBatchResult> SyncOneVoiceAsync(CancellationToken cancellationToken, bool initialize)
    {
        const int batchSize = 1;

        // 1. Chuẩn hóa dữ liệu bài học và dọn dẹp sạch toàn bộ voice thừa trong CSDL lẫn file vật lý
        if (initialize)
        {
            await RecoverInterruptedVoiceFilesAsync(Path.Combine(_environment.WebRootPath, "uploads", "audio"), cancellationToken);
            _db.ChangeTracker.Clear();
        }

        var createdVi = 0;
        var createdEn = 0;
        var failed = 0;
        var updatedItems = 0;
        var errors = new List<string>();

        var totalLessons = await _db.LearningItems.CountAsync(cancellationToken);

        if (initialize)
        {
            // Chỉ quét toàn bộ một lần ở request khởi tạo. Các request sau chỉ nạp
            // những bài liên quan đến voice vừa xử lý để tránh debugger giữ cả đồ thị JSON.
            for (var offset = 0; offset < totalLessons; offset += 20)
            {
                var page = await _db.LearningItems.AsNoTracking()
                    .Include(x => x.Questions.OrderBy(q => q.SortOrder))
                    .OrderBy(x => x.Id).Skip(offset).Take(20)
                    .ToListAsync(cancellationToken);
                foreach (var lesson in page)
                    await EnsureAndGetVoiceEntriesForLessonAsync(lesson, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                _db.ChangeTracker.Clear();
            }
            await EnsureBilingualListenVoiceRowsAsync(cancellationToken);
            foreach (var entry in await _db.TextToSpeechCaches.Where(x => x.UsageType != ManualUsageType).ToListAsync(cancellationToken))
            {
                if (!HasVoiceFile(entry.AudioUrl)) entry.Status = "missing";
                if (!HasVoiceFile(entry.AudioUrlEn)) entry.StatusEn = "missing";
            }
            await _db.SaveChangesAsync(cancellationToken);
            _db.ChangeTracker.Clear();
        }

        // 3. Quét và tạo file cho các dòng còn thiếu Voice VI hoặc Voice EN trong TextToSpeechCaches
        var missingVoicesQuery = _db.TextToSpeechCaches
            .Where(x => x.UsageType != "legacy" &&
                        x.UsageType != ManualUsageType &&
                        x.UsageType != "title" &&
                        x.UsageType != "instruction" &&
                        (((x.Status == null || x.Status != "failed") && (x.AudioUrl == null || x.AudioUrl == "" || x.Status == null || x.Status != "ready")) ||
                         ((x.StatusEn == null || x.StatusEn != "failed") && (x.AudioUrlEn == null || x.AudioUrlEn == "" || x.StatusEn == null || x.StatusEn != "ready"))))
            .OrderBy(x => x.UpdatedAt).ThenBy(x => x.Id);

        var missingVoicesBatch = await missingVoicesQuery.Take(1).ToListAsync(cancellationToken);

        foreach (var entry in missingVoicesBatch)
        {
            if (cancellationToken.IsCancellationRequested) break;
            if (!IsSpeakableText(entry.NormalizedText))
            {
                entry.Status = "failed";
                entry.StatusEn = "failed";
                entry.LastError = entry.LastErrorEn = "Nội dung voice không hợp lệ.";
                errors.Add(entry.LastError);
                failed++;
                await _db.SaveChangesAsync(cancellationToken);
                continue;
            }

            // Sinh Voice VI nếu còn thiếu
            if (entry.Status != "failed" && (string.IsNullOrWhiteSpace(entry.AudioUrl) || entry.Status != "ready"))
            {
                try
                {
                    entry.AudioUrl = await GenerateVoiceCacheFileAsync(entry, cancellationToken);
                    entry.Status = "ready";
                    entry.LastError = null;
                    createdVi++;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    entry.AudioUrl = string.Empty;
                    entry.Status = "failed";
                    entry.LastError = CompactErrorMessage(ex.Message, 1000);
                    failed++;
                    errors.Add($"VI [{entry.Name}]: {CompactErrorMessage(ex.Message)}");
                }
            }

            // Sinh Voice EN nếu còn thiếu - sinh đồng thời cho cùng một bản ghi song ngữ!
            if (entry.StatusEn != "failed" && (string.IsNullOrWhiteSpace(entry.AudioUrlEn) || entry.StatusEn != "ready"))
            {
                try
                {
                    entry.AudioUrlEn = await GenerateVoiceCacheFileEnAsync(entry, cancellationToken);
                    entry.StatusEn = "ready";
                    entry.LastErrorEn = null;
                    createdEn++;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    entry.AudioUrlEn = string.Empty;
                    entry.StatusEn = "failed";
                    entry.LastErrorEn = CompactErrorMessage(ex.Message, 1000);
                    failed++;
                    errors.Add($"EN [{entry.Name}]: {CompactErrorMessage(ex.Message)}");
                }
            }

            entry.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            InvalidateVoiceLookupCache();
        }

        // 4. Lấy bài học cần liên kết URL
        var stillHasMissingVoices = await missingVoicesQuery.AnyAsync(cancellationToken);
        var processedTexts = missingVoicesBatch
            .Select(x => NormalizeSpeechText(string.IsNullOrWhiteSpace(x.OriginalText) ? x.NormalizedText : x.OriginalText))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        List<LearningItem> unlinkedLessons;
        if (stillHasMissingVoices)
        {
            if (processedTexts.Count > 0)
            {
                var affectedIds = await FindLearningItemIdsUsingVoiceTextsAsync(processedTexts, cancellationToken);
                unlinkedLessons = await LoadLearningItemsByIdsAsync(affectedIds.Take(20).ToHashSet(), cancellationToken);
            }
            else
            {
                unlinkedLessons = new List<LearningItem>();
            }
        }
        else
        {
            // Khi file voice đã đủ, liên kết nốt theo trang nhỏ thay vì đưa toàn bộ
            // bài học vào bộ nhớ trong một request cuối rất lớn.
            var pageIds = await GetUnlinkedLearningItemIdsAsync(batchSize * 3, cancellationToken);
            unlinkedLessons = await LoadLearningItemsByIdsAsync(pageIds, cancellationToken);
        }
        foreach (var lesson in unlinkedLessons)
        {
            if (cancellationToken.IsCancellationRequested) break;

            // Đảm bảo các voice của bài học này tồn tại trong CSDL
            try
            {
                if (await LinkVoiceUrlsForLearningItemAsync(lesson, cancellationToken))
                {
                    updatedItems++;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                failed++;
                errors.Add($"Đồng bộ [{lesson.Title}]: {CompactErrorMessage(ex.Message)}");
            }
        }
        if (unlinkedLessons.Count > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        _db.ChangeTracker.Clear();

        var totalEntries = await _db.TextToSpeechCaches.CountAsync(cancellationToken);
        var remainingMissingVi = await _db.TextToSpeechCaches.CountAsync(x => x.UsageType != "legacy" && x.UsageType != ManualUsageType && x.UsageType != "title" && x.UsageType != "instruction" && (x.AudioUrl == null || x.AudioUrl == "" || x.Status == null || x.Status != "ready"), cancellationToken);
        var remainingMissingEn = await _db.TextToSpeechCaches.CountAsync(x => x.UsageType != "legacy" && x.UsageType != ManualUsageType && x.UsageType != "title" && x.UsageType != "instruction" && (x.AudioUrlEn == null || x.AudioUrlEn == "" || x.StatusEn == null || x.StatusEn != "ready"), cancellationToken);
        var remainingUnlinkedLessons = (remainingMissingVi + remainingMissingEn > 0)
            ? 0
            : await CountUnlinkedLearningItemsAsync(cancellationToken);

        var remainingTotal = remainingMissingVi + remainingMissingEn + remainingUnlinkedLessons;
        var isCompleted = remainingTotal == 0;

        var result = new VoiceLibrarySyncBatchResult(
            totalLessons,
            totalEntries,
            remainingMissingVi,
            remainingMissingEn,
            missingVoicesBatch.Count + unlinkedLessons.Count,
            createdVi,
            createdEn,
            failed,
            updatedItems,
            remainingTotal,
            isCompleted,
            errors,
            stillHasMissingVoices || updatedItems > 0);
        _db.ChangeTracker.Clear();
        return result;
    }

    public async Task<VoiceLibraryRebuildResult> ResetAndRebuildAllVoicesAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("[VoiceRebuild] Bắt đầu dọn dẹp và tái thiết lập toàn bộ kho Voice chuẩn nữ...");

        var protectedAudioPaths = await GetManualVoiceStoragePathsAsync(cancellationToken);

        // 2. Dọn sạch các file audio cũ trong wwwroot/uploads/audio/
        try
        {
            var folder = Path.Combine(_environment.WebRootPath, "uploads", "audio");
            if (Directory.Exists(folder))
            {
                foreach (var file in Directory.EnumerateFiles(folder, "voice-*.mp3"))
                {
                    var relativePath = $"/uploads/audio/{Path.GetFileName(file)}";
                    if (protectedAudioPaths.Contains(relativePath)) continue;
                    try { File.Delete(file); } catch { }
                }
                foreach (var file in Directory.EnumerateFiles(folder, "voice-en-*.mp3"))
                {
                    var relativePath = $"/uploads/audio/{Path.GetFileName(file)}";
                    if (protectedAudioPaths.Contains(relativePath)) continue;
                    try { File.Delete(file); } catch { }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[VoiceRebuild] Cảnh báo khi dọn file audio cũ");
        }

        // 3. Xóa sạch dữ liệu TextToSpeechCaches và MediaAssets audio
        var oldCaches = await _db.TextToSpeechCaches
            .Where(x => x.UsageType != ManualUsageType)
            .ToListAsync(cancellationToken);
        _db.TextToSpeechCaches.RemoveRange(oldCaches);

        var oldAudioAssets = await _db.MediaAssets
            .Where(x => x.AssetType == "audio" && (x.StoragePath == null || !protectedAudioPaths.Contains(x.StoragePath)))
            .ToListAsync(cancellationToken);
        _db.MediaAssets.RemoveRange(oldAudioAssets);
        await _db.SaveChangesAsync(cancellationToken);

        // 4. Quét toàn bộ 447 bài học và trích xuất tất cả text duy nhất (Deduplication)
        var allLessons = await _db.LearningItems
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        var uniqueTexts = new Dictionary<string, (string UsageType, string RawText, string TextEn, int InitialCount)>(StringComparer.OrdinalIgnoreCase);

        void Collect(string? text, string usageType, string? textEn = null)
        {
            if (!IsSpeakableText(text)) return;
            var norm = NormalizeSpeechText(text!);
            if (string.IsNullOrWhiteSpace(norm) || !IsSpeakableText(norm)) return;

            if (uniqueTexts.TryGetValue(norm, out var existing))
            {
                uniqueTexts[norm] = (existing.UsageType, existing.RawText, string.IsNullOrWhiteSpace(existing.TextEn) ? Clean(textEn) : existing.TextEn, existing.InitialCount + 1);
            }
            else
            {
                uniqueTexts[norm] = (usageType, text!, Clean(textEn), 1);
            }
        }

        // 4.1. Phản hồi sư phạm chuẩn
        Collect("Giỏi lắm", "correct-feedback", "Great job!");
        Collect("Con thử lại nhé", "retry-feedback");
        Collect("Xuất sắc, con đã hoàn thành bài học!", "correct-feedback");

        // 4.2. Ký tự chữ cái & chữ số chuẩn
        var standardSymbols = new[]
        {
            "A", "Ă", "Â", "B", "C", "D", "Đ", "E", "Ê", "G", "H", "I", "K", "L", "M", "N", "O", "Ô", "Ơ", "P", "Q", "R", "S", "T", "U", "Ư", "V", "X", "Y",
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10"
        };
        foreach (var sym in standardSymbols)
        {
            Collect(sym, "option");
        }

        // 4.3. Quét từng bài học trong CSDL (chỉ quét câu hỏi, đáp án, phản hồi, bài nghe)
        foreach (var lesson in allLessons)
        {

            foreach (var q in lesson.Questions)
            {
                Collect(q.PromptText, lesson.InteractionType == InteractionTypes.Tracing ? "tracing-prompt" : "question");
                var cor = ReadJsonString(q.FeedbackJson, "correct");
                if (!string.IsNullOrWhiteSpace(cor)) Collect(cor, "correct-feedback");
                var ret = ReadJsonString(q.FeedbackJson, "retry");
                if (!string.IsNullOrWhiteSpace(ret)) Collect(ret, "retry-feedback");

                var payload = ParsePayloadObject(q.PayloadJson);
                if (lesson.InteractionType == InteractionTypes.StoryChoice)
                {
                    Collect(ReadJsonString(payload, "speechText"), "content");
                }

                foreach (var opt in CollectOptionSpeechLabels(payload))
                {
                    Collect(opt, "option");
                }
            }
        }

        // 5. Khởi tạo danh sách bản ghi duy nhất trong TextToSpeechCaches
        var voiceVi = "vi-VN-HoaiMyNeural";
        var voiceEn = "en-US-JennyNeural";

        var newEntries = new List<TextToSpeechCache>();
        foreach (var (norm, (usage, raw, textEn, count)) in uniqueTexts)
        {
            var key = BuildTextToSpeechCacheKey(norm);
            var entry = new TextToSpeechCache
            {
                Id = Guid.NewGuid(),
                Provider = "edge",
                Voice = voiceVi,
                VoiceEn = voiceEn,
                ModelId = "neural",
                Format = "mp3",
                TextHash = key.TextHash,
                Name = BuildVoiceName(usage, norm),
                UsageType = usage,
                NormalizedText = AudioAltText(norm),
                OriginalText = AudioOriginalText(raw),
                TextEn = textEn,
                AudioUrl = string.Empty,
                AudioUrlEn = string.Empty,
                Status = "missing",
                StatusEn = "missing",
                ReuseCount = count,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            newEntries.Add(entry);
        }

        _db.TextToSpeechCaches.AddRange(newEntries);
        await _db.SaveChangesAsync(cancellationToken);
        await EnsureBilingualListenVoiceRowsAsync(cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        newEntries.AddRange(await _db.TextToSpeechCaches
            .Where(x => x.UsageType == BilingualListenUsageType)
            .ToListAsync(cancellationToken));
        _logger.LogInformation("[VoiceRebuild] Đã nạp {Count} bản ghi Voice duy nhất.", newEntries.Count);

        // 6. Sinh file âm thanh Nữ song ngữ (Hoài My - VN, Jenny - EN)
        var genVi = 0;
        var genEn = 0;
        var failed = 0;
        var processedCount = 0;

        foreach (var entry in newEntries)
        {
            if (cancellationToken.IsCancellationRequested) break;

            // 6.1 Dịch sang Tiếng Anh nếu chưa có
            if (string.IsNullOrWhiteSpace(entry.TextEn))
            {
                try
                {
                    entry.TextEn = await PreschoolTranslationHelper.TranslateToEnglishAsync(entry.OriginalText);
                }
                catch
                {
                    entry.TextEn = entry.NormalizedText;
                }
            }

            // 6.2 Sinh Voice VN (Hoài My - Nữ)
            try
            {
                entry.AudioUrl = await GenerateVoiceCacheFileAsync(entry, cancellationToken);
                entry.Status = "ready";
                entry.LastError = null;
                genVi++;
            }
            catch (Exception ex)
            {
                entry.Status = "failed";
                entry.LastError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                failed++;
            }

            // 6.3 Sinh Voice EN (Jenny - Nữ)
            if (!string.IsNullOrWhiteSpace(entry.TextEn))
            {
                try
                {
                    entry.AudioUrlEn = await GenerateVoiceCacheFileEnAsync(entry, cancellationToken);
                    entry.StatusEn = "ready";
                    entry.LastErrorEn = null;
                    genEn++;
                }
                catch (Exception ex)
                {
                    entry.StatusEn = "failed";
                    entry.LastErrorEn = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                    failed++;
                }
            }

            entry.UpdatedAt = DateTimeOffset.UtcNow;
            processedCount++;

            // Lưu định kỳ mỗi 15 bản ghi để không bị mất tiến độ hoặc lỗi Concurrency
            if (processedCount % 15 == 0)
            {
                try
                {
                    await _db.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[VoiceRebuild] Lỗi lưu batch {Count}", processedCount);
                }
            }
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[VoiceRebuild] Lỗi lưu batch cuối");
        }

        _logger.LogInformation("[VoiceRebuild] Đã sinh {Vi} file VN, {En} file EN (lỗi {Failed}).", genVi, genEn, failed);

        // 7. Liên kết lại URL cho 100% bài học trong hệ thống
        var updatedLessons = 0;
        foreach (var lesson in allLessons)
        {
            try
            {
                if (await LinkVoiceUrlsForLearningItemAsync(lesson, cancellationToken))
                {
                    updatedLessons++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[VoiceRebuild] Lỗi liên kết bài học {Title}", lesson.Title);
            }
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[VoiceRebuild] Lỗi lưu liên kết bài học");
        }

        _logger.LogInformation("[VoiceRebuild] Đã liên kết xong voice cho {Count} bài học.", updatedLessons);

        return new VoiceLibraryRebuildResult(newEntries.Count, genVi, genEn, failed, updatedLessons);
    }

    private static bool IsLessonFullySynced(LearningItem item)
    {
        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        if (question is null) return true;

        var payload = ParsePayloadObject(question.PayloadJson);
        var questionVi = ReadJsonString(payload, "questionAudioUrl");
        var questionEn = ReadJsonString(payload, "questionAudioUrlEn");
        var hasOptionEn = payload.ContainsKey("optionAudioEn");
        var hasOptionVi = payload.ContainsKey("optionAudio");

        return !string.IsNullOrWhiteSpace(questionVi) &&
               !string.IsNullOrWhiteSpace(questionEn) &&
               hasOptionEn &&
               hasOptionVi;
    }

    private async Task<HashSet<Guid>> FindLearningItemIdsUsingVoiceTextsAsync(
        HashSet<string> texts,
        CancellationToken cancellationToken)
    {
        var result = new HashSet<Guid>();
        foreach (var text in texts)
        {
            var ids = await _db.Questions
                .AsNoTracking()
                .Where(x => x.PromptText == text ||
                            x.FeedbackJson.Contains(text) ||
                            x.PayloadJson.Contains(text))
                .Select(x => x.LearningItemId)
                .Distinct()
                .ToListAsync(cancellationToken);
            result.UnionWith(ids);
        }

        return result;
    }

    private async Task<List<LearningItem>> LoadLearningItemsByIdsAsync(
        HashSet<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0) return new List<LearningItem>();

        return await _db.LearningItems
            .Where(x => ids.Contains(x.Id))
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    private async Task<HashSet<Guid>> GetUnlinkedLearningItemIdsAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        var ids = await UnlinkedLearningItemsQuery()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .Select(x => x.Id)
            .Take(Math.Max(1, limit))
            .ToListAsync(cancellationToken);
        return ids.ToHashSet();
    }

    private Task<int> CountUnlinkedLearningItemsAsync(CancellationToken cancellationToken)
    {
        return UnlinkedLearningItemsQuery().CountAsync(cancellationToken);
    }

    private IQueryable<LearningItem> UnlinkedLearningItemsQuery()
    {
        const string questionVi = "\"questionAudioUrl\":\"/uploads/audio/";
        const string questionEn = "\"questionAudioUrlEn\":\"/uploads/audio/";
        const string optionVi = "\"optionAudio\":";
        const string optionEn = "\"optionAudioEn\":";

        return _db.LearningItems.Where(item => item.Questions.Any(q =>
            !q.PayloadJson.Contains(questionVi) ||
            !q.PayloadJson.Contains(questionEn) ||
            !q.PayloadJson.Contains(optionVi) ||
            !q.PayloadJson.Contains(optionEn)));
    }

    private static bool LearningItemUsesAnyVoiceText(LearningItem item, HashSet<string> texts)
    {
        if (texts.Count == 0) return false;

        static bool Matches(string? value, HashSet<string> candidates)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return candidates.Contains(NormalizeSpeechText(value));
        }

        foreach (var question in item.Questions)
        {
            if (Matches(question.PromptText, texts) ||
                Matches(ReadJsonString(question.FeedbackJson, "correct"), texts) ||
                Matches(ReadJsonString(question.FeedbackJson, "retry"), texts))
            {
                return true;
            }

            var payload = ParsePayloadObject(question.PayloadJson);
            if (item.InteractionType == InteractionTypes.StoryChoice &&
                Matches(ReadJsonString(payload, "speechText"), texts))
            {
                return true;
            }

            if (CollectOptionSpeechLabels(payload).Any(x => Matches(x, texts)))
            {
                return true;
            }
        }

        return false;
    }

    private static readonly HashSet<string> GenericPromptsToClean = new(StringComparer.OrdinalIgnoreCase)
    {
        "Những đáp án nào phù hợp?",
        "Những đáp án nào phù hợp",
        "Thứ tự đúng là gì?",
        "Thứ tự đúng là gì",
        "Con vừa nghe thấy gì?",
        "Con vừa nghe thấy gì",
        "Mỗi vật thuộc nhóm nào?",
        "Mỗi vật thuộc nhóm nào",
        "Con hãy nối đủ các cặp.",
        "Con hãy nối đủ các cặp",
        "Vật nào đúng?",
        "Đáp án nào đúng?",
        "Con chọn đáp án đúng.",
        "Con chọn đáp án đúng"
    };

    public async Task<int> CleanupAllRedundantDatabaseAndVoiceFilesAsync(CancellationToken cancellationToken = default)
    {
        // Thu thập danh sách tất cả các chuỗi text thực sự đang được dùng trong các bài học.
        var activeTextHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var lessons = await _db.LearningItems
            .Include(x => x.Questions)
            .ToListAsync(cancellationToken);

        foreach (var item in lessons)
        {
            void AddText(string? txt)
            {
                if (string.IsNullOrWhiteSpace(txt)) return;
                var norm = NormalizeSpeechText(txt);
                if (IsSpeakableText(norm))
                {
                    var key = BuildTextToSpeechCacheKey(norm);
                    activeTextHashes.Add(key.TextHash);
                }
            }

            // Chỉ thu thập voice cho nội dung thực tế phát âm thanh (câu hỏi, phản hồi, bài nghe, đáp án)
            // Không thu thập cho Title và InstructionText của bài học
            var q = item.Questions.FirstOrDefault();
            if (q is null) continue;

            AddText(q.PromptText);
            AddText(ReadJsonString(q.FeedbackJson, "correct"));
            AddText(ReadJsonString(q.FeedbackJson, "retry"));

            var payload = ParsePayloadObject(q.PayloadJson);
            if (item.InteractionType == InteractionTypes.StoryChoice)
            {
                AddText(ReadJsonString(payload, "speechText"));
            }

            foreach (var label in CollectOptionSpeechLabels(payload))
            {
                AddText(label);
            }
        }

        foreach (var pair in CollectBilingualListenVoicePairs())
            activeTextHashes.Add(BuildBilingualListenCacheKey(pair).TextHash);

        // 3. Tìm tất cả các dòng TextToSpeechCaches không còn được bài học nào sử dụng, hoặc là title / instruction / legacy / generic rác
        var allCaches = await _db.TextToSpeechCaches.ToListAsync(cancellationToken);
        var redundantEntries = allCaches.Where(x =>
            !IsManualVoiceEntry(x) &&
            (x.UsageType == "title" ||
             x.UsageType == "instruction" ||
             x.UsageType == "legacy" ||
             GenericPromptsToClean.Contains(x.NormalizedText) ||
             GenericPromptsToClean.Contains(x.OriginalText) ||
             !activeTextHashes.Contains(x.TextHash))
        ).ToList();

        var deletedCount = 0;
        foreach (var entry in redundantEntries)
        {
            _db.TextToSpeechCaches.Remove(entry);
            deletedCount++;
        }

        if (deletedCount > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        // 4. Xóa tất cả các file mp3 mồ côi trên ổ đĩa
        deletedCount += await CleanupRedundantAudioAssetRowsAsync(cancellationToken);
        await CleanupAllOrphanedPhysicalAudioFilesAsync(cancellationToken);

        return deletedCount;
    }

    public async Task<int> CleanupAllOrphanedPhysicalAudioFilesAsync(CancellationToken cancellationToken = default)
    {
        var folder = Path.Combine(_environment.WebRootPath, "uploads", "audio");
        if (!Directory.Exists(folder))
        {
            return 0;
        }

        // Một request có thể dừng sau khi edge-tts đã ghi xong file nhưng trước SaveChanges.
        // Khôi phục URL theo tên file xác định trước khi xem file đó là mồ côi.
        await RecoverInterruptedVoiceFilesAsync(folder, cancellationToken);

        // Gom tất cả các URL âm thanh hợp lệ đang được lưu trong CSDL
        var activeUrls = await GetActiveVoiceStoragePathsAsync(cancellationToken);

        var deletedCount = 0;
        var filesOnDisk = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly);

        foreach (var file in filesOnDisk)
        {
            var fileName = Path.GetFileName(file);
            var relativePath = $"/uploads/audio/{fileName}";

            if (!activeUrls.Contains(relativePath))
            {
                try
                {
                    File.Delete(file);
                    deletedCount++;
                }
                catch
                {
                }
            }
        }

        return deletedCount;
    }

    private async Task<int> RecoverInterruptedVoiceFilesAsync(string folder, CancellationToken cancellationToken)
    {
        var candidates = await _db.TextToSpeechCaches
            .Where(x => x.UsageType != "legacy" && x.UsageType != ManualUsageType &&
                        ((x.AudioUrl == null || x.AudioUrl == "" || x.Status != "ready") ||
                         (x.AudioUrlEn == null || x.AudioUrlEn == "" || x.StatusEn != "ready")))
            .ToListAsync(cancellationToken);

        var voiceVi = _configuration["VoiceLibrary:Voice"]?.Trim();
        if (string.IsNullOrWhiteSpace(voiceVi)) voiceVi = "vi-VN-HoaiMyNeural";
        var rateVi = _configuration["VoiceLibrary:Rate"]?.Trim();
        if (string.IsNullOrWhiteSpace(rateVi)) rateVi = "-10%";
        var voiceEn = _configuration["VoiceLibrary:VoiceEn"]?.Trim();
        if (string.IsNullOrWhiteSpace(voiceEn)) voiceEn = "en-US-JennyNeural";
        var rateEn = _configuration["VoiceLibrary:RateEn"]?.Trim();
        if (string.IsNullOrWhiteSpace(rateEn)) rateEn = "-18%";

        var recovered = 0;
        foreach (var entry in candidates)
        {
            if (string.IsNullOrWhiteSpace(entry.AudioUrl) || entry.Status != "ready")
            {
                var textVi = ResolveTextForSpeechSynthesis(NormalizeSpeechText(
                    string.IsNullOrWhiteSpace(entry.OriginalText) ? entry.NormalizedText : entry.OriginalText));
                if (!string.IsNullOrWhiteSpace(textVi))
                {
                    var fileName = BuildDeterministicVoiceFileName("voice", entry.Name, textVi, voiceVi, rateVi);
                    var filePath = Path.Combine(folder, fileName);
                    if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                    {
                        entry.AudioUrl = $"/uploads/audio/{fileName}";
                        entry.Status = "ready";
                        entry.LastError = null;
                        entry.UpdatedAt = DateTimeOffset.UtcNow;
                        await EnsureMediaAssetForAudioAsync(entry.AudioUrl, fileName, AudioCacheKey(entry.NormalizedText), cancellationToken);
                        recovered++;
                    }
                }
            }

            if ((string.IsNullOrWhiteSpace(entry.AudioUrlEn) || entry.StatusEn != "ready") &&
                !string.IsNullOrWhiteSpace(entry.TextEn))
            {
                var fileName = BuildDeterministicVoiceFileName("voice-en", entry.Name, entry.TextEn, voiceEn, rateEn);
                var filePath = Path.Combine(folder, fileName);
                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                {
                    entry.AudioUrlEn = $"/uploads/audio/{fileName}";
                    entry.StatusEn = "ready";
                    entry.LastErrorEn = null;
                    entry.UpdatedAt = DateTimeOffset.UtcNow;
                    await EnsureMediaAssetForAudioAsync(entry.AudioUrlEn, fileName, AudioCacheKey($"en:{entry.NormalizedText}"), cancellationToken);
                    recovered++;
                }
            }
        }

        if (recovered > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("[VoiceSync] Đã khôi phục {Count} file tạo xong trước khi tiến trình bị gián đoạn.", recovered);
        }

        return recovered;
    }

    private async Task<int> CleanupRedundantAudioAssetRowsAsync(CancellationToken cancellationToken)
    {
        var activeUrls = await GetActiveVoiceStoragePathsAsync(cancellationToken);
        var audioAssets = await _db.MediaAssets
            .Where(x => x.AssetType == "audio" && !string.IsNullOrEmpty(x.StoragePath))
            .ToListAsync(cancellationToken);
        var redundantAssets = audioAssets
            .Where(x => !activeUrls.Contains(NormalizeStoragePath(x.StoragePath!)))
            .ToList();

        if (redundantAssets.Count == 0)
        {
            return 0;
        }

        _db.MediaAssets.RemoveRange(redundantAssets);
        await _db.SaveChangesAsync(cancellationToken);
        return redundantAssets.Count;
    }

    private async Task<HashSet<string>> GetActiveVoiceStoragePathsAsync(CancellationToken cancellationToken)
    {
        var activeUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var viUrls = await _db.TextToSpeechCaches
            .Where(x => !string.IsNullOrEmpty(x.AudioUrl))
            .Select(x => x.AudioUrl!)
            .ToListAsync(cancellationToken);
        foreach (var u in viUrls) activeUrls.Add(NormalizeStoragePath(u));

        var enUrls = await _db.TextToSpeechCaches
            .Where(x => !string.IsNullOrEmpty(x.AudioUrlEn))
            .Select(x => x.AudioUrlEn!)
            .ToListAsync(cancellationToken);
        foreach (var u in enUrls) activeUrls.Add(NormalizeStoragePath(u));

        return activeUrls;
    }

    private async Task<HashSet<string>> GetManualVoiceStoragePathsAsync(CancellationToken cancellationToken)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var entries = await _db.TextToSpeechCaches
            .Where(x => x.UsageType == ManualUsageType)
            .Select(x => new { x.AudioUrl, x.AudioUrlEn })
            .ToListAsync(cancellationToken);

        foreach (var entry in entries)
        {
            if (!string.IsNullOrWhiteSpace(entry.AudioUrl))
            {
                paths.Add(NormalizeStoragePath(entry.AudioUrl));
            }
            if (!string.IsNullOrWhiteSpace(entry.AudioUrlEn))
            {
                paths.Add(NormalizeStoragePath(entry.AudioUrlEn));
            }
        }

        return paths;
    }

    private static string NormalizeStoragePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        var p = path.Trim().Replace('\\', '/');
        return p.StartsWith('/') ? p : "/" + p;
    }

    private static bool IsManualVoiceEntry(TextToSpeechCache entry)
    {
        return string.Equals(entry.UsageType, ManualUsageType, StringComparison.OrdinalIgnoreCase);
    }

    public void DeletePhysicalAudioFile(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        try
        {
            var relative = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.WebRootPath, relative);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
        }
    }

    private async Task<List<TextToSpeechCache>> EnsureAndGetVoiceEntriesForLessonAsync(LearningItem item, CancellationToken cancellationToken)
    {
        var result = new List<TextToSpeechCache>();
        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        if (question is null) return result;

        var payload = ParsePayloadObject(question.PayloadJson);

        void AddIfValid(TextToSpeechCache? entry)
        {
            if (entry != null && !result.Any(x => x.Id == entry.Id))
            {
                result.Add(entry);
            }
        }

        // Chỉ tạo Voice cho câu hỏi bài học (PromptText), phản hồi đúng/sai, nội dung nghe và các đáp án
        AddIfValid(await EnsureVoiceEntryAsync(question.PromptText, item.InteractionType == InteractionTypes.Tracing ? "tracing-prompt" : "question", item.Title, cancellationToken));
        AddIfValid(await EnsureVoiceEntryAsync(ReadJsonString(question.FeedbackJson, "correct"), "correct-feedback", item.Title, cancellationToken));
        AddIfValid(await EnsureVoiceEntryAsync(ReadJsonString(question.FeedbackJson, "retry"), "retry-feedback", item.Title, cancellationToken));

        if (item.InteractionType == InteractionTypes.StoryChoice)
        {
            AddIfValid(await EnsureVoiceEntryAsync(ReadJsonString(payload, "speechText"), "content", item.Title, cancellationToken));
        }

        foreach (var label in CollectOptionSpeechLabels(payload))
        {
            AddIfValid(await EnsureVoiceEntryAsync(label, "option", item.Title, cancellationToken));
        }

        return result;
    }

    private async Task<bool> LessonNeedsVoiceGenerationAsync(LearningItem item, CancellationToken cancellationToken)
    {
        var entries = await EnsureAndGetVoiceEntriesForLessonAsync(item, cancellationToken);
        return entries.Any(x => (x.AudioUrl == null || x.AudioUrl == "" || x.Status == null || x.Status != "ready") ||
                                (x.AudioUrlEn == null || x.AudioUrlEn == "" || x.StatusEn == null || x.StatusEn != "ready"));
    }

    public async Task<VoiceLibraryResetResult> ResetAndRebuildAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.MigrateAsync(cancellationToken);

        var protectedAudioPaths = await GetManualVoiceStoragePathsAsync(cancellationToken);
        var deletedFiles = DeleteAudioFiles(protectedAudioPaths);
        var deletedVoiceRows = await _db.TextToSpeechCaches
            .Where(x => x.UsageType != ManualUsageType)
            .ExecuteDeleteAsync(cancellationToken);
        var deletedAudioRows = await _db.MediaAssets
            .Where(x => x.AssetType == "audio" && (x.StoragePath == null || !protectedAudioPaths.Contains(x.StoragePath)))
            .ExecuteDeleteAsync(cancellationToken);

        var items = await _db.LearningItems
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            await EnsureVoiceRowsForLearningItemAsync(item, cancellationToken);
        }
        await EnsureBilingualListenVoiceRowsAsync(cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var voiceRowsCreated = await _db.TextToSpeechCaches.CountAsync(cancellationToken);
        var entries = await _db.TextToSpeechCaches
            .Where(x => x.UsageType != ManualUsageType &&
                        (string.IsNullOrWhiteSpace(x.AudioUrl) || x.Status != "ready" ||
                         string.IsNullOrWhiteSpace(x.AudioUrlEn) || x.StatusEn != "ready"))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var created = 0;
        var failed = 0;
        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (string.IsNullOrWhiteSpace(entry.AudioUrl) || entry.Status != "ready")
                {
                    entry.AudioUrl = await GenerateVoiceCacheFileAsync(entry, cancellationToken);
                    entry.Status = "ready";
                    entry.LastError = null;
                }

                if (string.IsNullOrWhiteSpace(entry.AudioUrlEn) || entry.StatusEn != "ready")
                {
                    entry.AudioUrlEn = await GenerateVoiceCacheFileEnAsync(entry, cancellationToken);
                    entry.StatusEn = "ready";
                    entry.LastErrorEn = null;
                }

                entry.UpdatedAt = DateTimeOffset.UtcNow;
                created += 1;
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                entry.LastError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                entry.UpdatedAt = DateTimeOffset.UtcNow;
                failed += 1;
                _logger.LogWarning(ex, "Cannot generate voice file for {VoiceName}", entry.Name);
                try { await _db.SaveChangesAsync(cancellationToken); } catch { }
            }
        }

        var updatedItems = 0;
        foreach (var item in items)
        {
            if (await LinkVoiceUrlsForLearningItemAsync(item, cancellationToken))
            {
                updatedItems += 1;
            }
        }
        await _db.SaveChangesAsync(cancellationToken);

        return new VoiceLibraryResetResult(
            deletedVoiceRows,
            deletedAudioRows,
            deletedFiles,
            items.Count,
            voiceRowsCreated,
            created,
            failed,
            updatedItems);
    }

    public async Task<VoiceLibraryRelinkResult> EnsureVoiceRowsAndRelinkAsync(CancellationToken cancellationToken = default)
    {
        var backfilled = 0;
        var items = await _db.LearningItems
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            await EnsureVoiceRowsForLearningItemAsync(item, cancellationToken);
        }
        await EnsureBilingualListenVoiceRowsAsync(cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var updatedItems = 0;
        foreach (var item in items)
        {
            if (await LinkVoiceUrlsForLearningItemAsync(item, cancellationToken))
            {
                updatedItems += 1;
            }
        }
        await _db.SaveChangesAsync(cancellationToken);

        return new VoiceLibraryRelinkResult(backfilled, items.Count, updatedItems);
    }

    private async Task<int> BackfillLegacyAudioAssetsAsync(CancellationToken cancellationToken)
    {
        var audioAssets = await _db.MediaAssets
            .Where(x => x.AssetType == "audio" && !string.IsNullOrWhiteSpace(x.AltText))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        var existingHashes = await _db.TextToSpeechCaches
            .Select(x => x.TextHash)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        var added = 0;
        foreach (var asset in audioAssets)
        {
            var voiceText = ExtractVoiceTextFromAltText(asset.AltText!);
            var normalizedText = NormalizeSpeechText(voiceText);
            if (string.IsNullOrWhiteSpace(normalizedText))
            {
                continue;
            }

            var key = BuildTextToSpeechCacheKey(normalizedText);
            if (existingHashes.Contains(key.TextHash))
            {
                continue;
            }

            var textEn = PreschoolTranslationHelper.TranslateToEnglish(voiceText);
            _db.TextToSpeechCaches.Add(new TextToSpeechCache
            {
                Id = Guid.NewGuid(),
                Provider = key.Provider,
                Voice = key.Voice,
                VoiceEn = "en-US-JennyNeural",
                ModelId = key.ModelId,
                Format = key.Format,
                TextHash = key.TextHash,
                Name = BuildVoiceName("legacy", null, normalizedText),
                UsageType = "legacy",
                NormalizedText = AudioAltText(normalizedText),
                OriginalText = AudioOriginalText(voiceText),
                TextEn = textEn,
                AudioUrl = asset.StoragePath,
                AudioUrlEn = string.Empty,
                Status = "ready",
                StatusEn = "missing",
                CreatedAt = asset.CreatedAt,
                UpdatedAt = DateTimeOffset.UtcNow
            });
            existingHashes.Add(key.TextHash);
            added += 1;
        }

        return added;
    }

    private async Task<int> EnsureBilingualListenVoiceRowsAsync(CancellationToken cancellationToken)
    {
        var pairs = CollectBilingualListenVoicePairs();
        var allCaches = await _db.TextToSpeechCaches.ToListAsync(cancellationToken);
        var validHashes = pairs
            .Select(x => BuildBilingualListenCacheKey(x).TextHash)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var obsoleteRows = allCaches
            .Where(x => x.UsageType == BilingualListenUsageType && !validHashes.Contains(x.TextHash))
            .ToList();
        var addedOrUpdated = obsoleteRows.Count;
        foreach (var obsolete in obsoleteRows)
        {
            // Nếu khóa cũ còn tồn tại sau bước dọn dẹp thì nội dung này cũng đang được
            // bài học thường sử dụng. Giữ voice VI, trả bản ghi về nhóm chung và tạo lại EN,
            // tránh xóa nhầm dữ liệu của bài học đã từng bị cấu trúc song ngữ cũ dùng chung.
            obsolete.UsageType = "content";
            obsolete.TextEn = string.Empty;
            obsolete.AudioUrlEn = string.Empty;
            obsolete.StatusEn = "missing";
            obsolete.LastErrorEn = null;
            obsolete.UpdatedAt = DateTimeOffset.UtcNow;
        }

        var reusableViByText = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var reusableEnByText = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var cached in allCaches)
        {
            if (cached.Status == "ready" && HasVoiceFile(cached.AudioUrl))
            {
                var original = NormalizeSpeechText(cached.OriginalText);
                var normalized = NormalizeSpeechText(cached.NormalizedText);
                if (!string.IsNullOrWhiteSpace(original)) reusableViByText.TryAdd(original, cached.AudioUrl);
                if (!string.IsNullOrWhiteSpace(normalized)) reusableViByText.TryAdd(normalized, cached.AudioUrl);
            }

            if (cached.StatusEn == "ready" && HasVoiceFile(cached.AudioUrlEn))
            {
                var english = NormalizeSpeechText(cached.TextEn ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(english)) reusableEnByText.TryAdd(english, cached.AudioUrlEn!);
            }
        }

        foreach (var pair in pairs)
        {
            var key = BuildBilingualListenCacheKey(pair);
            var entry = allCaches.FirstOrDefault(x =>
                x.UsageType == BilingualListenUsageType && x.TextHash == key.TextHash);
            if (entry is null)
            {
                var voiceVi = _configuration["VoiceLibrary:Voice"]?.Trim();
                var voiceEn = _configuration["VoiceLibrary:VoiceEn"]?.Trim();
                entry = new TextToSpeechCache
                {
                    Id = Guid.NewGuid(),
                    Provider = key.Provider,
                    Voice = string.IsNullOrWhiteSpace(voiceVi) ? "vi-VN-HoaiMyNeural" : voiceVi,
                    VoiceEn = string.IsNullOrWhiteSpace(voiceEn) ? "en-US-JennyNeural" : voiceEn,
                    ModelId = key.ModelId,
                    Format = key.Format,
                    TextHash = key.TextHash,
                    Name = BuildVoiceName(BilingualListenUsageType, $"{pair.TextEn}-{pair.TextVi}"),
                    UsageType = BilingualListenUsageType,
                    NormalizedText = AudioAltText(pair.TextVi),
                    OriginalText = AudioOriginalText(pair.TextVi),
                    TextEn = pair.TextEn,
                    AudioUrl = string.Empty,
                    AudioUrlEn = string.Empty,
                    Status = "missing",
                    StatusEn = "missing",
                    ReuseCount = 1,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _db.TextToSpeechCaches.Add(entry);
                allCaches.Add(entry);
                addedOrUpdated++;
            }

            entry.UsageType = BilingualListenUsageType;
            entry.NormalizedText = AudioAltText(pair.TextVi);
            entry.OriginalText = AudioOriginalText(pair.TextVi);
            entry.TextEn = pair.TextEn;

            // Dòng cache là riêng cho bài nghe; file trùng khớp tuyệt đối vẫn được tái sử dụng.
            if (!HasVoiceFile(entry.AudioUrl))
            {
                var hasReusableVi = reusableViByText.TryGetValue(pair.TextVi, out var reusableViUrl);
                entry.AudioUrl = hasReusableVi ? reusableViUrl! : string.Empty;
                entry.Status = hasReusableVi ? "ready" : "missing";
                if (hasReusableVi) entry.ReuseCount++;
            }
            if (entry.Status == "ready" && HasVoiceFile(entry.AudioUrl))
                reusableViByText.TryAdd(pair.TextVi, entry.AudioUrl);

            if (!HasVoiceFile(entry.AudioUrlEn))
            {
                var hasReusableEn = reusableEnByText.TryGetValue(pair.TextEn, out var reusableEnUrl);
                entry.AudioUrlEn = hasReusableEn ? reusableEnUrl! : string.Empty;
                entry.StatusEn = hasReusableEn ? "ready" : "missing";
                if (hasReusableEn) entry.ReuseCount++;
            }
            if (entry.StatusEn == "ready" && HasVoiceFile(entry.AudioUrlEn))
                reusableEnByText.TryAdd(pair.TextEn, entry.AudioUrlEn!);

            entry.UpdatedAt = DateTimeOffset.UtcNow;
        }

        return addedOrUpdated;
    }

    private TextToSpeechCacheKey BuildBilingualListenCacheKey(BilingualListenVoicePair pair)
    {
        return BuildTextToSpeechCacheKey($"{BilingualListenUsageType}|{pair.TextEn}|{pair.TextVi}");
    }

    private bool HasVoiceFile(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("/uploads/audio/", StringComparison.Ordinal)) return false;
        var path = Path.Combine(_environment.WebRootPath, "uploads", "audio", Path.GetFileName(url));
        return File.Exists(path) && new FileInfo(path).Length > 0;
    }

    private IReadOnlyList<BilingualListenVoicePair> CollectBilingualListenVoicePairs()
    {
        var pairs = new Dictionary<string, BilingualListenVoicePair>(StringComparer.OrdinalIgnoreCase);

        void AddPair(string? textVi, string? textEn, string usageType = "bilingual-listen")
        {
            var cleanVi = NormalizeSpeechText(textVi ?? string.Empty);
            var cleanEn = NormalizeSpeechText(textEn ?? string.Empty);
            if (!IsSpeakableText(cleanVi) || string.IsNullOrWhiteSpace(cleanEn))
            {
                return;
            }

            pairs.TryAdd($"{cleanEn}\n{cleanVi}", new BilingualListenVoicePair(cleanVi, cleanEn, usageType));
        }

        foreach (var letter in BilingualListenCatalog.Letters)
        {
            AddPair($"Chữ {letter.Symbol}", letter.Symbol);
            AddPair(letter.MeaningVi, letter.Word);
            AddPair(letter.ExampleVi, letter.ExampleEn);
        }

        foreach (var number in BilingualListenCatalog.Numbers)
        {
            AddPair($"Số {number.Number}", number.Number.ToString(System.Globalization.CultureInfo.InvariantCulture));
            AddPair(number.Number.ToString(System.Globalization.CultureInfo.InvariantCulture), $"Number {number.Name}");
            AddPair(number.ExampleVi, number.ExampleEn);
        }

        return pairs.Values.ToList();
    }

    private int DeleteAudioFiles(HashSet<string>? protectedAudioPaths = null)
    {
        var folder = Path.Combine(_environment.WebRootPath, "uploads", "audio");
        if (!Directory.Exists(folder))
        {
            return 0;
        }

        var deleted = 0;
        foreach (var file in Directory.EnumerateFiles(folder))
        {
            try
            {
                var relativePath = $"/uploads/audio/{Path.GetFileName(file)}";
                if (protectedAudioPaths?.Contains(relativePath) == true)
                {
                    continue;
                }

                File.Delete(file);
                deleted += 1;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cannot delete audio file {AudioFile}", file);
            }
        }

        return deleted;
    }

    public async Task EnsureVoiceRowsForLearningItemAsync(LearningItem item, CancellationToken cancellationToken = default)
    {
        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        if (question is null)
        {
            return;
        }

        var payload = ParsePayloadObject(question.PayloadJson);
        await EnsureVoiceEntryAsync(question.PromptText, item.InteractionType == InteractionTypes.Tracing ? "tracing-prompt" : "question", item.Title, cancellationToken);
        await EnsureVoiceEntryAsync(ReadJsonString(question.FeedbackJson, "correct"), "correct-feedback", item.Title, cancellationToken);
        await EnsureVoiceEntryAsync(ReadJsonString(question.FeedbackJson, "retry"), "retry-feedback", item.Title, cancellationToken);

        if (item.InteractionType == InteractionTypes.StoryChoice)
        {
            await EnsureVoiceEntryAsync(ReadJsonString(payload, "speechText"), "content", item.Title, cancellationToken);
        }

        foreach (var label in CollectOptionSpeechLabels(payload))
        {
            await EnsureVoiceEntryAsync(label, "option", item.Title, cancellationToken);
        }
    }

    public async Task<bool> LinkVoiceUrlsForLearningItemAsync(LearningItem item, CancellationToken cancellationToken = default)
    {
        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        if (question is null)
        {
            return false;
        }

        var payload = ParsePayloadObject(question.PayloadJson);

        // Xóa sạch các key cũ thừa nếu còn sót trong payload
        payload.Remove("titleAudioUrl");
        payload.Remove("titleAudioUrlEn");
        payload.Remove("instructionAudioUrl");
        payload.Remove("instructionAudioUrlEn");
        payload.Remove("instructionSpeechText");

        // Phản hồi đúng
        var correctText = ReadJsonString(question.FeedbackJson, "correct");
        if (string.IsNullOrWhiteSpace(correctText)) correctText = "Giỏi lắm";
        payload["correctAudioUrl"] = await ResolveVoiceAudioUrlAsync(correctText, cancellationToken) ?? string.Empty;
        payload["correctAudioUrlEn"] = await ResolveVoiceAudioUrlEnAsync(correctText, cancellationToken) ?? string.Empty;

        // Phản hồi sai / thử lại
        var retryText = ReadJsonString(question.FeedbackJson, "retry");
        if (string.IsNullOrWhiteSpace(retryText)) retryText = "Con thử lại nhé";
        payload["retryAudioUrl"] = await ResolveVoiceAudioUrlAsync(retryText, cancellationToken) ?? string.Empty;
        payload["retryAudioUrlEn"] = await ResolveVoiceAudioUrlEnAsync(retryText, cancellationToken) ?? string.Empty;

        // Câu hỏi / Yêu cầu chính
        var questionUrl = await ResolveVoiceAudioUrlAsync(question.PromptText, cancellationToken) ?? string.Empty;
        var questionUrlEn = await ResolveVoiceAudioUrlEnAsync(question.PromptText, cancellationToken) ?? string.Empty;
        payload["questionAudioUrl"] = questionUrl;
        payload["questionAudioUrlEn"] = questionUrlEn;

        if (item.InteractionType == InteractionTypes.Tracing)
        {
            payload["audioUrl"] = questionUrl;
            payload["audioUrlEn"] = questionUrlEn;
        }
        else if (item.InteractionType == InteractionTypes.ListenAndChoose)
        {
            // Dạng nghe ngắn dùng duy nhất voice của câu hỏi, không có nội dung nghe thứ hai.
            payload["speechText"] = string.Empty;
            payload["speechTextEn"] = string.Empty;
            payload["audioUrl"] = string.Empty;
            payload["audioUrlEn"] = string.Empty;
        }
        else if (item.InteractionType == InteractionTypes.StoryChoice)
        {
            var speechText = ReadJsonString(payload, "speechText");
            var speechUrl = await ResolveVoiceAudioUrlAsync(speechText, cancellationToken);
            var speechUrlEn = await ResolveVoiceAudioUrlEnAsync(speechText, cancellationToken);
            payload["audioUrl"] = !string.IsNullOrWhiteSpace(speechUrl) ? speechUrl : questionUrl;
            payload["audioUrlEn"] = !string.IsNullOrWhiteSpace(speechUrlEn) ? speechUrlEn : questionUrlEn;
        }

        // Bản đồ đáp án (optionAudio cho VI, optionAudioEn cho EN)
        var audioMap = new JsonObject();
        var audioMapEn = new JsonObject();
        foreach (var label in CollectOptionSpeechLabels(payload))
        {
            var cleanLabel = Clean(label);
            var urlVi = await ResolveVoiceAudioUrlAsync(label, cancellationToken) ?? string.Empty;
            var urlEn = await ResolveVoiceAudioUrlEnAsync(label, cancellationToken) ?? string.Empty;

            audioMap[cleanLabel] = urlVi;
            audioMapEn[cleanLabel] = urlEn;
            if (!string.Equals(cleanLabel, label, StringComparison.Ordinal))
            {
                audioMap[label] = urlVi;
                audioMapEn[label] = urlEn;
            }
        }
        payload["optionAudio"] = audioMap;
        payload["optionAudioEn"] = audioMapEn;

        var payloadJson = payload.ToJsonString();
        if (string.Equals(question.PayloadJson, payloadJson, StringComparison.Ordinal) &&
            string.Equals(item.ContentJson, payloadJson, StringComparison.Ordinal))
        {
            return false;
        }

        item.ContentJson = payloadJson;
        question.PayloadJson = payloadJson;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        return true;
    }

    public async Task EnsureStandardCorrectFeedbackVoiceAsync(CancellationToken cancellationToken = default)
    {
        const string textVi = "Giỏi lắm";
        const string textEn = "Great job!";

        var entry = await _db.TextToSpeechCaches
            .FirstOrDefaultAsync(x => x.UsageType != BilingualListenUsageType &&
                (x.NormalizedText == textVi || x.OriginalText == textVi), cancellationToken)
            ?? await EnsureVoiceEntryAsync(textVi, "correct-feedback", cancellationToken: cancellationToken);
        if (entry is null) return;

        entry.TextEn = textEn;
        if (!HasVoiceFile(entry.AudioUrl))
        {
            try
            {
                entry.AudioUrl = await GenerateVoiceCacheFileAsync(entry, cancellationToken);
                entry.Status = "ready";
                entry.LastError = null;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                entry.Status = "missing";
                entry.LastError = CompactErrorMessage(ex.Message, 1000);
                _logger.LogWarning(ex, "Không thể tạo voice VI cho phản hồi chuẩn '{Feedback}'.", textVi);
            }
        }

        if (!HasVoiceFile(entry.AudioUrlEn))
        {
            try
            {
                entry.AudioUrlEn = await GenerateVoiceCacheFileEnAsync(entry, cancellationToken);
                entry.StatusEn = "ready";
                entry.LastErrorEn = null;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                entry.StatusEn = "missing";
                entry.LastErrorEn = CompactErrorMessage(ex.Message, 1000);
                _logger.LogWarning(ex, "Không thể tạo voice EN cho phản hồi chuẩn '{Feedback}'.", textVi);
            }
        }

        entry.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        InvalidateVoiceLookupCache();

        var lessons = await _db.LearningItems
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .Where(x => x.Questions.Any(q => q.FeedbackJson.Contains(textVi)))
            .ToListAsync(cancellationToken);
        foreach (var lesson in lessons)
        {
            var lessonChanged = false;
            var firstQuestion = lesson.Questions.OrderBy(q => q.SortOrder).FirstOrDefault();
            foreach (var question in lesson.Questions.Where(q => q.FeedbackJson.Contains(textVi)))
            {
                var payload = ParsePayloadObject(question.PayloadJson);
                payload["correctSpeechText"] = textVi;
                payload["correctAudioUrl"] = HasVoiceFile(entry.AudioUrl) ? entry.AudioUrl : string.Empty;
                payload["correctAudioUrlEn"] = HasVoiceFile(entry.AudioUrlEn) ? entry.AudioUrlEn : string.Empty;
                var payloadJson = payload.ToJsonString();
                if (!string.Equals(question.PayloadJson, payloadJson, StringComparison.Ordinal))
                {
                    question.PayloadJson = payloadJson;
                    lessonChanged = true;
                }

                if (question == firstQuestion && !string.Equals(lesson.ContentJson, payloadJson, StringComparison.Ordinal))
                {
                    lesson.ContentJson = payloadJson;
                    lessonChanged = true;
                }
            }
            if (lessonChanged) lesson.UpdatedAt = DateTimeOffset.UtcNow;
        }

        if (lessons.Count > 0) await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<TextToSpeechCache?> EnsureVoiceEntryAsync(string? text, string usageType, string? lessonTitle = null, CancellationToken cancellationToken = default)
    {
        if (!IsSpeakableText(text))
        {
            return null;
        }

        var normalizedText = NormalizeSpeechText(text ?? string.Empty);
        if (string.IsNullOrWhiteSpace(normalizedText) || !IsSpeakableText(normalizedText))
        {
            return null;
        }

        var key = BuildTextToSpeechCacheKey(normalizedText);
        var cleanLower = normalizedText.ToLowerInvariant();

        // 1. Kiểm tra trong ChangeTracker trước để tránh trùng lặp trong cùng 1 request
        var tracked = _db.ChangeTracker.Entries<TextToSpeechCache>()
            .Select(x => x.Entity)
            .Where(x => x.UsageType != BilingualListenUsageType)
            .FirstOrDefault(x => x.TextHash == key.TextHash ||
                                 x.NormalizedText.ToLower() == cleanLower ||
                                 x.OriginalText.ToLower() == cleanLower);
        if (tracked is not null)
        {
            tracked.ReuseCount += 1;
            tracked.UpdatedAt = DateTimeOffset.UtcNow;
            return tracked;
        }

        // 2. Kiểm tra trong Database
        var existing = await _db.TextToSpeechCaches.Where(x => x.UsageType != BilingualListenUsageType).FirstOrDefaultAsync(x =>
            x.TextHash == key.TextHash ||
            x.NormalizedText.ToLower() == cleanLower ||
            x.OriginalText.ToLower() == cleanLower,
            cancellationToken);
        if (existing is not null)
        {
            existing.ReuseCount += 1;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            return existing;
        }

        var voiceEn = _configuration["VoiceLibrary:VoiceEn"]?.Trim();
        if (string.IsNullOrWhiteSpace(voiceEn)) voiceEn = "en-US-JennyNeural";
        var voiceVi = _configuration["VoiceLibrary:Voice"]?.Trim();
        if (string.IsNullOrWhiteSpace(voiceVi)) voiceVi = "vi-VN-HoaiMyNeural";

        var entry = new TextToSpeechCache
        {
            Id = Guid.NewGuid(),
            Provider = "edge",
            Voice = voiceVi,
            VoiceEn = voiceEn,
            ModelId = "neural",
            Format = "mp3",
            TextHash = key.TextHash,
            Name = BuildVoiceName(usageType, normalizedText),
            UsageType = usageType,
            NormalizedText = AudioAltText(normalizedText),
            OriginalText = AudioOriginalText(text ?? normalizedText),
            TextEn = string.Empty,
            AudioUrl = string.Empty,
            AudioUrlEn = string.Empty,
            Status = "missing",
            StatusEn = "missing",
            ReuseCount = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _db.TextToSpeechCaches.Add(entry);
        return entry;
    }

    public async Task<string?> ResolveBilingualListenAudioUrlAsync(
        string? text,
        string lang,
        CancellationToken cancellationToken = default)
    {
        var normalizedText = NormalizeSpeechText(text ?? string.Empty);
        if (string.IsNullOrWhiteSpace(normalizedText)) return null;

        var isEnglish = lang.StartsWith("en", StringComparison.OrdinalIgnoreCase);
        var entry = isEnglish
            ? await _db.TextToSpeechCaches
                .Where(x => x.UsageType == BilingualListenUsageType &&
                            x.StatusEn == "ready" &&
                            !string.IsNullOrEmpty(x.AudioUrlEn) &&
                            x.TextEn == normalizedText)
                .OrderByDescending(x => x.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken)
            : await _db.TextToSpeechCaches
                .Where(x => x.UsageType == BilingualListenUsageType &&
                            x.Status == "ready" &&
                            !string.IsNullOrEmpty(x.AudioUrl) &&
                            (x.NormalizedText == normalizedText || x.OriginalText == normalizedText))
                .OrderByDescending(x => x.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

        var audioUrl = isEnglish ? entry?.AudioUrlEn : entry?.AudioUrl;
        return HasVoiceFile(audioUrl) ? audioUrl : null;
    }

    public async Task<string?> ResolveVoiceAudioUrlAsync(string? text, CancellationToken cancellationToken = default)
    {
        var rawText = (text ?? string.Empty).Trim();
        var normalizedText = NormalizeSpeechText(rawText);
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return null;
        }

        var generalCaches = await GetCachedGeneralVoicesAsync(cancellationToken);

        // Bóc tách chữ/số để xác định truy vấn riêng biệt
        string bareLetter = normalizedText.ToLowerInvariant();
        if (bareLetter.StartsWith("chữ ")) bareLetter = bareLetter.Substring(4).Trim();
        else if (bareLetter.StartsWith("số ")) bareLetter = bareLetter.Substring(3).Trim();
        bool isShortCharOrNum = bareLetter.Length <= 3;

        TextToSpeechCache? entry = null;

        // 0. Nếu truy vấn cho chữ cái hoặc chữ số riêng biệt: ƯU TIÊN tìm mục "option", "game-voice" của chữ/số đó
        // TUYỆT ĐỐI không lấy nhầm câu hỏi (question) hay bài tô nét (tracing-prompt)
        if (isShortCharOrNum)
        {
            var letterVariant = $"chữ {bareLetter}";
            var numberVariant = $"số {bareLetter}";

            // Ưu tiên 1: Mục option hoặc game-voice khớp chữ/số
            entry = generalCaches.FirstOrDefault(x =>
                x.Status == "ready" &&
                !string.IsNullOrEmpty(x.AudioUrl) &&
                (x.UsageType == "option" || x.UsageType == "game-voice" || x.UsageType == "content") &&
                (string.Equals(x.NormalizedText, bareLetter, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(x.OriginalText, bareLetter, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(x.NormalizedText, letterVariant, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(x.OriginalText, letterVariant, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(x.NormalizedText, numberVariant, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(x.OriginalText, numberVariant, StringComparison.OrdinalIgnoreCase)));

            // Ưu tiên 2: Bất kỳ mục nào khớp chữ cái/số nhưng KHÔNG PHẢI question/tracing-prompt
            if (entry is null)
            {
                entry = generalCaches.FirstOrDefault(x =>
                    x.Status == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrl) &&
                    x.UsageType != "question" && x.UsageType != "tracing-prompt" &&
                    (string.Equals(x.NormalizedText, bareLetter, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(x.OriginalText, bareLetter, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(x.NormalizedText, letterVariant, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(x.OriginalText, letterVariant, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(x.NormalizedText, numberVariant, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(x.OriginalText, numberVariant, StringComparison.OrdinalIgnoreCase)));
            }

            // Với chữ/số ngắn: Nếu đã tìm trong cache chữ/số mà không có, dừng ngay tại đây
            // để GamesController tự sinh voice chuẩn HoaiMy đọc chữ/số, TUYỆT ĐỐI không rơi vào câu hỏi!
            if (entry is { Status: "ready", AudioUrl.Length: > 0 })
            {
                return entry.AudioUrl;
            }
            return null;
        }

        // 1. Khớp chính xác theo TextHash và chuỗi chuẩn hóa (chỉ dành cho câu từ dài)
        if (entry is null)
        {
            var key = BuildTextToSpeechCacheKey(normalizedText);
            entry = generalCaches.FirstOrDefault(x =>
                (x.Provider == key.Provider &&
                 x.Voice == key.Voice &&
                 x.ModelId == key.ModelId &&
                 x.Format == key.Format &&
                 x.TextHash == key.TextHash &&
                 x.Status == "ready" &&
                 !string.IsNullOrEmpty(x.AudioUrl)) ||
                (x.Status == "ready" &&
                 !string.IsNullOrEmpty(x.AudioUrl) &&
                 (x.NormalizedText == normalizedText || x.OriginalText == rawText || x.NormalizedText == rawText)));
        }

        // 2. Cắt bỏ dấu câu thừa (?, ., !, :, ;)
        if (entry is null)
        {
            var stripped = normalizedText.TrimEnd('?', '.', '!', ':', ';', ' ');
            entry = generalCaches.FirstOrDefault(x =>
                x.Status == "ready" &&
                !string.IsNullOrEmpty(x.AudioUrl) &&
                (x.NormalizedText == stripped || x.OriginalText == stripped));
        }

        // 3. Khớp phản hồi chuẩn sư phạm
        if (entry is null)
        {
            var lower = normalizedText.ToLowerInvariant();
            if (lower.Contains("giỏi") || lower.Contains("đúng rồi") || lower.Contains("xuất sắc"))
            {
                entry = generalCaches.FirstOrDefault(x =>
                    x.Status == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrl) &&
                    (x.UsageType == "correct-feedback" || x.NormalizedText.Contains("Giỏi lắm") || x.OriginalText.Contains("Giỏi lắm")));
            }
            else if (lower.Contains("thử lại") || lower.Contains("chưa đúng") || lower.Contains("cố lên"))
            {
                entry = generalCaches.FirstOrDefault(x =>
                    x.Status == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrl) &&
                    (x.UsageType == "retry-feedback" || x.NormalizedText.Contains("thử lại") || x.OriginalText.Contains("thử lại")));
            }
        }

        // 4. Khớp theo slug/tên bài học trong TextToSpeechCaches (chỉ cho câu dài)
        if (entry is null)
        {
            var slug = NormalizeCode(normalizedText);
            if (!string.IsNullOrWhiteSpace(slug) && slug.Length >= 3)
            {
                entry = generalCaches.FirstOrDefault(x =>
                    x.Status == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrl) &&
                    (x.Name.Contains(slug) || x.NormalizedText.Contains(normalizedText) || x.OriginalText.Contains(normalizedText)));
            }
        }

        if (entry is { Status: "ready", AudioUrl.Length: > 0 })
        {
            return entry.AudioUrl;
        }

        return null;
    }

    public async Task<string?> ResolveVoiceAudioUrlEnAsync(string? text, CancellationToken cancellationToken = default)
    {
        var rawText = (text ?? string.Empty).Trim();
        var normalizedText = NormalizeSpeechText(rawText);
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return null;
        }

        var generalCaches = await GetCachedGeneralVoicesAsync(cancellationToken);

        // 1. Khớp chính xác theo TextHash và chuỗi chuẩn hóa
        var key = BuildTextToSpeechCacheKey(normalizedText);
        var entry = generalCaches.FirstOrDefault(x =>
            (x.Provider == key.Provider &&
             x.Voice == key.Voice &&
             x.ModelId == key.ModelId &&
             x.Format == key.Format &&
             x.TextHash == key.TextHash &&
             x.StatusEn == "ready" &&
             !string.IsNullOrEmpty(x.AudioUrlEn)) ||
            (x.StatusEn == "ready" &&
             !string.IsNullOrEmpty(x.AudioUrlEn) &&
             (x.NormalizedText == normalizedText || x.OriginalText == rawText || x.NormalizedText == rawText || x.TextEn == rawText || x.TextEn == normalizedText ||
              (x.TextEn != null && (x.TextEn == $"Number {rawText}" || x.TextEn == $"Letter {rawText.ToUpperInvariant()}")))));

        // 2. Cắt bỏ dấu câu thừa (?, ., !, :, ;)
        if (entry is null)
        {
            var stripped = normalizedText.TrimEnd('?', '.', '!', ':', ';', ' ');
            entry = generalCaches.FirstOrDefault(x =>
                x.StatusEn == "ready" &&
                !string.IsNullOrEmpty(x.AudioUrlEn) &&
                (x.NormalizedText == stripped || x.OriginalText == stripped || x.TextEn == stripped ||
                 (x.TextEn != null && (x.TextEn == $"Number {stripped}" || x.TextEn == $"Letter {stripped.ToUpperInvariant()}"))));
        }

        // 3. Khớp chữ cái / chữ số Tiếng Anh (Ví dụ: "A", "1")
        if (entry is null && normalizedText.Length <= 3)
        {
            var lower = normalizedText.ToLowerInvariant();
            var numVariant = $"number {lower}";
            var letterVariant = $"letter {lower}";
            entry = generalCaches.FirstOrDefault(x =>
                x.StatusEn == "ready" &&
                !string.IsNullOrEmpty(x.AudioUrlEn) &&
                ((x.TextEn != null && (x.TextEn.ToLower() == lower || x.TextEn.ToLower() == numVariant || x.TextEn.ToLower() == letterVariant)) ||
                 x.NormalizedText.ToLower() == lower ||
                 x.OriginalText.ToLower() == lower));
        }

        // 4. Khớp phản hồi chuẩn sư phạm tiếng Anh
        if (entry is null)
        {
            var lower = normalizedText.ToLowerInvariant();
            if (lower.Contains("giỏi") || lower.Contains("đúng rồi") || lower.Contains("great") || lower.Contains("correct"))
            {
                entry = generalCaches.FirstOrDefault(x =>
                    x.StatusEn == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrlEn) &&
                    (x.UsageType == "correct-feedback" || (x.TextEn != null && x.TextEn.Contains("Great"))));
            }
            else if (lower.Contains("thử lại") || lower.Contains("chưa đúng") || lower.Contains("try again"))
            {
                entry = generalCaches.FirstOrDefault(x =>
                    x.StatusEn == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrlEn) &&
                    (x.UsageType == "retry-feedback" || (x.TextEn != null && x.TextEn.Contains("Try again"))));
            }
        }

        // 5. Khớp theo slug/tên bài học trong TextToSpeechCaches
        if (entry is null)
        {
            var slug = NormalizeCode(normalizedText);
            if (!string.IsNullOrWhiteSpace(slug) && slug.Length >= 3)
            {
                entry = generalCaches.FirstOrDefault(x =>
                    x.StatusEn == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrlEn) &&
                    (x.Name.Contains(slug) || (x.TextEn != null && x.TextEn.Contains(normalizedText))));
            }
        }

        if (entry is { StatusEn: "ready", AudioUrlEn.Length: > 0 })
        {
            return entry.AudioUrlEn;
        }

        return null;
    }

    public async Task<string> GenerateVoiceCacheFileAsync(TextToSpeechCache entry, CancellationToken cancellationToken = default)
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

        var storedName = BuildDeterministicVoiceFileName("voice", entry.Name, text, voice, rate);
        var diskPath = Path.Combine(folder, storedName);
        var storagePath = $"/uploads/audio/{storedName}";
        if (File.Exists(diskPath) && new FileInfo(diskPath).Length > 0)
        {
            await EnsureMediaAssetForAudioAsync(storagePath, storedName, AudioCacheKey(entry.NormalizedText), cancellationToken);
            return storagePath;
        }

        try
        {
            await RunEdgeTextToSpeechAsync(text, voice, rate, diskPath, cancellationToken);
        }
        catch
        {
            if (File.Exists(diskPath))
            {
                File.Delete(diskPath);
            }

            throw;
        }
        await EnsureMediaAssetForAudioAsync(storagePath, storedName, AudioCacheKey(entry.NormalizedText), cancellationToken);
        return storagePath;
    }

    public async Task<string> GenerateVoiceCacheFileEnAsync(TextToSpeechCache entry, CancellationToken cancellationToken = default)
    {
        var textEn = entry.TextEn;
        if (string.IsNullOrWhiteSpace(textEn))
        {
            textEn = await PreschoolTranslationHelper.TranslateToEnglishAsync(string.IsNullOrWhiteSpace(entry.OriginalText) ? entry.NormalizedText : entry.OriginalText);
            entry.TextEn = textEn;
        }

        if (string.IsNullOrWhiteSpace(textEn))
        {
            throw new InvalidOperationException("Voice không có nội dung tiếng Anh để tạo file.");
        }

        var folder = Path.Combine(_environment.WebRootPath, "uploads", "audio");
        Directory.CreateDirectory(folder);
        var voiceEn = _configuration["VoiceLibrary:VoiceEn"]?.Trim();
        if (string.IsNullOrWhiteSpace(voiceEn))
        {
            voiceEn = "en-US-JennyNeural";
        }
        var rateEn = _configuration["VoiceLibrary:RateEn"]?.Trim();
        if (string.IsNullOrWhiteSpace(rateEn))
        {
            rateEn = "-18%";
        }

        var storedName = BuildDeterministicVoiceFileName("voice-en", entry.Name, textEn, voiceEn, rateEn);
        var diskPath = Path.Combine(folder, storedName);
        var storagePath = $"/uploads/audio/{storedName}";
        if (File.Exists(diskPath) && new FileInfo(diskPath).Length > 0)
        {
            await EnsureMediaAssetForAudioAsync(storagePath, storedName, AudioCacheKey($"en:{entry.NormalizedText}"), cancellationToken);
            return storagePath;
        }

        try
        {
            await RunEdgeTextToSpeechAsync(textEn, voiceEn, rateEn, diskPath, cancellationToken);
        }
        catch
        {
            if (File.Exists(diskPath))
            {
                File.Delete(diskPath);
            }

            throw;
        }

        await EnsureMediaAssetForAudioAsync(storagePath, storedName, AudioCacheKey($"en:{entry.NormalizedText}"), cancellationToken);
        return storagePath;
    }

    public async Task<string?> EnsureAudioFileAsync(string text, string lang = "vi", string? customRate = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var cleanText = text.Trim();
        var isEn = lang.StartsWith("en", StringComparison.OrdinalIgnoreCase);

        // Runtime callers may only reuse the central cache. Voice generation is handled by admin sync/rebuild flows.
        try
        {
            if (!isEn)
            {
                var cached = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                    x.Status == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrl) &&
                    (x.OriginalText == cleanText || x.NormalizedText == cleanText), cancellationToken);
                if (cached != null && !string.IsNullOrEmpty(cached.AudioUrl))
                {
                    var localCheck = Path.Combine(_environment.WebRootPath, cached.AudioUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(localCheck) && new FileInfo(localCheck).Length > 0)
                    {
                        return cached.AudioUrl;
                    }
                }
            }
            else
            {
                var lower = cleanText.ToLowerInvariant();
                var numVariant = $"number {lower}";
                var letterVariant = $"letter {lower}";
                var cached = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                    x.StatusEn == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrlEn) &&
                    (x.TextEn == cleanText || x.OriginalText == cleanText || x.NormalizedText == cleanText ||
                     (x.TextEn != null && (x.TextEn.ToLower() == numVariant || x.TextEn.ToLower() == letterVariant))), cancellationToken);
                if (cached != null && !string.IsNullOrEmpty(cached.AudioUrlEn))
                {
                    var localCheck = Path.Combine(_environment.WebRootPath, cached.AudioUrlEn.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(localCheck) && new FileInfo(localCheck).Length > 0)
                    {
                        return cached.AudioUrlEn;
                    }
                }
            }
        }
        catch
        {
            // Bỏ qua nếu db đang bận
        }

        return null;
    }

    public async Task PreGenerateBilingualAudioAsync(CancellationToken cancellationToken = default)
    {
        await EnsureBilingualListenVoiceRowsAsync(cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var entries = await _db.TextToSpeechCaches
            .Where(x => x.UsageType == "bilingual-listen" &&
                        (string.IsNullOrWhiteSpace(x.AudioUrl) || x.Status != "ready" ||
                         string.IsNullOrWhiteSpace(x.AudioUrlEn) || x.StatusEn != "ready"))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(entry.AudioUrl) || entry.Status != "ready")
            {
                entry.AudioUrl = await GenerateVoiceCacheFileAsync(entry, cancellationToken);
                entry.Status = "ready";
                entry.LastError = null;
            }

            if (!string.IsNullOrWhiteSpace(entry.TextEn) &&
                (string.IsNullOrWhiteSpace(entry.AudioUrlEn) || entry.StatusEn != "ready"))
            {
                entry.AudioUrlEn = await GenerateVoiceCacheFileEnAsync(entry, cancellationToken);
                entry.StatusEn = "ready";
                entry.LastErrorEn = null;
            }

            entry.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<(int Created, int Failed, int UpdatedItems)> GenerateMissingAndRelinkAsync(int maxItems = 0, CancellationToken cancellationToken = default)
    {
        // Luồng sinh bổ sung không dọn/xóa kho voice hiện có. Trước tiên chỉ tạo các dòng
        // cache còn thiếu và liên kết lại những file đã sẵn sàng.
        var initialRelink = await EnsureVoiceRowsAndRelinkAsync(cancellationToken);

        if (maxItems > 0)
        {
            var limitedResult = await SyncAndGenerateBatchAsync(maxItems, cancellationToken, initialize: false);
            return (
                limitedResult.CreatedVi + limitedResult.CreatedEn,
                limitedResult.Failed,
                initialRelink.LearningItemsUpdated + limitedResult.UpdatedItems);
        }

        var created = 0;
        var failed = 0;
        var updatedItems = initialRelink.LearningItemsUpdated;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var batchResult = await SyncAndGenerateBatchAsync(
                batchSize: 10,
                cancellationToken,
                initialize: false);

            created += batchResult.CreatedVi + batchResult.CreatedEn;
            failed += batchResult.Failed;
            updatedItems += batchResult.UpdatedItems;

            if (batchResult.IsCompleted)
            {
                break;
            }

            var madeProgress = batchResult.CreatedVi > 0 ||
                               batchResult.CreatedEn > 0 ||
                               batchResult.UpdatedItems > 0;
            if (!madeProgress)
            {
                _logger.LogWarning(
                    "Dừng đồng bộ Voice vì không thể tạo thêm file. Còn thiếu VI={MissingVi}, EN={MissingEn}, tổng phần việc={RemainingMissing}.",
                    batchResult.MissingVi,
                    batchResult.MissingEn,
                    batchResult.RemainingMissing);
                break;
            }
        }

        // Luôn liên kết lại các voice đã sẵn sàng, kể cả khi nhà cung cấp TTS tạm thời lỗi.
        var relinkResult = await EnsureVoiceRowsAndRelinkAsync(cancellationToken);
        updatedItems += relinkResult.LearningItemsUpdated;

        return (created, failed, updatedItems);
    }

    private static async Task RunEdgeTextToSpeechAsync(string text, string voice, string rate, string outputPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Văn bản rỗng, không thể tạo file âm thanh.");
        }

        var cleanText = text.Trim()
            .Replace("“", "\"")
            .Replace("”", "\"")
            .Replace("‘", "'")
            .Replace("’", "'")
            .Replace("\r", " ")
            .Replace("\n", " ");

        // 1. Thử tạo âm thanh trực tiếp qua WebSocket của Edge TTS
        try
        {
            if (await SynthesizeViaDirectEdgeWebSocketAsync(cleanText, voice, rate, outputPath, cancellationToken))
            {
                if (File.Exists(outputPath) && new FileInfo(outputPath).Length > 0)
                {
                    return;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Fallback sang python subprocess
        }

        var voiceCandidates = new List<string> { voice };
        if (voice.StartsWith("en-", StringComparison.OrdinalIgnoreCase))
        {
            if (!voice.Equals("en-US-JennyNeural", StringComparison.OrdinalIgnoreCase)) voiceCandidates.Add("en-US-JennyNeural");
            if (!voice.Equals("en-US-AriaNeural", StringComparison.OrdinalIgnoreCase)) voiceCandidates.Add("en-US-AriaNeural");
        }
        else if (voice.StartsWith("vi-", StringComparison.OrdinalIgnoreCase))
        {
            if (!voice.Equals("vi-VN-HoaiMyNeural", StringComparison.OrdinalIgnoreCase)) voiceCandidates.Add("vi-VN-HoaiMyNeural");
            if (!voice.Equals("vi-VN-NamMinhNeural", StringComparison.OrdinalIgnoreCase)) voiceCandidates.Add("vi-VN-NamMinhNeural");
        }

        var candidates = new[]
        {
            ("python", new[] { "-m", "edge_tts" }),
            ("py", new[] { "-m", "edge_tts" })
        };

        var errors = new List<string>();

        foreach (var currentVoice in voiceCandidates)
        {
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
                startInfo.ArgumentList.Add(currentVoice);
                startInfo.ArgumentList.Add("--rate");
                startInfo.ArgumentList.Add(rate);
                startInfo.ArgumentList.Add("--text");
                startInfo.ArgumentList.Add(cleanText);
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

                    var outputTask = ReadProcessTailAsync(process.StandardOutput);
                    var errorTask = ReadProcessTailAsync(process.StandardError);
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeout.CancelAfter(TimeSpan.FromSeconds(45));
                    try
                    {
                        await process.WaitForExitAsync(timeout.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        if (!process.HasExited) process.Kill(entireProcessTree: true);
                        await process.WaitForExitAsync(CancellationToken.None);
                        await Task.WhenAll(outputTask, errorTask);
                        cancellationToken.ThrowIfCancellationRequested();
                        if (File.Exists(outputPath))
                        {
                            try { File.Delete(outputPath); } catch { }
                        }
                        errors.Add($"{fileName}: quá thời gian tạo voice.");
                        continue;
                    }

                    var stdout = await outputTask;
                    var stderr = await errorTask;
                    if (process.ExitCode == 0 && File.Exists(outputPath) && new FileInfo(outputPath).Length > 0)
                    {
                        return;
                    }

                    if (File.Exists(outputPath))
                    {
                        try { File.Delete(outputPath); } catch { }
                    }

                    var err = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
                    errors.Add($"{fileName} ({currentVoice}): {CompactErrorMessage(err, 500)}");
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    if (File.Exists(outputPath))
                    {
                        try { File.Delete(outputPath); } catch { }
                    }
                    errors.Add($"{fileName} ({currentVoice}): {CompactErrorMessage(ex.Message, 500)}");
                }
            }
        }

        // 3. Fallback: Google TTS nếu cả Edge WebSocket và python subprocess đều không tạo được audio
        var lang = voice.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "vi";
        if (await SynthesizeViaGoogleTtsFallbackAsync(cleanText, lang, outputPath, cancellationToken))
        {
            if (File.Exists(outputPath) && new FileInfo(outputPath).Length > 0)
            {
                return;
            }
        }

        throw new InvalidOperationException(CompactErrorMessage(
            string.Join(" | ", errors.Where(x => !string.IsNullOrWhiteSpace(x))),
            2000));
    }

    private static async Task<bool> SynthesizeViaGoogleTtsFallbackAsync(string text, string lang, string outputPath, CancellationToken cancellationToken)
    {
        try
        {
            var shortLang = lang.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "vi";
            var cleanText = text.Length > 200 ? text[..200] : text;
            var url = $"https://translate.google.com/translate_tts?ie=UTF-8&tl={shortLang}&client=tw-ob&q={Uri.EscapeDataString(cleanText)}";
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
            var bytes = await httpClient.GetByteArrayAsync(url, cancellationToken);
            if (bytes is { Length: > 0 })
            {
                var dir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
                await File.WriteAllBytesAsync(outputPath, bytes, cancellationToken);
                return true;
            }
        }
        catch
        {
        }
        return false;
    }

    private static string CompactErrorMessage(string? message, int maxLength = 350)
    {
        if (string.IsNullOrWhiteSpace(message)) return "Không xác định được nguyên nhân.";
        var compact = string.Join(' ', message
            .Split(new[] { '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return compact.Length <= maxLength ? compact : "…" + compact[^(maxLength - 1)..];
    }

    private static async Task<string> ReadProcessTailAsync(StreamReader reader)
    {
        var tail = new StringBuilder();
        var buffer = new char[1024];
        int count;
        while ((count = await reader.ReadAsync(buffer.AsMemory())) > 0)
        {
            tail.Append(buffer, 0, count);
            if (tail.Length > 4000) tail.Remove(0, tail.Length - 4000);
        }
        return tail.ToString();
    }

    private const string EdgeTrustedClientToken = "6A5AA1D4EAFF4E9FB37E23D68491D6F4";
    private const long WinEpochSeconds = 11644473600L;

    private static string GenerateSecMsGec()
    {
        var nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var winSeconds = nowSeconds + WinEpochSeconds;
        winSeconds -= (winSeconds % 300);
        var ticks = winSeconds * 10_000_000L;
        var toHash = $"{ticks}{EdgeTrustedClientToken}";
        return Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(toHash)));
    }

    private static async Task<bool> SynthesizeViaDirectEdgeWebSocketAsync(string text, string voice, string rate, string outputPath, CancellationToken cancellationToken)
    {
        var voiceCandidates = new List<string> { voice };
        if (voice.StartsWith("en-", StringComparison.OrdinalIgnoreCase))
        {
            if (!voice.Equals("en-US-JennyNeural", StringComparison.OrdinalIgnoreCase)) voiceCandidates.Add("en-US-JennyNeural");
            if (!voice.Equals("en-US-AriaNeural", StringComparison.OrdinalIgnoreCase)) voiceCandidates.Add("en-US-AriaNeural");
        }
        else if (voice.StartsWith("vi-", StringComparison.OrdinalIgnoreCase))
        {
            if (!voice.Equals("vi-VN-HoaiMyNeural", StringComparison.OrdinalIgnoreCase)) voiceCandidates.Add("vi-VN-HoaiMyNeural");
            if (!voice.Equals("vi-VN-NamMinhNeural", StringComparison.OrdinalIgnoreCase)) voiceCandidates.Add("vi-VN-NamMinhNeural");
        }

        foreach (var currentVoice in voiceCandidates)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(25));
                using var ws = new ClientWebSocket();
                ws.Options.SetRequestHeader("Origin", "chrome-extension://jdiccldimpdaibmpdkjnbmckianbfold");
                ws.Options.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36 Edg/130.0.0.0");
                ws.Options.SetRequestHeader("Pragma", "no-cache");
                ws.Options.SetRequestHeader("Cache-Control", "no-cache");

                var connectionId = Guid.NewGuid().ToString("N");
                var secMsGec = GenerateSecMsGec();
                var uri = new Uri($"wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1?TrustedClientToken={EdgeTrustedClientToken}&Sec-MS-GEC={secMsGec}&Sec-MS-GEC-Version=1-130.0.2849.68&ConnectionId={connectionId}");
                await ws.ConnectAsync(uri, cts.Token);

                var configPayload = "Content-Type:application/json; charset=utf-8\r\nPath:speech.config\r\n\r\n{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{\"sentenceBoundaryEnabled\":\"false\",\"wordBoundaryEnabled\":\"false\"},\"outputFormat\":\"audio-24khz-48kbitrate-mono-mp3\"}}}}";
                var configBytes = Encoding.UTF8.GetBytes(configPayload);
                await ws.SendAsync(new ArraySegment<byte>(configBytes), WebSocketMessageType.Text, true, cts.Token);

                var requestId = Guid.NewGuid().ToString("N");
                var lang = currentVoice.StartsWith("vi-", StringComparison.OrdinalIgnoreCase) ? "vi-VN" : "en-US";
                var escapedText = SecurityElement.Escape(text);
                var ssml = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='{lang}'><voice name='{currentVoice}'><prosody pitch='+0Hz' rate='{rate}' volume='+0%'>{escapedText}</prosody></voice></speak>";
                var ssmlPayload = $"X-RequestId:{requestId}\r\nContent-Type:application/ssml+xml\r\nX-Timestamp:{DateTime.UtcNow:o}\r\nPath:ssml\r\n\r\n{ssml}";
                var ssmlBytes = Encoding.UTF8.GetBytes(ssmlPayload);
                await ws.SendAsync(new ArraySegment<byte>(ssmlBytes), WebSocketMessageType.Text, true, cts.Token);

                using var audioMs = new MemoryStream();
                var buffer = new byte[16384];
                var inBinaryAudio = false;

                while (ws.State == WebSocketState.Open && !cts.Token.IsCancellationRequested)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var textMsg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        if (textMsg.Contains("Path:turn.end", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        if (!inBinaryAudio)
                        {
                            if (result.Count > 2)
                            {
                                var headerLength = (buffer[0] << 8) | buffer[1];
                                var headerBytes = 2 + headerLength;
                                if (result.Count > headerBytes)
                                {
                                    audioMs.Write(buffer, headerBytes, result.Count - headerBytes);
                                }
                            }
                        }
                        else
                        {
                            audioMs.Write(buffer, 0, result.Count);
                        }
                        inBinaryAudio = !result.EndOfMessage;
                    }
                }

                if (audioMs.Length > 0)
                {
                    var dir = Path.GetDirectoryName(outputPath);
                    if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
                    await File.WriteAllBytesAsync(outputPath, audioMs.ToArray(), cancellationToken);
                    return true;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                // Thử ứng viên giọng tiếp theo
            }
        }

        return false;
    }

    private TextToSpeechCacheKey BuildTextToSpeechCacheKey(string normalizedText)
    {
        var cleanText = normalizedText.Trim().ToLowerInvariant();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(cleanText))).ToLowerInvariant();
        return new TextToSpeechCacheKey("edge", "vi-VN-HoaiMyNeural", "neural", "mp3", hash);
    }

    private static IEnumerable<string> CollectOptionSpeechLabels(JsonObject payload)
    {
        foreach (var value in ReadJsonStringArray(payload, "choices")) yield return value;
        foreach (var value in ReadJsonStringArray(payload, "items")) yield return value;
        foreach (var value in ReadJsonStringArray(payload, "categories")) yield return value;
        foreach (var value in ReadJsonMappingLabels(payload, "pairs")) yield return value;
        foreach (var value in ReadJsonMappingLabels(payload, "mappings")) yield return value;

        var targetLabel = ReadJsonString(payload, "targetLabel");
        if (!string.IsNullOrWhiteSpace(targetLabel)) yield return targetLabel;

        var leftLabel = ReadJsonString(payload, "leftLabel");
        if (!string.IsNullOrWhiteSpace(leftLabel)) yield return leftLabel;

        var rightLabel = ReadJsonString(payload, "rightLabel");
        if (!string.IsNullOrWhiteSpace(rightLabel)) yield return rightLabel;
    }

    private static bool IsSpeakableText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var trimmed = text.Trim();
        if (trimmed.Length == 0) return false;

        // Bỏ qua đường dẫn file ảnh, video, âm thanh
        if (trimmed.StartsWith("/") || trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var lower = trimmed.ToLowerInvariant();
        if (lower.EndsWith(".png") || lower.EndsWith(".jpg") || lower.EndsWith(".jpeg") || lower.EndsWith(".gif") || lower.EndsWith(".webp") || lower.EndsWith(".svg") || lower.EndsWith(".mp3") || lower.EndsWith(".wav") || lower.EndsWith(".m4a"))
        {
            return false;
        }

        // Bỏ qua nếu không chứa chữ cái hoặc số (chỉ toàn ký tự dấu = - _ / .)
        if (!trimmed.Any(char.IsLetterOrDigit))
        {
            return false;
        }

        return true;
    }

    private static IEnumerable<string> ReadJsonStringArray(JsonObject payload, string propertyName)
    {
        if (!payload.TryGetPropertyValue(propertyName, out var node) || node is not JsonArray array)
        {
            yield break;
        }

        foreach (var item in array)
        {
            if (item?.GetValue<string>() is { Length: > 0 } value && IsSpeakableText(value))
            {
                yield return value;
            }
        }
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
            var right = ReadJsonString(item, "right");
            if (IsSpeakableText(left)) yield return left!;
            if (IsSpeakableText(right)) yield return right!;
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

    private static string ReadJsonString(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            return ReadJsonString(ParsePayloadObject(json), propertyName);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ReadJsonString(JsonObject payload, string propertyName)
    {
        if (!payload.TryGetPropertyValue(propertyName, out var node) || node is null)
        {
            return string.Empty;
        }

        return node.GetValueKind() == System.Text.Json.JsonValueKind.String ? node.GetValue<string>() : node.ToJsonString();
    }

    public static string BuildVoiceName(string usageType, string normalizedText)
    {
        var prefix = usageType switch
        {
            "title" => "Tiêu đề",
            "instruction" => "Hướng dẫn",
            "question" => "Câu hỏi",
            "correct-feedback" => "Khen ngợi",
            "retry-feedback" => "Nhắc nhở",
            "option" => "Đáp án",
            "content" => "Bài nghe",
            "tracing-prompt" => "Tô nét",
            _ => "Voice"
        };
        var clean = normalizedText.Trim();
        var sample = clean.Length > 45 ? clean[..45] + "..." : clean;
        return AudioAltText($"{prefix}: {sample}".Trim());
    }

    public static string BuildVoiceName(string usageType, string? lessonTitle, string normalizedText)
    {
        return BuildVoiceName(usageType, normalizedText);
    }

    private static string AudioAltText(string text) => text.Length > 500 ? text[..500] : text;
    private static string AudioOriginalText(string text) => text.Length > 1000 ? text[..1000] : text;
    private static string NormalizeSpeechText(string text) => string.Join(' ', Clean(text).Split(' ', StringSplitOptions.RemoveEmptyEntries));
    private static string ExtractVoiceTextFromAltText(string altText)
    {
        const string prefix = "tts:v1:";
        var cleaned = Clean(altText);
        return cleaned.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? cleaned[prefix.Length..]
            : cleaned;
    }

    private static string ResolveTextForSpeechSynthesis(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var trimmed = text.Trim();
        if (trimmed.All(c => c is '●' or '○' or '•'))
        {
            return $"{trimmed.Length} chấm tròn";
        }
        if (trimmed.All(c => c is '▲' or '△'))
        {
            return $"{trimmed.Length} hình tam giác";
        }
        if (trimmed.All(c => c is '■' or '□'))
        {
            return $"{trimmed.Length} hình vuông";
        }
        if (trimmed.All(c => c is '★' or '☆'))
        {
            return $"{trimmed.Length} ngôi sao";
        }
        if (trimmed.All(c => c is '◆' or '◇'))
        {
            return $"{trimmed.Length} hình thoi";
        }
        if (trimmed.All(c => c is '♥' or '❤'))
        {
            return $"{trimmed.Length} trái tim";
        }

        return text
            .Replace("△", "hình tam giác")
            .Replace("▲", "hình tam giác")
            .Replace("□", "hình vuông")
            .Replace("■", "hình vuông")
            .Replace("○", "hình tròn")
            .Replace("●", "hình tròn")
            .Replace("◇", "hình thoi")
            .Replace("◆", "hình thoi")
            .Replace("☆", "ngôi sao")
            .Replace("★", "ngôi sao");
    }

    private static string AudioCacheKey(string normalizedText)
    {
        var key = $"tts:v1:{normalizedText.ToLowerInvariant()}";
        return key.Length > 500 ? key[..500] : key;
    }

    private async Task EnsureMediaAssetForAudioAsync(string storagePath, string fileName, string altText, CancellationToken cancellationToken)
    {
        var normalizedPath = NormalizeStoragePath(storagePath);
        var exists = await _db.MediaAssets.AnyAsync(x =>
            x.AssetType == "audio" &&
            x.StoragePath != null &&
            x.StoragePath == normalizedPath,
            cancellationToken);
        if (exists)
        {
            return;
        }

        _db.MediaAssets.Add(new MediaAsset
        {
            Id = Guid.NewGuid(),
            AssetType = "audio",
            FileName = fileName,
            ContentType = "audio/mpeg",
            StoragePath = normalizedPath,
            AltText = altText,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private static string BuildDeterministicVoiceFileName(string prefix, string displayName, string text, string voice, string rate)
    {
        var slug = NormalizeCode(displayName);
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = "audio";
        }
        if (slug.Length > 80)
        {
            slug = slug[..80].Trim('-');
        }

        var source = $"{prefix}|{voice}|{rate}|{NormalizeSpeechText(text).ToLowerInvariant()}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source))).ToLowerInvariant();
        return $"{prefix}-{slug}-{hash[..16]}.mp3";
    }

    private static string Clean(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string NormalizeCode(string value)
    {
        var normalized = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var ch in normalized)
        {
            var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
            }
            else if (builder.Length == 0 || builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        return builder.ToString().Trim('-');
    }

    private sealed record TextToSpeechCacheKey(string Provider, string Voice, string ModelId, string Format, string TextHash);
}
