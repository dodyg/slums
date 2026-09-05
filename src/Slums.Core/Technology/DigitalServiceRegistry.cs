using Slums.Core.World;

namespace Slums.Core.Technology;

public static class DigitalServiceRegistry
{
    private static readonly IReadOnlyList<DigitalServiceActionDefinition> Definitions =
    [
        new(DigitalServiceActionType.SubmitBiometricAppeal, "Submit Biometric Appeal", "Use the handset to correct a disputed identity record. The form may be accepted, but the review remains visible to the institution.", LocationId.Home, 6, 90, 5, 8)
    ];

    public static IReadOnlyList<DigitalServiceActionDefinition> All => Definitions;

    public static DigitalServiceActionDefinition Get(DigitalServiceActionType actionType)
    {
        return Definitions.First(definition => definition.Type == actionType);
    }
}
