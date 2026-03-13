using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed record TalkNpcContext(
    IReadOnlyList<NpcId> ReachableNpcs,
    PlayerCharacter Player,
    RelationshipState Relationships,
    int CurrentDay,
    int HonestShiftsCompleted,
    int CrimesCommitted,
    int PolicePressure)
{
    public static TalkNpcContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new TalkNpcContext(
            gameSession.GetReachableNpcs(),
            gameSession.Player,
            gameSession.Relationships,
            gameSession.Clock.Day,
            gameSession.HonestShiftsCompleted,
            gameSession.CrimesCommitted,
            gameSession.PolicePressure);
    }
}
