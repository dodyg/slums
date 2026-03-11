namespace Slums.Core.Characters;

public static class BackgroundRegistry
{
    private static readonly Background DefaultMedicalSchoolDropout = new()
    {
        Type = BackgroundType.MedicalSchoolDropout,
        Name = "Medical School Dropout",
        Description = "You were studying to become a doctor, but your family's financial crisis forced you to quit. Your medical knowledge still helps you care for your mother.",
        StoryIntro = "Three years of medical school. Three years of dreaming of a white coat and a stethoscope. Then Baba died, and the tuition money evaporated. Now you watch your mother cough and pray it's not serious.",
        StartingMoney = 80,
        StartingHealth = 100,
        StartingEnergy = 70,
        StartingHunger = 75,
        StartingStress = 35,
        MotherStartingHealth = 60,
        FoodStockpile = 2,
        InkIntroKnot = "intro_medical"
    };

    private static readonly Background DefaultReleasedPoliticalPrisoner = new()
    {
        Type = BackgroundType.ReleasedPoliticalPrisoner,
        Name = "Released Political Prisoner",
        Description = "You spent two years in detention for participating in a protest. Now you're out, but the shadow of your arrest follows you. Employers are wary.",
        StoryIntro = "The cell door opened eight months ago. Your mother aged ten years in the two you were inside. The neighbors whisper. The amn el-dawla file never really closes. But you're still here.",
        StartingMoney = 30,
        StartingHealth = 80,
        StartingEnergy = 60,
        StartingHunger = 60,
        StartingStress = 50,
        MotherStartingHealth = 50,
        FoodStockpile = 1,
        InkIntroKnot = "intro_prisoner"
    };

    private static readonly Background DefaultSudaneseRefugee = new()
    {
        Type = BackgroundType.SudaneseRefugee,
        Name = "Sudanese Refugee",
        Description = "You fled Khartoum with your mother when the fighting intensified. Cairo was supposed to be temporary. That was three years ago. Your Arabic has a slight accent that marks you as different.",
        StoryIntro = "You still dream of the Nile in Khartoum, before the jets came. Your mother keeps her Sudanese ID in a plastic bag, as if she might need to prove who she is at any moment. You are Egyptian by law, but the ayna question never stops.",
        StartingMoney = 50,
        StartingHealth = 90,
        StartingEnergy = 75,
        StartingHunger = 70,
        StartingStress = 40,
        MotherStartingHealth = 65,
        FoodStockpile = 2,
        InkIntroKnot = "intro_sudanese"
    };

    private static IReadOnlyList<Background> _backgrounds =
    [
        DefaultMedicalSchoolDropout,
        DefaultReleasedPoliticalPrisoner,
        DefaultSudaneseRefugee
    ];

    public static Background MedicalSchoolDropout => GetByType(BackgroundType.MedicalSchoolDropout);

    public static Background ReleasedPoliticalPrisoner => GetByType(BackgroundType.ReleasedPoliticalPrisoner);

    public static Background SudaneseRefugee => GetByType(BackgroundType.SudaneseRefugee);

    public static IReadOnlyList<Background> AllBackgrounds => _backgrounds;

    public static void Configure(IEnumerable<Background> backgrounds)
    {
        ArgumentNullException.ThrowIfNull(backgrounds);

        var configuredBackgrounds = backgrounds.Where(static background => background is not null).ToArray();
        if (configuredBackgrounds.Length > 0)
        {
            _backgrounds = configuredBackgrounds;
        }
    }

    public static Background GetByType(BackgroundType type) => type switch
    {
        BackgroundType.MedicalSchoolDropout => _backgrounds.FirstOrDefault(static background => background.Type == BackgroundType.MedicalSchoolDropout) ?? DefaultMedicalSchoolDropout,
        BackgroundType.ReleasedPoliticalPrisoner => _backgrounds.FirstOrDefault(static background => background.Type == BackgroundType.ReleasedPoliticalPrisoner) ?? DefaultReleasedPoliticalPrisoner,
        BackgroundType.SudaneseRefugee => _backgrounds.FirstOrDefault(static background => background.Type == BackgroundType.SudaneseRefugee) ?? DefaultSudaneseRefugee,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
