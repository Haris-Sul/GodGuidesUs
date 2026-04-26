using GodGuidesUs.Api.Models;

namespace GodGuidesUs.Api.Services;

public interface IAiService
{
    Task<float[]> GetEmbeddingAsync(string text);

    Task<GuidanceResponseDto> GenerateGuidanceAsync(List<ChatMessage> history, List<VerseModel> context);
}