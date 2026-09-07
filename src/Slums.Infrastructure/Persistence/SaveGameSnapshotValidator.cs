using Slums.Core.Characters;
using Slums.Core.Community;
using Slums.Core.Economy;
using Slums.Core.Endings;
using Slums.Core.Expenses;
using Slums.Core.Home;
using Slums.Core.Information;
using Slums.Core.Inventory;
using Slums.Core.Investments;
using Slums.Core.Jobs;
using Slums.Core.Narrative;
using Slums.Core.Phone;
using Slums.Core.Relationships;
using Slums.Core.Robotics;
using Slums.Core.Rumors;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.Technology;
using Slums.Core.Weather;
using Slums.Core.World;
using Slums.Core.World.News;

namespace Slums.Infrastructure.Persistence;

internal static class SaveGameSnapshotValidator
{
    private const int MaxCollectionEntries = 10_000;

    public static void Validate(GameSessionSnapshot snapshot, List<string> problems)
    {
        if (snapshot.Clock is null || snapshot.Player is null || snapshot.World is null ||
            snapshot.Relationships is null || snapshot.JobProgress is null || snapshot.Crime is null ||
            snapshot.Work is null || snapshot.Run is null || snapshot.Narrative is null ||
            snapshot.HouseholdAssets is null || snapshot.Ramadan is null || snapshot.CommunityEvents is null ||
            snapshot.CommunityAdaptation is null || snapshot.DistrictHeat is null || snapshot.Territory is null ||
            snapshot.Economy is null || snapshot.Phone is null || snapshot.Tips is null || snapshot.News is null ||
            snapshot.Infrastructure is null || snapshot.CityCrisis is null || snapshot.Inventory is null ||
            snapshot.Technology is null || snapshot.CharacterArcs is null)
        {
            problems.Add("save snapshot is missing one or more state sections");

            if (snapshot.Clock is not null)
            {
                ValidateClock(snapshot.Clock, problems);
            }
            if (snapshot.Player is not null)
            {
                ValidatePlayer(snapshot.Player, problems);
            }
            if (snapshot.Relationships is not null)
            {
                ValidateRelationshipSections(snapshot.Relationships, problems);
            }
            if (snapshot.JobProgress is not null)
            {
                ValidateJobSections(snapshot.JobProgress, problems);
            }
            if (snapshot.Crime is not null)
            {
                ValidatePercentage(snapshot.Crime.PolicePressure, "police pressure", problems);
            }
            if (snapshot.CityCrisis is not null)
            {
                ValidateCrisis(snapshot.CityCrisis, problems);
            }
            return;
        }

        ValidateClock(snapshot.Clock, problems);
        ValidatePlayer(snapshot.Player, problems);
        ValidateRelationshipSections(snapshot.Relationships, problems);
        ValidateJobSections(snapshot.JobProgress, problems);
        ValidateWorld(snapshot.World, snapshot.Clock.Day, problems);
        ValidateEnumName<WeatherType>(snapshot.CurrentWeather, "weather", problems);
        ValidateRun(snapshot.Run, snapshot.Clock.Day, problems);
        ValidateNarrative(snapshot.Narrative, problems);
        ValidateHouseholdAssets(snapshot.HouseholdAssets, snapshot.Clock.Day, problems);
        ValidateInvestments(snapshot.Investments, problems);
        ValidateTrainedSkills(snapshot.TrainedSkillsToday, problems);
        ValidateHomeUpgrades(snapshot.HomeUpgrades, problems);
        ValidateRamadan(snapshot.Ramadan, problems);
        ValidateCommunityEvents(snapshot.CommunityEvents, snapshot.Clock.Day, problems);
        ValidateCommunityAdaptation(snapshot.CommunityAdaptation, problems);
        ValidateDistrictHeat(snapshot.DistrictHeat, problems);
        ValidateTerritory(snapshot.Territory, snapshot.Clock.Day, problems);
        ValidateEconomy(snapshot.Economy, snapshot.Clock.Day, problems);
        ValidatePhone(snapshot.Phone, snapshot.Clock.Day, problems);
        ValidateTips(snapshot.Tips, snapshot.Clock.Day, problems);
        ValidateRumors(snapshot.Rumors, snapshot.Clock.Day, problems);
        ValidateNews(snapshot.News, snapshot.Clock.Day, problems);
        ValidateInfrastructure(snapshot.Infrastructure, snapshot.Clock.Day, problems);
        ValidateInventory(snapshot.Inventory, problems);
        ValidateTechnology(snapshot.Technology, snapshot.Clock.Day, problems);
        ValidateCharacterArcs(snapshot.CharacterArcs, problems);
        ValidateJournal(snapshot.Journal, snapshot.Clock.Day, problems);
        ValidateRandomState(snapshot.RandomState, problems);
    }

    private static void ValidateClock(GameSessionClockSnapshot clock, List<string> problems)
    {
        if (clock.Day < 1)
        {
            problems.Add($"day {clock.Day} is below 1");
        }
        if (clock.Hour is < 0 or > 23)
        {
            problems.Add($"hour {clock.Hour} is outside 0..23");
        }
        if (clock.Minute is < 0 or > 59)
        {
            problems.Add($"minute {clock.Minute} is outside 0..59");
        }
    }

