using System.Collections.ObjectModel;
using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Infrastructure.Persistence;

public sealed record GameStateDto
{
    public int Money { get; init; }
    public int Hunger { get; init; }
    public int Energy { get; init; }
    public int Health { get; init; }
    public int Stress { get; init; }
    public int MotherHealth { get; init; }
    public int FoodStockpile { get; init; }
    public int Day { get; init; }
    public int Hour { get; init; }
    public int Minute { get; init; }
    public BackgroundType BackgroundType { get; init; }
    public string CurrentLocationId { get; init; } = LocationId.Home.Value;
    public int PolicePressure { get; init; }
    public int TotalCrimeEarnings { get; init; }
    public int CrimesCommitted { get; init; }
    public int DaysSurvived { get; init; }
    public Dictionary<string, int> SkillLevels { get; init; } = [];
    public Dictionary<string, int> NpcTrust { get; init; } = [];
    public Dictionary<string, int> NpcLastSeenDay { get; init; } = [];
    public Dictionary<string, int> FactionReputation { get; init; } = [];
    public Collection<string> StoryFlags { get; init; } = [];

    public static GameStateDto FromGameState(GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        return new GameStateDto
        {
            Money = gameState.Player.Stats.Money,
            Hunger = gameState.Player.Stats.Hunger,
            Energy = gameState.Player.Stats.Energy,
            Health = gameState.Player.Stats.Health,
            Stress = gameState.Player.Stats.Stress,
            MotherHealth = gameState.Player.Household.MotherHealth,
            FoodStockpile = gameState.Player.Household.FoodStockpile,
            Day = gameState.Clock.Day,
            Hour = gameState.Clock.Hour,
            Minute = gameState.Clock.Minute,
            BackgroundType = gameState.Player.BackgroundType,
            CurrentLocationId = gameState.World.CurrentLocationId.Value,
            PolicePressure = gameState.PolicePressure,
            TotalCrimeEarnings = gameState.TotalCrimeEarnings,
            CrimesCommitted = gameState.CrimesCommitted,
            DaysSurvived = gameState.DaysSurvived,
            SkillLevels = gameState.Player.Skills.Levels.ToDictionary(static pair => pair.Key.ToString(), static pair => pair.Value),
            NpcTrust = gameState.Relationships.NpcRelationships.ToDictionary(static pair => pair.Key.ToString(), static pair => pair.Value.Trust),
            NpcLastSeenDay = gameState.Relationships.NpcRelationships.ToDictionary(static pair => pair.Key.ToString(), static pair => pair.Value.LastSeenDay),
            FactionReputation = gameState.Relationships.FactionStandings.ToDictionary(static pair => pair.Key.ToString(), static pair => pair.Value.Reputation),
            StoryFlags = new Collection<string>([.. gameState.StoryFlags])
        };
    }

    public GameState ToGameState(Guid runId)
    {
        var gameState = new GameState();
        gameState.SetRunId(runId);
        gameState.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType));
        gameState.Player.Stats.SetMoney(Money);
        gameState.Player.Stats.SetHunger(Hunger);
        gameState.Player.Stats.SetEnergy(Energy);
        gameState.Player.Stats.SetHealth(Health);
        gameState.Player.Stats.SetStress(Stress);
        gameState.Player.Household.SetMotherHealth(MotherHealth);
        gameState.Player.Household.SetFoodStockpile(FoodStockpile);
        gameState.Player.Skills.Restore(SkillLevels.Select(static pair => new KeyValuePair<SkillId, int>(Enum.Parse<SkillId>(pair.Key), pair.Value)));
        gameState.Clock.SetTime(Day, Hour, Minute);
        gameState.World.TravelTo(new LocationId(CurrentLocationId));
        gameState.SetPolicePressure(PolicePressure);
        gameState.SetCrimeCounters(TotalCrimeEarnings, CrimesCommitted);
        gameState.SetDaysSurvived(DaysSurvived);
        gameState.RestoreStoryFlags(StoryFlags);

        foreach (var npcId in Enum.GetValues<NpcId>())
        {
            var key = npcId.ToString();
            var trust = NpcTrust.GetValueOrDefault(key);
            var lastSeenDay = NpcLastSeenDay.GetValueOrDefault(key);
            gameState.Relationships.SetNpcRelationship(npcId, trust, lastSeenDay);
        }

        foreach (var factionId in Enum.GetValues<FactionId>())
        {
            var key = factionId.ToString();
            gameState.Relationships.SetFactionStanding(factionId, FactionReputation.GetValueOrDefault(key));
        }

        return gameState;
    }
}