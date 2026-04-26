using System.Text;
using System.Text.Json;
using GodGuidesUs.Api.Models;
using Microsoft.Extensions.Options;

namespace GodGuidesUs.Api.Services;

public class GoogleAiService(HttpClient httpClient, IOptions<GoogleAiSettings> googleAiSettings) : IAiService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly GoogleAiSettings _settings = googleAiSettings.Value;

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("text cannot be empty", nameof(text));
        }

        EnsureApiKeyIsConfigured();

        var payload = new
        {
            model = "models/gemini-embedding-001",
            content = new
            {
                parts = new[]
                {
                    new { text }
                }
            },
            outputDimensionality = 768
        };

        var request = new HttpRequestMessage(HttpMethod.Post, BuildEndpointUrl("models/gemini-embedding-001:embedContent"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonSerializerOptions), Encoding.UTF8, "application/json")
        };

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var jsonDocument = await JsonDocument.ParseAsync(responseStream);

        if (!jsonDocument.RootElement.TryGetProperty("embedding", out var embeddingElement) ||
            !embeddingElement.TryGetProperty("values", out var valuesElement))
        {
            throw new InvalidOperationException("embedding response did not contain embedding.values");
        }

        var values = valuesElement
            .EnumerateArray()
            .Select(value => value.GetSingle())
            .ToArray();

        if (values.Length != 768)
        {
            throw new InvalidOperationException($"embedding response returned {values.Length} dimensions instead of 768");
        }

        return values;
    }

    public async Task<GuidanceResponseDto> GenerateGuidanceAsync(List<ChatMessage> history, List<VerseModel> context)
    {
        EnsureApiKeyIsConfigured();

        var normalizedHistory = (history ?? [])
            .Where(message => !string.IsNullOrWhiteSpace(message.Content))
            .Select(message => new
            {
                role = NormalizeRole(message.Role),
                parts = new[]
                {
                    new { text = message.Content }
                }
            })
            .ToList();

        var formattedContext = BuildContextInstruction(context ?? []);

        if (normalizedHistory.Count == 0)
        {
            normalizedHistory.Add(new
            {
                role = "user",
                parts = new[]
                {
                    new { text = "please provide guidance based on the available quran and tafsir context" }
                }
            });
        }

        var payload = new
        {
            system_instruction = new
            {
                parts = new[]
                {
                    new { text = formattedContext }
                }
            },
            contents = normalizedHistory,
            generationConfig = new
            {
                thinkingConfig = new
                {
                    thinkingLevel = "high"
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, BuildEndpointUrl("models/gemma-4-26b-a4b-it:generateContent"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonSerializerOptions), Encoding.UTF8, "application/json")
        };

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var jsonDocument = await JsonDocument.ParseAsync(responseStream);

        if (!jsonDocument.RootElement.TryGetProperty("candidates", out var candidatesElement) ||
            candidatesElement.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("generation response did not contain candidates");
        }

        var firstCandidate = candidatesElement[0];
        if (!firstCandidate.TryGetProperty("content", out var contentElement) ||
            !contentElement.TryGetProperty("parts", out var partsElement))
        {
            throw new InvalidOperationException("generation response did not contain content.parts");
        }

        var textParts = partsElement
            .EnumerateArray()
            .Where(part => part.TryGetProperty("text", out _))
            .Select(part => part.GetProperty("text").GetString()?.Trim())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(text => text!)
            .ToList();

        if (textParts.Count == 0)
        {
            throw new InvalidOperationException("generation response returned empty text");
        }

        var message = textParts[^1];
        var thoughts = textParts.Count > 1
            ? string.Join("\n\n", textParts.Take(textParts.Count - 1))
            : string.Empty;

        return new GuidanceResponseDto(thoughts, message);
    }

    private static string BuildContextInstruction(List<VerseModel> context)
    {
        var responseStyleRules = "You are an empathetic, conversational Islamic guide. Keep responses concise: maximum 1 to 2 short paragraphs. Avoid heavy markdown (no large headers and no long bulleted lists). Do not give exhaustive answers. Offer one practical piece of wisdom and always end with exactly one natural follow-up question to encourage back-and-forth conversation.";

        if (context.Count == 0)
        {
            return $"{responseStyleRules}\n\nNo retrieved verses were provided for this turn. Rely on generally accepted Islamic principles and keep the reply gentle and practical.";
        }

        var contextLines = context
            .Select((verse, index) =>
                $"[{index + 1}] {verse.Text}");

        return $"{responseStyleRules}\n\nUse the retrieved Quran/tafsir context below as your primary source for this response.\n\nRetrieved context:\n{string.Join("\n\n", contextLines)}";
    }

    private static string NormalizeRole(string role)
    {
        var normalizedRole = role.Trim().ToLowerInvariant();

        return normalizedRole switch
        {
            "assistant" => "model",
            "model" => "model",
            _ => "user"
        };
    }

    private void EnsureApiKeyIsConfigured()
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("GoogleAi:ApiKey is missing from configuration");
        }
    }

    private string BuildEndpointUrl(string modelOperationPath)
    {
        var baseUrl = _settings.BaseUrl?.TrimEnd('/') ?? "https://generativelanguage.googleapis.com/v1beta";
        return $"{baseUrl}/{modelOperationPath}?key={_settings.ApiKey}";
    }
}