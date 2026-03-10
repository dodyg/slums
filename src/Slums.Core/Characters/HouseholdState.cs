namespace Slums.Core.Characters;

using System.Text;

public sealed class HouseholdState
{
    private bool _motherAlive = true;
    private int _motherHealth = 70;
    private int _motherNeedsMedicine;
    private int _foodStockpile = 3;

    public bool MotherAlive => _motherAlive;
    public int MotherHealth => _motherHealth;
    public int FoodStockpile => _foodStockpile;
    public bool MotherNeedsCare => _motherHealth < 50;
    public bool HasEnoughFood => _foodStockpile > 0;

    public void SetMotherHealth(int value)
    {
        _motherHealth = Math.Max(0, Math.Min(100, value));
        if (_motherHealth <= 0)
        {
            _motherAlive = false;
        }
    }

    public void SetFoodStockpile(int value) => _foodStockpile = Math.Max(0, value);

    public void ConsumeFood()
    {
        if (_foodStockpile > 0)
        {
            _foodStockpile--;
        }
    }

    public void AddFood(int amount) => _foodStockpile += amount;

    public void UpdateMotherHealth(int change)
    {
        var newHealth = _motherHealth + change;
        _motherHealth = Math.Max(0, Math.Min(100, newHealth));
        if (_motherHealth <= 2)
        {
            _motherAlive = false;
        }
    }

    public void ApplyDailyDecay()
    {
        if (!HasEnoughFood)
        {
            UpdateMotherHealth(-5);
        }

        if (_motherNeedsMedicine > 0)
        {
            _motherNeedsMedicine--;
            if (_motherNeedsMedicine == 5)
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
        _motherNeedsMedicine = 3;
    }
}
