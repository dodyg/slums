using Slums.Core.Relationships;

namespace Slums.Application.Activities;

public sealed class TalkSceneRequestFactory
{
#pragma warning disable CA1822
    public TalkSceneRequest Create(TalkNpcContext context, NpcId npcId)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Relationships.RecordContact(npcId, context.CurrentDay);

        var knotName = NpcRegistry.GetConversationKnot(
            npcId,
            context.Relationships,
            context.PolicePressure,
            context.CurrentDay,
            context.HonestShiftsCompleted,
            context.CrimesCommitted,
            context.Player.Stats.Money,
            context.Player.Household.MotherHealth);

        context.Relationships.RecordSeenConversation(npcId, knotName);
        return new TalkSceneRequest(knotName, context.SceneState);
    }
}
