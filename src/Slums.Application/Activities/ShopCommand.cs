using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class ShopCommand
{
    public bool Execute(GameSession gameSession, ShopOptionId optionId)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return optionId switch
        {
            ShopOptionId.OpenHouseholdAssets => true,
            ShopOptionId.BuyFood => gameSession.BuyFood(),
            ShopOptionId.BuyMedicine => gameSession.BuyMedicine(),
            ShopOptionId.TakeMotherToClinic => gameSession.TakeMotherToClinic().Success,
            _ => throw new ArgumentOutOfRangeException(nameof(optionId), optionId, null)
        };
    }
}
