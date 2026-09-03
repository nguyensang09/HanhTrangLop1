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
    IReadOnlyList<string> ErrorMessages);

public sealed record VoiceAuditStatsResult(
    int TotalVoices,
    int ReadyVoicesVi,
    int MissingVoicesVi,
    int ReadyVoicesEn,
    int MissingVoicesEn,
    int TotalLessons,
    int FullySyncedLessons);

public sealed class VoiceLibraryMaintenanceService
{
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

    public async Task<VoiceAuditStatsResult> GetVoiceAuditStatsAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.MigrateAsync(cancellationToken);

        var totalVoices = await _db.TextToSpeechCaches.CountAsync(cancellationToken);
        var readyVi = await _db.TextToSpeechCaches.CountAsync(x => x.Status == "ready" && x.AudioUrl != null && x.AudioUrl != "", cancellationToken);
        var missingVi = await _db.TextToSpeechCaches.CountAsync(x => x.Status == null || x.Status != "ready" || x.AudioUrl == null || x.AudioUrl == "", cancellationToken);
        var readyEn = await _db.TextToSpeechCaches.CountAsync(x => x.StatusEn == "ready" && x.AudioUrlEn != null && x.AudioUrlEn != "", cancellationToken);
        var missingEn = await _db.TextToSpeechCaches.CountAsync(x => x.StatusEn == null || x.StatusEn != "ready" || x.AudioUrlEn == null || x.AudioUrlEn == "", cancellationToken);

        var totalLessons = await _db.LearningItems.CountAsync(cancellationToken);
        var fullySyncedLessons = await _db.LearningItems
            .CountAsync(x => x.ContentJson.Contains("questionAudioUrlEn") && x.ContentJson.Contains("questionAudioUrl"), cancellationToken);

