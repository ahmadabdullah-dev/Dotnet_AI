namespace Dotnet_AI.Controllers;

using Dotnet_AI.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("ai")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService) => _aiService = aiService;

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateRequest request, CancellationToken cancellationToken)
    {
        var result = await _aiService.GenerateTextAsync(request.Prompt, cancellationToken);

        return Ok(result);
    }

    [HttpPost("extract-metadata")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> ExtractMetadata(IFormFileCollection files, CancellationToken cancellationToken)
    {
        var fileTuples = new List<(byte[] FileBytes, string MimeType)>();

        foreach (var file in files)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, cancellationToken);
            fileTuples.Add((ms.ToArray(), file.ContentType));
        }

        var result = await _aiService.ExtractDocumentMetadataAsync(fileTuples, cancellationToken);
        return Ok(result);
    }
}

public record GenerateRequest(string Prompt);