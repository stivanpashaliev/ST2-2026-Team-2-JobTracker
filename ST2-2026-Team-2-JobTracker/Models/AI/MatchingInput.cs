namespace ST2_2026_Team_2_JobTracker.Models.AI;

public sealed record MatchingInput(
    string JobTitle,
    string JobDescription,
    string CompanyName,
    string CandidateProfile);
