using GodGuidesUs.Api.Models;
using GodGuidesUs.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GodGuidesUs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiTestController(IAiService aiService) : ControllerBase
{
    [HttpGet("embedding")]
    public async Task<IActionResult> GetEmbeddingAsync([FromQuery] string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest("query parameter 'text' is required");
        }

        var embedding = await aiService.GetEmbeddingAsync(text);
        return Ok(embedding);
    }

    [HttpGet("guidance")]
    public async Task<IActionResult> GetGuidanceAsync([FromQuery] string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest("query parameter 'text' is required");
        }

        var history = new List<ChatMessage>
        {
            new()
            {
                Role = "user",
                Content = text
            }
        };

        var guidance = await aiService.GenerateGuidanceAsync(history, []);
        return Ok(guidance);
    }
}