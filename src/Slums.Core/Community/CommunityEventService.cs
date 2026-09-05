using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Diagnostics;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Community;

/// <summary>Applies community event attendance and early emergency support rules.</summary>
internal static class CommunityEventService
{
    internal static IReadOnlyList<CommunityEventDefinition> GetAvailable(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var events = new List<CommunityEventDefinition>();
        var dayOfWeek = session.Clock.DayOfWeek;
        var isRamadan = session.RamadanState.IsActive;

        foreach (var evt in CommunityEventRegistry.AllEvents)
        {
            if (evt.RequiresFriday && dayOfWeek != GameDayOfWeek.Friday)
            {
                continue;
            }

            if (evt.RequiresRamadan && !isRamadan)
            {
                continue;
            }

            if (evt.RequiresNpcInvitation && !session.EventAttendance.HasTeaCircleInvitation)
            {
                continue;
            }

            if (evt.HasPickpocketRisk && session.World.CurrentDistrict != DistrictId.Imbaba)
            {
                continue;
            }

            events.Add(evt);
        }

        return events;
    }

    internal static bool Attend(GameSession session, CommunityEventId eventId, Random? random)
    {
        ArgumentNullException.ThrowIfNull(session);
        var definition = CommunityEventRegistry.GetById(eventId);
        if (definition is null)
        {
            return false;
        }

        var before = session.CaptureStats();
        random ??= session.SharedRandom;
        var available = GetAvailable(session);
        if (available.All(evt => evt.Id != eventId))
        {
            session.RaiseEvent($"{definition.Name} is not available right now.");
            session.RecordMutation(MutationCategories.GuardRejected, "AttendCommunityEvent", before, session.CaptureStats(), "Event not available");
            return false;
        }

        if (session.EventAttendance.AttendedThisWeek.Contains(eventId))
        {
            session.RaiseEvent($"You already attended {definition.Name} this week.");
            session.RecordMutation(MutationCategories.GuardRejected, "AttendCommunityEvent", before, session.CaptureStats(), "Already attended this week");
            return false;
        }

        if (session.Player.Stats.Money < definition.MoneyCost)
        {
            session.RaiseEvent($"You cannot afford the {definition.MoneyCost} LE contribution.");
            session.RecordMutation(MutationCategories.GuardRejected, "AttendCommunityEvent", before, session.CaptureStats(), $"Cannot afford {definition.MoneyCost} LE");
            return false;
        }

        if (!session.CanCompleteActivityToday(definition.TimeCostMinutes))
        {
            session.RaiseEvent("Not enough time in the day for that.");
            session.RecordMutation(MutationCategories.GuardRejected, "AttendCommunityEvent", before, session.CaptureStats(), "Not enough time");
            return false;
        }

        if (definition.MoneyCost > 0)
        {
            session.Player.Stats.ModifyMoney(-definition.MoneyCost);
        }

        session.Player.Stats.ModifyStress(definition.StressChange);
        var trustGained = ApplyTrust(session, definition, random);
        var backgroundBonus = ApplyBackgroundBonus(session, definition);
        if (definition.ProvidesFoodAccess)
        {
            session.Player.Nutrition.Eat(MealQuality.Basic);
        }

        if (definition.HasPickpocketRisk)
        {
#pragma warning disable CA5394
            var roll = random.Next(100);
            if (roll < 10)
            {
                var stolen = random.Next(5, 16);
#pragma warning restore CA5394
                session.Player.Stats.ModifyMoney(-stolen);
                session.RaiseEvent($"A pickpocket slips away with {stolen} LE from your pocket!");
            }
        }

        session.EventAttendance.RecordAttendance(eventId, session.Clock.Day);
        var trustMessage = trustGained > 0 ? $" Trust +{trustGained} with neighbors." : "";
        var backgroundMessage = backgroundBonus > 0 ? $" Background bonus: +{backgroundBonus} trust." : "";
        session.RaiseEvent($"You attend {definition.Name}. Stress {definition.StressChange}.{trustMessage}{backgroundMessage}");
        session.RecordMutation(MutationCategories.Community, "AttendCommunityEvent", before, session.CaptureStats(), $"{definition.Name} (stress {definition.StressChange}, trust gained: {trustGained})");
        session.AdvanceTime(definition.TimeCostMinutes);
        return true;
    }

