namespace Slums.Application.Activities;

public sealed class ShopMenuStatusQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<ShopMenuStatus> GetStatuses(ShopMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        var statuses = new List<ShopMenuStatus>
        {
            new(ShopOptionId.BuyFood, "Buy Food", context.FoodCost, context.Money >= context.FoodCost),
            new(ShopOptionId.BuyMedicine, "Buy Medicine", context.MedicineCost, context.Money >= context.MedicineCost)
        };

        if (context.HasClinicServices)
        {
            var note = context.ClinicOpenToday
                ? $"Improves your mother's health. Open today. Cost: {context.ClinicVisitCost} LE."
                : $"Closed on {context.ClinicDayName}. Open: {context.ClinicOpenDaysSummary}.";

            statuses.Add(new ShopMenuStatus(
                ShopOptionId.TakeMotherToClinic,
                "Take Mother to Clinic",
                context.ClinicVisitCost,
                context.ClinicOpenToday && context.Money >= context.ClinicVisitCost,
                note));
        }

        return statuses;
    }
}
