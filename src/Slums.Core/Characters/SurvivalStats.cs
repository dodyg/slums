namespace Slums.Core.Characters;

public sealed class SurvivalStats
{
    private const int MaxStatValue = 100;
    private const int MinStatValue = 0;

    public int Money { get; private set; } = 100;
    public int Hunger { get; private set; } = 80;
    public int Energy { get; private set; } = 80;
    public int Health { get; private set; } = 100;
    public int Stress { get; private set; } = 20;

    public bool IsStarving => Hunger <= 10;
    public bool IsExhausted => Energy <= 10;
    public bool IsSick => Health <= 30;
    public bool IsOverstressed => Stress >= 80;

    public void SetMoney(int value) => Money = Math.Max(0, value);
    public void SetHunger(int value) => Hunger = Clamp(value);
    public void SetEnergy(int value) => Energy = Clamp(value);
    public void SetHealth(int value) => Health = Clamp(value);
    public void SetStress(int value) => Stress = Clamp(value);

    public void ModifyHunger(int amount)
    {
        Hunger = Clamp(Hunger + amount);
    }

    public void ModifyEnergy(int amount)
    {
        Energy = Clamp(Energy + amount);
    }

    public void ModifyHealth(int amount)
    {
        Health = Clamp(Health + amount);
    }

    public void ModifyStress(int amount)
    {
        Stress = Clamp(Stress + amount);
    }

    public void ModifyMoney(int amount)
    {
        Money += amount;
        if (Money < 0)
        {
            Money = 0;
        }
    }

    public void Rest()
    {
        Energy = Clamp(Energy + 30);
        Hunger = Clamp(Hunger - 10);
        Stress = Clamp(Stress - 15);
    }

    public void Eat(int quality)
    {
        ModifyHunger(quality);
        ModifyEnergy(quality / 4);
    }

    public void ApplyDailyDecay()
    {
        ModifyHunger(-12);
        ModifyEnergy(-10);
        ModifyStress(3);
    }

    private static int Clamp(int value)
    {
        return Math.Max(MinStatValue, Math.Min(MaxStatValue, value));
    }
}
