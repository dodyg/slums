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
    private readonly PlayerIdentityState _identity;

    public PlayerCharacter()
        : this(new PlayerIdentityState(), new SurvivalStats(), new NutritionState(), new HouseholdCareState(), new HouseholdAssetsState(), new SkillState())
    {
    }

    internal PlayerCharacter(
        PlayerIdentityState identity,
        SurvivalStats stats,
        NutritionState nutrition,
        HouseholdCareState household,
        HouseholdAssetsState householdAssets,
        SkillState skills)
    {
        _identity = identity ?? throw new ArgumentNullException(nameof(identity));
        Stats = stats ?? throw new ArgumentNullException(nameof(stats));
        Nutrition = nutrition ?? throw new ArgumentNullException(nameof(nutrition));
        Household = household ?? throw new ArgumentNullException(nameof(household));
        HouseholdAssets = householdAssets ?? throw new ArgumentNullException(nameof(householdAssets));
        Skills = skills ?? throw new ArgumentNullException(nameof(skills));

        Stats.SetHunger(Nutrition.Satiety);
    }

    public string Name
    {
        get => _identity.Name;
        set => _identity.Name = value;
    }

    public int Age
    {
        get => _identity.Age;
        init => _identity.Age = value;
    }

    public BackgroundType BackgroundType
    {
        get => _identity.BackgroundType;
        private set => _identity.BackgroundType = value;
    }

    public Gender Gender
    {
        get => _identity.Gender;
        set => _identity.Gender = value;
    }

    public Background? Background
    {
        get => _identity.Background;
        private set => _identity.Background = value;
    }

    public SurvivalStats Stats { get; }

    public NutritionState Nutrition { get; }

    public HouseholdCareState Household { get; }

    public HouseholdAssetsState HouseholdAssets { get; }

    public SkillState Skills { get; }

    public bool HasSelectedBackground
    {
        get => _identity.HasSelectedBackground;
        private set => _identity.HasSelectedBackground = value;
    }

    public void ApplyGender(Gender gender)
    {
        Gender = gender;
        Name = GenderModifiers.DefaultName(gender);
    }

    public void ApplyBackground(Background background)
    {
        ArgumentNullException.ThrowIfNull(background);

        BackgroundType = background.Type;
        Background = background;
        HasSelectedBackground = true;

        Stats.SetMoney(background.StartingMoney);
        Stats.SetHealth(background.StartingHealth);
        Stats.SetEnergy(background.StartingEnergy);
        Nutrition.SetSatiety(background.StartingHunger);
        Stats.SetHunger(Nutrition.Satiety);
        Stats.SetStress(background.StartingStress);
        Household.SetMotherHealth(background.MotherStartingHealth);
        Household.SetFoodStockpile(background.FoodStockpile);
        Household.SetMedicineStock(0);

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
