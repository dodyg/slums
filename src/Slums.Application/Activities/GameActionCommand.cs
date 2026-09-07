using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class GameActionCommand
{
    public bool Execute(GameSession gameSession, GameActionId actionId, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return actionId switch
        {
            GameActionId.Rest => gameSession.RestAtHome(),
            GameActionId.EatAtHome => gameSession.EatAtHome(),
            GameActionId.EatStreetFood => gameSession.EatStreetFood(),
            GameActionId.GiveMotherMedicine => gameSession.GiveMotherMedicine(),
            GameActionId.CheckOnMother => ExecuteMotherStatusCheck(gameSession),
            GameActionId.EmergencySupport => gameSession.RequestEmergencySupport(),
            GameActionId.PreserveFood => gameSession.PreserveFood(),
            GameActionId.EndDay => ExecuteEndDay(gameSession, random),
            _ => throw new ArgumentOutOfRangeException(nameof(actionId), actionId, "This action requires dedicated UI flow or a different command.")
        };
    }

    private static bool ExecuteMotherStatusCheck(GameSession gameSession)
    {
        gameSession.CheckOnMother();
        return true;
    }

    private static bool ExecuteEndDay(GameSession gameSession, Random? random)
    {
        gameSession.EndDay(random);
        return true;
    }
}
