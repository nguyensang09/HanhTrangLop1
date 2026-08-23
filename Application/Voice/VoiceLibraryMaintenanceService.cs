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
            .Where(x => string.IsNullOrWhiteSpace(x.AudioUrl) || x.Status != "ready")
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var created = 0;
        var failed = 0;
        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                entry.AudioUrl = await GenerateVoiceCacheFileAsync(entry, cancellationToken);
                entry.Status = "ready";
                entry.LastError = null;
                entry.UpdatedAt = DateTimeOffset.UtcNow;
                created += 1;
            }
            catch (Exception ex)
            {
                entry.AudioUrl = string.Empty;
                entry.Status = "missing";
                entry.LastError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                entry.UpdatedAt = DateTimeOffset.UtcNow;
                failed += 1;
                _logger.LogWarning(ex, "Cannot generate voice file for {VoiceName}", entry.Name);
            }
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

            _db.TextToSpeechCaches.Add(new TextToSpeechCache
            {
                Id = Guid.NewGuid(),
                Provider = key.Provider,
                Voice = key.Voice,
                ModelId = key.ModelId,
                Format = key.Format,
                TextHash = key.TextHash,
                Name = BuildVoiceName("legacy", null, normalizedText),
                UsageType = "legacy",
                NormalizedText = AudioAltText(normalizedText),
                OriginalText = AudioOriginalText(voiceText),
                AudioUrl = asset.StoragePath,
                Status = "ready",
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

    private async Task EnsureVoiceRowsForLearningItemAsync(LearningItem item, CancellationToken cancellationToken)
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

    private async Task<bool> LinkVoiceUrlsForLearningItemAsync(LearningItem item, CancellationToken cancellationToken)
    {
        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        if (question is null)
        {
            return false;
        }

        var payload = ParsePayloadObject(question.PayloadJson);
        payload["titleAudioUrl"] = await ResolveVoiceAudioUrlAsync(item.Title, cancellationToken) ?? string.Empty;
        payload["instructionAudioUrl"] = await ResolveVoiceAudioUrlAsync(item.InstructionText, cancellationToken) ?? string.Empty;
        payload["correctAudioUrl"] = await ResolveVoiceAudioUrlAsync(ReadJsonString(question.FeedbackJson, "correct"), cancellationToken) ?? string.Empty;
        payload["retryAudioUrl"] = await ResolveVoiceAudioUrlAsync(ReadJsonString(question.FeedbackJson, "retry"), cancellationToken) ?? string.Empty;

        var questionUrl = await ResolveVoiceAudioUrlAsync(question.PromptText, cancellationToken) ?? string.Empty;
        payload["questionAudioUrl"] = questionUrl;
        if (item.InteractionType == InteractionTypes.Tracing)
        {
            payload["audioUrl"] = questionUrl;
        }
        else if (item.InteractionType is InteractionTypes.ListenAndChoose or InteractionTypes.StoryChoice)
        {
            payload["audioUrl"] = await ResolveVoiceAudioUrlAsync(ReadJsonString(payload, "speechText"), cancellationToken) ?? string.Empty;
        }

        var audioMap = new JsonObject();
        foreach (var label in CollectOptionSpeechLabels(payload))
        {
            audioMap[Clean(label)] = await ResolveVoiceAudioUrlAsync(label, cancellationToken) ?? string.Empty;
        }
        payload["optionAudio"] = audioMap;

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

    private async Task<TextToSpeechCache?> EnsureVoiceEntryAsync(string? text, string usageType, string? lessonTitle, CancellationToken cancellationToken)
    {
        var normalizedText = NormalizeSpeechText(text ?? string.Empty);
        if (string.IsNullOrWhiteSpace(normalizedText))
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

        var entry = new TextToSpeechCache
        {
            Id = Guid.NewGuid(),
            Provider = key.Provider,
            Voice = key.Voice,
            ModelId = key.ModelId,
            Format = key.Format,
            TextHash = key.TextHash,
            Name = BuildVoiceName(usageType, lessonTitle, normalizedText),
            UsageType = usageType,
            NormalizedText = AudioAltText(normalizedText),
            OriginalText = AudioOriginalText(text ?? normalizedText),
            AudioUrl = string.Empty,
            Status = "missing",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _db.TextToSpeechCaches.Add(entry);
        return entry;
    }

    private async Task<string?> ResolveVoiceAudioUrlAsync(string? text, CancellationToken cancellationToken)
    {
        var normalizedText = NormalizeSpeechText(text ?? string.Empty);
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return null;
        }

        var key = BuildTextToSpeechCacheKey(normalizedText);
        var entry = await _db.TextToSpeechCaches.FirstOrDefaultAsync(x =>
            x.Provider == key.Provider &&
            x.Voice == key.Voice &&
            x.ModelId == key.ModelId &&
            x.Format == key.Format &&
            x.TextHash == key.TextHash,
            cancellationToken);
        return entry is { Status: "ready", AudioUrl.Length: > 0 } ? entry.AudioUrl : null;
    }

    private async Task<string> GenerateVoiceCacheFileAsync(TextToSpeechCache entry, CancellationToken cancellationToken)
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

    public async Task<(int Created, int Failed, int UpdatedItems)> GenerateMissingAndRelinkAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.MigrateAsync(cancellationToken);
        var entries = await _db.TextToSpeechCaches
            .Where(x => string.IsNullOrWhiteSpace(x.AudioUrl) || x.Status != "ready")
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var created = 0;
        var failed = 0;
        foreach (var entry in entries)
        {
            try
            {
                entry.AudioUrl = await GenerateVoiceCacheFileAsync(entry, cancellationToken);
                entry.Status = "ready";
                entry.LastError = null;
                entry.UpdatedAt = DateTimeOffset.UtcNow;
                created += 1;
            }
            catch (Exception ex)
            {
                entry.AudioUrl = string.Empty;
                entry.Status = "missing";
                entry.LastError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                entry.UpdatedAt = DateTimeOffset.UtcNow;
                failed += 1;
            }
        }
        await _db.SaveChangesAsync(cancellationToken);

        var items = await _db.LearningItems
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
        var updatedItems = 0;
        foreach (var item in items)
        {
            if (await LinkVoiceUrlsForLearningItemAsync(item, cancellationToken))
            {
                updatedItems += 1;
            }
        }
        await _db.SaveChangesAsync(cancellationToken);

        return (created, failed, updatedItems);
    }

    public async Task<string> BuildReportAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.MigrateAsync(cancellationToken);

        var voiceRows = await _db.TextToSpeechCaches.CountAsync(cancellationToken);
        var audioRows = await _db.MediaAssets.CountAsync(x => x.AssetType == "audio", cancellationToken);
        var readyRows = await _db.TextToSpeechCaches.CountAsync(x => x.Status == "ready" && x.AudioUrl != string.Empty, cancellationToken);
        var missingRows = await _db.TextToSpeechCaches
            .Where(x => x.Status != "ready" || x.AudioUrl == string.Empty)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Name, x.NormalizedText, x.LastError })
            .ToListAsync(cancellationToken);
        var usageRows = await _db.TextToSpeechCaches
            .GroupBy(x => x.UsageType)
            .Select(x => new { UsageType = x.Key, Count = x.Count() })
            .OrderBy(x => x.UsageType)
            .ToListAsync(cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine($"VoiceRows={voiceRows}");
        builder.AppendLine($"ReadyRows={readyRows}");
        builder.AppendLine($"MissingRows={missingRows.Count}");
        builder.AppendLine($"AudioAssetRows={audioRows}");
        builder.AppendLine("UsageTypes:");
        foreach (var row in usageRows)
        {
            builder.AppendLine($"  {row.UsageType}: {row.Count}");
        }

        if (missingRows.Count > 0)
        {
            builder.AppendLine("Missing:");
            foreach (var row in missingRows)
            {
                builder.AppendLine($"  {row.Name} | {row.NormalizedText} | {row.LastError}");
            }
        }

        return builder.ToString();
    }

    private static async Task RunEdgeTextToSpeechAsync(string text, string voice, string rate, string outputPath, CancellationToken cancellationToken)
    {
        var candidates = new[]
        {
            ("python", new[] { "-m", "edge_tts" }),
            ("py", new[] { "-m", "edge_tts" })
        };
        var errors = new List<string>();
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
            startInfo.ArgumentList.Add(voice);
            startInfo.ArgumentList.Add("--rate");
            startInfo.ArgumentList.Add(rate);
            startInfo.ArgumentList.Add("--text");
            startInfo.ArgumentList.Add(text);
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
                    errors.Add($"{fileName}: quá thời gian tạo voice.");
                    continue;
                }

                var stdout = await outputTask;
                var stderr = await errorTask;
                if (process.ExitCode == 0 && File.Exists(outputPath) && new FileInfo(outputPath).Length > 0)
                {
                    return;
                }

                errors.Add($"{fileName}: {stderr} {stdout}".Trim());
            }
            catch (Exception ex)
            {
                errors.Add($"{fileName}: {ex.Message}");
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

    private static IEnumerable<string> ReadJsonStringArray(JsonObject payload, string propertyName)
    {
        if (!payload.TryGetPropertyValue(propertyName, out var node) || node is not JsonArray array)
        {
            yield break;
        }

        foreach (var item in array)
        {
            if (item?.GetValue<string>() is { Length: > 0 } value)
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
            if (!string.IsNullOrWhiteSpace(left)) yield return left;
            if (!string.IsNullOrWhiteSpace(right)) yield return right;
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

    private static string AudioAltText(string text) => text.Length > 180 ? text[..180] : text;
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

    private static string ResolveTextForSpeechSynthesis(string text) => text switch
    {
        "△" or "▲" => "hình tam giác",
        "□" or "■" => "hình vuông",
        "○" or "●" => "hình tròn",
        "◇" or "◆" => "hình thoi",
        "☆" or "★" => "ngôi sao",
        _ => text
    };
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
