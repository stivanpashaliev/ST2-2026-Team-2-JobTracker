namespace ST2_2026_Team_2_JobTracker.Common.Helpers;

/// <summary>
/// Normalizes MVC input before DataAnnotations validation, so length and URL
/// checks apply to the exact value that will be saved in the database.
/// </summary>
public static class TextNormalization
{
    public static string Required(string? value) => value?.Trim() ?? string.Empty;

    public static string? Optional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
