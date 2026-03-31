namespace Slums.Core.Characters;

public sealed class NutritionState
{
    private const int DefaultSatiety = 75;
    private const int MaxValue = 100;
    private const int DailySatietyDecay = 15;

    public int Satiety { get; private set; } = DefaultSatiety;

    public int DaysUndereating { get; private set; }

    public MealQuality LastMealQuality { get; private set; } = MealQuality.None;

    public bool AteToday => LastMealQuality != MealQuality.None;

    public void SetSatiety(int value)
    {
        Satiety = Math.Clamp(value, 0, MaxValue);
    }

    public void SetDaysUndereating(int value)
    {
        DaysUndereating = Math.Max(0, value);
    }

    public void ModifySatiety(int amount)
    {
        Satiety = Math.Clamp(Satiety + amount, 0, MaxValue);
    }

    public void Eat(MealQuality quality)
    {
        var satietyGain = quality switch
        {
            MealQuality.Scraps => 10,
            MealQuality.Basic => 22,
            MealQuality.HotMeal => 35,
            _ => 0
        };

        ModifySatiety(satietyGain);
        LastMealQuality = quality;
    }

    public void BeginNewDay()
    {
        LastMealQuality = MealQuality.None;
    }

    public NutritionDecayResult ResolveDay()
    {
        Satiety = Math.Clamp(Satiety - DailySatietyDecay, 0, MaxValue);

        var energyDelta = 0;
        var healthDelta = 0;
        var stressDelta = 0;

        if (!AteToday)
        {
            DaysUndereating++;
            energyDelta -= 12;
            stressDelta += 6;
        }
        else if (LastMealQuality == MealQuality.Scraps)
        {
            DaysUndereating++;
            energyDelta -= 5;
            stressDelta += 2;
        }
        else
        {
            DaysUndereating = 0;
            if (LastMealQuality == MealQuality.HotMeal)
            {
                stressDelta -= 3;
            }
        }

        if (DaysUndereating >= 2)
        {
            healthDelta -= 5;
        }

        return new NutritionDecayResult(energyDelta, healthDelta, stressDelta);
    }
}