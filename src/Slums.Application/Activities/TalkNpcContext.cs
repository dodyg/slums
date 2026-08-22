using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Application.Narrative;

namespace Slums.Application.Activities;

public sealed record TalkNpcContext(
    IReadOnlyList<NpcId> ReachableNpcs,
    PlayerCharacter Player,
    RelationshipState Relationships,
    int CurrentDay,
    int HonestShiftsCompleted,
    int CrimesCommitted,
    int PolicePressure,
    NarrativeSceneState SceneState,
    IReadOnlyDictionary<NpcId, NpcAvailability> Availability)
{
    public Random Random { get; init; } = Random.Shared;

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
            gameSession.PolicePressure,
            NarrativeSceneState.Create(gameSession),
            gameSession.GetNpcAvailability().ToDictionary(static item => item.Npc))
        {
            Random = gameSession.SharedRandom
        };
    }
}
