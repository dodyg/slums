using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Information;

public sealed record Tip
{
    public string Id { get; init; } = TipId.Generate();
    public TipType Type { get; init; }
    public NpcId Source { get; init; }
    public string Content { get; init; } = "";
    public int DayGenerated { get; init; }
    public int ExpiresAfterDay { get; init; }
    public DistrictId? RelevantDistrict { get; init; }
    public bool Acknowledged { get; init; }
    public bool Ignored { get; init; }
    public bool Delivered { get; init; }
    public bool IsEmergency { get; init; }

    public bool IsExpired(int currentDay)
    {
        return currentDay > ExpiresAfterDay;
    }

    public Tip WithAcknowledged()
    {
        return this with { Acknowledged = true };
    }

    public Tip WithIgnored()
    {
        return this with { Ignored = true };
    }

    public Tip WithDelivered()
    {
        return this with { Delivered = true };
    }
}
