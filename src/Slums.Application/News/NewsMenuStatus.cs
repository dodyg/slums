namespace Slums.Application.News;

public sealed record NewsMenuStatus(IReadOnlyList<NewsFlashDisplay> Flashes);

public sealed record NewsFlashDisplay(
    string Id,
    string Headline,
    string Body,
    string Source,
    string Reliability,
    int DaysRemaining,
    IReadOnlyList<string> AffectedAreas,
    IReadOnlyList<NewsResponseDisplay> Responses,
    bool Acknowledged);

public sealed record NewsResponseDisplay(
    string Id,
    string Label,
    string CostSummary,
    bool IsAvailable,
    string DisabledReason);
