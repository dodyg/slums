namespace Slums.Core.Expenses;

public static class RecurringExpenses
{
    public const int DailyRentCost = 20;
    public const int CheapMealCost = 5;
    public const int GoodMealCost = 15;
    public const int MedicineCost = 40;
    public const int CheapFoodStockpile = 15;
    public const int TravelCost = 2;

    public static int CalculateDailyExpenses(HouseholdNeeds needs)
    {
        ArgumentNullException.ThrowIfNull(needs);

        var total = DailyRentCost;

        if (needs.NeedsFood)
        {
            total += CheapMealCost;
        }

        if (needs.NeedsMedicine)
        {
            total += MedicineCost;
        }

        return total;
    }
}

public sealed class HouseholdNeeds
{
    public bool NeedsFood { get; set; }
    public bool NeedsMedicine { get; set; }
    public bool NeedsRentPayment { get; set; }
}
