using Slums.Core.State;
using Slums.Core.Skills;
using Slums.Core.World;

namespace Slums.Application.Activities;

public sealed record ShopMenuContext(
    LocationId CurrentLocationId,
    string? LocationName,
    bool HasHouseholdAssetsAccess,
    int Money,
    int FoodCost,
    int MedicineCost,
    int FoodStockpile,
    int MotherHealth,
    bool MotherNeedsCare,
    bool HasClinicServices,
    bool ClinicOpenToday,
    int ClinicVisitCost,
    string ClinicDayName,
    string ClinicOpenDaysSummary,
    int ProvisioningSkillLevel,
    int FoodBundleUnits,
    ProvisioningMealPlan MealPreview)
{
    public static ShopMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var clinicStatus = gameSession.GetCurrentLocationClinicStatus();
        var currentLocation = gameSession.World.GetCurrentLocation();

        return new ShopMenuContext(
            gameSession.World.CurrentLocationId,
            currentLocation?.Name,
            gameSession.CanUseHouseholdAssets(),
            gameSession.Player.Stats.Money,
            gameSession.GetFoodCost(),
            gameSession.GetMedicineCost(),
            gameSession.Player.Household.FoodStockpile,
            gameSession.Player.Household.MotherHealth,
            gameSession.Player.Household.MotherNeedsCare,
            clinicStatus.HasClinicServices,
            clinicStatus.IsOpenToday,
            clinicStatus.VisitCost,
            clinicStatus.CurrentDayName,
            clinicStatus.OpenDaysSummary,
            gameSession.Player.Skills.GetLevel(SkillId.Provisioning),
            ProvisioningCalculator.GetFoodBundleUnits(gameSession.Player.Skills.GetLevel(SkillId.Provisioning)),
            gameSession.GetProvisioningMealPlan());
    }
}
