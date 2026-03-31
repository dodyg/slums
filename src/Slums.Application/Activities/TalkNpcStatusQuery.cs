using Slums.Core.Relationships;
using Slums.Core.Skills;

namespace Slums.Application.Activities;

public sealed class TalkNpcStatusQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<TalkNpcStatus> GetStatuses(TalkNpcContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        return context
            .ReachableNpcs
            .Select(npcId => BuildStatus(context, npcId))
            .ToArray();
    }

    private static TalkNpcStatus BuildStatus(TalkNpcContext context, NpcId npcId)
    {
        var relationship = context.Relationships.GetNpcRelationship(npcId);
        return new TalkNpcStatus(
            npcId,
            NpcRegistry.GetName(npcId),
            relationship.Trust,
            GetSummary(context, npcId, relationship),
            GetFactionLink(context, npcId),
            GetMemoryFlags(context, relationship),
            GetTriggerSignals(context, npcId, relationship));
    }

    private static string GetSummary(TalkNpcContext context, NpcId npcId, NpcRelationship relationship)
    {
        var maintainingDoubleLife = context.HonestShiftsCompleted >= 3 && context.CrimesCommitted > 0;

        return npcId switch
        {
            NpcId.LandlordHajjMahmoud when context.Player.Stats.Money < 40 && relationship.Trust >= 15 => "You are short on rent, but he still sees you as a woman trying to keep her word.",
            NpcId.LandlordHajjMahmoud when context.Player.Stats.Money < 40 => "You are visibly short this week. Rent talk will be about what you cannot pay.",
            NpcId.LandlordHajjMahmoud when relationship.Trust <= -15 => "Hostile over money and respect. Rent talk will be tense.",
            NpcId.LandlordHajjMahmoud when relationship.Trust >= 15 => "Warm enough that negotiation may stay civil.",
            NpcId.LandlordHajjMahmoud => "Watching your reliability more than your excuses.",
            NpcId.FixerUmmKarim when maintainingDoubleLife && relationship.Trust >= 10 => "She sees the honest-work cover and the criminal ambition together, and is judging whether that makes you useful or sloppy.",
            NpcId.FixerUmmKarim when relationship.Trust >= 25 => "She has stopped testing whether you are serious and started testing whether you are useful under pressure.",
            NpcId.FixerUmmKarim when relationship.LastRefusalDay > 0 && context.CurrentDay - relationship.LastRefusalDay <= 3 => "She remembers recent hesitation and is testing your nerve.",
            NpcId.FixerUmmKarim when context.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation >= 15 => "Your local standing makes business talk easier.",
            NpcId.FixerUmmKarim => "Still deciding whether you are useful or just desperate.",
            NpcId.OfficerKhalid when context.PolicePressure >= 70 => "Checkpoint mood. The heat is changing how he reads you.",
            NpcId.OfficerKhalid when relationship.Trust <= -10 => "He has started filing you under trouble even on quieter days.",
            NpcId.OfficerKhalid => "Routine on the surface, but never casual.",
            NpcId.NeighborMona when context.PolicePressure >= 70 && context.CrimesCommitted > 0 => "She can feel police attention moving through the building and is deciding how much she dares to say out loud.",
            NpcId.NeighborMona when context.Player.Stats.Money < 40 => "She can see the week tightening around you and is deciding how directly to say it.",
            NpcId.NeighborMona when relationship.WasHelped => "She remembers mutual help and treats you like part of the stairwell.",
            NpcId.NeighborMona when relationship.Trust >= 15 => "Neighborly warmth is solid for now.",
            NpcId.NeighborMona => "Friendly, but still measuring what kind of trouble follows you.",
            NpcId.NurseSalma when relationship.HasUnpaidDebt && relationship.Trust >= 15 => "You still owe her, but the debt has become personal enough to test character rather than bookkeeping.",
            NpcId.NurseSalma when relationship.HasUnpaidDebt => "You still owe her for help she should not have had to give.",
            NpcId.NurseSalma when context.Player.Household.MotherHealth < 40 => "She is likely to stop talking in generalities and focus on your mother's condition.",
            NpcId.NurseSalma when maintainingDoubleLife => "She notices when your stories and your days stop matching.",
            NpcId.NurseSalma when relationship.Trust >= 15 => "Trust is strong enough for softer conversations.",
            NpcId.NurseSalma => "Professional, careful, and not easy to fool.",
            NpcId.WorkshopBossAbuSamir when relationship.WasEmbarrassed => "Your last mistake still hangs in the room.",
            NpcId.WorkshopBossAbuSamir when relationship.Trust <= -10 => "He is past annoyance and into open coldness.",
            NpcId.WorkshopBossAbuSamir when relationship.Trust >= 15 => "He trusts your hands more than before.",
            NpcId.WorkshopBossAbuSamir => "Still judging whether you are worth the table space.",
            NpcId.CafeOwnerNadia when maintainingDoubleLife => "She notices when work clothes and street heat overlap.",
            NpcId.CafeOwnerNadia when relationship.Trust <= -10 => "She is no longer covering the room with charm when you walk in.",
            NpcId.CafeOwnerNadia when relationship.Trust >= 15 => "She is warmer with you than she used to be.",
            NpcId.CafeOwnerNadia => "Watching how you handle pressure in public.",
            NpcId.FenceHanan when relationship.Trust <= -10 => "She thinks you are more noise than margin.",
            NpcId.FenceHanan when context.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation >= 15 => "Your standing makes her less guarded.",
            NpcId.FenceHanan => "Transactional, sharp, and never sentimental.",
            NpcId.RunnerYoussef when context.PolicePressure >= 70 => "He is restless; the route is too hot to ignore.",
            NpcId.RunnerYoussef when relationship.Trust >= 15 && context.CrimesCommitted >= 2 => "He talks to you like someone already half inside the route network.",
            NpcId.RunnerYoussef => "Quick, alert, and always half-turned toward the street.",
            NpcId.PharmacistMariam when context.Player.Household.MotherHealth < 40 => "She is likely to answer as a pharmacist first and a conversationalist second.",
            NpcId.PharmacistMariam when relationship.Trust >= 15 => "She treats you like a helper, not just another woman pricing painkillers.",
            NpcId.PharmacistMariam => "Calm, exact, and always counting what families can no longer afford.",
            NpcId.DispatcherSafaa when relationship.RecentContactCount >= 3 => "You are becoming a known face in the depot rhythm, which changes the tone.",
            NpcId.DispatcherSafaa when relationship.Trust >= 15 => "She trusts you to handle the depot without getting swallowed by it.",
            NpcId.DispatcherSafaa => "Sharp-voiced, fast-moving, and measuring who can survive the route board.",
            NpcId.LaundryOwnerIman when context.Player.Stats.Money < 50 => "She can read a lean week in the way you ask questions about cloth and cost.",
            NpcId.LaundryOwnerIman when relationship.Trust >= 15 => "She trusts your hands with the cleaner work and the front counter.",
            NpcId.LaundryOwnerIman => "Practical, overheated, and always one ruined shirt away from anger.",
            _ => "Hard to read."
        };
    }

    private static string? GetFactionLink(TalkNpcContext context, NpcId npcId)
    {
        return npcId switch
        {
            NpcId.FixerUmmKarim or NpcId.FenceHanan => $"Imbaba Crew: {context.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation}",
            NpcId.RunnerYoussef => $"Dokki Thugs: {context.Relationships.GetFactionStanding(FactionId.DokkiThugs).Reputation}",
            NpcId.OfficerKhalid => $"Police pressure: {context.PolicePressure}",
            _ => null
        };
    }

    private static List<string> GetMemoryFlags(TalkNpcContext context, NpcRelationship relationship)
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

        if (context.Player.Skills.GetLevel(SkillId.Persuasion) >= 3 && relationship.Trust >= 0)
        {
            flags.Add("Your Persuasion makes positive trust gains stronger.");
        }

        return flags;
    }

    private static List<string> GetTriggerSignals(TalkNpcContext context, NpcId npcId, NpcRelationship relationship)
    {
        var signals = new List<string>();
        var maintainingDoubleLife = context.HonestShiftsCompleted >= 3 && context.CrimesCommitted > 0;

        switch (npcId)
        {
            case NpcId.LandlordHajjMahmoud:
                if (context.Player.Stats.Money < 40)
                {
                    signals.Add("Low money is forcing the conversation onto rent.");
                }

                if (relationship.Trust >= 15)
                {
                    signals.Add("High trust is softening how he handles the shortage.");
                }
                break;

            case NpcId.FixerUmmKarim:
                if (maintainingDoubleLife && relationship.Trust >= 10)
                {
                    signals.Add("Your honest shifts and crime history are both visible to her now.");
                }

                if (relationship.LastRefusalDay > 0 && context.CurrentDay - relationship.LastRefusalDay <= 3)
                {
                    signals.Add("Your recent refusal is still fresh in her memory.");
                }
                break;

            case NpcId.NeighborMona:
                if (context.PolicePressure >= 70 && context.CrimesCommitted > 0)
                {
                    signals.Add("Police heat is close enough to home for Mona to change her tone.");
                }

                if (relationship.WasHelped)
                {
                    signals.Add("Past mutual help is making the stairwell feel more like a side than a hallway.");
                }
                break;

            case NpcId.NurseSalma:
                if (relationship.HasUnpaidDebt)
                {
                    signals.Add("You still owe Salma for help she already gave.");
                }

                if (relationship.HasUnpaidDebt && relationship.Trust >= 15)
                {
                    signals.Add("High trust is changing the debt from bookkeeping into concern.");
                }

                if (maintainingDoubleLife)
                {
                    signals.Add("Your double life is starting to show in a clinical setting.");
                }
                break;

            case NpcId.OfficerKhalid:
                if (context.PolicePressure >= 70)
                {
                    signals.Add("High pressure is making Khalid's attention feel personal.");
                }

                if (context.CrimesCommitted >= 2 && relationship.Trust <= 0)
                {
                    signals.Add("Your record and his distrust are a dangerous combination.");
                }
                break;

            case NpcId.WorkshopBossAbuSamir:
                if (relationship.WasEmbarrassed)
                {
                    signals.Add("The embarrassment still hangs between you.");
                }

                if (relationship.WasEmbarrassed && relationship.Trust >= 5)
                {
                    signals.Add("Your recovery is starting to earn back his patience.");
                }
                break;

            case NpcId.CafeOwnerNadia:
                if (maintainingDoubleLife && relationship.Trust >= 10)
                {
                    signals.Add("Nadia can see the gap between your work clothes and your street.");
                }
                break;

            case NpcId.RunnerYoussef:
                if (context.CrimesCommitted >= 3 && relationship.Trust >= 15)
                {
                    signals.Add("Youssef is treating you like part of the route, not just a customer.");
                }
                break;

            case NpcId.FenceHanan:
                if (context.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation >= 15)
                {
                    signals.Add("Your faction standing makes Hanan more willing to share.");
                }
                break;

            case NpcId.PharmacistMariam:
                if (context.Player.Household.MotherHealth < 40)
                {
                    signals.Add("Your mother's condition is pressing Mariam toward urgency.");
                }
                break;

            case NpcId.DispatcherSafaa:
                if (relationship.RecentContactCount >= 3)
                {
                    signals.Add("Regular contact is making Safaa treat you like part of the depot rhythm.");
                }
                break;

            case NpcId.LaundryOwnerIman:
                if (context.Player.Stats.Money < 50 && relationship.Trust >= 10)
                {
                    signals.Add("Iman can see the lean week and may quietly offer help.");
                }
                break;
        }

        return signals;
    }
}
