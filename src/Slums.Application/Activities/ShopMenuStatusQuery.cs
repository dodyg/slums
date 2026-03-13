namespace Slums.Application.Activities;

public sealed class ShopMenuStatusQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<ShopMenuStatus> GetStatuses(ShopMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        return
        [
            new ShopMenuStatus("Buy Food", context.FoodCost, context.Money >= context.FoodCost),
            new ShopMenuStatus("Buy Medicine", context.MedicineCost, context.Money >= context.MedicineCost)
        ];
    }
}
