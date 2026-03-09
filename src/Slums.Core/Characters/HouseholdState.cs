namespace Slums.Core.Characters;

public sealed class HouseholdState
{
    public bool MotherAlive { get; private set; } = true;
    public int MotherHealth { get; private set; } = 70;
    public int MotherNeedsMedicine { get; private set; }
    public int FoodStockpile { get; private set; } = 3;

    public bool MotherNeedsCare => MotherHealth < 50;
    public bool HasEnoughFood => FoodStockpile > 0;

    public void ConsumeFood()
    {
        if (FoodStockpile > 0)
        {
            FoodStockpile--;
        }
    }

    public void AddFood(int amount)
    {
        FoodStockpile += amount;
    }

    public void UpdateMotherHealth(int change)
    {
        MotherHealth = Math.Max(0, Math.Min(100, MotherHealth + change));
        if (MotherHealth <= 0)
        {
            MotherAlive = false;
        }
    }

    public void ApplyDailyDecay()
    {
        if (!HasEnoughFood)
        {
            UpdateMotherHealth(-5);
        }

        if (MotherNeedsMedicine > 0)
        {
            MotherNeedsMedicine--;
            if (MotherNeedsMedicine == 0)
            {
                UpdateMotherHealth(10);
            }
        }
        else if (MotherNeedsCare)
        {
            UpdateMotherHealth(-2);
        }
    }

    public void RequestMedicine()
    {
        MotherNeedsMedicine = 3;
    }
}
