using Slums.Core.Expenses;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Application.Activities;

/// <summary>
/// A side-effect-free estimate of the cash pressure visible at the start of a day.
/// </summary>
public sealed record SurvivalForecast(
    int CashOnHand,
    int RentDueToday,
    int FoodBundleCost,
    int MealsPerFoodBundle,
    int MedicineBundleCost,
    int DosesPerMedicineBundle,
    int TransportReserve,
    int DailyCashNeed,
    int CashRunwayDays,
    int CashRunwayAfterMedicineDays,
    int FoodMealsOnHand,
    int MedicineDosesOnHand)
{
    public static SurvivalForecast Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var transportReserve = WorldState.AllLocations
            .Where(static location => location.HasJobOpportunities)
            .Select(location => gameSession.GetTravelCost(location.Id))
            .DefaultIfEmpty(RecurringExpenses.TravelCost)
            .Min();
        var mealsPerFoodBundle = 3;
        var dailyFoodCost = DivideRoundUp(gameSession.GetFoodCost(), mealsPerFoodBundle);
        var dailyCashNeed = RecurringExpenses.DailyRentCost + dailyFoodCost + transportReserve;
        var medicineReserve = gameSession.Player.Household.MedicineStock >= 2
            ? 0
            : gameSession.GetMedicineCost();

        return new SurvivalForecast(
            gameSession.Player.Stats.Money,
            RecurringExpenses.DailyRentCost,
            gameSession.GetFoodCost(),
            mealsPerFoodBundle,
            gameSession.GetMedicineCost(),
            2,
            transportReserve,
            dailyCashNeed,
            gameSession.Player.Stats.Money / dailyCashNeed,
            Math.Max(0, gameSession.Player.Stats.Money - medicineReserve) / dailyCashNeed,
            gameSession.Player.Household.FoodStockpile,
            gameSession.Player.Household.MedicineStock);
    }

    private static int DivideRoundUp(int numerator, int denominator)
    {
        return (numerator + denominator - 1) / denominator;
    }
}
