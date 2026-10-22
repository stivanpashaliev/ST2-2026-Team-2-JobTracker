namespace ST2_2026_Team_2_JobTracker.Services.AI;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/";
    public string Model { get; set; } = "gemini-3.8-flash";

    // Set via dotnet user-secrets or an environment variable, NEVER commit the key.
    public string ApiKey { get; set; } = string.Empty;
}
