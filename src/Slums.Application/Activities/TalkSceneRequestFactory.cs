using Slums.Core.Relationships;

namespace Slums.Application.Activities;

public sealed class TalkSceneRequestFactory
{
#pragma warning disable CA1822
    public TalkSceneRequest Create(TalkNpcContext context, NpcId npcId, Random? random = null)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Relationships.RecordContact(npcId, context.CurrentDay);

        var selectedRandom = random ?? context.Random;
        var knotName = NpcRegistry.GetConversationKnot(
            npcId,
            context.Relationships,
            context.PolicePressure,
            context.CurrentDay,
            context.HonestShiftsCompleted,
            context.CrimesCommitted,
            context.Player.Stats.Money,
            context.Player.Household.MotherHealth,
            selectedRandom);

        var variantId = NpcRegistry.GetConversationVariantId(
            npcId,
            context.Relationships,
            context.PolicePressure,
            context.CurrentDay,
            context.HonestShiftsCompleted,
            context.CrimesCommitted,
            context.Player.Stats.Money,
            context.Player.Household.MotherHealth,
            selectedRandom);

        context.Relationships.RecordSeenConversation(npcId, knotName);
        context.Relationships.RecordSeenConversationVariant(npcId, variantId);
        return new TalkSceneRequest(knotName, context.SceneState with { ConversationVariantId = variantId }, variantId);
    }
}
