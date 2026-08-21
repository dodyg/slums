namespace Slums.Core.World.News;

public sealed record NewsFlashDefinition
{
    public string Id { get; init; } = string.Empty;
    public NewsCategory Category { get; init; }
    public string Headline { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string SourceLabel { get; init; } = string.Empty;
    public NewsSourceType SourceType { get; init; }
    public NewsReliability Reliability { get; init; }
    public IReadOnlyList<DistrictId> AffectedDistricts { get; init; } = [];
    public int MinimumDay { get; init; }
    public int Weight { get; init; }
    public int CooldownDays { get; init; }
    public int DurationDays { get; init; }
    public string? InkKnot { get; init; }
    public IReadOnlyList<NewsEffectDefinition> Effects { get; init; } = [];
    public IReadOnlyList<NewsResponseDefinition> Responses { get; init; } = [];
}
