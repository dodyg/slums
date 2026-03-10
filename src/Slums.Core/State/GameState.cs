using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Expenses;
using Slums.Core.Jobs;
using Slums.Core.World;

namespace Slums.Core.State;

public sealed class GameState
{
    private const int EndOfDayHour = 22;

    public Guid RunId { get; } = Guid.NewGuid();
    public GameClock Clock { get; } = new();
    public PlayerCharacter Player { get; } = new();
    public WorldState World { get; } = new();
    public JobService Jobs { get; } = new();
    public bool IsGameOver { get; private set; }
    public string? GameOverReason { get; private set; }

    public event EventHandler<GameEventArgs>? GameEvent;

    public void AdvanceTime(int minutes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minutes);

        while (minutes > 0)
        {
            var currentMinutes = (Clock.Hour * 60) + Clock.Minute;
            var endOfDayMinutes = EndOfDayHour * 60;

            if (currentMinutes >= endOfDayMinutes)
            {
                EndDay();
                if (IsGameOver)
                {
                    return;
                }

                continue;
            }

            var minutesUntilEndOfDay = endOfDayMinutes - currentMinutes;
            var minutesToAdvance = Math.Min(minutes, minutesUntilEndOfDay);

            Clock.AdvanceMinutes(minutesToAdvance);
            minutes -= minutesToAdvance;

            if (Clock.IsEndOfDay && !IsGameOver)
            {
                EndDay();
                if (IsGameOver)
                {
                    return;
                }
            }
        }
    }

    public void EndDay()
    {
        Player.Stats.ApplyDailyDecay();
        Player.Household.ApplyDailyDecay();

        if (Player.Stats.Money >= RecurringExpenses.DailyRentCost)
        {
            Player.Stats.ModifyMoney(-RecurringExpenses.DailyRentCost);
            RaiseEvent($"Paid rent: {RecurringExpenses.DailyRentCost} LE");
        }
        else
        {
            RaiseEvent("Could not pay rent! The landlord is angry.");
        }

        if (Player.Household.HasEnoughFood)
        {
            Player.Household.ConsumeFood();
            Player.Stats.Eat(20);
        }
        else
        {
            Player.Stats.ModifyHunger(-10);
            RaiseEvent("No food at home. Your family goes hungry.");
        }

        CheckGameOverConditions();
        Clock.AdvanceToNextDay();
    }

    public void RestAtHome()
    {
        Player.Stats.Rest();
        AdvanceTime(8 * 60);
        RaiseEvent("You rest at home. 8 hours pass.");
    }

    public bool TryTravelTo(LocationId locationId)
    {
        var location = WorldState.AllLocations.FirstOrDefault(l => l.Id == locationId);
        if (location is null)
        {
            return false;
        }

        if (Player.Stats.Money < RecurringExpenses.TravelCost)
        {
            RaiseEvent("Not enough money for transport.");
            return false;
        }

        Player.Stats.ModifyMoney(-RecurringExpenses.TravelCost);
        Player.Stats.ModifyEnergy(-5);
        AdvanceTime(location.TravelTimeMinutes);
        World.TravelTo(locationId);

        RaiseEvent($"Traveled to {location.Name}.");
        return true;
    }

    public JobResult WorkJob(JobShift job)
    {
        ArgumentNullException.ThrowIfNull(job);

        var location = World.GetCurrentLocation();
        if (location is null)
        {
            return JobResult.Failed("You are nowhere.");
        }

        var result = Jobs.PerformJob(job, Player, location);
        
        if (result.Success)
        {
            AdvanceTime(job.DurationMinutes);
            RaiseEvent(result.Message);
        }
        else
        {
            RaiseEvent(result.Message);
        }

        CheckGameOverConditions();
        return result;
    }

    public bool BuyFood()
    {
        if (Player.Stats.Money < RecurringExpenses.CheapFoodStockpile)
        {
            RaiseEvent($"Not enough money. Food costs {RecurringExpenses.CheapFoodStockpile} LE.");
            return false;
        }

        Player.Stats.ModifyMoney(-RecurringExpenses.CheapFoodStockpile);
        Player.Household.AddFood(3);
        RaiseEvent($"Bought food supplies for {RecurringExpenses.CheapFoodStockpile} LE. Stockpile: {Player.Household.FoodStockpile}");
        return true;
    }

    public bool BuyMedicine()
    {
        if (Player.Stats.Money < RecurringExpenses.MedicineCost)
        {
            RaiseEvent($"Not enough money. Medicine costs {RecurringExpenses.MedicineCost} LE.");
            return false;
        }

        Player.Stats.ModifyMoney(-RecurringExpenses.MedicineCost);
        Player.Household.UpdateMotherHealth(30);
        RaiseEvent($"Bought medicine for {RecurringExpenses.MedicineCost} LE. Mother's health improved.");
        return true;
    }

    private void CheckGameOverConditions()
    {
        if (Player.Stats.Health <= 0)
        {
            IsGameOver = true;
            GameOverReason = "Your health has failed completely.";
        }
        else if (!Player.Household.MotherAlive)
        {
            IsGameOver = true;
            GameOverReason = "Your mother has passed away. The grief is unbearable.";
        }
        else if (Player.Stats.IsStarving && Player.Stats.IsExhausted && Player.Stats.Money <= 0)
        {
            IsGameOver = true;
            GameOverReason = "Destitution. You have nothing left.";
        }
    }

    private void RaiseEvent(string message)
    {
        GameEvent?.Invoke(this, new GameEventArgs(message));
    }

    public IReadOnlyList<string> GetStatusSummary()
    {
        return
        [
            $"Day {Clock.Day} - {Clock.TimeOfDay}",
            $"Time: {Clock.Hour:D2}:{Clock.Minute:D2}",
            $"Money: {Player.Stats.Money} LE",
            $"Hunger: {Player.Stats.Hunger}%",
            $"Energy: {Player.Stats.Energy}%",
            $"Health: {Player.Stats.Health}%",
            $"Stress: {Player.Stats.Stress}%",
            $"Location: {World.GetCurrentLocation()?.Name ?? "Unknown"}"
        ];
    }
}

public sealed class GameEventArgs(string message) : EventArgs
{
    public string Message { get; } = message;
}
