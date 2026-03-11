using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Application.Activities;

public sealed class GameStatusPageQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<GameStatusPage> GetPages(GameState gameState)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameState);

        return
        [
            BuildSurvivalPage(gameState),
            BuildSkillsPage(gameState),
            BuildNetworkPage(gameState),
            BuildSignalsPage(gameState),
            BuildProgressPage(gameState)
        ];
    }

    private static GameStatusPage BuildSurvivalPage(GameState gameState)
    {
        var household = gameState.Player.Household;
        var location = gameState.World.GetCurrentLocation()?.Name ?? "Unknown";

        return new GameStatusPage(
            "Survival",
            [
                $"Day {gameState.Clock.Day} at {gameState.Clock.Hour:D2}:{gameState.Clock.Minute:D2}",
                $"Location: {location}",
                $"District: {DistrictInfo.GetName(gameState.World.CurrentDistrict)}",
                $"Money: {gameState.Player.Stats.Money} LE",
                $"Police pressure: {gameState.PolicePressure}",
                $"Food: {household.FoodStockpile} | Medicine: {household.MedicineStock}",
                $"Local prices: food {gameState.GetFoodCost()} LE | street {gameState.GetStreetFoodCost()} LE | medicine {gameState.GetMedicineCost()} LE",
                $"Mother: {household.MotherHealth}% {household.MotherCondition}",
                $"Mother fed today: {ToYesNo(household.FedMotherToday)}"
            ]);
    }

    private static GameStatusPage BuildSkillsPage(GameState gameState)
    {
        var lines = Enum
            .GetValues<SkillId>()
            .Select(skillId => $"{GetSkillName(skillId)}: {gameState.Player.Skills.GetLevel(skillId)}")
            .ToList();

        lines.Add($"Medicine price: {gameState.GetMedicineCost()} LE");

        return new GameStatusPage("Skills", lines);
    }

    private static GameStatusPage BuildNetworkPage(GameState gameState)
    {
        var lines = new List<string>
        {
            $"Imbaba Crew: {gameState.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation}",
            $"Dokki Thugs: {gameState.Relationships.GetFactionStanding(FactionId.DokkiThugs).Reputation}",
            $"Ex-Prisoner Net: {gameState.Relationships.GetFactionStanding(FactionId.ExPrisonerNetwork).Reputation}"
        };

        foreach (var npcId in GetKeyNpcOrder())
        {
            var relationship = gameState.Relationships.GetNpcRelationship(npcId);
            lines.Add($"{NpcRegistry.GetName(npcId)}: trust {relationship.Trust}");
        }

        return new GameStatusPage("Network", lines);
    }

    private static GameStatusPage BuildProgressPage(GameState gameState)
    {
        var lines = new List<string>
        {
            $"Days survived: {gameState.DaysSurvived}",
            $"Honest shifts: {gameState.HonestShiftsCompleted}",
            $"Honest earnings: {gameState.TotalHonestWorkEarnings} LE",
            $"Crimes committed: {gameState.CrimesCommitted}",
            $"Crime earnings: {gameState.TotalCrimeEarnings} LE",
            $"Last crime day: {gameState.LastCrimeDay}",
            $"Last honest-work day: {gameState.LastHonestWorkDay}"
        };

        lines.AddRange(GetTrajectoryHints(gameState).Select(static hint => $"Hint: {hint}"));
        return new GameStatusPage("Progress", lines);
    }

    private static GameStatusPage BuildSignalsPage(GameState gameState)
    {
        var lines = new List<string>();

        if (gameState.LastCrimeDay > 0 && gameState.Clock.Day - gameState.LastCrimeDay <= 1 && gameState.PolicePressure >= 60)
        {
            lines.Add("Public-facing work is likely to trigger suspicion while street heat is this high.");
        }

        if (gameState.TotalCrimeEarnings >= 150 && gameState.CrimesCommitted >= 2 && gameState.Player.Household.MotherHealth < 65 && !gameState.HasStoryFlag("event_mother_wrong_money_seen"))
        {
            lines.Add("Home is primed for a tense reaction to sudden money.");
        }

        if (gameState.PolicePressure >= 60 && gameState.Relationships.GetNpcRelationship(NpcId.NeighborMona).Trust >= 15 && !gameState.HasStoryFlag("event_neighbor_watch_seen"))
        {
            lines.Add("Mona is currently positioned to warn you if the building gets watched.");
        }

        if (gameState.Player.BackgroundType == BackgroundType.MedicalSchoolDropout && !gameState.HasStoryFlag("background_medical_clinic_seen"))
        {
            lines.Add("A successful clinic shift can still trigger the medical-dropout clinic reflection.");
        }

        if (gameState.Player.BackgroundType == BackgroundType.MedicalSchoolDropout &&
            gameState.Relationships.GetNpcRelationship(NpcId.NurseSalma).Trust >= 12 &&
            gameState.Player.Household.MotherHealth < 65)
        {
            lines.Add("Salma is in a position to quietly help with medicine after a good clinic day.");
        }

        if (gameState.CrimesCommitted == 0 && !gameState.HasStoryFlag("crime_first_success"))
        {
            lines.Add("Your first successful crime still has a unique aftermath beat waiting.");
        }

        if (lines.Count == 0)
        {
            lines.Add("No high-priority narrative signals are active right now.");
        }

        return new GameStatusPage("Signals", lines);
    }

    private static IEnumerable<string> GetTrajectoryHints(GameState gameState)
    {
        if (gameState.CrimesCommitted == 0)
        {
            yield return "Clean run intact so far.";
        }

        if (gameState.PolicePressure >= 85)
        {
            yield return "Heat is high enough to threaten arrest or a buried-by-heat ending.";
        }

        if (gameState.Player.Stats.Money > 500 && gameState.CrimesCommitted <= 3 && gameState.Player.Household.MotherHealth > 60)
        {
            yield return "Luxor route conditions are currently strong.";
        }

        if (gameState.TotalCrimeEarnings >= 1000 && gameState.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation > 50)
        {
            yield return "Crime-kingpin route is within reach.";
        }

        if (gameState.CrimesCommitted > 0 && gameState.LastCrimeDay > 0 && gameState.Clock.Day - gameState.LastCrimeDay >= 5)
        {
            yield return "You have been away from crime long enough to keep a leaving-crime route alive.";
        }

        if (gameState.DaysSurvived >= 30 && gameState.CrimesCommitted == 0 && gameState.Player.Stats.Money > 200 && gameState.PolicePressure < 20)
        {
            yield return "Honest-stability conditions are already met.";
        }
    }

    private static IEnumerable<NpcId> GetKeyNpcOrder()
    {
        yield return NpcId.LandlordHajjMahmoud;
        yield return NpcId.NeighborMona;
        yield return NpcId.NurseSalma;
        yield return NpcId.PharmacistMariam;
        yield return NpcId.DispatcherSafaa;
        yield return NpcId.LaundryOwnerIman;
        yield return NpcId.FixerUmmKarim;
        yield return NpcId.FenceHanan;
        yield return NpcId.RunnerYoussef;
    }

    private static string GetSkillName(SkillId skillId)
    {
        return skillId switch
        {
            SkillId.StreetSmarts => "Street Smarts",
            _ => skillId.ToString()
        };
    }

    private static string ToYesNo(bool value)
    {
        return value ? "Yes" : "No";
    }
}