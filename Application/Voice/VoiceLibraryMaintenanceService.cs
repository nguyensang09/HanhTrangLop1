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

    public async Task<VoiceLibraryRebuildResult> ResetAndRebuildAllVoicesAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("[VoiceRebuild] Bắt đầu dọn dẹp và tái thiết lập toàn bộ kho Voice chuẩn nữ...");

        // 1. Chuẩn hóa dữ liệu bài học cũ
        await LegacyLearningItemNormalizer.NormalizeAsync(_db);

        // 2. Dọn sạch các file audio cũ trong wwwroot/uploads/audio/
        try
        {
            var folder = Path.Combine(_environment.WebRootPath, "uploads", "audio");
            if (Directory.Exists(folder))
            {
                foreach (var file in Directory.EnumerateFiles(folder, "voice-*.mp3"))
                {
                    try { File.Delete(file); } catch { }
                }
                foreach (var file in Directory.EnumerateFiles(folder, "voice-en-*.mp3"))
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[VoiceRebuild] Cảnh báo khi dọn file audio cũ");
        }

        // 3. Xóa sạch dữ liệu TextToSpeechCaches và MediaAssets audio
        var oldCaches = await _db.TextToSpeechCaches.ToListAsync(cancellationToken);
        _db.TextToSpeechCaches.RemoveRange(oldCaches);

        var oldAudioAssets = await _db.MediaAssets.Where(x => x.AssetType == "audio").ToListAsync(cancellationToken);
        _db.MediaAssets.RemoveRange(oldAudioAssets);
        await _db.SaveChangesAsync(cancellationToken);

        // 4. Quét toàn bộ 447 bài học và trích xuất tất cả text duy nhất (Deduplication)
        var allLessons = await _db.LearningItems
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        var uniqueTexts = new Dictionary<string, (string UsageType, string RawText, int InitialCount)>(StringComparer.OrdinalIgnoreCase);

        void Collect(string? text, string usageType)
        {
            if (!IsSpeakableText(text)) return;
            var norm = NormalizeSpeechText(text!);
            if (string.IsNullOrWhiteSpace(norm) || !IsSpeakableText(norm)) return;

            if (uniqueTexts.TryGetValue(norm, out var existing))
            {
                uniqueTexts[norm] = (existing.UsageType, existing.RawText, existing.InitialCount + 1);
            }
            else
            {
                uniqueTexts[norm] = (usageType, text!, 1);
            }
        }

        // 4.1. Phản hồi sư phạm chuẩn
        Collect("Giỏi lắm, con làm đúng rồi!", "correct-feedback");
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
                if (lesson.InteractionType is InteractionTypes.ListenAndChoose or InteractionTypes.StoryChoice)
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
        foreach (var (norm, (usage, raw, count)) in uniqueTexts)
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
                TextEn = string.Empty,
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
            if (item.InteractionType is InteractionTypes.ListenAndChoose or InteractionTypes.StoryChoice)
            {
                AddText(ReadJsonString(payload, "speechText"));
            }

            foreach (var label in CollectOptionSpeechLabels(payload))
            {
                AddText(label);
            }
        }

        // 3. Tìm tất cả các dòng TextToSpeechCaches không còn được bài học nào sử dụng, hoặc là title / instruction / legacy / generic rác
        var allCaches = await _db.TextToSpeechCaches.ToListAsync(cancellationToken);
        var redundantEntries = allCaches.Where(x =>
            x.UsageType == "title" ||
            x.UsageType == "instruction" ||
            x.UsageType == "legacy" ||
            GenericPromptsToClean.Contains(x.NormalizedText) ||
            GenericPromptsToClean.Contains(x.OriginalText) ||
            !activeTextHashes.Contains(x.TextHash)
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

        // Xóa sạch các key cũ thừa nếu còn sót trong payload
        payload.Remove("titleAudioUrl");
        payload.Remove("titleAudioUrlEn");
        payload.Remove("instructionAudioUrl");
        payload.Remove("instructionAudioUrlEn");
        payload.Remove("instructionSpeechText");

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
        else if (item.InteractionType is InteractionTypes.ListenAndChoose or InteractionTypes.StoryChoice)
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
        var existing = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
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

    public async Task<string?> ResolveVoiceAudioUrlAsync(string? text, CancellationToken cancellationToken = default)
    {
        var rawText = (text ?? string.Empty).Trim();
        var normalizedText = NormalizeSpeechText(rawText);
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return null;
        }

        // 1. Khớp chính xác theo TextHash và chuỗi chuẩn hóa
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

        // 2. Cắt bỏ dấu câu thừa (?, ., !, :, ;)
        if (entry is null)
        {
            var stripped = normalizedText.TrimEnd('?', '.', '!', ':', ';', ' ');
            entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                x.Status == "ready" &&
                !string.IsNullOrEmpty(x.AudioUrl) &&
                (x.NormalizedText == stripped || x.OriginalText == stripped),
                cancellationToken);
        }

        // 3. Khớp biến thể chữ cái hoặc chữ số (Ví dụ: "A" -> "Chữ A", "1" -> "Số 1")
        if (entry is null && normalizedText.Length <= 3)
        {
            var letterVariant = $"chữ {normalizedText.ToLowerInvariant()}";
            var numberVariant = $"số {normalizedText.ToLowerInvariant()}";
            entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                x.Status == "ready" &&
                !string.IsNullOrEmpty(x.AudioUrl) &&
                (x.NormalizedText.ToLower() == letterVariant ||
                 x.OriginalText.ToLower() == letterVariant ||
                 x.NormalizedText.ToLower() == numberVariant ||
                 x.OriginalText.ToLower() == numberVariant),
                cancellationToken);
        }

        // 4. Khớp phản hồi chuẩn sư phạm
        if (entry is null)
        {
            var lower = normalizedText.ToLowerInvariant();
            if (lower.Contains("giỏi") || lower.Contains("đúng rồi") || lower.Contains("xuất sắc"))
            {
                entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                    x.Status == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrl) &&
                    (x.UsageType == "correct-feedback" || x.NormalizedText.Contains("Giỏi lắm") || x.OriginalText.Contains("Giỏi lắm")),
                    cancellationToken);
            }
            else if (lower.Contains("thử lại") || lower.Contains("chưa đúng") || lower.Contains("cố lên"))
            {
                entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                    x.Status == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrl) &&
                    (x.UsageType == "retry-feedback" || x.NormalizedText.Contains("thử lại") || x.OriginalText.Contains("thử lại")),
                    cancellationToken);
            }
        }

        // 5. Khớp theo slug/tên bài học trong TextToSpeechCaches
        if (entry is null)
        {
            var slug = NormalizeCode(normalizedText);
            if (!string.IsNullOrWhiteSpace(slug) && slug.Length >= 3)
            {
                entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                    x.Status == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrl) &&
                    (x.Name.Contains(slug) || x.NormalizedText.Contains(normalizedText) || x.OriginalText.Contains(normalizedText)),
                    cancellationToken);
            }
        }

        if (entry is { Status: "ready", AudioUrl.Length: > 0 })
        {
            return entry.AudioUrl;
        }

        // 6. Quét file vật lý có sẵn trên thư mục uploads/audio
        try
        {
            var folder = Path.Combine(_environment.WebRootPath, "uploads", "audio");
            if (Directory.Exists(folder))
            {
                var slug = NormalizeCode(normalizedText);
                if (!string.IsNullOrWhiteSpace(slug) && slug.Length >= 2)
                {
                    var file = Directory.EnumerateFiles(folder, $"voice-*{slug}*.mp3")
                        .FirstOrDefault(f => !Path.GetFileName(f).StartsWith("voice-en-", StringComparison.OrdinalIgnoreCase));
                    if (file != null)
                    {
                        return $"/uploads/audio/{Path.GetFileName(file)}";
                    }
                }
            }

            // Kiểm tra thư mục bilingual-female
            var femaleFolder = Path.Combine(_environment.WebRootPath, "uploads", "audio", "bilingual-female");
            if (Directory.Exists(femaleFolder))
            {
                var slug = NormalizeCode(normalizedText);
                if (!string.IsNullOrWhiteSpace(slug))
                {
                    var femaleFile = Directory.EnumerateFiles(femaleFolder, $"female-vi-*{slug}*.mp3").FirstOrDefault();
                    if (femaleFile != null)
                    {
                        return $"/uploads/audio/bilingual-female/{Path.GetFileName(femaleFile)}";
                    }
                }
            }
        }
        catch
        {
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

        // 1. Khớp chính xác theo TextHash và chuỗi chuẩn hóa
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
             (x.NormalizedText == normalizedText || x.OriginalText == rawText || x.NormalizedText == rawText || x.TextEn == rawText || x.TextEn == normalizedText)),
            cancellationToken);

        // 2. Cắt bỏ dấu câu thừa (?, ., !, :, ;)
        if (entry is null)
        {
            var stripped = normalizedText.TrimEnd('?', '.', '!', ':', ';', ' ');
            entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                x.StatusEn == "ready" &&
                !string.IsNullOrEmpty(x.AudioUrlEn) &&
                (x.NormalizedText == stripped || x.OriginalText == stripped || x.TextEn == stripped),
                cancellationToken);
        }

        // 3. Khớp chữ cái / chữ số Tiếng Anh (Ví dụ: "A", "1")
        if (entry is null && normalizedText.Length <= 3)
        {
            var lower = normalizedText.ToLowerInvariant();
            entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                x.StatusEn == "ready" &&
                !string.IsNullOrEmpty(x.AudioUrlEn) &&
                ((x.TextEn != null && x.TextEn.ToLower() == lower) ||
                 x.NormalizedText.ToLower() == lower ||
                 x.OriginalText.ToLower() == lower),
                cancellationToken);
        }

        // 4. Khớp phản hồi chuẩn sư phạm tiếng Anh
        if (entry is null)
        {
            var lower = normalizedText.ToLowerInvariant();
            if (lower.Contains("giỏi") || lower.Contains("đúng rồi") || lower.Contains("great") || lower.Contains("correct"))
            {
                entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                    x.StatusEn == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrlEn) &&
                    (x.UsageType == "correct-feedback" || (x.TextEn != null && x.TextEn.Contains("Great"))),
                    cancellationToken);
            }
            else if (lower.Contains("thử lại") || lower.Contains("chưa đúng") || lower.Contains("try again"))
            {
                entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                    x.StatusEn == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrlEn) &&
                    (x.UsageType == "retry-feedback" || (x.TextEn != null && x.TextEn.Contains("Try again"))),
                    cancellationToken);
            }
        }

        // 5. Khớp theo slug/tên bài học trong TextToSpeechCaches
        if (entry is null)
        {
            var slug = NormalizeCode(normalizedText);
            if (!string.IsNullOrWhiteSpace(slug) && slug.Length >= 3)
            {
                entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
                    x.StatusEn == "ready" &&
                    !string.IsNullOrEmpty(x.AudioUrlEn) &&
                    (x.Name.Contains(slug) || (x.TextEn != null && x.TextEn.Contains(normalizedText))),
                    cancellationToken);
            }
        }

        if (entry is { StatusEn: "ready", AudioUrlEn.Length: > 0 })
        {
            return entry.AudioUrlEn;
        }

        // 6. Quét file vật lý có sẵn trên thư mục uploads/audio
        try
        {
            var folder = Path.Combine(_environment.WebRootPath, "uploads", "audio");
            if (Directory.Exists(folder))
            {
                var slug = NormalizeCode(normalizedText);
                if (!string.IsNullOrWhiteSpace(slug) && slug.Length >= 2)
                {
                    var file = Directory.EnumerateFiles(folder, $"voice-en-*{slug}*.mp3").FirstOrDefault();
                    if (file != null)
                    {
                        return $"/uploads/audio/{Path.GetFileName(file)}";
                    }
                }
            }

            // Kiểm tra thư mục bilingual-female
            var femaleFolder = Path.Combine(_environment.WebRootPath, "uploads", "audio", "bilingual-female");
            if (Directory.Exists(femaleFolder))
            {
                var slug = NormalizeCode(normalizedText);
                if (!string.IsNullOrWhiteSpace(slug))
                {
                    var femaleFile = Directory.EnumerateFiles(femaleFolder, $"female-en-*{slug}*.mp3").FirstOrDefault();
                    if (femaleFile != null)
                    {
                        return $"/uploads/audio/bilingual-female/{Path.GetFileName(femaleFile)}";
                    }
                }
            }
        }
        catch
        {
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