    private static void ValidateCrisis(GameSessionCrisisSnapshot crisis, List<string> problems)
    {
        AddNonNegative(crisis.BeatIndex, "crisis beat", problems);
        AddNonNegative(crisis.EvidenceCollected, "crisis evidence", problems);
        AddNonNegative(crisis.ResourcesCommitted, "crisis resources", problems);
        AddNonNegative(crisis.DecisionDay, "crisis decision day", problems);
        AddNonNegative(crisis.CallbackDueDay, "crisis callback due day", problems);
        ValidatePercentage(crisis.CooperativeCondition, "crisis cooperative condition", problems);
        ValidateEnum(crisis.Decision, "crisis decision", problems);
        ValidateEnum(crisis.Resolution, "crisis resolution", problems);
        ValidateEnum(crisis.PendingCallbackDecision, "crisis pending callback decision", problems);
    }

    private static void ValidatePlayer(GameSessionPlayerSnapshot player, List<string> problems)
    {
        ValidateEnum(player.BackgroundType, "background", problems);
        ValidateEnum(player.Gender, "gender", problems);
        AddNonNegative(player.Money, "money", problems);
        ValidatePercentage(player.Satiety, "satiety", problems);
        AddNonNegative(player.DaysUndereating, "days undereating", problems);
        ValidatePercentage(player.Energy, "energy", problems);
        ValidatePercentage(player.Health, "health", problems);
        ValidatePercentage(player.Stress, "stress", problems);
        ValidatePercentage(player.MotherHealth, "mother health", problems);
        AddNonNegative(player.FoodStockpile, "food stockpile", problems);
        AddNonNegative(player.PreservedMealUnits, "preserved meal units", problems);
        AddNonNegative(player.MedicineStock, "medicine stock", problems);
        ValidateSkillMap(player.SkillLevelsById, problems);
    }

    private static void ValidateRelationshipSections(GameSessionRelationshipSnapshot relationships, List<string> problems)
    {
        if (!HasReasonableCount(relationships.Npcs, "NPC relationships", problems) || !HasReasonableCount(relationships.Factions, "faction standings", problems))
        {
            return;
        }

        if (relationships.Npcs.Count != Enum.GetValues<NpcId>().Length)
        {
            problems.Add($"relationships contain {relationships.Npcs.Count} NPC entries; expected {Enum.GetValues<NpcId>().Length}");
        }
        if (relationships.Factions.Count != Enum.GetValues<FactionId>().Length)
        {
            problems.Add($"relationships contain {relationships.Factions.Count} faction entries; expected {Enum.GetValues<FactionId>().Length}");
        }

        foreach (var pair in relationships.Npcs)
        {
            if (pair.Value is null)
            {
                problems.Add($"relationship NPC '{pair.Key}' has no state");
            }
        }
    }

    private static void ValidateJobSections(GameSessionJobProgressSnapshot jobs, List<string> problems)
    {
        if (!HasReasonableCount(jobs.Tracks, "job tracks", problems))
        {
            return;
        }

        if (jobs.Tracks.Count != Enum.GetValues<JobType>().Length)
        {
            problems.Add($"job tracks contain {jobs.Tracks.Count} entries; expected {Enum.GetValues<JobType>().Length}");
        }

        foreach (var pair in jobs.Tracks)
        {
            if (pair.Value is null)
            {
                problems.Add($"job track '{pair.Key}' has no state");
            }
        }
    }

    private static void ValidateWorld(GameSessionWorldSnapshot world, int currentDay, List<string> problems)
    {
        if (string.IsNullOrWhiteSpace(world.CurrentLocationId) ||
            !LocationId.All.Any(location => location.Value == world.CurrentLocationId))
        {
            problems.Add($"current location '{world.CurrentLocationId}' is not a declared location");
        }

        if (!HasReasonableCount(world.ActiveDistrictConditions, "active district conditions", problems))
        {
            return;
        }

        var districts = new HashSet<DistrictId>();
        foreach (var condition in world.ActiveDistrictConditions)
        {
            if (condition is null)
            {
                problems.Add("active district conditions contains a null entry");
                continue;
            }

            if (!TryParseEnum(condition.District, out DistrictId district))
            {
                problems.Add($"active condition district '{condition.District}' is not declared");
            }
            else if (!districts.Add(district))
            {
                problems.Add($"active district conditions contain duplicate district {district}");
            }

            if (string.IsNullOrWhiteSpace(condition.ConditionId))
            {
                problems.Add("active district condition has an empty condition id");
            }
            else if (DistrictConditionRegistry.GetById(condition.ConditionId) is null)
            {
                problems.Add($"active district condition '{condition.ConditionId}' is not declared");
            }

            if (currentDay < 1)
            {
                problems.Add("active district conditions cannot be validated before day 1");
            }
        }
    }

