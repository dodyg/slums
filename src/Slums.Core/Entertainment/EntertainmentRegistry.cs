namespace Slums.Core.Entertainment;

public sealed class EntertainmentRegistry
{
    private static readonly EntertainmentActivity[] Activities =
    [
        new EntertainmentActivity(
            EntertainmentActivityType.Coffee,
            "Coffee",
            "A quick cup of ahwa with the neighborhood regulars.",
            BaseCost: 8,
            DurationMinutes: 30,
            StressReduction: 8,
            EnergyCost: 0,
            RequiresCafe: true,
            RequiresBar: false,
            RequiresBilliards: false),
        new EntertainmentActivity(
            EntertainmentActivityType.Shisha,
            "Shisha",
            "Apple-flavored smoke curling through the afternoon conversations.",
            BaseCost: 30,
            DurationMinutes: 90,
            StressReduction: 15,
            EnergyCost: 2,
            RequiresCafe: true,
            RequiresBar: false,
            RequiresBilliards: false),
        new EntertainmentActivity(
            EntertainmentActivityType.Billiards,
            "Billiards",
            "A game of pool with the depot workers, winner buys tea.",
            BaseCost: 25,
            DurationMinutes: 60,
            StressReduction: 12,
            EnergyCost: 5,
            RequiresCafe: false,
            RequiresBar: false,
            RequiresBilliards: true),
        new EntertainmentActivity(
            EntertainmentActivityType.BarDrinking,
            "Bar Drinking",
            "Stella and aragi in the back room, voices low, eyes watching.",
            BaseCost: 65,
            DurationMinutes: 120,
            StressReduction: 20,
            EnergyCost: 8,
            RequiresCafe: false,
            RequiresBar: true,
            RequiresBilliards: false),
        new EntertainmentActivity(
            EntertainmentActivityType.FootballWatching,
            "Football Watching",
            "The cafe crowd erupts around a flickering TV as Al-Ahly scores.",
            BaseCost: 14,
            DurationMinutes: 120,
            StressReduction: 18,
            EnergyCost: 3,
            RequiresCafe: true,
            RequiresBar: false,
            RequiresBilliards: false),
        new EntertainmentActivity(
            EntertainmentActivityType.SocialHangout,
            "Social Hangout",
            "Just sitting with friends, trading stories and complaints.",
            BaseCost: 5,
            DurationMinutes: 60,
            StressReduction: 12,
            EnergyCost: 0,
            RequiresCafe: true,
            RequiresBar: false,
            RequiresBilliards: false)
    ];

    public static IReadOnlyList<EntertainmentActivity> AllActivities => Activities;

    public static IEnumerable<EntertainmentActivity> GetActivitiesForLocation(bool hasCafe, bool hasBar, bool hasBilliards)
    {
        return Activities.Where(a =>
            (!a.RequiresCafe || hasCafe) &&
            (!a.RequiresBar || hasBar) &&
            (!a.RequiresBilliards || hasBilliards));
    }
}
