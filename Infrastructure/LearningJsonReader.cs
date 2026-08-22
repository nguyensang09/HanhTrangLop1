using System.Text.Json;
using HanhTrangLop1.Models.ViewModels;

namespace HanhTrangLop1.Infrastructure;

public static class LearningJsonReader
{
    public static IReadOnlyList<ChoiceOptionViewModel> ReadChoices(string payloadJson)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson);
        if (!document.RootElement.TryGetProperty("choices", out var choicesElement) || choicesElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var choices = new List<ChoiceOptionViewModel>();
        foreach (var item in choicesElement.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                var value = item.GetString() ?? string.Empty;
                choices.Add(new ChoiceOptionViewModel { Value = value, Text = value });
                continue;
            }

            if (item.ValueKind == JsonValueKind.Object)
            {
                var value = item.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty;
                var text = item.TryGetProperty("text", out var textNode) ? textNode.GetString() ?? value : value;
                if (item.TryGetProperty("count", out var countNode))
                {
                    text = $"{countNode.GetInt32()} đồ vật";
                }

                choices.Add(new ChoiceOptionViewModel { Value = value, Text = text });
            }
        }

        return choices;
    }

    public static string ReadCorrectAnswer(string correctAnswerJson)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(correctAnswerJson) ? "{}" : correctAnswerJson);
        return document.RootElement.TryGetProperty("value", out var value)
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    public static string ReadStringProperty(string json, string propertyName, string fallback)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        return document.RootElement.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? fallback
            : fallback;
    }

    public static int ReadIntProperty(string json, string propertyName, int fallback)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        return document.RootElement.TryGetProperty(propertyName, out var value) && value.TryGetInt32(out var number)
            ? number
            : fallback;
    }

    public static bool ReadBoolProperty(string json, string propertyName, bool fallback)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        return document.RootElement.TryGetProperty(propertyName, out var value) &&
               (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
            ? value.GetBoolean()
            : fallback;
    }

    public static string ReadFeedback(string feedbackJson, bool isCorrect)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(feedbackJson) ? "{}" : feedbackJson);
        var propertyName = isCorrect ? "correct" : "retry";
        return document.RootElement.TryGetProperty(propertyName, out var value)
            ? value.GetString() ?? string.Empty
            : isCorrect ? "Giỏi lắm, con làm đúng rồi!" : "Không sao, mình thử lại nhẹ nhàng nhé.";
    }
}
