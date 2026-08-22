using Slums.Core.State;

namespace Slums.Application.Technology;

public static class TechnologyObligationCommand
{
    public static bool Execute(GameSession gameSession, TechnologyObligationAction action)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        switch (action)
        {
            case TechnologyObligationAction.RecordHandsetUse:
                gameSession.Technology.RecordHandsetUse();
                return true;
            case TechnologyObligationAction.RecordTransitPermitReview:
                gameSession.Technology.RecordTransitPermitReview();
                return true;
            case TechnologyObligationAction.ResolveTransitPermitReview:
                gameSession.Technology.ResolveTransitPermitReview();
                return true;
            case TechnologyObligationAction.RecordBiometricAppeal:
                gameSession.Technology.RecordBiometricAppeal();
                return true;
            case TechnologyObligationAction.ResolveBiometricAppeal:
                gameSession.Technology.ResolveBiometricAppeal();
                return true;
            case TechnologyObligationAction.RecordTelemedicineTriage:
                return gameSession.Technology.RecordTelemedicineTriage(gameSession.Clock.Day);
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, "Unknown technology obligation action.");
        }
    }
}
