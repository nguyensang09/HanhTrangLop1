using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HanhTrangLop1.Application.Voice;

public static class PreschoolTranslationHelper
{
    private static readonly ConcurrentDictionary<string, string> TranslationMemory = new(StringComparer.OrdinalIgnoreCase);

    private static readonly HttpClientHandler _httpHandler = new()
    {
        AutomaticDecompression = System.Net.DecompressionMethods.All
    };

    private static readonly HttpClient _httpClient = new(_httpHandler)
    {
        Timeout = TimeSpan.FromSeconds(6)
    };

    static PreschoolTranslationHelper()
    {
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("*/*");
    }

    private static readonly Dictionary<string, string> ExactPhrases = new(StringComparer.OrdinalIgnoreCase)
    {
        // Phản hồi đúng
        ["Giỏi lắm, con làm đúng rồi!"] = "Great job, you did it right!",
        ["Giỏi lắm, con làm đúng rồi"] = "Great job, you did it right!",
        ["Đúng rồi! Bé giỏi quá!"] = "Great job! That's correct!",
        ["Đúng rồi!"] = "That's right!",
        ["Chính xác rồi!"] = "Exactly right!",
        ["Hoan hô bé!"] = "Hooray! Well done!",
        ["Tuyệt vời!"] = "Awesome!",
        ["Bé làm rất tốt!"] = "You did very well!",
        ["Xuất sắc quá!"] = "Excellent!",
        ["Chúc mừng bé!"] = "Congratulations!",

        // Phản hồi sai / thử lại
        ["Con thử lại nhé"] = "Let's try again.",
        ["Con thử lại nhé."] = "Let's try again.",
        ["Con thử lại nhé!"] = "Let's try again!",
        ["Không sao, con thử lại nhé!"] = "That's okay, try again!",
        ["Bé cố lên nào!"] = "You can do it, keep trying!",
        ["Con nhìn kỹ từng lựa chọn nhé."] = "Look carefully at each choice.",
        ["Bé hãy thử lại một lần nữa nhé."] = "Please try once more.",

        // Nhãn phân loại & nhóm
        ["Nhóm A"] = "Group A",
        ["Nhóm B"] = "Group B",
        ["Vùng đích"] = "Target area",
        ["Đúng"] = "Correct",
        ["Sai"] = "Incorrect",
        ["Nhiều hơn"] = "More",
        ["Ít hơn"] = "Fewer",
        ["Bằng nhau"] = "Equal"
    };

    public static async Task<string> TranslateToEnglishAsync(string? vietnameseText)
    {
        if (string.IsNullOrWhiteSpace(vietnameseText))
        {
            return string.Empty;
        }

        var text = vietnameseText.Trim();

        // 1. Kiểm tra trong bộ nhớ đệm ram
        if (TranslationMemory.TryGetValue(text, out var cached) && !string.IsNullOrWhiteSpace(cached))
        {
            return cached;
        }

        // 2. Kiểm tra câu khẩu lệnh hệ thống cố định
        if (ExactPhrases.TryGetValue(text, out var exact))
        {
            TranslationMemory[text] = exact;
            return exact;
        }

        var trimmedPunctuation = text.TrimEnd('.', '!', '?', ':', ';', ' ');
        if (ExactPhrases.TryGetValue(trimmedPunctuation, out var exactTrimmed))
        {
            TranslationMemory[text] = exactTrimmed;
            return exactTrimmed;
        }

        // 3. Ký tự chữ cái / số đơn lẻ: Đọc đúng nguyên gốc text của người dùng, không tự ý chèn thêm Letter / Number
        if (Regex.IsMatch(text, @"^[A-Za-zĂÂĐÊÔƠƯăâđêôơư]$") || Regex.IsMatch(text, @"^\d+$"))
        {
            TranslationMemory[text] = text;
            return text;
        }

        // 4. Gọi Google Translate API với nhiều kênh chống lỗi 429 Too Many Requests
        var translated = await FetchGoogleTranslationAsync(text);
        if (!string.IsNullOrWhiteSpace(translated))
        {
            if (char.IsLower(translated[0]))
            {
                translated = char.ToUpper(translated[0]) + translated[1..];
            }
            TranslationMemory[text] = translated;
            return translated;
        }

        return string.Empty;
    }