    internal static bool RequestEmergencySupport(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        if (!session.CanRequestEmergencySupport)
        {
            session.RaiseEvent("No emergency community support is available for this run.");
            session.RecordMutation(MutationCategories.GuardRejected, "RequestEmergencySupport", before, session.CaptureStats(), "Support already claimed, expired, or background not selected");
            return false;
        }

        session.ClaimEmergencySupport();
        switch (session.Player.BackgroundType)
        {
            case BackgroundType.MedicalSchoolDropout:
                session.Player.Household.AddMedicine(2);
                session.Relationships.ModifyNpcTrust(NpcId.NurseSalma, 2);
                session.RaiseEvent("Salma puts two clinic doses aside for your mother. You spend an hour collecting them and promising to return the favor.");
                break;
            case BackgroundType.ReleasedPoliticalPrisoner:
                session.Player.Stats.ModifyMoney(30);
                session.Player.Household.AddStaples(1);
                session.Relationships.ModifyNpcTrust(NpcId.NeighborMona, 2);
                session.RaiseEvent("Mona gathers a small mutual-aid envelope and one food parcel. It is help, not a solution, and it costs an hour to arrange safely.");
                break;
            case BackgroundType.SudaneseRefugee:
                session.Player.Household.AddStaples(3);
                session.Player.Stats.ModifyStress(-4);
                session.Relationships.ModifyNpcTrust(NpcId.NeighborMona, 2);
                session.RaiseEvent("The Sudanese women's kitchen sends bread, beans, and tea upstairs. You spend an hour carrying containers back through the lane.");
                break;
            default:
                throw new InvalidOperationException($"Unsupported background {session.Player.BackgroundType}.");
        }

        session.AdvanceTime(GameSession.EmergencySupportDurationMinutes);
        session.RecordMutation(MutationCategories.Community, "RequestEmergencySupport", before, session.CaptureStats(), $"Emergency support claimed for {session.Player.BackgroundType}");
        return true;
    }

    private static int ApplyTrust(GameSession session, CommunityEventDefinition definition, Random random)
    {
        var communityNpcs = new[] { NpcId.LandlordHajjMahmoud, NpcId.FixerUmmKarim, NpcId.NeighborMona, NpcId.NurseSalma, NpcId.CafeOwnerNadia };
        var count = Math.Min(definition.TrustGainCount, communityNpcs.Length);
#pragma warning disable CA5394
        var selected = communityNpcs.OrderBy(_ => random.Next()).Take(count).ToArray();
#pragma warning restore CA5394
        var totalTrust = 0;
        foreach (var npcId in selected)
        {
            var trust = definition.TrustGainAmount
                + (session.Player.Skills.GetLevel(SkillId.CommunityOrganizing) >= SkillThresholds.FirstMeaningfulLevel ? 1 : 0);
            session.Relationships.ModifyNpcTrust(npcId, trust);
            totalTrust += trust;
        }

        return totalTrust;
    }

    private static int ApplyBackgroundBonus(GameSession session, CommunityEventDefinition definition)
    {
        var bonus = 0;
        var background = session.Player.BackgroundType;
        if (background == BackgroundType.SudaneseRefugee && definition.Id == CommunityEventId.FridayRooftopGathering)
        {
            bonus = 2;
            session.Relationships.ModifyNpcTrust(NpcId.NeighborMona, bonus);
        }
        else if (background == BackgroundType.ReleasedPoliticalPrisoner)
        {
            if (session.EventAttendance.TotalAttended <= 3)
            {
                return 0;
            }

            bonus = 1;
        }
        else if (background == BackgroundType.MedicalSchoolDropout)
        {
            bonus = 1;
            session.Relationships.ModifyNpcTrust(NpcId.NurseSalma, bonus);
        }

        return bonus;
    }
}