    private static void ValidateRun(GameSessionRunSnapshot run, int currentDay, List<string> problems)
    {
        if (run.RunId == Guid.Empty)
        {
            problems.Add("run id is empty");
        }

        AddNonNegative(run.DaysSurvived, "days survived", problems);
        AddNonNegative(run.UnpaidRentDays, "unpaid rent days", problems);
        AddNonNegative(run.AccumulatedRentDebt, "accumulated rent debt", problems);
        AddNonNegative(run.RentGraceDaysRemaining, "rent grace days", problems);
        if (run.UnpaidRentDays > RentState.EvictionThreshold || run.RentGraceDaysRemaining > RentState.EvictionThreshold)
        {
            problems.Add("rent state contains an impossible day count");
        }

        ValidateNullableEnum(run.EndingId, "ending", problems);
        ValidateNullableEnum(run.PendingEndingId, "pending ending", problems);
        if (run.PendingEndingKnot is not null && !EndingKnotCatalog.AllKnownKnots.Contains(run.PendingEndingKnot))
        {
            problems.Add($"pending ending knot '{run.PendingEndingKnot}' is not declared");
        }

        if (run.DaysSurvived > currentDay)
        {
            problems.Add("days survived is ahead of the saved clock");
        }

        if (run.IsGameOver && run.EndingId is null)
        {
            problems.Add("game-over save has no ending id");
        }
    }

