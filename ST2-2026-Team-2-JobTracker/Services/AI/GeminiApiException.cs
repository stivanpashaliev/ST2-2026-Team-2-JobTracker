namespace ST2_2026_Team_2_JobTracker.Services.AI;

// Only safe, user-facing error messages; never includes API keys or raw provider responses.
public sealed class GeminiApiException(string message) : Exception(message)
{
}
