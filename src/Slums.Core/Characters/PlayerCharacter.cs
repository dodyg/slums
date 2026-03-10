namespace Slums.Core.Characters;

public enum BackgroundType
{
    MedicalSchoolDropout,
    ReleasedPoliticalPrisoner,
    SudaneseRefugee
}

public sealed class PlayerCharacter
{
    public string Name { get; set; } = "Amira";
    public int Age { get; init; } = 24;
    public BackgroundType BackgroundType { get; private set; } = BackgroundType.MedicalSchoolDropout;
    public Background? Background { get; private set; }
    public SurvivalStats Stats { get; } = new();
    public HouseholdState Household { get; } = new();
    public bool HasSelectedBackground { get; private set; }

    public void ApplyBackground(Background background)
    {
        ArgumentNullException.ThrowIfNull(background);

        BackgroundType = background.Type;
        Background = background;
        HasSelectedBackground = true;

        Stats.SetMoney(background.StartingMoney);
        Stats.SetHealth(background.StartingHealth);
        Stats.SetEnergy(background.StartingEnergy);
        Stats.SetHunger(background.StartingHunger);
        Stats.SetStress(background.StartingStress);
        Household.SetMotherHealth(background.MotherStartingHealth);
        Household.SetFoodStockpile(background.FoodStockpile);
    }
}
