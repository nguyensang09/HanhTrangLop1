namespace HanhTrangLop1.Infrastructure.TextToSpeech;

public interface ITextToSpeechService
{
    bool CanGenerate { get; }
    Task<GeneratedSpeech?> GenerateAsync(string text, CancellationToken cancellationToken = default);
}

public sealed record GeneratedSpeech(byte[] Content, string ContentType, string Extension);
