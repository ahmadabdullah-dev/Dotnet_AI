namespace Dotnet_AI.Services;
public interface IAiService
{
    Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default);

    Task<string> ExtractDocumentMetadataAsync(
        List<(byte[] FileBytes, string MimeType)> files,
        CancellationToken cancellationToken = default);
}