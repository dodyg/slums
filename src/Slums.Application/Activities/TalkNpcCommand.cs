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

        var availability = gameSession.GetNpcAvailability().First(item => item.Npc == npcId);
        if (NpcScheduleRegistry.All.Count > 0 && !availability.IsAvailable)
        {
            gameSession.AddEventMessage(availability.Reason);
            return null;
        }

        var request = _requestFactory.Create(TalkNpcContext.Create(gameSession), npcId, random);
        gameSession.AdvanceTime(GameSession.ConversationDurationMinutes);
        return request;
    }
}
