using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class TalkNpcStatusQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<TalkNpcStatus> GetStatuses(GameState gameState)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameState);

        return gameState
            .GetReachableNpcs()
            .Select(npcId => BuildStatus(gameState, npcId))
            .ToArray();
    }

    private static TalkNpcStatus BuildStatus(GameState gameState, NpcId npcId)
    {
        var relationship = gameState.Relationships.GetNpcRelationship(npcId);
        return new TalkNpcStatus(
            npcId,
            NpcRegistry.GetName(npcId),
            relationship.Trust,
            GetSummary(gameState, npcId, relationship),
            GetFactionLink(gameState, npcId),
            GetMemoryFlags(gameState, relationship));
    }

    private static string GetSummary(GameState gameState, NpcId npcId, NpcRelationship relationship)
    {
        var maintainingDoubleLife = gameState.HonestShiftsCompleted >= 3 && gameState.CrimesCommitted > 0;

        return npcId switch
        {
            NpcId.LandlordHajjMahmoud when relationship.Trust <= -15 => "Hostile over money and respect. Rent talk will be tense.",
            NpcId.LandlordHajjMahmoud when relationship.Trust >= 15 => "Warm enough that negotiation may stay civil.",
            NpcId.LandlordHajjMahmoud => "Watching your reliability more than your excuses.",
            NpcId.FixerUmmKarim when relationship.LastRefusalDay > 0 && gameState.Clock.Day - relationship.LastRefusalDay <= 3 => "She remembers recent hesitation and is testing your nerve.",
            NpcId.FixerUmmKarim when gameState.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation >= 15 => "Your local standing makes business talk easier.",
            NpcId.FixerUmmKarim => "Still deciding whether you are useful or just desperate.",
            NpcId.OfficerKhalid when gameState.PolicePressure >= 70 => "Checkpoint mood. The heat is changing how he reads you.",
            NpcId.OfficerKhalid => "Routine on the surface, but never casual.",
            NpcId.NeighborMona when relationship.WasHelped => "She remembers mutual help and treats you like part of the stairwell.",
            NpcId.NeighborMona when relationship.Trust >= 15 => "Neighborly warmth is solid for now.",
            NpcId.NeighborMona => "Friendly, but still measuring what kind of trouble follows you.",
            NpcId.NurseSalma when relationship.HasUnpaidDebt => "You still owe her for help she should not have had to give.",
            NpcId.NurseSalma when maintainingDoubleLife => "She notices when your stories and your days stop matching.",
            NpcId.NurseSalma when relationship.Trust >= 15 => "Trust is strong enough for softer conversations.",
            NpcId.NurseSalma => "Professional, careful, and not easy to fool.",
            NpcId.WorkshopBossAbuSamir when relationship.WasEmbarrassed => "Your last mistake still hangs in the room.",
            NpcId.WorkshopBossAbuSamir when relationship.Trust >= 15 => "He trusts your hands more than before.",
            NpcId.WorkshopBossAbuSamir => "Still judging whether you are worth the table space.",
            NpcId.CafeOwnerNadia when maintainingDoubleLife => "She notices when work clothes and street heat overlap.",
            NpcId.CafeOwnerNadia when relationship.Trust >= 15 => "She is warmer with you than she used to be.",
            NpcId.CafeOwnerNadia => "Watching how you handle pressure in public.",
            NpcId.FenceHanan when gameState.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation >= 15 => "Your standing makes her less guarded.",
            NpcId.FenceHanan => "Transactional, sharp, and never sentimental.",
            NpcId.RunnerYoussef when gameState.PolicePressure >= 70 => "He is restless; the route is too hot to ignore.",
            NpcId.RunnerYoussef => "Quick, alert, and always half-turned toward the street.",
            NpcId.PharmacistMariam when relationship.Trust >= 15 => "She treats you like a helper, not just another woman pricing painkillers.",
            NpcId.PharmacistMariam => "Calm, exact, and always counting what families can no longer afford.",
            NpcId.DispatcherSafaa when relationship.Trust >= 15 => "She trusts you to handle the depot without getting swallowed by it.",
            NpcId.DispatcherSafaa => "Sharp-voiced, fast-moving, and measuring who can survive the route board.",
            NpcId.LaundryOwnerIman when relationship.Trust >= 15 => "She trusts your hands with the cleaner work and the front counter.",
            NpcId.LaundryOwnerIman => "Practical, overheated, and always one ruined shirt away from anger.",
            _ => "Hard to read."
        };
    }

    private static string? GetFactionLink(GameState gameState, NpcId npcId)
    {
        return npcId switch
        {
            NpcId.FixerUmmKarim or NpcId.FenceHanan => $"Imbaba Crew: {gameState.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation}",
            NpcId.RunnerYoussef => $"Dokki Thugs: {gameState.Relationships.GetFactionStanding(FactionId.DokkiThugs).Reputation}",
            NpcId.OfficerKhalid => $"Police pressure: {gameState.PolicePressure}",
            _ => null
        };
    }

    private static List<string> GetMemoryFlags(GameState gameState, NpcRelationship relationship)
    {
        var flags = new List<string>();

        if (relationship.HasUnpaidDebt)
        {
            flags.Add("Unpaid debt");
        }

        if (relationship.WasEmbarrassed)
        {
            flags.Add("Remembers embarrassment");
        }

        if (relationship.WasHelped)
        {
            flags.Add("Remembers help");
        }

        if (relationship.LastFavorDay > 0)
        {
            flags.Add($"Last favor: day {relationship.LastFavorDay}");
        }

        if (relationship.LastRefusalDay > 0)
        {
            flags.Add($"Last refusal: day {relationship.LastRefusalDay}");
        }

        if (relationship.RecentContactCount > 0)
        {
            flags.Add($"Recent contact: {relationship.RecentContactCount}");
        }

        if (relationship.LastSeenDay > 0)
        {
            flags.Add($"Last seen: day {relationship.LastSeenDay}");
        }

        if (gameState.Player.Skills.GetLevel(Core.Skills.SkillId.Persuasion) >= 3 && relationship.Trust >= 0)
        {
            flags.Add("Your Persuasion makes positive trust gains stronger.");
        }

        return flags;
    }
}