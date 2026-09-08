using Slums.Core.Skills;
using Slums.Core.World;

namespace Slums.Core.Technology;

/// <summary>Code-owned default digital service definitions used when a catalog is built without JSON content.</summary>
public static class DigitalServiceDefinitions
{
    /// <summary>Gets the default digital service action definitions.</summary>
    public static IReadOnlyList<DigitalServiceActionDefinition> Defaults { get; } =
    [
        new(DigitalServiceActionType.SubmitBiometricAppeal, "Submit Biometric Appeal", "Use the handset to correct a disputed identity record. The form may be accepted, but the review remains visible to the institution.", LocationId.Home, SkillThresholds.HighLevel, 90, 5, 8)
    ];
}