        return new VoiceAuditStatsResult(
            totalVoices,
            readyVi,
            missingVi,
            readyEn,
            missingEn,
            totalLessons,
            fullySyncedLessons);
    }

    public async Task<string> BuildReportAsync(CancellationToken cancellationToken = default)
    {
        var stats = await GetVoiceAuditStatsAsync(cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine($"TotalVoices: {stats.TotalVoices}");
        builder.AppendLine($"ReadyVoicesVi: {stats.ReadyVoicesVi}");
        builder.AppendLine($"MissingVoicesVi: {stats.MissingVoicesVi}");
        builder.AppendLine($"ReadyVoicesEn: {stats.ReadyVoicesEn}");
        builder.AppendLine($"MissingVoicesEn: {stats.MissingVoicesEn}");
        builder.AppendLine($"TotalLessons: {stats.TotalLessons}");
        builder.AppendLine($"FullySyncedLessons: {stats.FullySyncedLessons}");
        return builder.ToString();
    }

    public async Task<VoiceLibrarySyncBatchResult> SyncAndGenerateBatchAsync(int batchSize = 1, CancellationToken cancellationToken = default)
    {
        await _db.Database.MigrateAsync(cancellationToken);
        batchSize = Math.Clamp(batchSize, 1, 10);

        // 1. Chuẩn hóa dữ liệu bài học và dọn dẹp sạch toàn bộ voice thừa trong CSDL lẫn file vật lý
        await CleanupAllRedundantDatabaseAndVoiceFilesAsync(cancellationToken);

        var createdVi = 0;
        var createdEn = 0;
        var failed = 0;
        var updatedItems = 0;
        var errors = new List<string>();

        // 2. Quét và tạo file cho các dòng còn thiếu Voice VI hoặc Voice EN trong TextToSpeechCaches
        var missingVoicesQuery = _db.TextToSpeechCaches
            .Where(x => x.UsageType != "legacy" &&
                        ((x.AudioUrl == null || x.AudioUrl == "" || x.Status == null || x.Status != "ready") ||
                         (x.AudioUrlEn == null || x.AudioUrlEn == "" || x.StatusEn == null || x.StatusEn != "ready")))
            .OrderBy(x => x.CreatedAt);

        var missingVoicesBatch = await missingVoicesQuery.Take(batchSize * 3).ToListAsync(cancellationToken);

        foreach (var entry in missingVoicesBatch)
        {
            if (cancellationToken.IsCancellationRequested) break;
            if (!IsSpeakableText(entry.NormalizedText)) continue;

            // Dịch tiếng Anh nếu chưa có
            if (string.IsNullOrWhiteSpace(entry.TextEn))
            {
                entry.TextEn = await PreschoolTranslationHelper.TranslateToEnglishAsync(string.IsNullOrWhiteSpace(entry.OriginalText) ? entry.NormalizedText : entry.OriginalText);
            }

            // Sinh Voice VI
            if (string.IsNullOrWhiteSpace(entry.AudioUrl) || entry.Status != "ready")
            {
                try
                {
                    entry.AudioUrl = await GenerateVoiceCacheFileAsync(entry, cancellationToken);
                    entry.Status = "ready";
                    entry.LastError = null;
                    createdVi++;
                }
                catch (Exception ex)
                {
                    entry.AudioUrl = string.Empty;
                    entry.Status = "failed";
                    entry.LastError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                    failed++;
                    errors.Add($"VI [{entry.Name}]: {ex.Message}");
                }
            }

            // Sinh Voice EN
            if (!string.IsNullOrWhiteSpace(entry.TextEn) && IsSpeakableText(entry.TextEn) && (string.IsNullOrWhiteSpace(entry.AudioUrlEn) || entry.StatusEn != "ready"))
            {
                try
                {
                    entry.AudioUrlEn = await GenerateVoiceCacheFileEnAsync(entry, cancellationToken);
                    entry.StatusEn = "ready";
                    entry.LastErrorEn = null;
                    createdEn++;
                }
                catch (Exception ex)
                {
                    entry.AudioUrlEn = string.Empty;
                    entry.StatusEn = "failed";
                    entry.LastErrorEn = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                    failed++;
                    errors.Add($"EN [{entry.Name}]: {ex.Message}");
                }
            }

            entry.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        // 3. Quét tất cả bài học để đảm bảo các đoạn text của bài học đều được nạp vào kho và liên kết URL
        var allLessons = await _db.LearningItems
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);

        // Lấy bài học cần liên kết URL
        var unlinkedLessons = allLessons.Where(l => !IsLessonFullySynced(l)).Take(batchSize).ToList();
        foreach (var lesson in unlinkedLessons)
        {
            if (cancellationToken.IsCancellationRequested) break;

            // Đảm bảo các voice của bài học này tồn tại trong CSDL
            await EnsureAndGetVoiceEntriesForLessonAsync(lesson, cancellationToken);

            if (await LinkVoiceUrlsForLearningItemAsync(lesson, cancellationToken))
            {
                updatedItems++;
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        var totalEntries = await _db.TextToSpeechCaches.CountAsync(cancellationToken);
        var remainingMissingVi = await _db.TextToSpeechCaches.CountAsync(x => x.UsageType != "legacy" && (x.AudioUrl == null || x.AudioUrl == "" || x.Status == null || x.Status != "ready"), cancellationToken);
        var remainingMissingEn = await _db.TextToSpeechCaches.CountAsync(x => x.UsageType != "legacy" && (x.AudioUrlEn == null || x.AudioUrlEn == "" || x.StatusEn == null || x.StatusEn != "ready"), cancellationToken);
        var remainingUnlinkedLessons = allLessons.Count(l => !IsLessonFullySynced(l));

        var remainingTotal = remainingMissingVi + remainingMissingEn + remainingUnlinkedLessons;
        var isCompleted = remainingTotal == 0;

        return new VoiceLibrarySyncBatchResult(
            allLessons.Count,
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
            errors);
    }

    private static bool IsLessonFullySynced(LearningItem item)
    {
        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        if (question is null) return true;

        var payload = ParsePayloadObject(question.PayloadJson);
        var titleEn = ReadJsonString(payload, "titleAudioUrlEn");
        var hasOptionEn = payload.ContainsKey("optionAudioEn");
        var hasOptionVi = payload.ContainsKey("optionAudio");

        return !string.IsNullOrWhiteSpace(titleEn) && hasOptionEn && hasOptionVi;
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
        // 1. Chuẩn hóa tất cả bài học trong DB (thay thế câu generic bằng nội dung có nghĩa)
        await LegacyLearningItemNormalizer.NormalizeAsync(_db);

        // 2. Thu thập danh sách tất cả các chuỗi text thực sự đang được dùng trong các bài học
        var activeTextHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var lessons = await _db.LearningItems
            .Include(x => x.Questions)
            .ToListAsync(cancellationToken);

        foreach (var item in lessons)
        {
            var q = item.Questions.FirstOrDefault();
            if (q is null) continue;

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

            AddText(q.PromptText);
            AddText(ReadJsonString(q.FeedbackJson, "correct"));
            AddText(ReadJsonString(q.FeedbackJson, "retry"));

            var payload = ParsePayloadObject(q.PayloadJson);
            if (item.InteractionType is InteractionTypes.ListenAndChoose or InteractionTypes.StoryChoice)
            {
                AddText(ReadJsonString(payload, "speechText"));
            }

            foreach (var label in CollectOptionSpeechLabels(payload))
            {
                AddText(label);
            }
        }

        // 3. Tìm tất cả các dòng TextToSpeechCaches không còn được bài học nào sử dụng hoặc thuộc loại legacy / title / instruction / generic rác
        // TUYỆT ĐỐI BẢO VỆ: Không bao giờ xóa các bản ghi âm thanh song ngữ (UsageType == "bilingual")
        var allCaches = await _db.TextToSpeechCaches.ToListAsync(cancellationToken);
        var redundantEntries = allCaches.Where(x =>
            x.UsageType != "bilingual" &&
            (x.UsageType == "legacy" ||
             x.UsageType == "title" ||
             x.UsageType == "instruction" ||
             GenericPromptsToClean.Contains(x.NormalizedText) ||
             GenericPromptsToClean.Contains(x.OriginalText) ||
             !activeTextHashes.Contains(x.TextHash))
        ).ToList();

        var deletedCount = 0;
        foreach (var entry in redundantEntries)
        {
            DeletePhysicalAudioFile(entry.AudioUrl);
            DeletePhysicalAudioFile(entry.AudioUrlEn);
            _db.TextToSpeechCaches.Remove(entry);
            deletedCount++;
        }

        if (deletedCount > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        // 4. Xóa tất cả các file mp3 mồ côi trên ổ đĩa
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

        // Gom tất cả các URL âm thanh hợp lệ đang được lưu trong CSDL
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

        var mediaUrls = await _db.MediaAssets
            .Where(x => x.AssetType == "audio" && !string.IsNullOrEmpty(x.StoragePath))
            .Select(x => x.StoragePath!)
            .ToListAsync(cancellationToken);
        foreach (var u in mediaUrls) activeUrls.Add(NormalizeStoragePath(u));

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

    private static string NormalizeStoragePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        var p = path.Trim().Replace('\\', '/');
        return p.StartsWith('/') ? p : "/" + p;
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

        if (item.InteractionType is InteractionTypes.ListenAndChoose or InteractionTypes.StoryChoice)
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

        var deletedFiles = DeleteAudioFiles();
        var deletedVoiceRows = await _db.TextToSpeechCaches.ExecuteDeleteAsync(cancellationToken);
        var deletedAudioRows = await _db.MediaAssets
            .Where(x => x.AssetType == "audio")
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
        await _db.SaveChangesAsync(cancellationToken);

        var voiceRowsCreated = await _db.TextToSpeechCaches.CountAsync(cancellationToken);
        var entries = await _db.TextToSpeechCaches
            .Where(x => string.IsNullOrWhiteSpace(x.AudioUrl) || x.Status != "ready" ||
                        string.IsNullOrWhiteSpace(x.AudioUrlEn) || x.StatusEn != "ready")
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
        var backfilled = await BackfillLegacyAudioAssetsAsync(cancellationToken);
        var items = await _db.LearningItems
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            await EnsureVoiceRowsForLearningItemAsync(item, cancellationToken);
        }
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

    private int DeleteAudioFiles()
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
        await EnsureVoiceEntryAsync(item.Title, "title", item.Title, cancellationToken);
        await EnsureVoiceEntryAsync(item.InstructionText, "instruction", item.Title, cancellationToken);
        await EnsureVoiceEntryAsync(question.PromptText, item.InteractionType == InteractionTypes.Tracing ? "tracing-prompt" : "question", item.Title, cancellationToken);
        await EnsureVoiceEntryAsync(ReadJsonString(question.FeedbackJson, "correct"), "correct-feedback", item.Title, cancellationToken);
        await EnsureVoiceEntryAsync(ReadJsonString(question.FeedbackJson, "retry"), "retry-feedback", item.Title, cancellationToken);

        if (item.InteractionType is InteractionTypes.ListenAndChoose or InteractionTypes.StoryChoice)
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

        // Tiêu đề
        payload["titleAudioUrl"] = await ResolveVoiceAudioUrlAsync(item.Title, cancellationToken) ?? string.Empty;
        payload["titleAudioUrlEn"] = await ResolveVoiceAudioUrlEnAsync(item.Title, cancellationToken) ?? string.Empty;

        // Lời hướng dẫn
        payload["instructionAudioUrl"] = await ResolveVoiceAudioUrlAsync(item.InstructionText, cancellationToken) ?? string.Empty;
        payload["instructionAudioUrlEn"] = await ResolveVoiceAudioUrlEnAsync(item.InstructionText, cancellationToken) ?? string.Empty;

        // Phản hồi đúng
        var correctText = ReadJsonString(question.FeedbackJson, "correct");
        if (string.IsNullOrWhiteSpace(correctText)) correctText = "Giỏi lắm, con làm đúng rồi!";
        payload["correctAudioUrl"] = await ResolveVoiceAudioUrlAsync(correctText, cancellationToken) ?? string.Empty;
        payload["correctAudioUrlEn"] = await ResolveVoiceAudioUrlEnAsync(correctText, cancellationToken) ?? string.Empty;

        // Phản hồi sai / thử lại
        var retryText = ReadJsonString(question.FeedbackJson, "retry");
        if (string.IsNullOrWhiteSpace(retryText)) retryText = "Con thử lại nhé";
        payload["retryAudioUrl"] = await ResolveVoiceAudioUrlAsync(retryText, cancellationToken) ?? string.Empty;
        payload["retryAudioUrlEn"] = await ResolveVoiceAudioUrlEnAsync(retryText, cancellationToken) ?? string.Empty;

        // Câu hỏi
        var questionUrl = await ResolveVoiceAudioUrlAsync(question.PromptText, cancellationToken) ?? string.Empty;
        var questionUrlEn = await ResolveVoiceAudioUrlEnAsync(question.PromptText, cancellationToken) ?? string.Empty;
        payload["questionAudioUrl"] = questionUrl;
        payload["questionAudioUrlEn"] = questionUrlEn;

        if (item.InteractionType == InteractionTypes.Tracing)
        {
            payload["audioUrl"] = questionUrl;
            payload["audioUrlEn"] = questionUrlEn;
        }
        else if (item.InteractionType is InteractionTypes.ListenAndChoose or InteractionTypes.StoryChoice)
        {
            var speechText = ReadJsonString(payload, "speechText");
            payload["audioUrl"] = await ResolveVoiceAudioUrlAsync(speechText, cancellationToken) ?? string.Empty;
            payload["audioUrlEn"] = await ResolveVoiceAudioUrlEnAsync(speechText, cancellationToken) ?? string.Empty;
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

    public async Task<TextToSpeechCache?> EnsureVoiceEntryAsync(string? text, string usageType, string? lessonTitle, CancellationToken cancellationToken = default)
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
        var tracked = _db.ChangeTracker.Entries<TextToSpeechCache>()
            .Select(x => x.Entity)
            .FirstOrDefault(x => x.TextHash == key.TextHash);
        if (tracked is not null)
        {
            tracked.ReuseCount += 1;
            return tracked;
        }

        var existing = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
            x.Provider == key.Provider &&
            x.Voice == key.Voice &&
            x.ModelId == key.ModelId &&
            x.Format == key.Format &&
            x.TextHash == key.TextHash,
            cancellationToken);
        if (existing is not null)
        {
            existing.ReuseCount += 1;
            return existing;
        }

        var voiceEn = _configuration["VoiceLibrary:VoiceEn"]?.Trim();
        if (string.IsNullOrWhiteSpace(voiceEn)) voiceEn = "en-US-JennyNeural";

        var entry = new TextToSpeechCache
        {
            Id = Guid.NewGuid(),
            Provider = key.Provider,
            Voice = key.Voice,
            VoiceEn = voiceEn,
            ModelId = key.ModelId,
            Format = key.Format,
            TextHash = key.TextHash,
            Name = BuildVoiceName(usageType, lessonTitle, normalizedText),
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

    public async Task<string?> ResolveVoiceAudioUrlAsync(string? text, CancellationToken cancellationToken = default)
    {
        var rawText = (text ?? string.Empty).Trim();
        var normalizedText = NormalizeSpeechText(rawText);
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return null;
        }

        var key = BuildTextToSpeechCacheKey(normalizedText);
        var entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
            (x.Provider == key.Provider &&
             x.Voice == key.Voice &&
             x.ModelId == key.ModelId &&
             x.Format == key.Format &&
             x.TextHash == key.TextHash &&
             x.Status == "ready" &&
             !string.IsNullOrEmpty(x.AudioUrl)) ||
            (x.Status == "ready" &&
             !string.IsNullOrEmpty(x.AudioUrl) &&
             (x.NormalizedText == normalizedText || x.OriginalText == rawText || x.NormalizedText == rawText)),
            cancellationToken);

        if (entry is null)
        {
            var stripped = normalizedText.TrimEnd('?', '.', '!', ':', ';', ' ');
            entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                x.Status == "ready" &&
                !string.IsNullOrEmpty(x.AudioUrl) &&
                (x.NormalizedText == stripped || x.OriginalText == stripped),
                cancellationToken);
        }

        return entry is { Status: "ready", AudioUrl.Length: > 0 } ? entry.AudioUrl : null;
    }

    public async Task<string?> ResolveVoiceAudioUrlEnAsync(string? text, CancellationToken cancellationToken = default)
    {
        var rawText = (text ?? string.Empty).Trim();
        var normalizedText = NormalizeSpeechText(rawText);
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return null;
        }

        var key = BuildTextToSpeechCacheKey(normalizedText);
        var entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
            (x.Provider == key.Provider &&
             x.Voice == key.Voice &&
             x.ModelId == key.ModelId &&
             x.Format == key.Format &&
             x.TextHash == key.TextHash &&
             x.StatusEn == "ready" &&
             !string.IsNullOrEmpty(x.AudioUrlEn)) ||
            (x.StatusEn == "ready" &&
             !string.IsNullOrEmpty(x.AudioUrlEn) &&
             (x.NormalizedText == normalizedText || x.OriginalText == rawText || x.NormalizedText == rawText)),
            cancellationToken);

        if (entry is null)
        {
            var stripped = normalizedText.TrimEnd('?', '.', '!', ':', ';', ' ');
            entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                x.StatusEn == "ready" &&
                !string.IsNullOrEmpty(x.AudioUrlEn) &&
                (x.NormalizedText == stripped || x.OriginalText == stripped),
                cancellationToken);
        }

        return entry is { StatusEn: "ready", AudioUrlEn.Length: > 0 } ? entry.AudioUrlEn : null;
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
        var storedName = $"voice-en-{NormalizeCode(entry.Name)}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.mp3";
        var diskPath = Path.Combine(folder, storedName);
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

        var storagePath = $"/uploads/audio/{storedName}";
        _db.MediaAssets.Add(new MediaAsset
        {
            Id = Guid.NewGuid(),
            AssetType = "audio",
            FileName = storedName,
            ContentType = "audio/mpeg",
            StoragePath = storagePath,
            AltText = AudioCacheKey($"en:{entry.NormalizedText}"),
            CreatedAt = DateTimeOffset.UtcNow
        });
        return storagePath;
    }

    public async Task<string?> EnsureAudioFileAsync(string text, string lang = "vi", string? customRate = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var cleanText = text.Trim();
        var isEn = lang.StartsWith("en", StringComparison.OrdinalIgnoreCase);
        var voice = isEn
            ? (_configuration["VoiceLibrary:VoiceEn"]?.Trim() ?? "en-US-JennyNeural")
            : (_configuration["VoiceLibrary:Voice"]?.Trim() ?? "vi-VN-HoaiMyNeural");
        var rate = customRate ?? (isEn
            ? (_configuration["VoiceLibrary:RateEn"]?.Trim() ?? "-15%")
            : (_configuration["VoiceLibrary:Rate"]?.Trim() ?? "-10%"));

        // 1. Kiểm tra TextToSpeechCaches trong database (chỉ nhận đúng dòng bilingual và đúng giọng NỮ HoaiMy / Jenny / Aria)
        try
        {
            if (!isEn)
            {
                var cached = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                    x.Status == "ready" &&
                    x.UsageType == "bilingual" &&
                    x.Voice == "vi-VN-HoaiMyNeural" &&
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
                var cached = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                    x.StatusEn == "ready" &&
                    x.UsageType == "bilingual" &&
                    (x.VoiceEn == "en-US-JennyNeural" || x.VoiceEn == "en-US-AriaNeural") &&
                    !string.IsNullOrEmpty(x.AudioUrlEn) &&
                    (x.TextEn == cleanText || x.OriginalText == cleanText || x.NormalizedText == cleanText), cancellationToken);
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

        // 2. Kiểm tra file trên thư mục riêng 100% giọng nữ: bilingual-female
        using var md5 = MD5.Create();
        var hash = Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes($"{lang}:{voice}:{rate}:{cleanText}"))).ToLowerInvariant();

        var folder = Path.Combine(_environment.WebRootPath, "uploads", "audio", "bilingual-female");
        Directory.CreateDirectory(folder);

        var fileName = $"female-{lang}-{hash}.mp3";
        var diskPath = Path.Combine(folder, fileName);
        var storagePath = $"/uploads/audio/bilingual-female/{fileName}";

        if (File.Exists(diskPath) && new FileInfo(diskPath).Length > 0)
        {
            return storagePath;
        }

        // 3. Gọi Edge TTS tạo file âm thanh chuẩn
        try
        {
            await RunEdgeTextToSpeechAsync(cleanText, voice, rate, diskPath, cancellationToken);
            if (File.Exists(diskPath) && new FileInfo(diskPath).Length > 0)
            {
                try
                {
                    _db.TextToSpeechCaches.Add(new TextToSpeechCache
                    {
                        Id = Guid.NewGuid(),
                        Provider = "edge",
                        Voice = voice,
                        ModelId = "neural",
                        Format = "mp3",
                        TextHash = hash,
                        Name = $"bilingual-{(isEn ? "en" : "vi")}-{NormalizeCode(cleanText)}",
                        UsageType = "bilingual",
                        NormalizedText = cleanText,
                        OriginalText = cleanText,
                        AudioUrl = isEn ? string.Empty : storagePath,
                        Status = isEn ? "missing" : "ready",
                        TextEn = isEn ? cleanText : null,
                        AudioUrlEn = isEn ? storagePath : null,
                        StatusEn = isEn ? "ready" : "missing",
                        VoiceEn = isEn ? voice : null,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
                    await _db.SaveChangesAsync(cancellationToken);
                }
                catch
                {
                }

                return storagePath;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể tạo file âm thanh song ngữ cho text: {Text}", cleanText);
        }

        return null;
    }

    public async Task PreGenerateBilingualAudioAsync(CancellationToken cancellationToken = default)
    {
        // 1. Chữ cái & Chữ số Tiếng Anh (giọng Nữ Jenny)
        var enLetters = new[]
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20"
        };

        // 2. Chữ cái & Chữ số Tiếng Việt (giọng Nữ Hoài My)
        var viLetters = new[]
        {
            "Chữ A", "Chữ B", "Chữ C", "Chữ D", "Chữ E", "Chữ F", "Chữ G", "Chữ H", "Chữ I", "Chữ J", "Chữ K", "Chữ L", "Chữ M", "Chữ N", "Chữ O", "Chữ P", "Chữ Q", "Chữ R", "Chữ S", "Chữ T", "Chữ U", "Chữ V", "Chữ W", "Chữ X", "Chữ Y", "Chữ Z",
            "Số 0", "Số 1", "Số 2", "Số 3", "Số 4", "Số 5", "Số 6", "Số 7", "Số 8", "Số 9", "Số 10", "Số 11", "Số 12", "Số 13", "Số 14", "Số 15", "Số 16", "Số 17", "Số 18", "Số 19", "Số 20"
        };

        // 3. Từ vựng Tiếng Anh (giọng Nữ Jenny)
        var enWords = new[]
        {
            "Apple", "Ball", "Cat", "Doll", "Egg", "Fan", "Garden", "Hand",
            "Icicle", "Jam", "Kangaroo", "Lamb", "Mushroom", "Net", "Orange", "Pet",
            "Quilt", "Rain", "Sunflower", "Train", "Underwear", "Vase", "Wagon",
            "X-ray", "Yo-yo", "Zebra",
            "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight",
            "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen",
            "Sixteen", "Seventeen", "Eighteen", "Nineteen", "Twenty"
        };

        // 4. Nghĩa Tiếng Việt (giọng Nữ Hoài My)
        var viWords = new[]
        {
            "Quả táo", "Quả bóng", "Con mèo", "Búp bê", "Quả trứng", "Chiếc quạt", "Khu vườn", "Bàn tay",
            "Cột băng", "Hũ mứt", "Chuột túi", "Cừu con", "Cây nấm", "Khung lưới", "Quả cam", "Thú cưng",
            "Chiếc chăn", "Cơn mưa", "Hoa hướng dương", "Tàu hỏa", "Quần áo nhỏ", "Bình hoa", "Xe kéo nhỏ",
            "Tia X-quang", "Đồ chơi Yo-yo", "Ngựa vằn",
            "Không", "Một", "Hai", "Ba", "Bốn", "Năm", "Sáu", "Bảy", "Tám",
            "Chín", "Mười", "Mười một", "Mười hai", "Mười ba", "Mười bốn", "Mười lăm",
            "Mười sáu", "Mười bảy", "Mười tám", "Mười chín", "Hai mươi"
        };

        foreach (var text in enLetters.Concat(enWords))
        {
            if (cancellationToken.IsCancellationRequested) break;
            try { await EnsureAudioFileAsync(text, "en", "-15%", cancellationToken); } catch { }
        }

        foreach (var text in viLetters.Concat(viWords))
        {
            if (cancellationToken.IsCancellationRequested) break;
            try { await EnsureAudioFileAsync(text, "vi", "-10%", cancellationToken); } catch { }
        }
    }

    public async Task<(int Created, int Failed, int UpdatedItems)> GenerateMissingAndRelinkAsync(int maxItems = 0, CancellationToken cancellationToken = default)
    {
        var batchResult = await SyncAndGenerateBatchAsync(maxItems > 0 ? maxItems : 30, cancellationToken);
        return (batchResult.CreatedVi + batchResult.CreatedEn, batchResult.Failed, batchResult.UpdatedItems);
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
        catch
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
        }

        var candidates = new[]
        {
            ("python", new[] { "-m", "edge_tts" }),
            ("py", new[] { "-m", "edge_tts" }),
            (@"C:\Program Files\PostgreSQL\18\pgAdmin 4\python\python.exe", new[] { "-m", "edge_tts" })
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

                    var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
                    var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
                    var exitTask = process.WaitForExitAsync(cancellationToken);
                    var completedTask = await Task.WhenAny(exitTask, Task.Delay(TimeSpan.FromSeconds(45), cancellationToken));
                    if (completedTask != exitTask)
                    {
                        try
                        {
                            process.Kill(entireProcessTree: true);
                        }
                        catch
                        {
                        }
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
                    errors.Add($"{fileName} ({currentVoice}): {err.Trim()}");
                }
                catch (Exception ex)
                {
                    if (File.Exists(outputPath))
                    {
                        try { File.Delete(outputPath); } catch { }
                    }
                    errors.Add($"{fileName} ({currentVoice}): {ex.Message}");
                }
            }
        }

        throw new InvalidOperationException(string.Join(" | ", errors.Where(x => !string.IsNullOrWhiteSpace(x))));
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
                ws.Options.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0");

                var connectionId = Guid.NewGuid().ToString("N");
                var uri = new Uri($"wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1?TrustedClientToken=6A5AA1D4EA6549728392C5533268B312&ConnectionId={connectionId}");
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
                    else if (result.MessageType == WebSocketMessageType.Binary && result.Count > 2)
                    {
                        var headerLength = (buffer[0] << 8) | buffer[1];
                        var headerBytes = 2 + headerLength;
                        if (result.Count > headerBytes)
                        {
                            audioMs.Write(buffer, headerBytes, result.Count - headerBytes);
                        }
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
            catch
            {
                // Thử ứng viên giọng tiếp theo
            }
        }

        return false;
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
            "content" => "Nội dung nghe",
            "tracing-prompt" => "Tô nét",
            _ => "Voice"
        };
        if (usageType is "correct-feedback" or "retry-feedback")
        {
            return prefix;
        }
        var suffix = string.IsNullOrWhiteSpace(lessonTitle) ? normalizedText : lessonTitle.Trim();
        return AudioAltText($"{prefix} - {suffix}");
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
