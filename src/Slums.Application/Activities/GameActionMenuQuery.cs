namespace Slums.Application.Activities;

public sealed class GameActionMenuQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<GameAction> GetActions(GameActionMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        var actions = new List<GameAction>
        {
            new(GameActionId.Rest, "Rest")
        };

        if (context.CurrentLocation?.HasJobOpportunities == true)
        {
            actions.Add(new GameAction(GameActionId.Work, "Work"));
        }

        if (context.CurrentLocation?.HasCrimeOpportunities == true)
        {
            actions.Add(new GameAction(GameActionId.Crime, "Crime"));
        }

        if (context.HasReachableNpcs)
        {
            actions.Add(new GameAction(GameActionId.Talk, "Talk"));
        }

        if (context.HasInvestmentOpportunities)
        {
            actions.Add(new GameAction(GameActionId.Invest, "Invest"));
        }

        if (context.CurrentLocation is { HasCafe: true } or { HasBar: true } or { HasBilliards: true })
        {
            actions.Add(new GameAction(GameActionId.Entertainment, "Entertainment"));
        }

        actions.Add(new GameAction(GameActionId.Shop, "Shop"));

        if (context.HasHouseholdAssetsAccess)
        {
            actions.Add(new GameAction(GameActionId.HouseholdAssets, "Household"));
        }

        if (context.IsAtHome)
        {
            actions.Add(new GameAction(GameActionId.EatAtHome, "Eat at Home"));
            actions.Add(new GameAction(GameActionId.CheckOnMother, "Check on Mother"));
            actions.Add(new GameAction(GameActionId.GiveMotherMedicine, "Give Mother Medicine"));
            actions.Add(new GameAction(GameActionId.TakeMotherToClinic, "Take Mother to Clinic"));
        }
        else
        {
            actions.Add(new GameAction(GameActionId.EatStreetFood, "Eat Street Food"));
        }

        actions.Add(new GameAction(GameActionId.Travel, "Travel"));
        actions.Add(new GameAction(GameActionId.SaveGame, "Save Game"));
        actions.Add(new GameAction(GameActionId.EndDay, "End Day"));

        return actions;
    }
}
