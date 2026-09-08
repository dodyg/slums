using Slums.Core.Inventory;
using Slums.Core.State;

namespace Slums.Application.Inventory;

public sealed class AcquireItemCommand
{
    public (bool Success, string Message) Execute(GameSession gameSession, string itemId, int quantity = 1)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var definition = gameSession.ContentCatalog.GetItem(itemId);
        if (definition is null)
        {
            return (false, "That item is not part of the current catalog.");
        }
        if (!gameSession.Inventory.Add(itemId, quantity, definition.MaximumQuantity))
        {
            return (false, $"You cannot carry more {definition.Name}.");
        }

        var message = $"Received {quantity} {definition.Name}.";
        gameSession.AddEventMessage(message);
        return (true, message);
    }
}
