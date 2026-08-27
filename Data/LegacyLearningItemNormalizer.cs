using System.Text.Json;
using System.Text.Json.Nodes;
using HanhTrangLop1.Models;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Data;

public static class LegacyLearningItemNormalizer
{
    public static async Task<int> NormalizeAsync(ApplicationDbContext db, ILogger? logger = null)
    {
        var items = await db.LearningItems
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync();

        var updated = 0;
        foreach (var item in items)
        {
            if (NormalizeItem(item))
            {
                updated += 1;
            }
        }

        if (updated > 0)
        {
            await db.SaveChangesAsync();
            logger?.LogInformation("Đã chuẩn hóa dữ liệu cũ cho {UpdatedCount} bài học.", updated);
        }

        return updated;
    }

    private static readonly HashSet<string> GenericPrompts = new(StringComparer.OrdinalIgnoreCase)
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

    public static string DeriveShortMapTitle(string? fullTitle, string? prompt)
    {
        var text = string.IsNullOrWhiteSpace(fullTitle) ? (prompt ?? string.Empty) : fullTitle;
        text = text.Trim();
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^(?:Bé hãy|Con hãy|Hãy|Chọn các|Chọn|Tô theo|Tô tranh|Sắp xếp|Tìm)\s+", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        text = text.Trim();
        if (text.Length > 50)
        {
            text = text[..50].TrimEnd();
        }
        return string.IsNullOrWhiteSpace(text) ? "Bài học" : char.ToUpper(text[0]) + text[1..];
    }

    private static bool NormalizeItem(LearningItem item)
    {
        var question = item.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
        if (question is null)
        {
            return false;
        }

        var changed = false;

        // Nếu PromptText là câu hỏi chung chung vô nghĩa, chuyển nội dung Title sang làm PromptText chính
        if (string.IsNullOrWhiteSpace(question.PromptText) || GenericPrompts.Contains(question.PromptText.Trim()))
        {
            question.PromptText = item.Title;
            changed = true;
        }

        var payload = ParseObject(question.PayloadJson);
        if (payload.Count == 0)
        {
            payload = ParseObject(item.ContentJson);
        }

        var correctFeedback = ReadJsonString(question.FeedbackJson, "correct");
        var retryFeedback = ReadJsonString(question.FeedbackJson, "retry");
        if (string.IsNullOrWhiteSpace(correctFeedback))
        {
            correctFeedback = "Giỏi lắm, con chọn đúng rồi!";
            question.FeedbackJson = SetJsonObject(question.FeedbackJson, "correct", correctFeedback);
            changed = true;
        }
        if (string.IsNullOrWhiteSpace(retryFeedback))
        {
            retryFeedback = "Không sao, mình thử lại nhẹ nhàng nhé.";
            question.FeedbackJson = SetJsonObject(question.FeedbackJson, "retry", retryFeedback);
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(ReadJsonString(question.HintJson, "level1")))
        {
            question.HintJson = SetJsonObject(question.HintJson, "level1", "Con nhìn kỹ từng lựa chọn nhé.");
            changed = true;
        }

        EnsureNumber(payload, "schemaVersion", 2, ref changed);
        EnsureString(payload, "activityType", item.InteractionType, ref changed);
        EnsureString(payload, "imageUrl", string.Empty, ref changed);
        EnsureString(payload, "imageAltText", string.Empty, ref changed);
        EnsureString(payload, "audioUrl", string.Empty, ref changed);
        EnsureString(payload, "titleAudioUrl", string.Empty, ref changed);
        EnsureString(payload, "questionAudioUrl", string.Empty, ref changed);
        EnsureString(payload, "instructionAudioUrl", string.Empty, ref changed);
        EnsureString(payload, "correctAudioUrl", string.Empty, ref changed);
        EnsureString(payload, "retryAudioUrl", string.Empty, ref changed);
        EnsureString(payload, "speechText", string.Empty, ref changed);
        EnsureString(payload, "instructionSpeechText", item.InstructionText, ref changed);
        EnsureString(payload, "questionSpeechText", question.PromptText, ref changed);
        EnsureString(payload, "correctSpeechText", correctFeedback, ref changed);
        EnsureString(payload, "retrySpeechText", retryFeedback, ref changed);
        EnsureObject(payload, "itemMedia", ref changed);
        EnsureObject(payload, "optionAudio", ref changed);

        // Đồng bộ questionSpeechText với prompt thực tế
        if (payload["questionSpeechText"]?.ToString() != question.PromptText)
        {
            payload["questionSpeechText"] = question.PromptText;
            changed = true;
        }

        if (!changed)
        {
            return false;
        }

        var payloadJson = payload.ToJsonString();
        item.ContentJson = payloadJson;
        question.PayloadJson = payloadJson;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        return true;
    }

    private static JsonObject ParseObject(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new JsonObject();
        }

        try
        {
            return JsonNode.Parse(json)?.AsObject() ?? new JsonObject();
        }
        catch (JsonException)
        {
            return new JsonObject();
        }
    }

    private static string ReadJsonString(string? json, string propertyName)
    {
        var payload = ParseObject(json);
        if (!payload.TryGetPropertyValue(propertyName, out var node) || node is null)
        {
            return string.Empty;
        }

        return node.GetValueKind() == JsonValueKind.String ? node.GetValue<string>() : node.ToJsonString();
    }

    private static string SetJsonObject(string? json, string propertyName, string value)
    {
        var payload = ParseObject(json);
        payload[propertyName] = value;
        return payload.ToJsonString();
    }

    private static void EnsureString(JsonObject payload, string propertyName, string value, ref bool changed)
    {
        if (payload.TryGetPropertyValue(propertyName, out var node) && node is not null)
        {
            return;
        }

        payload[propertyName] = value;
        changed = true;
    }

    private static void EnsureNumber(JsonObject payload, string propertyName, int value, ref bool changed)
    {
        if (payload.TryGetPropertyValue(propertyName, out var node) && node is not null)
        {
            return;
        }

        payload[propertyName] = value;
        changed = true;
    }

    private static void EnsureObject(JsonObject payload, string propertyName, ref bool changed)
    {
        if (payload.TryGetPropertyValue(propertyName, out var node) && node is JsonObject)
        {
            return;
        }

        payload[propertyName] = new JsonObject();
        changed = true;
    }
}
