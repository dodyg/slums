using Slums.Application.Narrative;
using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class TalkSceneRequestFactory
{
#pragma warning disable CA1822
    public TalkSceneRequest Create(GameSession gameSession, TalkNpcContext context, NpcId npcId)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentNullException.ThrowIfNull(context);

        gameSession.Relationships.RecordContact(npcId, context.CurrentDay);

        var knotName = NpcRegistry.GetConversationKnot(
            npcId,
            context.Relationships,
            context.PolicePressure,
            context.CurrentDay,
            context.HonestShiftsCompleted,
            context.CrimesCommitted,
            context.Player.Stats.Money,
            context.Player.Household.MotherHealth);

        gameSession.Relationships.RecordSeenConversation(npcId, knotName);
        return new TalkSceneRequest(knotName, NarrativeSceneState.Create(gameSession));
    }
}
