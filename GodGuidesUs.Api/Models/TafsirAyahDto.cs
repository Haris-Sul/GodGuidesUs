using System.Text.Json.Serialization;

namespace GodGuidesUs.Api.Models;

public class TafsirAyahDto
{
    [JsonPropertyName("ayah")]
    public int Ayah { get; set; }

    [JsonPropertyName("surah")]
    public int Surah { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}