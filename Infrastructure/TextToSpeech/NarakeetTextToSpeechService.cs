using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;

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
            return null;
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return bytes.Length == 0
            ? null
            : new GeneratedSpeech(bytes, ContentType(format), $".{format}");
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

    private static string ContentType(string format) =>
        format == "m4a" ? "audio/mp4" : "audio/mpeg";
}
