using Slums.Core.World;

namespace Slums.Application.Activities;

public sealed class ShopMenuStatusQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<ShopMenuStatus> GetStatuses(ShopMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        var statuses = new List<ShopMenuStatus>();

        if (context.HasHouseholdAssetsAccess)
        {
            statuses.Add(new ShopMenuStatus(
                ShopOptionId.OpenHouseholdAssets,
                GetHouseholdOptionName(context),
                0,
                true,
                GetHouseholdOptionNote(context)));
        }

        statuses.Add(new ShopMenuStatus(ShopOptionId.BuyFood, "Buy Food", context.FoodCost, context.Money >= context.FoodCost));
        statuses.Add(new ShopMenuStatus(ShopOptionId.BuyMedicine, "Buy Medicine", context.MedicineCost, context.Money >= context.MedicineCost));

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

    private static string GetHouseholdOptionName(ShopMenuContext context)
    {
        return context.CurrentLocationId switch
        {
            var locationId when locationId == LocationId.PlantShop => "Buy Plants",
            var locationId when locationId == LocationId.FishMarket => "Buy Fish Tank",
            _ => "Pets & Plants"
        };
    }

    private static string GetHouseholdOptionNote(ShopMenuContext context)
    {
        var locationName = context.LocationName ?? "this location";

        return context.CurrentLocationId switch
        {
            var locationId when locationId == LocationId.PlantShop =>
                $"Open the household-assets catalog for {locationName} to buy herbs, flowers, and aloe.",
            var locationId when locationId == LocationId.FishMarket =>
                $"Open the household-assets catalog for {locationName} to purchase the fish tank.",
            _ =>
                "Open the household-assets screen to adopt cats, cover weekly care, or manage plant upgrades."
        };
    }
}
