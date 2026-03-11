namespace Slums.Core.Characters;

using Slums.Core.Skills;

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
    public SkillState Skills { get; } = new();
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

        Skills.Restore([]);
        switch (background.Type)
        {
            case BackgroundType.MedicalSchoolDropout:
                Skills.SetLevel(SkillId.Medical, 3);
                break;
            case BackgroundType.ReleasedPoliticalPrisoner:
                Skills.SetLevel(SkillId.Persuasion, 2);
                Skills.SetLevel(SkillId.StreetSmarts, 1);
                break;
            case BackgroundType.SudaneseRefugee:
                Skills.SetLevel(SkillId.StreetSmarts, 2);
                Skills.SetLevel(SkillId.Physical, 1);
                break;
        }
    }
}
