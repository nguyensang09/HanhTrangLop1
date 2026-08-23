namespace HanhTrangLop1.Infrastructure.TextToSpeech;

public class TextToSpeechOptions
{
    public string Provider { get; set; } = "Browser";
    public string ApiKey { get; set; } = string.Empty;
    public string Voice { get; set; } = string.Empty;
    public double VoiceSpeed { get; set; } = 0.9;
    public string Format { get; set; } = "mp3";
    public int TimeoutSeconds { get; set; } = 30;

    public bool CanGenerate =>
        Provider.Equals("Narakeet", StringComparison.OrdinalIgnoreCase) &&
        !string.IsNullOrWhiteSpace(ApiKey);
}
