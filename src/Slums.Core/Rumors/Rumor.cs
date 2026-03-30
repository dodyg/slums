using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Rumors;

public sealed record Rumor(
    RumorId Id,
    string SourceAction,
    DistrictId District,
    int DayCreated,
    int InitialIntensity,
    bool IsPositive,
    IReadOnlySet<NpcId> AffectedNpcs,
    int TrustModifier,
    HashSet<NpcId> NpcsWhoHeard)
{
    public int Intensity { get; set; } = InitialIntensity;
    public int Age { get; set; }

    public bool IsExpired => Intensity <= 0 || Age > 4;

    public void Decay()
    {
        var decayAmount = IsPositive ? 3 : 2;
        Intensity = Math.Max(0, Intensity - decayAmount);
        Age++;
    }
}
