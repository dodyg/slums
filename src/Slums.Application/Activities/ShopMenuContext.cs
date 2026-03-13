using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed record ShopMenuContext(
    int Money,
    int FoodCost,
    int MedicineCost,
    int FoodStockpile,
    int MotherHealth,
    bool MotherNeedsCare)
{
    public static ShopMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new ShopMenuContext(
            gameSession.Player.Stats.Money,
            gameSession.GetFoodCost(),
            gameSession.GetMedicineCost(),
            gameSession.Player.Household.FoodStockpile,
            gameSession.Player.Household.MotherHealth,
            gameSession.Player.Household.MotherNeedsCare);
    }
}