    private static void ValidateNarrative(GameSessionNarrativeSnapshot narrative, List<string> problems)
    {
        ValidateStringList(narrative.StoryFlags, "story flags", problems, requireNonEmpty: true);
        ValidateStringList(narrative.PendingNarrativeScenes, "pending narrative scenes", problems, requireNonEmpty: true);
        ValidateDictionary(narrative.RandomEventHistory, "random event history", problems);
        foreach (var pair in narrative.RandomEventHistory)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
            {
                problems.Add("random event history contains an empty event id");
            }
            AddNonNegative(pair.Value, $"random event count '{pair.Key}'", problems);
        }
    }

    private static void ValidateHouseholdAssets(GameSessionHouseholdAssetsSnapshot assets, int currentDay, List<string> problems)
    {
        if (!HasReasonableCount(assets.Pets, "pets", problems) || !HasReasonableCount(assets.Plants, "plants", problems) || !HasReasonableCount(assets.Robots, "robots", problems))
        {
            return;
        }

        var catCount = assets.Pets.Count(pet => pet?.Type == PetType.Cat);
        var fishCount = assets.Pets.Count(pet => pet?.Type == PetType.Fish);
        if (assets.Pets.Any(pet => pet is null))
        {
            problems.Add("pets contains a null entry");
        }
        if (catCount > 3 || fishCount > 1)
        {
            problems.Add("pet limits are exceeded");
        }

        foreach (var pet in assets.Pets)
        {
            if (pet is null)
            {
                continue;
            }
            ValidateEnum(pet.Type, "pet type", problems);
            ValidateDay(pet.AcquiredOnDay, currentDay, "pet acquisition day", problems);
            AddNonNegative(pet.LastUpkeepPaidWeek, "pet upkeep week", problems);
            AddNonNegative(pet.DecorationsPaidWeek, "pet decoration week", problems);
            AddNonNegative(pet.WaterConditionerPaidWeek, "pet conditioner week", problems);
        }

        var plantIds = new HashSet<Guid>();
        foreach (var plant in assets.Plants)
        {
            if (plant is null)
            {
                problems.Add("plants contains a null entry");
                continue;
            }
            if (plant.Id == Guid.Empty || !plantIds.Add(plant.Id))
            {
                problems.Add("plants contain an empty or duplicate id");
            }
            ValidateEnum(plant.Type, "plant type", problems);
            ValidateDay(plant.AcquiredOnDay, currentDay, "plant acquisition day", problems);
            ValidateDay(plant.LastHarvestDay, currentDay, "plant harvest day", problems);
            AddNonNegative(plant.LastBaseCarePaidWeek, "plant care week", problems);
            AddNonNegative(plant.FertilizerPaidWeek, "plant fertilizer week", problems);
            AddNonNegative(plant.IrrigationPaidWeek, "plant irrigation week", problems);
        }

        var robotIds = new HashSet<Guid>();
        foreach (var robot in assets.Robots)
        {
            if (robot is null)
            {
                problems.Add("robots contains a null entry");
                continue;
            }
            if (robot.Id == Guid.Empty || !robotIds.Add(robot.Id))
            {
                problems.Add("robots contain an empty or duplicate id");
            }
            ValidateEnum(robot.Type, "robot type", problems);
            ValidateDay(robot.AcquiredOnDay, currentDay, "robot acquisition day", problems);
            ValidatePercentage(robot.Condition, "robot condition", problems);
        }

        AddNonNegative(assets.RobotParts, "robot parts", problems);
        AddNonNegative(assets.LastStreetCatEncounterDay, "street cat encounter day", problems);
        AddNonNegative(assets.TotalHerbEarnings, "herb earnings", problems);
    }

    private static void ValidateInvestments(IReadOnlyList<InvestmentSnapshot>? investments, List<string> problems)
    {
        if (!HasReasonableCount(investments, "investments", problems))
        {
            return;
        }
        var types = new HashSet<InvestmentType>();
        foreach (var investment in investments!)
        {
            if (investment is null)
            {
                problems.Add("investments contains a null entry");
                continue;
            }
            ValidateEnum(investment.Type, "investment type", problems);
            if (!types.Add(investment.Type))
            {
                problems.Add($"investments contain duplicate type {investment.Type}");
            }
            AddNonNegative(investment.InvestedAmount, "invested amount", problems);
            AddNonNegative(investment.WeeklyIncomeMin, "minimum weekly income", problems);
            AddNonNegative(investment.WeeklyIncomeMax, "maximum weekly income", problems);
            AddNonNegative(investment.WeeksActive, "investment active weeks", problems);
            if (investment.WeeklyIncomeMin > investment.WeeklyIncomeMax)
            {
                problems.Add($"investment {investment.Type} has inverted income bounds");
            }
            ValidateEnum(investment.PurchaseDistrict, "investment purchase district", problems);
        }
    }

    private static void ValidateTrainedSkills(IReadOnlyDictionary<string, bool>? skills, List<string> problems)
    {
        if (skills is null)
        {
            problems.Add("trained skills are missing");
            return;
        }
        ValidateDictionary(skills, "trained skills", problems);
        foreach (var key in skills.Keys)
        {
            if (!TryParseEnum<SkillId>(key, out _))
            {
                problems.Add($"trained skill '{key}' is not declared");
            }
        }
    }

    private static void ValidateHomeUpgrades(IReadOnlyList<string>? upgrades, List<string> problems)
    {
        if (!HasReasonableCount(upgrades, "home upgrades", problems))
        {
            return;
        }
        var seen = new HashSet<HomeUpgrade>();
        foreach (var upgrade in upgrades!)
        {
            if (!TryParseEnum(upgrade, out HomeUpgrade value) || !seen.Add(value))
            {
                problems.Add($"home upgrade '{upgrade}' is unknown or duplicated");
            }
        }
    }

    private static void ValidateRamadan(GameSessionRamadanSnapshot ramadan, List<string> problems)
    {
        AddNonNegative(ramadan.DaysFasting, "Ramadan fasting days", problems);
        AddNonNegative(ramadan.DaysRemaining, "Ramadan days remaining", problems);
        if (!ramadan.IsActive && (ramadan.PlayerIsFasting || ramadan.DaysRemaining > 0))
        {
            problems.Add("inactive Ramadan state contains active fasting data");
        }
        if (ramadan.DaysFasting > 30 || ramadan.DaysRemaining > 30)
        {
            problems.Add("Ramadan state exceeds the 30-day cycle");
        }
    }

    private static void ValidateCommunityEvents(GameSessionCommunityEventSnapshot events, int currentDay, List<string> problems)
    {
        AddNonNegative(events.ConsecutiveSkips, "community consecutive skips", problems);
        AddNonNegative(events.TotalAttended, "community attendance", problems);
        AddNonNegative(events.LastAttendanceDay, "last attendance day", problems);
        AddNonNegative(events.LastWeekResetDay, "community week reset day", problems);
        ValidateDay(events.LastAttendanceDay, currentDay, "last attendance day", problems, allowZero: true);
        ValidateDay(events.LastWeekResetDay, currentDay, "community week reset day", problems, allowZero: true);
        ValidateStringList(events.AttendedThisWeek, "attended community events", problems, requireNonEmpty: true);
        foreach (var eventId in events.AttendedThisWeek)
        {
            if (!TryParseEnum<CommunityEventId>(eventId, out _))
            {
                problems.Add($"community event '{eventId}' is not declared");
            }
        }
    }

    private static void ValidateCommunityAdaptation(GameSessionCommunityAdaptationSnapshot adaptation, List<string> problems)
    {
        AddNonNegative(adaptation.CoolingRoomDaysRemaining, "cooling room days", problems);
        AddNonNegative(adaptation.WaterReserveUnits, "water reserve units", problems);
        AddNonNegative(adaptation.SuccessfulActions, "successful adaptation actions", problems);
        AddNonNegative(adaptation.ShelterContributions, "shelter contributions", problems);
    }

    private static void ValidateDistrictHeat(GameSessionDistrictHeatSnapshot heat, List<string> problems)
    {
        ValidateFinite(heat.DecayRateModifier, "heat decay modifier", problems);
        if (heat.DecayRateModifier <= 0 || heat.DecayRateModifier > 10)
        {
            problems.Add("heat decay modifier is outside 0..10");
        }
        if (!HasReasonableCount(heat.Entries, "district heat entries", problems))
        {
            return;
        }
        var districts = new HashSet<DistrictId>();
        foreach (var entry in heat.Entries)
        {
            if (entry is null || !TryParseEnum(entry.District, out DistrictId district) || !districts.Add(district))
            {
                problems.Add($"district heat entry '{entry?.District}' is unknown or duplicated");
                continue;
            }
            ValidatePercentage(entry.Heat, $"heat for {district}", problems);
            ValidatePercentage(entry.BaselineHeat, $"baseline heat for {district}", problems);
            AddNonNegative(entry.DecayRate, $"decay rate for {district}", problems);
        }
    }

    private static void ValidateTerritory(GameSessionTerritorySnapshot territory, int currentDay, List<string> problems)
    {
        if (!HasReasonableCount(territory.Districts, "territory districts", problems))
        {
            return;
        }
        var districts = new HashSet<DistrictId>();
        foreach (var entry in territory.Districts)
        {
            if (entry is null || !TryParseEnum(entry.District, out DistrictId district) || !districts.Add(district))
            {
                problems.Add($"territory district '{entry?.District}' is unknown or duplicated");
                continue;
            }
            ValidatePercentage(entry.Tension, $"tension for {district}", problems);
            ValidateDay(entry.LastConflictDay, currentDay, $"last conflict day for {district}", problems, allowZero: true);
            ValidateFactionMap(entry.FactionInfluence, $"influence for {district}", problems);
        }
    }

    private static void ValidateEconomy(GameSessionEconomySnapshot economy, int currentDay, List<string> problems)
    {
        if (!HasReasonableCount(economy.NpcEconomies, "NPC economies", problems) || !HasReasonableCount(economy.PlayerDebts, "player debts", problems))
        {
            return;
        }
        var npcs = new HashSet<NpcId>();
        foreach (var entry in economy.NpcEconomies)
        {
            if (entry is null || !TryParseEnum(entry.Npc, out NpcId npc) || !npcs.Add(npc))
            {
                problems.Add($"NPC economy '{entry?.Npc}' is unknown or duplicated");
                continue;
            }
            if (!TryParseEnum<NpcWealthLevel>(entry.WealthLevel, out _))
            {
                problems.Add($"NPC economy {npc} has unknown wealth level '{entry.WealthLevel}'");
            }
            if (entry.Generosity is < -100 or > 100)
            {
                problems.Add($"NPC generosity for {npc} is outside -100..100");
            }
            ValidateDay(entry.LastHardshipDay, currentDay, $"hardship day for {npc}", problems, allowZero: true);
            ValidateDay(entry.LastWindfallDay, currentDay, $"windfall day for {npc}", problems, allowZero: true);
            ValidateDay(entry.GenerousUntilDay, currentDay + 100, $"generous-until day for {npc}", problems, allowZero: true);
            ValidateDebtorAmounts(entry.MoneyOwedTo, $"money owed to {npc}", problems);
            ValidateDebtorAmounts(entry.MoneyOwedBy, $"money owed by {npc}", problems);
        }
        foreach (var debt in economy.PlayerDebts)
        {
            if (debt is null)
            {
                problems.Add("player debts contains a null entry");
                continue;
            }
            ValidateEnumName<DebtSource>(debt.Source, "debt source", problems);
            ValidateEnumName<DebtCollectionState>(debt.CollectionState, "debt collection state", problems);
            AddNonNegative(debt.AmountOwed, "debt amount", problems);
            AddNonNegative(debt.InterestWeeklyBasisPoints, "debt interest", problems);
            ValidateDay(debt.DueDay, currentDay + 1000, "debt due day", problems, allowZero: true);
            ValidateDay(debt.OriginDay, currentDay, "debt origin day", problems, allowZero: true);
            if (debt.CreditorNpcId is int creditor && !Enum.IsDefined((NpcId)creditor))
            {
                problems.Add($"debt creditor NPC '{creditor}' is not declared");
            }
        }
    }

    private static void ValidatePhone(GameSessionPhoneSnapshot phone, int currentDay, List<string> problems)
    {
        AddNonNegative(phone.CreditRemaining, "phone credit", problems);
        AddNonNegative(phone.DaysSinceCreditRefill, "days since phone refill", problems);
        ValidatePercentage(phone.HandsetCondition, "handset condition", problems);
        if (phone.PhoneLost && !phone.PhoneLostDay.HasValue)
        {
            problems.Add("lost phone has no loss day");
        }
        if (phone.PhoneLostDay is int lostDay)
        {
            ValidateDay(lostDay, currentDay, "phone loss day", problems, allowZero: true, allowFuture: true);
        }
        if (!HasReasonableCount(phone.Messages, "phone messages", problems))
        {
            return;
        }
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var message in phone.Messages)
        {
            if (message is null || string.IsNullOrWhiteSpace(message.Id) || !ids.Add(message.Id))
            {
                problems.Add($"phone message '{message?.Id}' is empty or duplicated");
                continue;
            }
            ValidateEnumName<PhoneMessageType>(message.Type, "phone message type", problems);
            if (string.IsNullOrWhiteSpace(message.SenderNpcId) || !TryParseEnum<NpcId>(message.SenderNpcId, out _))
            {
                problems.Add($"phone message '{message.Id}' has an unknown sender NPC");
            }
            ValidateDay(message.DayReceived, currentDay, "phone message day", problems, allowFuture: true);
            if (message.ExpiresAfterDay is int expiry)
            {
                ValidateDay(expiry, currentDay + 1000, "phone message expiry day", problems);
            }
            AddNonNegative(message.ResponseTimeCost, "phone response time cost", problems);
            AddNonNegative(message.ResponseMoneyCost, "phone response money cost", problems);
        }
    }

    private static void ValidateTips(GameSessionTipSnapshot tips, int currentDay, List<string> problems)
    {
        if (!HasReasonableCount(tips.Tips, "tips", problems) || tips.IgnoredCounts is null)
        {
            if (tips.IgnoredCounts is null)
            {
                problems.Add("ignored tip counts are missing");
            }
            return;
        }
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var tip in tips.Tips)
        {
            if (tip is null || string.IsNullOrWhiteSpace(tip.Id) || !ids.Add(tip.Id))
            {
                problems.Add($"tip '{tip?.Id}' is empty or duplicated");
                continue;
            }
            ValidateEnumName<TipType>(tip.Type, "tip type", problems);
            ValidateEnumName<NpcId>(tip.Source, "tip source", problems);
            ValidateDay(tip.DayGenerated, currentDay, "tip generation day", problems);
            ValidateDay(tip.ExpiresAfterDay, currentDay + 1000, "tip expiry day", problems);
            if (tip.RelevantDistrict is not null && !TryParseEnum<DistrictId>(tip.RelevantDistrict, out _))
            {
                problems.Add($"tip '{tip.Id}' has an unknown relevant district");
            }
        }
        foreach (var pair in tips.IgnoredCounts)
        {
            ValidateEnumName<NpcId>(pair.Key, "ignored tip source", problems);
            AddNonNegative(pair.Value, "ignored tip count", problems);
        }
    }

    private static void ValidateRumors(IReadOnlyList<RumorSnapshot>? rumors, int currentDay, List<string> problems)
    {
        if (!HasReasonableCount(rumors, "rumors", problems))
        {
            return;
        }
        foreach (var rumor in rumors!)
        {
            if (rumor is null)
            {
                problems.Add("rumors contains a null entry");
                continue;
            }
            ValidateEnumName<RumorId>(rumor.Id, "rumor id", problems);
            ValidateEnumName<DistrictId>(rumor.District, "rumor district", problems);
            ValidateDay(rumor.DayCreated, currentDay, "rumor creation day", problems);
            ValidatePercentage(rumor.InitialIntensity, "rumor initial intensity", problems);
            ValidatePercentage(rumor.Intensity, "rumor intensity", problems);
            AddNonNegative(rumor.Age, "rumor age", problems);
            ValidateStringList(rumor.AffectedNpcs, "rumor affected NPCs", problems, requireNonEmpty: true);
            ValidateStringList(rumor.NpcsWhoHeard, "rumor heard NPCs", problems, requireNonEmpty: true);
            foreach (var npc in rumor.AffectedNpcs.Concat(rumor.NpcsWhoHeard))
            {
                ValidateEnumName<NpcId>(npc, "rumor NPC", problems);
            }
        }
    }

    private static void ValidateNews(GameSessionNewsSnapshot news, int currentDay, List<string> problems)
    {
        AddNonNegative(news.LastGeneratedDay, "last news generation day", problems);
        if (!HasReasonableCount(news.ActiveFlashes, "active news", problems) || !HasReasonableCount(news.SeenDefinitionIds, "seen news", problems))
        {
            return;
        }
        var activeIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var flash in news.ActiveFlashes)
        {
            if (flash is null || string.IsNullOrWhiteSpace(flash.DefinitionId) || !activeIds.Add(flash.DefinitionId))
            {
                problems.Add($"active news '{flash?.DefinitionId}' is empty or duplicated");
                continue;
            }
            if (NewsRegistry.GetById(flash.DefinitionId) is null)
            {
                problems.Add($"active news '{flash.DefinitionId}' is not declared");
            }
            ValidateDay(flash.StartDay, currentDay, "news start day", problems, allowFuture: true);
            ValidateDay(flash.ExpiryDay, currentDay + 1000, "news expiry day", problems);
            if (flash.ExpiryDay < flash.StartDay)
            {
                problems.Add($"active news '{flash.DefinitionId}' has an inverted date range");
            }
        }
        ValidateStringList(news.SeenDefinitionIds, "seen news", problems, requireNonEmpty: true);
        foreach (var id in news.SeenDefinitionIds)
        {
            if (NewsRegistry.GetById(id) is null)
            {
                problems.Add($"seen news '{id}' is not declared");
            }
        }
        ValidateDictionary(news.LastGeneratedByCategory, "news category history", problems);
        foreach (var pair in news.LastGeneratedByCategory)
        {
            ValidateEnumName<NewsCategory>(pair.Key, "news category", problems);
            ValidateDay(pair.Value, currentDay, "news category generation day", problems, allowZero: true, allowFuture: true);
        }
    }

    private static void ValidateInfrastructure(GameSessionInfrastructureSnapshot infrastructure, int currentDay, List<string> problems)
    {
        if (!HasReasonableCount(infrastructure.Services, "infrastructure services", problems))
        {
            return;
        }
        var keys = new HashSet<(DistrictId, InfrastructureServiceType)>();
        foreach (var service in infrastructure.Services)
        {
            var district = service.District;
            var type = service.Service;
            if (!Enum.IsDefined(district) || !Enum.IsDefined(type) || !keys.Add((district, type)))
            {
                problems.Add("infrastructure contains an unknown or duplicate service");
                continue;
            }
            ValidateEnum(service.Severity, "infrastructure severity", problems);
            ValidateDay(service.StartDay, currentDay, "infrastructure start day", problems, allowFuture: true);
            AddNonNegative(service.RemainingDays, "infrastructure remaining days", problems);
            if (service.Severity == InfrastructureSeverity.Normal || service.RemainingDays <= 0)
            {
                problems.Add($"infrastructure {district}/{type} is not an active disruption");
            }
        }
    }

    private static void ValidateInventory(GameSessionInventorySnapshot inventory, List<string> problems)
    {
        ValidateDictionary(inventory.Quantities, "inventory", problems);
        foreach (var pair in inventory.Quantities)
        {
            if (string.IsNullOrWhiteSpace(pair.Key) || ItemRegistry.GetById(pair.Key) is null)
            {
                problems.Add($"inventory item '{pair.Key}' is not declared");
                continue;
            }
            if (pair.Value <= 0 || pair.Value > ItemRegistry.GetById(pair.Key)!.MaximumQuantity)
            {
                problems.Add($"inventory item '{pair.Key}' has quantity {pair.Value} outside its configured limit");
            }
        }
    }

    private static void ValidateTechnology(GameSessionTechnologySnapshot technology, int currentDay, List<string> problems)
    {
        ValidatePercentage(technology.HandsetDataExposure, "handset data exposure", problems);
        ValidatePercentage(technology.MicrogridRepairDebt, "microgrid repair debt", problems);
        ValidatePercentage(technology.MicrogridStorageCondition, "microgrid storage condition", problems);
        ValidatePercentage(technology.WaterPumpCondition, "water pump condition", problems);
        ValidatePercentage(technology.AllocationModelConfidence, "allocation model confidence", problems);
        ValidateDay(technology.LastTelemedicineTriageDay, currentDay, "telemedicine triage day", problems, allowZero: true);
    }

    private static void ValidateCharacterArcs(GameSessionCharacterArcSnapshot arcs, List<string> problems)
    {
        ValidateDictionary(arcs.Beats, "character arc beats", problems);
        ValidateDictionary(arcs.Decisions, "character arc decisions", problems);
        foreach (var pair in arcs.Beats)
        {
            if (!TryParseEnum<CentralCharacterId>(pair.Key, out _))
            {
                problems.Add($"character arc '{pair.Key}' is not declared");
            }
            if (pair.Value is < 0 or > 6)
            {
                problems.Add($"character arc beat '{pair.Key}' is outside 0..6");
            }
        }
        foreach (var pair in arcs.Decisions)
        {
            if (!TryParseEnum<CentralCharacterId>(pair.Key, out CentralCharacterId character) ||
                !TryParseEnum<CentralArcDecision>(pair.Value, out CentralArcDecision decision) ||
                !IsDecisionForCharacter(character, decision))
            {
                problems.Add($"character arc decision '{pair.Key}:{pair.Value}' is invalid");
            }
        }
    }

    private static void ValidateJournal(IReadOnlyList<EventJournalEntry>? journal, int currentDay, List<string> problems)
    {
        if (journal is null)
        {
            problems.Add("event journal is missing");
            return;
        }
        if (journal.Count > EventJournal.MaxEntries)
        {
            problems.Add($"event journal contains {journal.Count} entries; maximum is {EventJournal.MaxEntries}");
        }
        foreach (var entry in journal)
        {
            if (entry is null)
            {
                problems.Add("event journal contains a null entry");
                continue;
            }
            ValidateDay(entry.Day, currentDay, "journal day", problems);
            ValidateEnum(entry.Source, "journal source", problems);
            if (string.IsNullOrWhiteSpace(entry.Message))
            {
                problems.Add("event journal contains an empty message");
            }
        }
    }

    private static void ValidateRandomState(Slums.Core.Randomness.GameRandomState? state, List<string> problems)
    {
        if (state is not null && (state.S0 | state.S1 | state.S2 | state.S3) == 0)
        {
            problems.Add("random state is all zero and cannot generate values");
        }
    }

    private static void ValidateSkillMap(IReadOnlyDictionary<string, int>? skills, List<string> problems)
    {
        if (!HasReasonableCount(skills, "skill levels", problems))
        {
            return;
        }
        foreach (var pair in skills!)
        {
            if (!TryParseEnum<SkillId>(pair.Key, out _))
            {
                problems.Add($"skill '{pair.Key}' is not declared");
            }
            if (pair.Value is < 0 or > 10)
            {
                problems.Add($"skill '{pair.Key}' level {pair.Value} is outside 0..10");
            }
        }
    }

    private static void ValidateFactionMap(IReadOnlyDictionary<string, int>? values, string label, List<string> problems)
    {
        if (!HasReasonableCount(values, label, problems))
        {
            return;
        }
        foreach (var pair in values!)
        {
            ValidateEnumName<FactionId>(pair.Key, label, problems);
            ValidatePercentage(pair.Value, $"{label} {pair.Key}", problems);
        }
    }

    private static void ValidateDebtorAmounts(IReadOnlyList<DebtorAmountSnapshot>? amounts, string label, List<string> problems)
    {
        if (!HasReasonableCount(amounts, label, problems))
        {
            return;
        }
        foreach (var amount in amounts!)
        {
            if (amount is null || amount.DebtorType is not ("Player" or "Npc"))
            {
                problems.Add($"{label} contains an invalid debtor");
                continue;
            }
            if (amount.DebtorType == "Npc" && (amount.NpcId is null || !TryParseEnum<NpcId>(amount.NpcId, out _)))
            {
                problems.Add($"{label} contains an unknown NPC debtor");
            }
            if (amount.DebtorType == "Player" && amount.NpcId is not null)
            {
                problems.Add($"{label} contains a player debtor with an NPC id");
            }
            AddNonNegative(amount.Amount, $"{label} amount", problems);
        }
    }

    private static bool IsDecisionForCharacter(CentralCharacterId character, CentralArcDecision decision)
    {
        return character switch
        {
            CentralCharacterId.Mother => decision is CentralArcDecision.MotherAcceptCare or CentralArcDecision.MotherKeepPrivate,
            CentralCharacterId.NeighborMona => decision is CentralArcDecision.MonaShareRota or CentralArcDecision.MonaKeepReserve,
            CentralCharacterId.NurseSalma => decision is CentralArcDecision.SalmaPublishEvidence or CentralArcDecision.SalmaProtectPatient,
            CentralCharacterId.HajjMahmoud => decision is CentralArcDecision.MahmoudOpenLedger or CentralArcDecision.MahmoudProtectReputation,
            CentralCharacterId.UmmKarim => decision is CentralArcDecision.UmmKarimShareWarning or CentralArcDecision.UmmKarimLimitExposure,
            _ => false
        };
    }

    private static bool HasReasonableCount<T>(IReadOnlyCollection<T>? values, string label, List<string> problems)
    {
        if (values is null)
        {
            problems.Add($"{label} are missing");
            return false;
        }
        if (values.Count > MaxCollectionEntries)
        {
            problems.Add($"{label} contain {values.Count} entries; maximum is {MaxCollectionEntries}");
            return false;
        }
        return true;
    }

    private static void ValidateStringList(IEnumerable<string>? values, string label, List<string> problems, bool requireNonEmpty)
    {
        if (values is null)
        {
            problems.Add($"{label} are missing");
            return;
        }
        var items = values.ToArray();
        if (items.Length > MaxCollectionEntries)
        {
            problems.Add($"{label} contain too many entries");
        }
        if (requireNonEmpty && items.Any(string.IsNullOrWhiteSpace))
        {
            problems.Add($"{label} contain an empty value");
        }
    }

    private static void ValidateDictionary<TKey, TValue>(IReadOnlyDictionary<TKey, TValue>? values, string label, List<string> problems)
        where TKey : notnull
    {
        if (values is null)
        {
            problems.Add($"{label} are missing");
        }
        else if (values.Count > MaxCollectionEntries)
        {
            problems.Add($"{label} contain too many entries");
        }
    }

    private static void ValidateDay(int value, int maximum, string label, List<string> problems, bool allowZero = false, bool allowFuture = false)
    {
        var upperBound = allowFuture ? maximum + 1000 : maximum;
        if (value < (allowZero ? 0 : 1) || value > upperBound)
        {
            problems.Add($"{label} {value} is outside the valid day range");
        }
    }

    private static void AddNonNegative(int value, string label, List<string> problems)
    {
        if (value < 0)
        {
            problems.Add($"{label} {value} is negative");
        }
    }

    private static void ValidatePercentage(int value, string label, List<string> problems)
    {
        if (value is < 0 or > 100)
        {
            problems.Add($"{label} {value} is outside 0..100");
        }
    }

    private static void ValidateFinite(double value, string label, List<string> problems)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            problems.Add($"{label} is not finite");
        }
    }

    private static void ValidateEnum<TEnum>(TEnum value, string label, List<string> problems)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            problems.Add($"{label} '{value}' is not declared");
        }
    }

    private static void ValidateNullableEnum<TEnum>(TEnum? value, string label, List<string> problems)
        where TEnum : struct, Enum
    {
        if (value is TEnum actual)
        {
            ValidateEnum(actual, label, problems);
        }
    }

    private static void ValidateEnumName<TEnum>(string? value, string label, List<string> problems)
        where TEnum : struct, Enum
    {
        if (value is null || !TryParseEnum<TEnum>(value, out _))
        {
            problems.Add($"{label} '{value}' is not declared");
        }
    }

    private static bool TryParseEnum<TEnum>(string? value, out TEnum result)
        where TEnum : struct, Enum
    {
        return Enum.TryParse(value, ignoreCase: false, out result) && Enum.IsDefined(result);
    }
}
