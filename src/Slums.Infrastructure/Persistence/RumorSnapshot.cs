using Slums.Core.Relationships;
using Slums.Core.Rumors;
using Slums.Core.World;

namespace Slums.Infrastructure.Persistence;

/// <summary>
/// Persistable representation of an active rumor, including its propagation state so that
/// restored sessions continue rumor decay and NPC trust effects exactly as the original run.
/// </summary>
public sealed record RumorSnapshot(
    string Id,
    string SourceAction,
    string District,
    int DayCreated,
    int InitialIntensity,
    bool IsPositive,
    IReadOnlyList<string> AffectedNpcs,
    int TrustModifier,
    IReadOnlyList<string> NpcsWhoHeard,
    int Intensity,
    int Age)
{
    public static RumorSnapshot Capture(Rumor rumor)
    {
        ArgumentNullException.ThrowIfNull(rumor);

        return new RumorSnapshot(
            rumor.Id.ToString(),
            rumor.SourceAction,
            rumor.District.ToString(),
            rumor.DayCreated,
            rumor.InitialIntensity,
            rumor.IsPositive,
            rumor.AffectedNpcs.Select(static npc => npc.ToString()).ToArray(),
            rumor.TrustModifier,
            rumor.NpcsWhoHeard.Select(static npc => npc.ToString()).ToArray(),
            rumor.Intensity,
            rumor.Age);
    }

    public Rumor Restore()
    {
        return new Rumor(
            Enum.Parse<RumorId>(Id),
            SourceAction,
            Enum.Parse<DistrictId>(District),
            DayCreated,
            InitialIntensity,
            IsPositive,
            AffectedNpcs.Select(static npc => Enum.Parse<NpcId>(npc)).ToHashSet(),
            TrustModifier,
            NpcsWhoHeard.Select(static npc => Enum.Parse<NpcId>(npc)).ToHashSet())
        {
            Intensity = Intensity,
            Age = Age
        };
    }
}
