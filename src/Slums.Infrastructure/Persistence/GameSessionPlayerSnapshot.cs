using Slums.Core.Characters;
using Slums.Core.Skills;
using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionPlayerSnapshot
{
    public BackgroundType BackgroundType { get; init; }

    public int Money { get; init; }

    public int Satiety { get; init; }

    public int DaysUndereating { get; init; }

    public int Energy { get; init; }

    public int Health { get; init; }

    public int Stress { get; init; }

    public int MotherHealth { get; init; }

    public int FoodStockpile { get; init; }

    public int MedicineStock { get; init; }

    public Dictionary<string, int> SkillLevelsById { get; init; } = [];

    public static GameSessionPlayerSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionPlayerSnapshot
        {
            BackgroundType = gameSession.Player.BackgroundType,
            Money = gameSession.Player.Stats.Money,
            Satiety = gameSession.Player.Nutrition.Satiety,
            DaysUndereating = gameSession.Player.Nutrition.DaysUndereating,
            Energy = gameSession.Player.Stats.Energy,
            Health = gameSession.Player.Stats.Health,
            Stress = gameSession.Player.Stats.Stress,
            MotherHealth = gameSession.Player.Household.MotherHealth,
            FoodStockpile = gameSession.Player.Household.FoodStockpile,
            MedicineStock = gameSession.Player.Household.MedicineStock,
            SkillLevelsById = gameSession.Player.Skills.Levels.ToDictionary(static pair => pair.Key.ToString(), static pair => pair.Value)
        };
    }

    public IEnumerable<KeyValuePair<SkillId, int>> EnumerateSkillLevels()
    {
        return SkillLevelsById.Select(static pair => new KeyValuePair<SkillId, int>(Enum.Parse<SkillId>(pair.Key), pair.Value));
    }
}
