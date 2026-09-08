using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.Economy;
using Slums.Core.Home;
using Slums.Core.Investments;
using Slums.Core.Relationships;
using Slums.Core.Robotics;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.Territory;
using Slums.Core.World;
using Slums.Core.World.News;

namespace Slums.Core.Expenses;

/// <summary>
/// Single calculation path for every player-facing price. Previews and commits both call into
/// this pipeline, so the quoted price can never drift from the charged price.
/// </summary>
internal static class PriceModifierPipeline
{
    internal static int GetFoodCost(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var (baseModifier, foodPriceShock) = GetFoodModifierCore(session, streetFood: false);
        var modifiedCost = LocationPricingService.GetFoodCost(session.World.CurrentDistrict)
            + baseModifier
            - ProvisioningCalculator.GetFoodPriceReduction(session.Player.Skills.GetLevel(SkillId.Provisioning), foodPriceShock);
        return Math.Max(1, modifiedCost);
    }

    internal static int GetStreetFoodCost(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var (baseModifier, _) = GetFoodModifierCore(session, streetFood: true);
        var modifiedCost = LocationPricingService.GetStreetFoodCost(session.World.CurrentDistrict) + baseModifier;
        return Math.Max(1, modifiedCost);
    }

    internal static int GetMedicineCost(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var districtModifier = GetDistrictModifier(session, session.World.CurrentDistrict, static effect => effect.MedicineCostModifier);
        var modifiedCost = LocationPricingService.GetMedicineCost(session.World.CurrentDistrict, session.World.CurrentLocationId, session.Relationships, session.Player.Skills)
            + districtModifier
            + InfrastructureImpactCalculator.GetMedicinePriceModifier(session.Infrastructure, session.World.CurrentDistrict);
        modifiedCost -= InvestmentPurchaseService.GetMedicineCostDiscount(session);
        return Math.Max(1, modifiedCost);
    }

    internal static int GetClinicVisitCost(GameSession session, Location location)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(location);
        var districtModifier = GetDistrictModifier(session, location.District, static effect => effect.ClinicVisitCostModifier);
        var schedule = session.GetCurrentSchedule();
        var scheduleDiscount = schedule.ClinicDiscount ? schedule.ClinicDiscountAmount : 0;
        if (scheduleDiscount > 0 && session.Player.BackgroundType == BackgroundType.MedicalSchoolDropout)
        {
            scheduleDiscount *= 2;
        }

        var modifiedCost = LocationPricingService.GetClinicVisitCost(location, session.Relationships, session.Player.Skills)
            + districtModifier
            - scheduleDiscount
            - RobotCapabilityRules.GetClinicCostReduction(session.Player.Robotics);
        return Math.Max(1, modifiedCost);
    }

    internal static int GetTravelCost(GameSession session, Location destination)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(destination);
        var districtModifier = GetDistrictModifier(session, destination.District, static effect => effect.TravelCostModifier);
        var modifiedCost = LocationPricingService.GetTravelCost(destination, session.Relationships)
            + districtModifier
            + session.CurrentWeather.TravelCostModifier
            + InfrastructureImpactCalculator.GetTravelCostModifier(session.Infrastructure, destination.District)
            + NewsImpactCalculator.GetTravelCostModifier(session.News, destination.District, session.ContentCatalog.NewsFlashes);
        return Math.Max(1, modifiedCost);
    }

    internal static int GetTravelEnergyCost(GameSession session, Location destination)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(destination);
        var districtModifier = GetDistrictModifier(session, destination.District, static effect => effect.TravelEnergyModifier);
        var modifiedCost = LocationPricingService.GetTravelEnergyCost(destination, session.Relationships)
            + districtModifier
            - RobotCapabilityRules.GetTransitEnergyReduction(session.Player.Robotics);
        return Math.Max(1, modifiedCost);
    }

    internal static int GetTravelTimeMinutes(GameSession session, Location destination)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(destination);
        var districtModifier = GetDistrictModifier(session, destination.District, static effect => effect.TravelTimeMinutesModifier);
        var modifiedMinutes = destination.TravelTimeMinutes
            + districtModifier
            + InfrastructureImpactCalculator.GetTravelTimeModifier(session.Infrastructure, destination.District);
        return Math.Max(1, modifiedMinutes);
    }

    private static int GetDistrictModifier(GameSession session, DistrictId district, Func<DistrictConditionEffect, int> selector)
    {
        return session.GetActiveDistrictConditionDefinition(district)?.Effect is { } effect ? selector(effect) : 0;
    }

    /// <summary>Shared modifier core for staple food and street food; the two contexts differ only in the district key and the staple-only discounts.</summary>
    private static (int BaseModifier, int FoodPriceShock) GetFoodModifierCore(GameSession session, bool streetFood)
    {
        var districtCondition = session.GetActiveDistrictConditionDefinition(session.World.CurrentDistrict);
        var districtModifier = districtCondition?.Effect is { } effect
            ? streetFood ? effect.StreetFoodCostModifier : effect.FoodCostModifier
            : 0;
        var schedule = session.GetCurrentSchedule();
        var seasonModifiers = SeasonModifiersRegistry.GetModifiers(session.GetCurrentSeason());
        var baseModifier = districtModifier + schedule.FoodCostModifier + seasonModifiers.FoodCostModifier + session.CurrentWeather.FoodCostModifier;
        if (session.Player.BackgroundType == BackgroundType.SudaneseRefugee && schedule.FoodCostModifier < 0)
        {
            baseModifier -= 1;
        }

        baseModifier += TerritoryDynamicsCalculator.GetFoodPriceModifier(session.Territory, session.World.CurrentDistrict);
        baseModifier += MealService.GetUmmKarimFoodDiscount(session);
        var foodPriceShock = NewsImpactCalculator.GetFoodPriceModifier(session.News, session.World.CurrentDistrict, session.ContentCatalog.NewsFlashes);
        baseModifier += foodPriceShock;
        if (streetFood)
        {
            return (baseModifier, foodPriceShock);
        }

        baseModifier -= InvestmentPurchaseService.GetFoodCostDiscount(session, session.World.CurrentDistrict);
        return (baseModifier, foodPriceShock);
    }
}
