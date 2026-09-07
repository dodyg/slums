namespace Slums.Application.News;

public sealed record NewsResponseDisplay(
    string Id,
    string Label,
    string CostSummary,
    bool IsAvailable,
    string DisabledReason);
