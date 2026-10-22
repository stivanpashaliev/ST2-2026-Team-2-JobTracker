using ST2_2026_Team_2_JobTracker.DesignPatterns.Strategy;
using ST2_2026_Team_2_JobTracker.Models.Enums;

namespace ST2_2026_Team_2_JobTracker.DesignPatterns.Factory;

/// <summary>
/// FACTORY PATTERN: creates (selects) the desired strategy from DI-registered implementations.
/// No controller/service code needs to know concrete strategy types.
/// </summary>
public sealed class MatchingStrategyFactory
{
    private readonly IReadOnlyDictionary<MatchingMode, IMatchingStrategy> _strategies;

    public MatchingStrategyFactory(IEnumerable<IMatchingStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(strategy => strategy.Mode);
    }

    public IMatchingStrategy Create(MatchingMode mode) =>
        _strategies.TryGetValue(mode, out var strategy)
            ? strategy
            : throw new ArgumentOutOfRangeException(nameof(mode), mode,
                "Unknown matching strategy.");
}
