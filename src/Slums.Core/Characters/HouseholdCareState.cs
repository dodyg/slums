namespace Slums.Core.Characters;

public sealed class HouseholdCareState
{
    private const int DefaultMotherHealth = 70;
    private const int DefaultStaplesUnits = 3;
    private const int MaxValue = 100;

    private int _motherHealth = DefaultMotherHealth;
    private bool _motherAlive = true;

    public int MotherHealth => _motherHealth;

    public bool MotherAlive => _motherAlive;

    public MotherCondition MotherCondition { get; private set; } = MotherCondition.Fragile;

    public bool MotherNeedsCare => MotherCondition != MotherCondition.Stable;

    public int StaplesUnits { get; private set; } = DefaultStaplesUnits;

    public int FoodStockpile => StaplesUnits;

    public bool HasEnoughFood => StaplesUnits > 0;

    public int MedicineStock { get; private set; }

    public bool FedMotherToday { get; private set; }

    public bool MedicationGivenToday { get; private set; }

    public bool CheckedOnMotherToday { get; private set; }

    public HouseholdCareState()
    {
        RefreshCondition();
    }

    public void SetMotherHealth(int value)
    {
        _motherHealth = Math.Clamp(value, 0, MaxValue);
        if (_motherHealth == 0)
        {
            _motherAlive = false;
        }

        RefreshCondition();
    }

    public void SetStaplesUnits(int value)
    {
        StaplesUnits = Math.Max(0, value);
    }

    public void SetFoodStockpile(int value)
    {
        SetStaplesUnits(value);
    }

    public void SetMedicineStock(int value)
    {
        MedicineStock = Math.Max(0, value);
    }

    public void AddStaples(int amount)
    {
        StaplesUnits = Math.Max(0, StaplesUnits + amount);
    }

    public void AddFood(int amount)
    {
        AddStaples(amount);
    }

    public void AddMedicine(int amount)
    {
        MedicineStock = Math.Max(0, MedicineStock + amount);
    }

    public bool FeedMother()
    {
        if (StaplesUnits <= 0)
        {
            return false;
        }

        StaplesUnits--;
        FedMotherToday = true;
        return true;
    }

    public void ConsumeFood()
    {
        if (StaplesUnits > 0)
        {
            StaplesUnits--;
        }
    }

    public bool GiveMedicine()
    {
        if (MedicineStock <= 0)
        {
            return false;
        }

        MedicineStock--;
        MedicationGivenToday = true;
        return true;
    }

    public void CheckOnMother()
    {
        CheckedOnMotherToday = true;
    }

    public MotherCareResolution ResolveDay()
    {
        var healthDelta = 0;
        var stressDelta = 0;

        switch (MotherCondition)
        {
            case MotherCondition.Stable:
                if (!FedMotherToday)
                {
                    healthDelta -= 3;
                }

                if (MedicationGivenToday)
                {
                    healthDelta += 2;
                }

                break;

            case MotherCondition.Fragile:
                if (!FedMotherToday)
                {
                    healthDelta -= 5;
                }

                if (!MedicationGivenToday)
                {
                    healthDelta -= 4;
                }

                if (FedMotherToday && MedicationGivenToday)
                {
                    healthDelta += 4;
                }

                break;

            case MotherCondition.Crisis:
                if (!FedMotherToday)
                {
                    healthDelta -= 8;
                }

                if (!MedicationGivenToday)
                {
                    healthDelta -= 7;
                }

                if (!CheckedOnMotherToday)
                {
                    stressDelta += 3;
                }

                if (FedMotherToday && MedicationGivenToday)
                {
                    healthDelta += 8;
                }

                break;
        }

        _motherHealth = Math.Clamp(_motherHealth + healthDelta, 0, MaxValue);
        if (_motherHealth == 0)
        {
            _motherAlive = false;
        }

        RefreshCondition();
        return new MotherCareResolution(healthDelta, stressDelta, MotherCondition, _motherAlive);
    }

    public void UpdateMotherHealth(int change)
    {
        _motherHealth = Math.Clamp(_motherHealth + change, 0, MaxValue);
        if (_motherHealth == 0)
        {
            _motherAlive = false;
        }

        RefreshCondition();
    }

    public void BeginNewDay()
    {
        FedMotherToday = false;
        MedicationGivenToday = false;
        CheckedOnMotherToday = false;
    }

    private void RefreshCondition()
    {
        MotherCondition = _motherHealth switch
        {
            >= 65 => MotherCondition.Stable,
            >= 30 => MotherCondition.Fragile,
            _ => MotherCondition.Crisis
        };
    }
}