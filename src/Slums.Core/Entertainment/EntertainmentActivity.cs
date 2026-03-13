namespace Slums.Core.Entertainment;

public sealed record EntertainmentActivity(
    EntertainmentActivityType Type,
    string Name,
    string Description,
    int BaseCost,
    int DurationMinutes,
    int StressReduction,
    int EnergyCost,
    bool RequiresCafe,
    bool RequiresBar,
    bool RequiresBilliards);
