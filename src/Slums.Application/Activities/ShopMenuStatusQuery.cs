using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class ShopMenuStatusQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<ShopMenuStatus> GetStatuses(GameState gameState)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var money = gameState.Player.Stats.Money;
        var foodCost = gameState.GetFoodCost();
        var medicineCost = gameState.GetMedicineCost();

        return
        [
            new ShopMenuStatus("Buy Food", foodCost, money >= foodCost),
            new ShopMenuStatus("Buy Medicine", medicineCost, money >= medicineCost)
        ];
    }
}
