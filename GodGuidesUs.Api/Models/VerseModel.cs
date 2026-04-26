using MongoDB.Bson.Serialization.Attributes;

namespace GodGuidesUs.Api.Models;

public class VerseModel
{
    [BsonId]
    public string Id { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public string Theme { get; set; } = string.Empty;

    public string Commentary { get; set; } = string.Empty;

    public float[] Vector { get; set; } = [];
}