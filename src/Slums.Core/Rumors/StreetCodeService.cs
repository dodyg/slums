using Slums.Core.Information;
using Slums.Core.Heat;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Rumors;

/// <summary>Applies the bounded social consequences of being seen with Officer Khalid.</summary>
public static class StreetCodeService
{
    private const int BaseObservationChance = 40;

    public static bool ObserveOfficerConversation(GameSession session, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        random ??= session.SharedRandom;

        if (session.World.CurrentLocationId == LocationId.Home || !IsRecentCrime(session))
        {
            return false;
        }

        AddWarningTip(session);

        var chance = session.LastPublicFacingWorkDay == session.Clock.Day ? 15 : BaseObservationChance;
        if (session.PolicePressure >= PolicePressureThresholds.Elevated)
        {
            chance += 10;
        }

        if (session.Relationships.GetNpcRelationship(NpcId.OfficerKhalid).Trust >= 20)
        {
            chance /= 2;
        }

#pragma warning disable CA5394
        if (random.Next(100) >= chance)
        {
            return false;
        }
#pragma warning restore CA5394

        session.Rumors.AddRumor(RumorGenerator.OnSeenWithPolice(session.World.CurrentDistrict, session.Clock.Day));
        session.RaiseEvent("Someone notices you speaking with Officer Khalid. In Cairo, observation becomes rumor before you get home.");
        return true;
    }

    public static void ApplyRumorConsequence(GameSession session, Rumor rumor, NpcId npcId)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(rumor);

        if (rumor.Id != RumorId.SeenWithPolice || !TryGetFaction(npcId, out var faction))
        {
            return;
        }

        if (session.Relationships.GetFactionStanding(faction).Reputation >= 0)
        {
            return;
        }

        var (flag, knot) = faction switch
        {
            FactionId.ImbabaCrew => (StoryFlags.StreetCodeRetaliationImbabaSeen, NarrativeKnots.StreetCodeRetaliationImbaba),
            FactionId.DokkiThugs => (StoryFlags.StreetCodeRetaliationDokkiSeen, NarrativeKnots.StreetCodeRetaliationDokki),
            FactionId.ExPrisonerNetwork => (StoryFlags.StreetCodeRetaliationPrisonerSeen, NarrativeKnots.StreetCodeRetaliationPrisoner),
            _ => (string.Empty, string.Empty)
        };

        if (session.TryQueueNarrativeTrigger(new NarrativeSceneTrigger(flag, knot)))
        {
            session.ModifyFactionReputation(faction, -3);
            session.RaiseEvent($"{NpcRegistry.GetName(npcId)} hears the police rumor. {faction} trust drops before anyone asks for your explanation.");
        }
    }

    private static bool IsRecentCrime(GameSession session)
        => session.LastCrimeDay > 0 && session.Clock.Day - session.LastCrimeDay <= 2;

    private static void AddWarningTip(GameSession session)
    {
        if (session.Tips.GetTipsFromNpc(NpcId.OfficerKhalid).Any(tip =>
                tip.DayGenerated == session.Clock.Day
                && tip.Content.Contains("being seen", StringComparison.Ordinal)))
        {
            return;
        }

        session.Tips.AddTip(new Tip
        {
            Type = TipType.CrimeWarning,
            Source = NpcId.OfficerKhalid,
            Content = "Khalid warns you that being seen with him after a recent job will travel through the street network.",
            DayGenerated = session.Clock.Day,
            ExpiresAfterDay = session.Clock.Day + 1
        });
        session.RaiseEvent("Khalid gives you a warning before the conversation turns personal: recent work and police contact do not stay separate for long.");
    }

    private static bool TryGetFaction(NpcId npcId, out FactionId faction)
    {
        faction = npcId switch
        {
            NpcId.FixerUmmKarim or NpcId.FenceHanan => FactionId.ImbabaCrew,
            NpcId.RunnerYoussef => FactionId.DokkiThugs,
            _ => default
        };
        return npcId is NpcId.FixerUmmKarim or NpcId.FenceHanan or NpcId.RunnerYoussef;
    }
}
