
namespace ST2_2026_Team_2_JobTracker.DesignPatterns.Singleton;

public sealed class ApplicationSettings
{
    private static readonly Lazy<ApplicationSettings> _instance =
        new(() => new ApplicationSettings());

    public static ApplicationSettings Instance => _instance.Value;

    public string ApplicationName { get; } = "JobTracker";

    public int DefaultPageSize { get; } = 10;

    private ApplicationSettings()
    {
    }
}
