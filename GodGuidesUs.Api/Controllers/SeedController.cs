using System.Text.Json;
using GodGuidesUs.Api.Models;
using GodGuidesUs.Api.Repositories;
using GodGuidesUs.Api.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace GodGuidesUs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController(
    IWebHostEnvironment webHostEnvironment,
    IAiService aiService,
    IVerseRepository verseRepository) : ControllerBase
{
    [HttpPost("seed-all-chapters")]
    public async Task<IActionResult> SeedAllChapters()
    {
        var tafsirDataFolder = Path.Combine(webHostEnvironment.ContentRootPath, "tafsir-data");
        if (!Directory.Exists(tafsirDataFolder))
        {
            return NotFound($"tafsir-data folder was not found at: {tafsirDataFolder}");
        }

        var chapterFiles = Directory
            .GetFiles(tafsirDataFolder, "*.json", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path)
            .ToArray();

        if (chapterFiles.Length == 0)
        {
            return BadRequest("no chapter json files were found in tafsir-data");
        }

        var insertedCount = 0;
        var skippedCount = 0;
        var failedCount = 0;

        foreach (var chapterFile in chapterFiles)
        {
            var chapterFileName = Path.GetFileNameWithoutExtension(chapterFile);
            var chapterRawJson = await System.IO.File.ReadAllTextAsync(chapterFile);
            var chapterData = JsonSerializer.Deserialize<TafsirChapterDto>(chapterRawJson);

            if (chapterData?.Ayahs is null || chapterData.Ayahs.Count == 0)
            {
                Console.WriteLine($"Skipping chapter file {chapterFileName}.json because ayahs array is empty or invalid.");
                continue;
            }

            foreach (var ayah in chapterData.Ayahs)
            {
                if (string.IsNullOrWhiteSpace(ayah.Text))
                {
                    Console.WriteLine($"Skipping Chapter {ayah.Surah}, Verse {ayah.Ayah} because text is empty.");
                    skippedCount++;
                    continue;
                }

                Console.WriteLine($"Processing Chapter {ayah.Surah}, Verse {ayah.Ayah}...");

                try
                {
                    var embedding = await aiService.GetEmbeddingAsync(ayah.Text);

                    var verse = new VerseModel
                    {
                        Id = $"{ayah.Surah}:{ayah.Ayah}",
                        Text = ayah.Text,
                        Vector = embedding
                    };

                    await verseRepository.InsertAsync(verse);
                    insertedCount++;
                }
                catch (MongoWriteException mongoWriteException) when (mongoWriteException.WriteError?.Category == ServerErrorCategory.DuplicateKey)
                {
                    Console.WriteLine($"Skipping Chapter {ayah.Surah}, Verse {ayah.Ayah} because it already exists.");
                    skippedCount++;
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Failed Chapter {ayah.Surah}, Verse {ayah.Ayah}: {exception.Message}");
                    failedCount++;
                }

                await Task.Delay(100);
            }
        }

        return Ok(new
        {
            totalFiles = chapterFiles.Length,
            inserted = insertedCount,
            skipped = skippedCount,
            failed = failedCount
        });
    }
}