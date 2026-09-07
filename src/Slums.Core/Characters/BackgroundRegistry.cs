namespace Slums.Core.Characters;

/// <summary>Provides configured background definitions to legacy callers.</summary>
public static class BackgroundRegistry
{
    private static IReadOnlyList<Background>? _backgrounds;

    public static Background MedicalSchoolDropout => GetByType(BackgroundType.MedicalSchoolDropout);
    public static Background ReleasedPoliticalPrisoner => GetByType(BackgroundType.ReleasedPoliticalPrisoner);
    public static Background SudaneseRefugee => GetByType(BackgroundType.SudaneseRefugee);

    public static IReadOnlyList<Background> AllBackgrounds => GetConfiguredBackgrounds();

    public static void Configure(IEnumerable<Background> backgrounds)
    {
        ArgumentNullException.ThrowIfNull(backgrounds);

        var configuredBackgrounds = backgrounds.Where(static background => background is not null).ToArray();
        if (configuredBackgrounds.Length == 0)
        {
            throw new InvalidOperationException("At least one background must be configured.");
        }

        _backgrounds = configuredBackgrounds;
    }

    public static Background GetByType(BackgroundType type) => type switch
    {
        BackgroundType.MedicalSchoolDropout => GetConfigured(BackgroundType.MedicalSchoolDropout),
        BackgroundType.ReleasedPoliticalPrisoner => GetConfigured(BackgroundType.ReleasedPoliticalPrisoner),
        BackgroundType.SudaneseRefugee => GetConfigured(BackgroundType.SudaneseRefugee),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    private static Background GetConfigured(BackgroundType type)
    {
        return GetConfiguredBackgrounds().FirstOrDefault(background => background.Type == type)
            ?? throw new InvalidOperationException($"No background configured for {type}.");
    }

    private static IReadOnlyList<Background> GetConfiguredBackgrounds()
    {
        return _backgrounds
            ?? throw new InvalidOperationException("Background content is not configured. Configure GameContentCatalog before querying backgrounds.");
    }
}
