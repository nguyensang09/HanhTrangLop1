using System.Globalization;
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

            // Đảo vị trí ở mỗi lượt hiển thị để trẻ không học thuộc vị trí đáp án đúng.
            // Giá trị đáp án không đổi nên việc chấm bài vẫn chính xác theo nội dung.
            for (var index = choices.Count - 1; index > 0; index--)
            {
                var swapIndex = Random.Shared.Next(index + 1);
                (choices[index], choices[swapIndex]) = (choices[swapIndex], choices[index]);
            }

            return choices;
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>
    /// Đọc đáp án đúng và luôn trả về dạng chuỗi canonical (các phần tử nối bằng '|').
    /// Chấp nhận mọi kiểu dữ liệu đã từng được soạn trong CorrectAnswerJson (chuỗi, số,
    /// true/false hoặc mảng) để tránh việc bé trả lời đúng nhưng hệ thống không chấm được.
    /// </summary>
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
                ? ReadAnswerElement(value)
                : string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Số phần tử đáp án cần có (dùng cho cơ chế tự động kiểm tra ở phía giao diện:
    /// bé chọn đủ số phần tử thì hệ thống tự chấm, không cần nút "Kiểm tra đáp án").
    /// </summary>
    public static int ReadCorrectAnswerCount(string correctAnswerJson)
    {
        var canonical = ReadCorrectAnswer(correctAnswerJson);
        return string.IsNullOrWhiteSpace(canonical)
            ? 0
            : canonical.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
    }

    private static string ReadAnswerElement(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Number => value.TryGetInt64(out var number)
            ? number.ToString(CultureInfo.InvariantCulture)
            : value.GetDouble().ToString(CultureInfo.InvariantCulture),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Array => string.Join('|', value.EnumerateArray()
            .Select(ReadAnswerElement)
            .Where(x => !string.IsNullOrWhiteSpace(x))),
        _ => string.Empty
    };

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
        var defaultFeedback = isCorrect ? "Giỏi lắm" : "Con thử lại nhé";
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
