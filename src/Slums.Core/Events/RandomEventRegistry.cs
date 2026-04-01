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
            new RandomEventEffect { StressChange = 8, InkKnot = "event_neighborhood_rumor" },
            3,
            12,
            static state => state.PolicePressure >= 60),
        new(
            "UnexpectedWork",
            "A neighbor offers a quick errand for cash.",
            new RandomEventEffect { MoneyChange = 22 },
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
            "HomeWaterCutCollection",
            "The building's water cuts out and everyone starts bargaining over buckets, stairs, and borrowed patience.",
            new RandomEventEffect { EnergyChange = -4, StressChange = 3, InkKnot = "event_home_water_cut_collection" },
            4,
            7,
            static state => state.World.CurrentLocationId == LocationId.Home && state.Player.Stats.Money < 60),
        new(
            "BakeryFlourShortage",
            "The forn runs short on flour and every tray becomes an argument about whose shift is supposed to absorb the shortage.",
            new RandomEventEffect { MoneyChange = 8, StressChange = 4, InkKnot = "event_bakery_flour_shortage" },
            4,
            6,
            static state => state.World.CurrentLocationId == LocationId.Bakery),
        new(
            "ClinicOverflow",
            "Rahma Clinic is overflowing and someone slips you a little cash to stay late.",
            new RandomEventEffect { MoneyChange = 18, StressChange = 4, InkKnot = "event_clinic_overflow" },
            4,
            7,
            static state => state.World.CurrentLocationId == LocationId.Clinic),
        new(
            "CallCenterScriptChange",
            "A script change hits the call floor without warning and every caller pretends your confusion is a personal insult.",
            new RandomEventEffect { MoneyChange = 6, StressChange = 6, InkKnot = "event_call_center_script_change" },
            5,
            6,
            static state => state.World.CurrentLocationId == LocationId.CallCenter),
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
            static state => state.World.CurrentLocationId == LocationId.Cafe),
        new(
            "NeighborhoodSolidarity",
            "Women in the stairwell quietly route food, warnings, and spare change where they are needed most.",
            new RandomEventEffect { FoodChange = 1, StressChange = -4, InkKnot = "event_neighborhood_solidarity" },
            5,
            9,
            static state => state.World.CurrentDistrict == DistrictId.Imbaba && state.Player.Stats.Stress >= 35),
        new(
            "DokkiCheckpointSweep",
            "A checkpoint sweep in Dokki turns every crossing into a test of tone, paperwork, and luck.",
            new RandomEventEffect { StressChange = 7, PolicePressureChange = 6, InkKnot = "event_dokki_checkpoint_sweep" },
            5,
            8,
            static state => state.World.CurrentDistrict == DistrictId.Dokki && state.PolicePressure >= 35),
        new(
            "DokkiTransportFriction",
            "Microbuses stall, routes break, and half the district arrives late and irritated.",
            new RandomEventEffect { EnergyChange = -6, StressChange = 4, InkKnot = "event_dokki_transport_friction" },
            5,
            7,
            static state => state.World.CurrentDistrict == DistrictId.Dokki && state.GetEventCount("DokkiCheckpointSweep") > 0),
        new(
            "ClinicSupplyShortage",
            "Rahma Clinic runs thin on basics, which means tempers rise before noon.",
            new RandomEventEffect { StressChange = 5, MoneyChange = -8, InkKnot = "event_clinic_supply_shortage" },
            5,
            7,
            static state => state.World.CurrentDistrict == DistrictId.ArdAlLiwa && state.World.CurrentLocationId == LocationId.Clinic),
        new(
            "ArdAlLiwaWorkshopSolidarity",
            "Someone in Ard al-Liwa passes work down the line instead of hoarding it for herself.",
            new RandomEventEffect { MoneyChange = 16, StressChange = -3, InkKnot = "event_ardalliwa_solidarity" },
            5,
            9,
            static state => state.World.CurrentDistrict == DistrictId.ArdAlLiwa && state.Player.Stats.Money < 120),
        new(
            "BulaqMedicineQueue",
            "A discount medicine queue in Bulaq al-Dakrour turns into a lesson in shortages, patience, and fast decisions.",
            new RandomEventEffect { StressChange = 4, MoneyChange = -6, InkKnot = "event_bulaq_medicine_queue" },
            5,
            8,
            static state => state.World.CurrentLocationId == LocationId.Pharmacy),
        new(
            "DepotFareShakeup",
            "At the microbus depot, fares change mid-argument and nobody agrees on who is supposed to absorb the loss.",
            new RandomEventEffect { MoneyChange = 10, StressChange = 5, InkKnot = "event_depot_fare_shakeup" },
            5,
            7,
            static state => state.World.CurrentLocationId == LocationId.Depot),
        new(
            "ShubraSteamBreak",
            "A broken steam line in Shubra leaves the laundry half-flooded, half-functional, and fully tense.",
            new RandomEventEffect { EnergyChange = -7, StressChange = 4, InkKnot = "event_shubra_steam_break" },
            5,
            7,
            static state => state.World.CurrentLocationId == LocationId.Laundry),
        new(
            "ShubraBlockSolidarity",
            "In Shubra, a building committee quietly routes leftovers, repair tips, and warnings to the women carrying too much.",
            new RandomEventEffect { FoodChange = 1, StressChange = -3, InkKnot = "event_shubra_block_solidarity" },
            5,
            8,
            static state => state.World.CurrentDistrict == DistrictId.Shubra && state.Player.Stats.Money < 120),
        new(
            "FishMarketCatch",
            "The early catch is plentiful and someone pays you to haul extra crates before the ice melts.",
            new RandomEventEffect { MoneyChange = 15, EnergyChange = -6 },
            3,
            6,
            static state => state.World.CurrentLocationId == LocationId.FishMarket),
        new(
            "StreetVendorPermitSweep",
            "Police check permits along the square. Vendors scatter and the ones without papers pay in stress and bribes.",
            new RandomEventEffect { StressChange = 6, MoneyChange = -10, InkKnot = "event_vendor_permit_sweep" },
            4,
            7,
            static state => state.World.CurrentLocationId == LocationId.Square && state.PolicePressure >= 25),
        new(
            "HomeRoofLeak",
            "Rain finds the cracks in the roof. Buckets, towels, and a long night of mopping.",
            new RandomEventEffect { EnergyChange = -5, StressChange = 3, MoneyChange = -5, InkKnot = "event_home_roof_leak" },
            3,
            6,
            static state => state.World.CurrentLocationId == LocationId.Home && state.Player.Stats.Money < 80),
        new(
            "MarketDiscountFind",
            "A vendor at the market quietly marks down bruised fruit and day-old bread.",
            new RandomEventEffect { FoodChange = 1, MoneyChange = -3 },
            3,
            7,
            static state => state.World.CurrentLocationId == LocationId.Market),
        new(
            "WorkshopFabricLeftover",
            "End-of-roll fabric gets passed around the workshop instead of hitting the scrap pile.",
            new RandomEventEffect { MoneyChange = 6, StressChange = -2, InkKnot = "event_workshop_fabric_leftover" },
            5,
            6,
            static state => state.World.CurrentLocationId == LocationId.Workshop && state.Player.Stats.Money < 100),
        new(
            "CafeBirthdayParty",
            "A birthday party spills out of the cafe back room. Tea, clapping, and a small tip for carrying extra chairs.",
            new RandomEventEffect { MoneyChange = 8, StressChange = -3, InkKnot = "event_cafe_birthday" },
            4,
            5,
            static state => state.World.CurrentLocationId == LocationId.Cafe),
        new(
            "DepotMorningRush",
            "The morning rush at the depot turns into overtime. More passengers, louder arguments, a little more cash.",
            new RandomEventEffect { MoneyChange = 12, EnergyChange = -8, StressChange = 4, InkKnot = "event_depot_morning_rush" },
            4,
            7,
            static state => state.World.CurrentLocationId == LocationId.Depot),
        new(
            "PharmacyGenericArrival",
            "A batch of generic medicine arrives at the pharmacy, cheap enough to matter.",
            new RandomEventEffect { MoneyChange = 4, StressChange = -2, InkKnot = "event_pharmacy_generic_arrival" },
            4,
            6,
            static state => state.World.CurrentLocationId == LocationId.Pharmacy),
        new(
            "ImbabaNightPatrol",
            "A night patrol sweeps through Imbaba. Everyone keeps their heads down and their voices low.",
            new RandomEventEffect { StressChange = 5, PolicePressureChange = 4, InkKnot = "event_imbaba_night_patrol" },
            5,
            7,
            static state => state.World.CurrentDistrict == DistrictId.Imbaba && state.PolicePressure >= 30),
        new(
            "FishMarketScrap",
            "A fish vendor throws scrap and unsold catch to anyone willing to clean up.",
            new RandomEventEffect { FoodChange = 1, EnergyChange = -4, InkKnot = "event_fish_market_scrap" },
            3,
            5,
            static state => state.World.CurrentLocationId == LocationId.FishMarket && state.Player.Stats.Money < 60)
    ];

    private static IReadOnlyList<RandomEvent> _events = DefaultEvents;

    public static IReadOnlyList<RandomEvent> AllEvents => _events;

    public static void Configure(IEnumerable<RandomEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var configuredEvents = events.Where(static item => item is not null).ToArray();
        if (configuredEvents.Length == 0)
        {
            throw new InvalidOperationException("At least one random event must be configured.");
        }

        _events = configuredEvents;
    }
}
