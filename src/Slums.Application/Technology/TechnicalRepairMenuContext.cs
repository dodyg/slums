using Slums.Core.State;
using Slums.Core.Technology;

namespace Slums.Application.Technology;

public sealed record TechnicalRepairMenuContext(
    int Money,
    int Energy,
    int TechnicalRepairSkillLevel,
    int SpareParts,
    int HandsetCondition,
    int SolarStorageCondition,
    IReadOnlyList<TechnicalRepairPreview> Actions)
{
    public static TechnicalRepairMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return new TechnicalRepairMenuContext(
            gameSession.Player.Stats.Money,
            gameSession.Player.Stats.Energy,
            gameSession.Player.Skills.GetLevel(Slums.Core.Skills.SkillId.RobotRepair),
            gameSession.Player.Robotics.Parts,
            gameSession.Phone.HandsetCondition,
            gameSession.Technology.MicrogridStorageCondition,
            gameSession.GetTechnicalRepairPreviews());
    }
}
