namespace GodGuidesUs.Api.Models;

public class GoogleAiSettings
{
    public const string SectionName = "GoogleAi";

    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/";

    public string ApiKey { get; set; } = string.Empty;
}