using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionCommunityAdaptationSnapshot
{
    public int CoolingRoomDaysRemaining { get; init; }
    public int WaterReserveUnits { get; init; }
    public int SuccessfulActions { get; init; }
    public int ShelterContributions { get; init; }

    public static GameSessionCommunityAdaptationSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return new GameSessionCommunityAdaptationSnapshot
        {
            CoolingRoomDaysRemaining = gameSession.CommunityAdaptation.CoolingRoomDaysRemaining,
            WaterReserveUnits = gameSession.CommunityAdaptation.WaterReserveUnits,
            SuccessfulActions = gameSession.CommunityAdaptation.SuccessfulActions,
            ShelterContributions = gameSession.CommunityAdaptation.ShelterContributions
        };
    }
}
