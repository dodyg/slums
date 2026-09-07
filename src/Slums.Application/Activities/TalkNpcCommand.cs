using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Application.Activities;

/// <summary>Starts one meaningful NPC conversation and charges its time cost.</summary>
public sealed class TalkNpcCommand
{
    private readonly TalkSceneRequestFactory _requestFactory = new();

    public TalkSceneRequest? Execute(GameSession gameSession, NpcId npcId, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        if (!gameSession.GetReachableNpcs().Contains(npcId))
        {
            gameSession.AddEventMessage($"{NpcRegistry.GetName(npcId)} is not reachable from here.");
            return null;
        }

        var relationship = gameSession.Relationships.GetNpcRelationship(npcId);
        if (relationship.LastSeenDay == gameSession.Clock.Day)
        {
            gameSession.AddEventMessage($"You already had a meaningful conversation with {NpcRegistry.GetName(npcId)} today.");
            return null;
        }

        var availability = gameSession.GetNpcAvailability().FirstOrDefault(item => item.Npc == npcId);
        if (NpcScheduleRegistry.All.Count > 0 && availability is null)
        {
            gameSession.AddEventMessage($"{NpcRegistry.GetName(npcId)} has no configured schedule and cannot be reached right now.");
            return null;
        }

        if (availability is not null && !availability.IsAvailable)
        {
            gameSession.AddEventMessage(availability.Reason);
            return null;
        }

        return _requestFactory.Create(TalkNpcContext.Create(gameSession), npcId, random);
    }

    /// <summary>Commits talk state after the narrative service has successfully started.</summary>
    public void Commit(GameSession gameSession, TalkSceneRequest request, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentNullException.ThrowIfNull(request);

        var npcId = request.NpcId;
        gameSession.Relationships.RecordContact(npcId, gameSession.Clock.Day);
        gameSession.Relationships.RecordSeenConversation(npcId, request.ConversationKnot);
        gameSession.Relationships.RecordSeenConversationVariant(npcId, request.VariantId);
        gameSession.Relationships.RecordSeenConversation(npcId, ConversationPoolRegistry.RecurringConversationKnot);
        if (npcId == NpcId.OfficerKhalid)
        {
            gameSession.HandleOfficerKhalidConversation(random);
        }
        gameSession.AdvanceTime(GameSession.ConversationDurationMinutes);
    }
}
