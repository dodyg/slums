using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Core.Endings;

public static class EndingService
{
    public static EndingId? CheckEndings(GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (!gameState.Player.Household.MotherAlive)
        {
            return EndingId.MotherDied;
        }

        if (gameState.Player.Stats.Health <= 0)
        {
            return EndingId.CollapseFromExhaustion;
        }

        if (gameState.Player.Stats.IsStarving && gameState.Player.Stats.IsExhausted && gameState.Player.Stats.Money <= 0)
        {
            return EndingId.Destitution;
        }

        if (gameState.PolicePressure >= 100)
        {
            return EndingId.Arrested;
        }

        if (gameState.DaysSurvived >= 30 &&
            gameState.Player.Stats.Money > 500 &&
            gameState.CrimesCommitted <= 3 &&
            gameState.Player.Household.MotherHealth > 60)
        {
            return EndingId.QuitTheLuxorDream;
        }

        if (gameState.DaysSurvived >= 30 &&
            gameState.CrimesCommitted == 0 &&
            gameState.Player.Stats.Money > 200 &&
            gameState.Player.Household.MotherAlive &&
            gameState.PolicePressure < 20)
        {
            return EndingId.StabilityHonestWork;
        }

        if (gameState.TotalCrimeEarnings >= 1000 &&
            gameState.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation > 50)
        {
            return EndingId.CrimeKingpin;
        }

        return null;
    }

    public static string GetMessage(EndingId endingId)
    {
        return endingId switch
        {
            EndingId.Destitution => "Destitution. Cairo keeps moving, but it leaves you behind.",
            EndingId.MotherDied => "Your mother is gone. The flat is suddenly unbearable.",
            EndingId.CollapseFromExhaustion => "Your health fails before the city does.",
            EndingId.StabilityHonestWork => "Against the odds, you carve out a narrow honest stability.",
            EndingId.CrimeKingpin => "You climb the ladder, but every rung belongs to someone dangerous.",
            EndingId.QuitTheLuxorDream => "You choose distance, family, and the possibility of a softer life in Luxor.",
            EndingId.Arrested => "The pressure finally breaks. A police van door closes on your future.",
            _ => throw new ArgumentOutOfRangeException(nameof(endingId))
        };
    }

    public static string? GetInkKnot(EndingId endingId)
    {
        return endingId switch
        {
            EndingId.StabilityHonestWork => "ending_stability",
            EndingId.QuitTheLuxorDream => "ending_luxor",
            EndingId.Arrested => "ending_arrested",
            _ => null
        };
    }
}