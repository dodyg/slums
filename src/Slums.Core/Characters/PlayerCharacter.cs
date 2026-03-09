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
    public BackgroundType Background { get; init; } = BackgroundType.MedicalSchoolDropout;
    public SurvivalStats Stats { get; } = new();
    public HouseholdState Household { get; } = new();

    public void ApplyBackgroundModifiers()
    {
        switch (Background)
        {
            case BackgroundType.MedicalSchoolDropout:
                Stats.ModifyMoney(50);
                Stats.ModifyStress(-10);
                break;
            case BackgroundType.ReleasedPoliticalPrisoner:
                Stats.ModifyMoney(-50);
                Stats.ModifyStress(20);
                Stats.ModifyHealth(-10);
                break;
            case BackgroundType.SudaneseRefugee:
                Stats.ModifyMoney(-30);
                Stats.ModifyStress(15);
                Stats.ModifyEnergy(-10);
                break;
        }
    }
}
