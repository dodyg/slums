using Slums.Core.State;
using Slums.Core.Technology;

namespace Slums.Application.Technology;

public sealed record DigitalServiceMenuContext(
    int Money,
    int Energy,
    int DigitalLiteracySkillLevel,
    bool PhoneOperational,
    bool BiometricAppealPending,
    int HandsetExposure,
    IReadOnlyList<DigitalServicePreview> Actions)
{
    public static DigitalServiceMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return new DigitalServiceMenuContext(
            gameSession.Player.Stats.Money,
            gameSession.Player.Stats.Energy,
            gameSession.Player.Skills.GetLevel(Slums.Core.Skills.SkillId.CyberHacking),
            gameSession.Phone.IsOperational(),
            gameSession.Technology.BiometricAppealPending,
            gameSession.Technology.HandsetDataExposure,
            gameSession.GetDigitalServicePreviews());
    }
}
