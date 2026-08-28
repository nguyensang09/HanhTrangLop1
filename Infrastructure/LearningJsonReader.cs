using System.Text.Json;
using HanhTrangLop1.Models.ViewModels;

namespace HanhTrangLop1.Infrastructure;

public static class LearningJsonReader
{
    public static IReadOnlyList<ChoiceOptionViewModel> ReadChoices(string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(payloadJson);
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
                    if (item.TryGetProperty("count", out var countNode) && countNode.TryGetInt32(out var countVal))
                    {
                        text = $"{countVal} đồ vật";
                    }

                    choices.Add(new ChoiceOptionViewModel { Value = value, Text = text });
                }
            }

            return choices;
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public static string ReadCorrectAnswer(string correctAnswerJson)
    {
        if (string.IsNullOrWhiteSpace(correctAnswerJson))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(correctAnswerJson);
            return document.RootElement.TryGetProperty("value", out var value)
                ? value.GetString() ?? string.Empty
                : string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    public static string ReadStringProperty(string json, string propertyName, string fallback)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return fallback;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? fallback
                : fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    public static int ReadIntProperty(string json, string propertyName, int fallback)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return fallback;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.TryGetProperty(propertyName, out var value) && value.TryGetInt32(out var number)
                ? number
                : fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    public static bool ReadBoolProperty(string json, string propertyName, bool fallback)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return fallback;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.TryGetProperty(propertyName, out var value) &&
                   (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
                ? value.GetBoolean()
                : fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    public static string ReadFeedback(string feedbackJson, bool isCorrect)
    {
        var defaultFeedback = isCorrect ? "Giỏi lắm, con làm đúng rồi!" : "Con thử lại nhé";
        if (string.IsNullOrWhiteSpace(feedbackJson))
        {
            return defaultFeedback;
        }

        try
        {
            using var document = JsonDocument.Parse(feedbackJson);
            var propertyName = isCorrect ? "correct" : "retry";
            return document.RootElement.TryGetProperty(propertyName, out var value)
                ? value.GetString() ?? defaultFeedback
                : defaultFeedback;
        }
        catch (JsonException)
        {
            return defaultFeedback;
        }
    }
}
