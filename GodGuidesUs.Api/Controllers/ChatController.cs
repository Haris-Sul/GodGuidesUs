using GodGuidesUs.Api.Models;
using GodGuidesUs.Api.Repositories;
using GodGuidesUs.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GodGuidesUs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(IAiService aiService, IVerseRepository verseRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ChatAsync([FromBody] List<ChatMessage>? history)
    {
        var normalizedHistory = (history ?? [])
            .Where(message => !string.IsNullOrWhiteSpace(message.Content))
            .Select(message => new ChatMessage
            {
                Role = message.Role,
                Content = message.Content.Trim()
            })
            .ToList();

        if (normalizedHistory.Count == 0)
        {
            return BadRequest("chat history must contain at least one message");
        }

        var latestUserMessage = normalizedHistory
            .LastOrDefault(message => string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase));

        if (latestUserMessage is null)
        {
            return BadRequest("chat history must contain at least one user message");
        }

        var latestEmbedding = await aiService.GetEmbeddingAsync(latestUserMessage.Content);
        var relevantVerses = await verseRepository.SearchVersesAsync(latestEmbedding);

        var textOnlyContext = relevantVerses
            .Select(verse => new VerseModel
            {
                Text = verse.Text
            })
            .ToList();

        var modelResponse = await aiService.GenerateGuidanceAsync(normalizedHistory, textOnlyContext);
        return Ok(modelResponse);
    }
}