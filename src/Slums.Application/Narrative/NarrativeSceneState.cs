using Slums.Core.State;

namespace Slums.Application.Narrative;

public sealed record NarrativeSceneState(
    int Money,
    int Health,
    int Energy,
    int Hunger,
    int Stress,
    int MotherHealth,
    int FoodStockpile,
    int Day,
    string? Background)
{
    public static NarrativeSceneState Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new NarrativeSceneState(
            gameSession.Player.Stats.Money,
            gameSession.Player.Stats.Health,
            gameSession.Player.Stats.Energy,
            gameSession.Player.Stats.Hunger,
            gameSession.Player.Stats.Stress,
            gameSession.Player.Household.MotherHealth,
            gameSession.Player.Household.FoodStockpile,
            gameSession.Clock.Day,
            gameSession.Player.Background?.Type.ToString());
    }
}
