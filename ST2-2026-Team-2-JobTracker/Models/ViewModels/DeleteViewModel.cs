namespace ST2_2026_Team_2_JobTracker.Models.ViewModels;

public class DeleteViewModel
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? BlockingMessage { get; set; }
}
