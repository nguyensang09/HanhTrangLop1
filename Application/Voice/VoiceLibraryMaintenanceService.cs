using System.Diagnostics;
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
        batchSize = Math.Clamp(batchSize, 1, 5);

        // Dọn dẹp các dòng rác (nếu có chứa đường dẫn file ảnh, JSON hoặc không phải văn bản đọc)
        var allCaches = await _db.TextToSpeechCaches.ToListAsync(cancellationToken);
        var unSpeakable = allCaches.Where(x => !IsSpeakableText(x.NormalizedText) || !IsSpeakableText(x.OriginalText)).ToList();
        if (unSpeakable.Count > 0)
        {
            _db.TextToSpeechCaches.RemoveRange(unSpeakable);
            await _db.SaveChangesAsync(cancellationToken);
        }

        // Lấy tất cả bài học
        var allLessons = await _db.LearningItems
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);

        var createdVi = 0;
        var createdEn = 0;
        var failed = 0;
        var updatedItems = 0;
        var errors = new List<string>();

        // Tìm danh sách các bài học còn thiếu Voice VI hoặc Voice EN
        var pendingLessons = new List<LearningItem>();
        foreach (var lesson in allLessons)
        {
            if (await LessonNeedsVoiceGenerationAsync(lesson, cancellationToken))
            {
                pendingLessons.Add(lesson);
            }
        }

        var lessonsToProcess = pendingLessons.Take(batchSize).ToList();

        foreach (var lesson in lessonsToProcess)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            // 1. Thu thập tất cả các đoạn text cần có voice cho bài học này
            var entries = await EnsureAndGetVoiceEntriesForLessonAsync(lesson, cancellationToken);

            // 2. Dịch tiếng Anh và sinh file âm thanh cho từng đoạn text của bài học này
            foreach (var entry in entries)
            {
                if (!IsSpeakableText(entry.NormalizedText))
                {
                    continue;
                }

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

            // 3. Liên kết ngay lập tức toàn bộ AudioUrl & AudioUrlEn vào bài học này
            if (await LinkVoiceUrlsForLearningItemAsync(lesson, cancellationToken))
            {
                updatedItems++;
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        var totalEntries = await _db.TextToSpeechCaches.CountAsync(cancellationToken);
        var missingVi = await _db.TextToSpeechCaches.CountAsync(x => x.AudioUrl == null || x.AudioUrl == "" || x.Status == null || x.Status != "ready", cancellationToken);
        var missingEn = await _db.TextToSpeechCaches.CountAsync(x => x.AudioUrlEn == null || x.AudioUrlEn == "" || x.StatusEn == null || x.StatusEn != "ready", cancellationToken);

        // Số bài học còn lại cần đồng bộ
        var remainingLessonsCount = Math.Max(0, pendingLessons.Count - lessonsToProcess.Count);

        return new VoiceLibrarySyncBatchResult(
            allLessons.Count,
            totalEntries,
            missingVi,
            missingEn,
            lessonsToProcess.Count,
            createdVi,
            createdEn,
            failed,
            updatedItems,
            remainingLessonsCount,
            remainingLessonsCount == 0,
            errors);
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

        AddIfValid(await EnsureVoiceEntryAsync(item.Title, "title", item.Title, cancellationToken));
        AddIfValid(await EnsureVoiceEntryAsync(item.InstructionText, "instruction", item.Title, cancellationToken));
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
        if (string.IsNullOrWhiteSpace(correctText)) correctText = "Giỏi lắm, con đã hoàn thành đúng!";
        payload["correctAudioUrl"] = await ResolveVoiceAudioUrlAsync(correctText, cancellationToken) ?? string.Empty;
        payload["correctAudioUrlEn"] = await ResolveVoiceAudioUrlEnAsync(correctText, cancellationToken) ?? string.Empty;

        // Phản hồi sai / thử lại
        var retryText = ReadJsonString(question.FeedbackJson, "retry");
        if (string.IsNullOrWhiteSpace(retryText)) retryText = "Chưa đúng rồi. Con quan sát kỹ và thử lại nhé.";
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
            audioMap[cleanLabel] = await ResolveVoiceAudioUrlAsync(label, cancellationToken) ?? string.Empty;
            audioMapEn[cleanLabel] = await ResolveVoiceAudioUrlEnAsync(label, cancellationToken) ?? string.Empty;
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

        var voiceCandidates = new List<string> { voice };
        if (voice.StartsWith("en-", StringComparison.OrdinalIgnoreCase))
        {
            if (!voice.Equals("en-US-JennyNeural", StringComparison.OrdinalIgnoreCase)) voiceCandidates.Add("en-US-JennyNeural");
            if (!voice.Equals("en-US-AriaNeural", StringComparison.OrdinalIgnoreCase)) voiceCandidates.Add("en-US-AriaNeural");
            if (!voice.Equals("en-US-GuyNeural", StringComparison.OrdinalIgnoreCase)) voiceCandidates.Add("en-US-GuyNeural");
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
