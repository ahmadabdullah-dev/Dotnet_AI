namespace Dotnet_AI.Services;

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

public class GeminiAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    public GeminiAiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini:ApiKey is not configured.");
        _model = configuration["Gemini:Model"] ?? "gemini-3.6-flash";
    }

    public async Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/{_model}:generateContent?key={_apiKey}";

        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);
        
        response.EnsureSuccessStatusCode();
      
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Gemini API error {(int)response.StatusCode}: {errorBody}");
        }
        var result = await response.Content
            .ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

        return result
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }

    public async Task<string> ExtractDocumentMetadataAsync(List<(byte[] FileBytes, string MimeType)> files, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/{_model}:generateContent?key={_apiKey}";

        var parts = new List<object>();

        foreach (var (fileBytes, mimeType) in files)
        {
            parts.Add(new
            {
                inline_data = new
                {
                    mime_type = mimeType,
                    data = Convert.ToBase64String(fileBytes)
                }
            });
        }

        parts.Add(new
        {
            text = "Extract all relevant metadata from the provided document(s). Return a valid JSON object."
        });

        var payload = new
        {
            contents = new[] { new { parts = parts.ToArray() } }
        };

        var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

        return result
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }
}