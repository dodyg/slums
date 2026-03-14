namespace Slums.Core.World;

public static class DistrictConditionRegistry
{
    private static readonly DistrictConditionDefinition[] DefaultDefinitions =
    [
        new()
        {
            Id = "imbaba_steady_day",
            District = DistrictId.Imbaba,
            Title = "Steady Day",
            BulletinText = "Imbaba is running on its usual pressure today.",
            GameplaySummary = "No extra district modifiers are pressing on the basics right now.",
            MinDay = 1,
            Weight = 1
        },
        new()
        {
            Id = "dokki_steady_day",
            District = DistrictId.Dokki,
            Title = "Steady Day",
            BulletinText = "Dokki is tense but broadly predictable today.",
            GameplaySummary = "No extra district modifiers are pressing on the basics right now.",
            MinDay = 1,
            Weight = 1
        },
        new()
        {
            Id = "ardalliwa_steady_day",
            District = DistrictId.ArdAlLiwa,
            Title = "Steady Day",
            BulletinText = "Ard al-Liwa is busy, but nothing is tipping over yet.",
            GameplaySummary = "No extra district modifiers are pressing on the basics right now.",
            MinDay = 1,
            Weight = 1
        },
        new()
        {
            Id = "bulaq_steady_day",
            District = DistrictId.BulaqAlDakrour,
            Title = "Steady Day",
            BulletinText = "Bulaq al-Dakrour is carrying its usual noise without a fresh shock.",
            GameplaySummary = "No extra district modifiers are pressing on the basics right now.",
            MinDay = 1,
            Weight = 1
        },
        new()
        {
            Id = "shubra_steady_day",
            District = DistrictId.Shubra,
            Title = "Steady Day",
            BulletinText = "Shubra is holding together without a new district squeeze today.",
            GameplaySummary = "No extra district modifiers are pressing on the basics right now.",
            MinDay = 1,
            Weight = 1
        },
        new()
        {
            Id = "imbaba_utility_cut",
            District = DistrictId.Imbaba,
            Title = "Utility Cut",
            BulletinText = "Imbaba is limping through another utility cut.",
            GameplaySummary = "Food runs cost more and moving in or out takes longer.",
            MinDay = 1,
            Weight = 4,
            Effect = new DistrictConditionEffect
            {
                TravelTimeMinutesModifier = 5,
                FoodCostModifier = 2,
                StreetFoodCostModifier = 1,
                SuppressedRandomEventIds = ["NeighborhoodSolidarity"]
            }
        },
        new()
        {
            Id = "imbaba_market_crackdown",
            District = DistrictId.Imbaba,
            Title = "Market Crackdown",
            BulletinText = "Uniforms and informants are pressing the market harder than usual.",
            GameplaySummary = "Crime runs hotter and petty work pays a little better if you keep your head down.",
            MinDay = 2,
            Weight = 3,
            Effect = new DistrictConditionEffect
            {
                WorkPayModifier = 2,
                CrimeDetectionRiskModifier = 7,
                BoostedRandomEventIds = ["NeighborhoodRumor"]
            }
        },
        new()
        {
            Id = "dokki_checkpoint_sweep",
            District = DistrictId.Dokki,
            Title = "Checkpoint Sweep",
            BulletinText = "Dokki crossings are full of checkpoint stops and slow questions today.",
            GameplaySummary = "Travel into Dokki is slower and crime carries extra detection risk.",
            MinDay = 2,
            Weight = 4,
            MinPolicePressure = 20,
            Effect = new DistrictConditionEffect
            {
                TravelTimeMinutesModifier = 10,
                TravelEnergyModifier = 2,
                CrimeDetectionRiskModifier = 9,
                BoostedRandomEventIds = ["DokkiCheckpointSweep", "DokkiTransportFriction"]
            }
        },
        new()
        {
            Id = "dokki_traffic_surge",
            District = DistrictId.Dokki,
            Title = "Traffic Surge",
            BulletinText = "Dokki is jammed with traffic, delays, and people arriving already angry.",
            GameplaySummary = "Travel is slower and Dokki shifts add a little more stress than usual.",
            MinDay = 1,
            Weight = 3,
            Effect = new DistrictConditionEffect
            {
                TravelTimeMinutesModifier = 8,
                WorkStressModifier = 3
            }
        },
        new()
        {
            Id = "ardalliwa_clinic_overflow",
            District = DistrictId.ArdAlLiwa,
            Title = "Clinic Overflow",
            BulletinText = "Ard al-Liwa clinics are overflowing and every errand there is running long.",
            GameplaySummary = "Clinic visits cost more, clinic work pays better, and medicine queues spill over.",
            MinDay = 1,
            Weight = 4,
            Effect = new DistrictConditionEffect
            {
                ClinicVisitCostModifier = 4,
                MedicineCostModifier = 2,
                WorkPayModifier = 3,
                BoostedRandomEventIds = ["ClinicOverflow", "ClinicSupplyShortage"]
            }
        },
        new()
        {
            Id = "ardalliwa_workshop_rush",
            District = DistrictId.ArdAlLiwa,
            Title = "Workshop Rush",
            BulletinText = "Ard al-Liwa workshops are pushing rush orders through every back room today.",
            GameplaySummary = "Workshop-side work pays slightly more but costs more stress.",
            MinDay = 2,
            Weight = 3,
            Effect = new DistrictConditionEffect
            {
                WorkPayModifier = 3,
                WorkStressModifier = 2,
                BoostedRandomEventIds = ["WorkshopRushOrder"]
            }
        },
        new()
        {
            Id = "bulaq_route_dispute",
            District = DistrictId.BulaqAlDakrour,
            Title = "Route Dispute",
            BulletinText = "Bulaq route disputes are dragging fares, queues, and tempers all over the district.",
            GameplaySummary = "Travel is rougher, depot work is tenser, and transport events are more likely.",
            MinDay = 1,
            Weight = 4,
            Effect = new DistrictConditionEffect
            {
                TravelEnergyModifier = 2,
                WorkStressModifier = 2,
                BoostedRandomEventIds = ["DepotFareShakeup", "BulaqMedicineQueue"]
            }
        },
        new()
        {
            Id = "bulaq_discount_queue",
            District = DistrictId.BulaqAlDakrour,
            Title = "Discount Queue Day",
            BulletinText = "Bulaq's discount counters are jammed and every queue is eating half a day.",
            GameplaySummary = "Medicine is cheaper, but travel and service waits are slower.",
            MinDay = 1,
            Weight = 3,
            Effect = new DistrictConditionEffect
            {
                TravelTimeMinutesModifier = 6,
                MedicineCostModifier = -3,
                BoostedRandomEventIds = ["BulaqMedicineQueue"]
            }
        },
        new()
        {
            Id = "shubra_steam_break",
            District = DistrictId.Shubra,
            Title = "Steam Break",
            BulletinText = "Shubra laundries are losing time to a steam break and the whole block feels it.",
            GameplaySummary = "Shubra work is more stressful and moving through the district costs extra energy.",
            MinDay = 1,
            Weight = 4,
            Effect = new DistrictConditionEffect
            {
                TravelEnergyModifier = 2,
                WorkStressModifier = 3,
                BoostedRandomEventIds = ["ShubraSteamBreak"]
            }
        },
        new()
        {
            Id = "shubra_committee_collection",
            District = DistrictId.Shubra,
            Title = "Committee Collection",
            BulletinText = "A Shubra building committee is collecting for repairs and quietly moving favors around.",
            GameplaySummary = "Food pressure eases a little, but money runs go further toward obligations than comfort.",
            MinDay = 2,
            Weight = 3,
            Effect = new DistrictConditionEffect
            {
                FoodCostModifier = -1,
                StreetFoodCostModifier = -1,
                BoostedRandomEventIds = ["ShubraBlockSolidarity"]
            }
        }
    ];

