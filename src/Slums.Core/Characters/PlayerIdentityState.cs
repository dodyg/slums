namespace Slums.Core.Characters;

internal sealed class PlayerIdentityState
{
    public string Name { get; set; } = "Amira";

    public int Age { get; set; } = 24;

    public Gender Gender { get; set; } = Gender.Female;

    public BackgroundType BackgroundType { get; set; } = BackgroundType.MedicalSchoolDropout;

    public Background? Background { get; set; }

    public bool HasSelectedBackground { get; set; }
}
