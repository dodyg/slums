using Slums.Core.Community;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed record CommunityActionMenuContext(
    int Money,
    int Energy,
    int CurrentDay,
    int CommunityOrganizingSkillLevel,
    int CoolingRoomDaysRemaining,
    int WaterReserveUnits,
    IReadOnlyList<CommunityActionPreview> Actions)
{
    public static CommunityActionMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return new CommunityActionMenuContext(
            gameSession.Player.Stats.Money,
            gameSession.Player.Stats.Energy,
            gameSession.Clock.Day,
            gameSession.Player.Skills.GetLevel(Slums.Core.Skills.SkillId.CommunityOrganizing),
            gameSession.CommunityAdaptation.CoolingRoomDaysRemaining,
            gameSession.CommunityAdaptation.WaterReserveUnits,
            gameSession.GetCommunityActionPreviews());
    }
}
