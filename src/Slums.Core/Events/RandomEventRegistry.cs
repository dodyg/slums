using Slums.Core.World;

namespace Slums.Core.Events;

public static class RandomEventRegistry
{
    private static readonly RandomEvent[] DefaultEvents =
    [
        new(
            "MotherHealthScare",
            "Your mother has a bad morning and needs extra care.",
            new RandomEventEffect { MotherHealthChange = -10, InkKnot = "event_mother_health_scare" },
            3,
            10,
            static state => state.Player.Household.MotherHealth < 50),
        new(
            "NeighborhoodRumor",
            "Rumors spread that the police are asking questions in the neighborhood.",
            new RandomEventEffect { StressChange = 10, InkKnot = "event_neighborhood_rumor" },
            3,
            12,
            static state => state.PolicePressure >= 60),
        new(
            "UnexpectedWork",
            "A neighbor offers a quick errand for cash.",
            new RandomEventEffect { MoneyChange = 20 },
            4,
            6,
            static state => state.World.CurrentLocationId == LocationId.Home),
        new(
            "PowerCut",
            "The power cuts out again, leaving everyone tired and irritable.",
            new RandomEventEffect { EnergyChange = -10 },
            3,
            8,
            static state => state.World.CurrentDistrict == DistrictId.Imbaba),
        new(
            "GoodNeighbour",
            "A good neighbour presses a bag of bread into your hand.",
            new RandomEventEffect { FoodChange = 1 },
            3,
            7,
            static state => state.World.CurrentLocationId == LocationId.Market),
        new(
            "ClinicOverflow",
            "Rahma Clinic is overflowing and someone slips you a little cash to stay late.",
            new RandomEventEffect { MoneyChange = 18, StressChange = 4, InkKnot = "event_clinic_overflow" },
            4,
            7,
            static state => state.World.CurrentLocationId == LocationId.Clinic),
        new(
            "WorkshopRushOrder",
            "A rush garment order leaves the workshop stifling and relentless.",
            new RandomEventEffect { MoneyChange = 12, EnergyChange = -8, StressChange = 6, InkKnot = "event_workshop_rush_order" },
            4,
            7,
            static state => state.World.CurrentLocationId == LocationId.Workshop),
        new(
            "CafeSpill",
            "A spilled tea tray earns you a sharp lesson and a small tip anyway.",
            new RandomEventEffect { MoneyChange = 10, StressChange = 3, InkKnot = "event_cafe_spill" },
            4,
            6,
            static state => state.World.CurrentLocationId == LocationId.Cafe)
    ];

    private static IReadOnlyList<RandomEvent> _events = DefaultEvents;

    public static IReadOnlyList<RandomEvent> AllEvents => _events;

    public static void Configure(IEnumerable<RandomEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var configuredEvents = events.Where(static item => item is not null).ToArray();
        _events = configuredEvents.Length == 0 ? DefaultEvents : configuredEvents;
    }
}