    private static IReadOnlyList<DistrictConditionDefinition> _definitions = DefaultDefinitions;

    public static IReadOnlyList<DistrictConditionDefinition> AllDefinitions => _definitions;

    public static void Configure(IEnumerable<DistrictConditionDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var configuredDefinitions = definitions.Where(static definition => definition is not null).ToArray();
        if (configuredDefinitions.Length == 0)
        {
            throw new InvalidOperationException("At least one district condition must be configured.");
        }

        ValidateDefinitions(configuredDefinitions);
        _definitions = configuredDefinitions;
    }

    public static DistrictConditionDefinition? GetById(string? conditionId)
    {
        if (string.IsNullOrWhiteSpace(conditionId))
        {
            return null;
        }

        return _definitions.FirstOrDefault(definition => string.Equals(definition.Id, conditionId, StringComparison.Ordinal));
    }

    public static IReadOnlyList<DistrictConditionDefinition> GetDefinitionsForDistrict(DistrictId districtId)
    {
        return _definitions.Where(definition => definition.District == districtId).ToArray();
    }

    private static void ValidateDefinitions(IReadOnlyList<DistrictConditionDefinition> definitions)
    {
        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var definition in definitions)
        {
            if (string.IsNullOrWhiteSpace(definition.Id))
            {
                throw new InvalidOperationException("District condition ids must be provided.");
            }

            if (!seenIds.Add(definition.Id))
            {
                throw new InvalidOperationException($"Duplicate district condition id '{definition.Id}'.");
            }

            if (string.IsNullOrWhiteSpace(definition.Title))
            {
                throw new InvalidOperationException($"District condition '{definition.Id}' must provide a title.");
            }

            if (string.IsNullOrWhiteSpace(definition.BulletinText))
            {
                throw new InvalidOperationException($"District condition '{definition.Id}' must provide bulletin text.");
            }

            if (string.IsNullOrWhiteSpace(definition.GameplaySummary))
            {
                throw new InvalidOperationException($"District condition '{definition.Id}' must provide a gameplay summary.");
            }

            if (definition.Weight <= 0)
            {
                throw new InvalidOperationException($"District condition '{definition.Id}' must have a positive weight.");
            }

            if (definition.Effect is null)
            {
                throw new InvalidOperationException($"District condition '{definition.Id}' must provide an effect.");
            }

            ValidateEventIds(definition.Id, definition.Effect.BoostedRandomEventIds, "boosted");
            ValidateEventIds(definition.Id, definition.Effect.SuppressedRandomEventIds, "suppressed");
        }

        foreach (var district in Enum.GetValues<DistrictId>())
        {
            if (definitions.All(definition => definition.District != district))
            {
                throw new InvalidOperationException($"District '{district}' must have at least one configured condition.");
            }
        }
    }

    private static void ValidateEventIds(string definitionId, IEnumerable<string> eventIds, string label)
    {
        if (eventIds is null)
        {
            throw new InvalidOperationException($"District condition '{definitionId}' must provide a {label} random event id list.");
        }

        foreach (var eventId in eventIds)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                throw new InvalidOperationException($"District condition '{definitionId}' contains an empty {label} random event id.");
            }
        }
    }
}