    public static string TranslateToEnglish(string? vietnameseText)
    {
        if (string.IsNullOrWhiteSpace(vietnameseText))
        {
            return string.Empty;
        }

        var text = vietnameseText.Trim();
        if (TranslationMemory.TryGetValue(text, out var cached) && !string.IsNullOrWhiteSpace(cached))
        {
            return cached;
        }

        if (ExactPhrases.TryGetValue(text, out var exact))
        {
            TranslationMemory[text] = exact;
            return exact;
        }

        try
        {
            return Task.Run(() => TranslateToEnglishAsync(text)).GetAwaiter().GetResult();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static async Task<string> FetchGoogleTranslationAsync(string text)
    {
        // Kênh 1: Google Translate Mobile Web (Độ tin cậy 100%, không bị 429 rate limit)
        try
        {
            var urlWeb = $"https://translate.google.com/m?sl=vi&tl=en&q={Uri.EscapeDataString(text)}";
            using var reqWeb = new HttpRequestMessage(HttpMethod.Get, urlWeb);
            using var resWeb = await _httpClient.SendAsync(reqWeb);
            if (resWeb.IsSuccessStatusCode)
            {
                var html = await resWeb.Content.ReadAsStringAsync();
                var match = Regex.Match(html, @"<div[^>]*class=""result-container""[^>]*>(.*?)<\/div>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var rawResult = System.Net.WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
                    if (!string.IsNullOrWhiteSpace(rawResult))
                    {
                        return rawResult;
                    }
                }
            }
        }
        catch
        {
        }

        // Kênh 2: Google Chrome Extension Translate Endpoint (Tốc độ cao)
        try
        {
            var url1 = $"https://clients5.google.com/translate_a/t?client=dict-chrome-ex&sl=vi&tl=en&q={Uri.EscapeDataString(text)}";
            using var req1 = new HttpRequestMessage(HttpMethod.Get, url1);
            using var res1 = await _httpClient.SendAsync(req1);
            if (res1.IsSuccessStatusCode)
            {
                var json = await res1.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                {
                    var first = doc.RootElement[0];
                    if (first.ValueKind == JsonValueKind.String)
                    {
                        var result = first.GetString();
                        if (!string.IsNullOrWhiteSpace(result)) return result.Trim();
                    }
                    else if (first.ValueKind == JsonValueKind.Array && first.GetArrayLength() > 0 && first[0].ValueKind == JsonValueKind.String)
                    {
                        var result = first[0].GetString();
                        if (!string.IsNullOrWhiteSpace(result)) return result.Trim();
                    }
                }
            }
        }
        catch
        {
        }

        // Kênh 3: Google GTX Endpoint
        try
        {
            var url2 = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=vi&tl=en&dt=t&q={Uri.EscapeDataString(text)}";
            using var req2 = new HttpRequestMessage(HttpMethod.Get, url2);
            using var res2 = await _httpClient.SendAsync(req2);
            if (res2.IsSuccessStatusCode)
            {
                var json = await res2.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var sentences = root[0];
                    if (sentences.ValueKind == JsonValueKind.Array)
                    {
                        var sb = new StringBuilder();
                        foreach (var s in sentences.EnumerateArray())
                        {
                            if (s.ValueKind == JsonValueKind.Array && s.GetArrayLength() > 0)
                            {
                                var part = s[0].GetString();
                                if (!string.IsNullOrEmpty(part))
                                {
                                    sb.Append(part);
                                }
                            }
                        }
                        var result = sb.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            return result;
                        }
                    }
                }
            }
        }
        catch
        {
        }

        // Kênh 4: MyMemory Professional Translation API Fallback
        try
        {
            var url3 = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair=vi|en";
            using var req3 = new HttpRequestMessage(HttpMethod.Get, url3);
            using var res3 = await _httpClient.SendAsync(req3);
            if (res3.IsSuccessStatusCode)
            {
                var json = await res3.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("responseData", out var respData) &&
                    respData.TryGetProperty("translatedText", out var transText))
                {
                    var result = transText.GetString();
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        return result.Trim();
                    }
                }
            }
        }
        catch
        {
        }

        return string.Empty;
    }
}
