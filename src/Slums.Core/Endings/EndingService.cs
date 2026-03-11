using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Core.Endings;

public static class EndingService
{
    public static EndingId? CheckEndings(GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var networkTrust = gameState.Relationships.GetNpcRelationship(NpcId.NeighborMona).Trust +
            gameState.Relationships.GetNpcRelationship(NpcId.NurseSalma).Trust +
            gameState.Relationships.GetNpcRelationship(NpcId.CafeOwnerNadia).Trust +
            gameState.Relationships.GetNpcRelationship(NpcId.FenceHanan).Trust;

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
            gameState.CrimesCommitted >= 6 &&
            gameState.PolicePressure >= 85 &&
            gameState.Player.Stats.Stress >= 70)
        {
            return EndingId.BuriedByHeat;
        }

        if (gameState.DaysSurvived >= 30 &&
            gameState.CrimesCommitted >= 4 &&
            gameState.TotalHonestWorkEarnings >= 180 &&
            gameState.LastCrimeDay > 0 &&
            gameState.Clock.Day - gameState.LastCrimeDay >= 5 &&
            gameState.PolicePressure < 60)
        {
            return EndingId.LeavingCrime;
        }

        if (gameState.DaysSurvived >= 30 &&
            networkTrust >= 140 &&
            gameState.Player.Household.MotherAlive &&
            gameState.Player.Stats.Money >= 120)
        {
            return EndingId.NetworkShelter;
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
            EndingId.NetworkShelter => "You never get rich, but people keep you from falling alone. In Cairo that becomes a kind of victory.",
            EndingId.LeavingCrime => "You carry what the crime years made of you, but you still manage to walk away before they take the rest.",
            EndingId.BuriedByHeat => "The money came fast enough to stain everything and slow enough to save nothing.",
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
            EndingId.NetworkShelter => "ending_network_shelter",
            EndingId.LeavingCrime => "ending_leaving_crime",
            EndingId.BuriedByHeat => "ending_buried_by_heat",
            _ => null
        };
    }
}