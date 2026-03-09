using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Expenses;
using Slums.Core.World;

namespace Slums.Core.State;

public sealed class GameState
{
    public Guid RunId { get; } = Guid.NewGuid();
    public GameClock Clock { get; } = new();
    public PlayerCharacter Player { get; } = new();
    public WorldState World { get; } = new();
    public bool IsGameOver { get; private set; }
    public string? GameOverReason { get; private set; }

    public event EventHandler<GameEventArgs>? GameEvent;

    public void AdvanceTime(int minutes)
    {
        Clock.AdvanceMinutes(minutes);

        if (Clock.Hour >= 22 && Clock.Minute >= 0)
        {
            EndDay();
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
        Clock.AdvanceHours(8);
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
        Clock.AdvanceMinutes(location.TravelTimeMinutes);
        World.TravelTo(locationId);

        RaiseEvent($"Traveled to {location.Name}.");
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
