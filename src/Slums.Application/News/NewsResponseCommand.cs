using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World.News;

namespace Slums.Application.News;

public sealed class NewsResponseCommand
{
    #pragma warning disable CA1822
    public (bool Success, string Message) Execute(GameSession gameSession, string newsId, string responseId)
    #pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentException.ThrowIfNullOrWhiteSpace(newsId);
        ArgumentException.ThrowIfNullOrWhiteSpace(responseId);

        var definition = gameSession.GetActiveNewsDefinitions().FirstOrDefault(candidate => candidate.Id == newsId);
        var active = gameSession.ActiveNews.FirstOrDefault(flash => flash.DefinitionId == newsId);
        var response = definition?.Responses.FirstOrDefault(candidate => candidate.Id == responseId);
        if (definition is null || active is null || response is null || !active.IsActive(gameSession.Clock.Day))
        {
            return (false, "That news response is no longer available.");
        }
        if (active.UsedResponseId is not null)
        {
            return (false, "You have already responded to this news flash.");
        }
        if (gameSession.Player.Stats.Money < response.MoneyCost)
        {
            return (false, $"You need {response.MoneyCost} LE to do that.");
        }
        if (response.RequiredItemId is not null && gameSession.Inventory.GetQuantity(response.RequiredItemId) < response.RequiredItemQuantity)
        {
            return (false, $"You need {response.RequiredItemQuantity} {response.RequiredItemId}.");
        }

        if (!NewsService.TryUseResponse(gameSession.News, definition, responseId))
        {
            return (false, "That response was already taken.");
        }

        if (response.MoneyCost > 0)
        {
            gameSession.AdjustMoney(-response.MoneyCost);
        }
        if (response.RequiredItemId is not null)
        {
            gameSession.Inventory.Remove(response.RequiredItemId, response.RequiredItemQuantity);
        }
        if (response.TrustChange != 0)
        {
            gameSession.ModifyNpcTrust(NpcId.NeighborMona, response.TrustChange);
        }
        if (response.TimeCostMinutes > 0)
        {
            gameSession.AdvanceTime(response.TimeCostMinutes);
        }

        var message = response.OutcomeMessage ?? $"You chose: {response.Label}.";
        gameSession.AddEventMessage(message);
        return (true, message);
    }
}
