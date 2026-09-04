using Slums.Core.Diagnostics;
using Slums.Core.State;

namespace Slums.Core.Entertainment;

/// <summary>Applies entertainment availability and activity rules to a session.</summary>
internal static class EntertainmentService
{
    internal static IReadOnlyList<EntertainmentActivity> GetAvailableActivities(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var location = session.World.GetCurrentLocation();
        if (location is null)
        {
            return [];
        }

        return EntertainmentRegistry.GetActivitiesForLocation(
            location.HasCafe,
            location.HasBar,
            location.HasBilliards).ToArray();
    }

    internal static bool Perform(GameSession session, EntertainmentActivity activity)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(activity);
        var before = session.CaptureStats();

        if (session.Player.Stats.Money < activity.BaseCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryPerformEntertainment", before, session.CaptureStats(), $"Cannot afford {activity.Name} (cost {activity.BaseCost} LE, have {session.Player.Stats.Money} LE)");
            session.RaiseEvent($"You cannot afford {activity.Name} right now.");
            return false;
        }

        if (session.Player.Stats.Energy < activity.EnergyCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryPerformEntertainment", before, session.CaptureStats(), $"Too tired for {activity.Name} (need {activity.EnergyCost} energy, have {session.Player.Stats.Energy})");
            session.RaiseEvent($"You are too tired for {activity.Name}.");
            return false;
        }

        var location = session.World.GetCurrentLocation();
        if (location is null)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryPerformEntertainment", before, session.CaptureStats(), "No current location");
            session.RaiseEvent("You are nowhere.");
            return false;
        }

        var availableActivities = GetAvailableActivities(session);
        if (!availableActivities.Contains(activity))
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryPerformEntertainment", before, session.CaptureStats(), $"{activity.Name} not available here");
            session.RaiseEvent($"{activity.Name} is not available here.");
            return false;
        }

        session.Player.Stats.ModifyMoney(-activity.BaseCost);
        session.Player.Stats.ModifyStress(-activity.StressReduction);
        if (activity.EnergyCost > 0)
        {
            session.Player.Stats.ModifyEnergy(-activity.EnergyCost);
        }

        session.RaiseEvent(GetFlavorMessage(activity));
        session.RecordMutation(MutationCategories.Entertainment, "TryPerformEntertainment", before, session.CaptureStats(), $"{activity.Name} (cost {activity.BaseCost} LE, stress -{activity.StressReduction})");
        session.AdvanceTime(activity.DurationMinutes);
        return true;
    }

    private static string GetFlavorMessage(EntertainmentActivity activity)
    {
        return activity.Type switch
        {
            EntertainmentActivityType.Coffee => "The coffee is strong and bitter. You feel a little lighter.",
            EntertainmentActivityType.Shisha => "Apple smoke curls around you. The afternoon drifts by.",
            EntertainmentActivityType.Billiards => "You win some, you lose some. The company is good.",
            EntertainmentActivityType.BarDrinking => "The drink burns going down. For a while, things feel far away.",
            EntertainmentActivityType.FootballWatching => "The crowd screams at the TV. You scream with them.",
            EntertainmentActivityType.SocialHangout => "Just talking. Just listening. It helps.",
            _ => $"You spent some time on {activity.Name}."
        };
    }
}
