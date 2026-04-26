using GodGuidesUs.Api.Models;
using GodGuidesUs.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GodGuidesUs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController(IVerseRepository verseRepository) : ControllerBase
{
    [HttpPost("insert-dummy-verse")]
    public async Task<IActionResult> InsertDummyVerseAsync()
    {
        var dummyVerse = new VerseModel
        {
            Text = "Indeed, with hardship comes ease.",
            Theme = "patience",
            Commentary = "A reminder that trials are followed by relief.",
            Vector = Enumerable.Range(0, 768)
                .Select(index => (float)(index + 1) / 1000f)
                .ToArray()
        };

        var insertedVerse = await verseRepository.InsertAsync(dummyVerse);
        return Ok(insertedVerse);
    }
}