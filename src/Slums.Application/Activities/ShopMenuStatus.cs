namespace Slums.Application.Activities;

public sealed record ShopMenuStatus(ShopOptionId OptionId, string Name, int Cost, bool CanAfford, string? Note = null);
