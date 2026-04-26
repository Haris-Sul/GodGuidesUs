using System.Text.Json.Serialization;

namespace GodGuidesUs.Api.Models;

public class TafsirChapterDto
{
    [JsonPropertyName("ayahs")]
    public List<TafsirAyahDto> Ayahs { get; set; } = [];
}