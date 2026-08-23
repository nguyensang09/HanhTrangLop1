using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HanhTrangLop1.Infrastructure.TextToSpeech;

public class NarakeetTextToSpeechService : ITextToSpeechService
{
    private readonly HttpClient _httpClient;
    private readonly TextToSpeechOptions _options;

    public NarakeetTextToSpeechService(HttpClient httpClient, IOptions<TextToSpeechOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public bool CanGenerate => _options.CanGenerate;

    public async Task<GeneratedSpeech?> GenerateAsync(string text, CancellationToken cancellationToken = default)
    {
        if (!CanGenerate || string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (_options.Provider.Equals("FptAi", StringComparison.OrdinalIgnoreCase))
        {
            return await GenerateWithFptAiAsync(text, cancellationToken);
        }

        if (_options.Provider.Equals("ElevenLabs", StringComparison.OrdinalIgnoreCase))
        {
            return await GenerateWithElevenLabsAsync(text, cancellationToken);
        }

        if (!_options.Provider.Equals("Narakeet", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var format = NormalizeFormat(_options.Format);
        var requestUri = $"https://api.narakeet.com/text-to-speech/{format}";
        var query = BuildQuery();
        if (!string.IsNullOrWhiteSpace(query))
        {
            requestUri += $"?{query}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(text.Trim(), System.Text.Encoding.UTF8, "text/plain")
        };
        request.Headers.Add("x-api-key", _options.ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowProviderErrorAsync("Narakeet", response, cancellationToken);
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return bytes.Length == 0
            ? null
            : new GeneratedSpeech(bytes, ContentType(format), $".{format}");
    }

    private async Task<GeneratedSpeech?> GenerateWithElevenLabsAsync(string text, CancellationToken cancellationToken)
    {
        var voiceId = string.IsNullOrWhiteSpace(_options.Voice)
            ? "JBFqnCBsd6RMkjVDRZzb"
            : _options.Voice.Trim();
        var outputFormat = NormalizeElevenLabsFormat(_options.Format);
        var requestUri = $"https://api.elevenlabs.io/v1/text-to-speech/{Uri.EscapeDataString(voiceId)}?output_format={Uri.EscapeDataString(outputFormat)}";
        var payload = new JsonObject
        {
            ["text"] = text.Trim(),
            ["model_id"] = string.IsNullOrWhiteSpace(_options.ModelId)
                ? "eleven_flash_v2_5"
                : _options.ModelId.Trim()
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(payload.ToJsonString(), System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.Add("xi-api-key", _options.ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowProviderErrorAsync(
                "ElevenLabs",
                response,
                cancellationToken,
                $"voice_id={voiceId}, model_id={payload["model_id"]?.GetValue<string>()}, output_format={outputFormat}");
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        if (bytes.Length == 0)
        {
            throw new InvalidOperationException("ElevenLabs trả về file âm thanh rỗng.");
        }

        return bytes.Length == 0
            ? null
            : new GeneratedSpeech(bytes, ContentType("mp3"), ".mp3");
    }

    private async Task<GeneratedSpeech?> GenerateWithFptAiAsync(string text, CancellationToken cancellationToken)
    {
        var format = NormalizeFptFormat(_options.Format);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.fpt.ai/hmi/tts/v5")
        {
            Content = new StringContent(text.Trim(), System.Text.Encoding.UTF8, "text/plain")
        };
        request.Headers.Add("api_key", _options.ApiKey);
        request.Headers.Add("voice", string.IsNullOrWhiteSpace(_options.Voice) ? "banmai" : _options.Voice.Trim());
        request.Headers.Add("speed", Math.Clamp(_options.Speed, -3, 3).ToString(CultureInfo.InvariantCulture));
        request.Headers.Add("format", format);
        request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowProviderErrorAsync("FPT AI", response, cancellationToken);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        if (!root.TryGetProperty("error", out var error) || error.GetInt32() != 0 ||
            !root.TryGetProperty("async", out var asyncUrlElement))
        {
            return null;
        }

        var asyncUrl = asyncUrlElement.GetString();
        if (string.IsNullOrWhiteSpace(asyncUrl))
        {
            return null;
        }

        var bytes = await DownloadAsyncResultAsync(asyncUrl, cancellationToken);
        return bytes is null || bytes.Length == 0
            ? null
            : new GeneratedSpeech(bytes, ContentType(format), $".{format}");
    }

    private async Task<byte[]?> DownloadAsyncResultAsync(string asyncUrl, CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(Math.Clamp(_options.TimeoutSeconds, 10, 120));
        while (DateTimeOffset.UtcNow < deadline)
        {
            using var response = await _httpClient.GetAsync(asyncUrl, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                if (bytes.Length > 0)
                {
                    return bytes;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }

        return null;
    }

    private static async Task ThrowProviderErrorAsync(
        string provider,
        HttpResponseMessage response,
        CancellationToken cancellationToken,
        string? requestSummary = null)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (body.Length > 800)
        {
            body = body[..800];
        }

        var summary = string.IsNullOrWhiteSpace(requestSummary) ? string.Empty : $" Request: {requestSummary}.";
        throw new InvalidOperationException(
            $"{provider} trả lỗi HTTP {(int)response.StatusCode} {response.ReasonPhrase}.{summary} Body: {body}");
    }

    private string BuildQuery()
    {
        var values = new List<string>();
        if (!string.IsNullOrWhiteSpace(_options.Voice))
        {
            values.Add($"voice={Uri.EscapeDataString(_options.Voice)}");
        }

        if (_options.VoiceSpeed > 0)
        {
            values.Add($"voice-speed={_options.VoiceSpeed.ToString("0.##", CultureInfo.InvariantCulture)}");
        }

        return string.Join('&', values);
    }

    private static string NormalizeFormat(string format) =>
        format.Equals("m4a", StringComparison.OrdinalIgnoreCase) ? "m4a" : "mp3";

    private static string NormalizeFptFormat(string format) =>
        format.Equals("wav", StringComparison.OrdinalIgnoreCase) ? "wav" : "mp3";

    private static string NormalizeElevenLabsFormat(string format) =>
        string.IsNullOrWhiteSpace(format) || format.Equals("mp3", StringComparison.OrdinalIgnoreCase)
            ? "mp3_44100_128"
            : format.Trim();

    private static string ContentType(string format) =>
        format == "m4a" ? "audio/mp4" :
        format == "wav" ? "audio/wav" : "audio/mpeg";
}
