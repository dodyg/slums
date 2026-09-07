using Slums.Core.Relationships;

namespace Slums.Application.Activities;

public sealed class TalkSceneRequestFactory
{
    public TalkSceneRequest Create(TalkNpcContext context, NpcId npcId, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        var selectedRandom = random ?? context.Random;
        var conversationContext = NpcRegistry.GetConversationContext(
            npcId,
            context.Relationships,
            context.PolicePressure,
            context.CurrentDay,
            context.HonestShiftsCompleted,
            context.CrimesCommitted,
            context.Player.Stats.Money,
            context.Player.Household.MotherHealth);
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

        return new TalkSceneRequest(
            npcId,
            ConversationPoolRegistry.RecurringConversationKnot,
            context.SceneState with
            {
                ConversationVariantId = variantId,
                ConversationContext = conversationContext,
                ConversationNpc = npcId.ToString()
            },
            variantId,
            knotName);
    }
}
