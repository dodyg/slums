using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class ShopCommand
{
#pragma warning disable CA1822
    public bool Execute(GameSession gameSession, ShopOptionId optionId)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return optionId switch
        {
            ShopOptionId.BuyFood => gameSession.BuyFood(),
            ShopOptionId.BuyMedicine => gameSession.BuyMedicine(),
            ShopOptionId.TakeMotherToClinic => gameSession.TakeMotherToClinic().Success,
            _ => throw new ArgumentOutOfRangeException(nameof(optionId), optionId, null)
        };
    }
}
