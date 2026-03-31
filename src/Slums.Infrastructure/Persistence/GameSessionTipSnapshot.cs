using Slums.Core.Information;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionTipSnapshot
{
    public IReadOnlyList<TipEntrySnapshot> Tips { get; init; } = [];
    public IReadOnlyDictionary<string, int> IgnoredCounts { get; init; } = new Dictionary<string, int>();

    public static GameSessionTipSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var ignoredCounts = new Dictionary<string, int>();
        foreach (var kvp in gameSession.Tips.IgnoredCounts)
        {
            ignoredCounts[kvp.Key.ToString()] = kvp.Value;
        }

        return new GameSessionTipSnapshot
        {
            Tips = gameSession.Tips.AllTips.Select(static t => new TipEntrySnapshot
            {
                Id = t.Id,
                Type = t.Type.ToString(),
                Source = t.Source.ToString(),
                Content = t.Content,
                DayGenerated = t.DayGenerated,
                ExpiresAfterDay = t.ExpiresAfterDay,
                RelevantDistrict = t.RelevantDistrict?.ToString(),
                Acknowledged = t.Acknowledged,
                Ignored = t.Ignored,
                Delivered = t.Delivered,
                IsEmergency = t.IsEmergency
            }).ToArray(),
            IgnoredCounts = ignoredCounts
        };
    }

    public void Restore(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var tips = Tips.Select(static s => new Tip
        {
            Id = s.Id,
            Type = Enum.Parse<TipType>(s.Type),
            Source = Enum.Parse<NpcId>(s.Source),
            Content = s.Content,
            DayGenerated = s.DayGenerated,
            ExpiresAfterDay = s.ExpiresAfterDay,
            RelevantDistrict = s.RelevantDistrict is not null ? Enum.Parse<DistrictId>(s.RelevantDistrict) : null,
            Acknowledged = s.Acknowledged,
            Ignored = s.Ignored,
            Delivered = s.Delivered,
            IsEmergency = s.IsEmergency
        });

        var ignoredCounts = new Dictionary<NpcId, int>();
        foreach (var kvp in IgnoredCounts)
        {
            ignoredCounts[Enum.Parse<NpcId>(kvp.Key)] = kvp.Value;
        }

        gameSession.RestoreTips(tips, ignoredCounts);
    }
}

public sealed record TipEntrySnapshot
{
    public string Id { get; init; } = "";
    public string Type { get; init; } = "";
    public string Source { get; init; } = "";
    public string Content { get; init; } = "";
    public int DayGenerated { get; init; }
    public int ExpiresAfterDay { get; init; }
    public string? RelevantDistrict { get; init; }
    public bool Acknowledged { get; init; }
    public bool Ignored { get; init; }
    public bool Delivered { get; init; }
    public bool IsEmergency { get; init; }
}
