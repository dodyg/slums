namespace Slums.Application.News;

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
