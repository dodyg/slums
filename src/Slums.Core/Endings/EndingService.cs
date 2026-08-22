using Slums.Core.Characters;
using Slums.Core.Economy;
using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Core.Endings;

public static class EndingService
{
    public static EndingId? CheckFailureEndings(GameSession gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (!gameState.Player.Household.MotherAlive)
        {
            return EndingId.MotherDied;
        }

        if (gameState.Player.Stats.Health <= 0 ||
            (gameState.Player.Stats.IsStarving && gameState.Player.Stats.IsExhausted && gameState.Player.Stats.Money <= 0))
        {
            return EndingId.Destitution;
        }

        if (gameState.PolicePressure >= 100 ||
            (gameState.DaysSurvived >= 30 &&
             gameState.CrimesCommitted >= 6 &&
             gameState.PolicePressure >= 85 &&
             gameState.Player.Stats.Stress >= 70))
        {
            return EndingId.Arrested;
        }

        if (gameState.UnpaidRentDays >= 7)
        {
            return EndingId.Eviction;
        }

        if (gameState.PlayerDebts.GetOverdueDebts(gameState.Clock.Day).Any(d => d.Source == DebtSource.LoanShark && d.CollectionState == DebtCollectionState.Critical))
        {
            return EndingId.Destitution;
        }

        return null;
    }

    public static IReadOnlyList<EndingId> GetAvailableEndings(GameSession gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (CheckFailureEndings(gameState) is not null || gameState.IsGameOver)
        {
            return [];
        }

        var endings = new List<EndingId>();
        if (CanChooseHonestStability(gameState))
        {
            endings.Add(EndingId.StabilityHonestWork);
        }

        if (CanChooseNetworkShelter(gameState))
        {
            endings.Add(EndingId.NetworkShelter);
        }

        if (CanChooseLuxor(gameState))
        {
            endings.Add(EndingId.QuitTheLuxorDream);
        }

        if (CanChooseCrimeKingpin(gameState))
        {
            endings.Add(EndingId.CrimeKingpin);
        }

        return endings;
    }

    public static EndingId? CheckEndings(GameSession gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (CheckFailureEndings(gameState) is { } failure)
        {
            return failure;
        }

        var available = GetAvailableEndings(gameState);
        return available.Count > 0 ? available[0] : null;
    }

    public static string GetChoiceLabel(EndingId endingId)
    {
        return endingId switch
        {
            EndingId.StabilityHonestWork => "Commit to honest stability",
            EndingId.NetworkShelter => "Accept community shelter",
            EndingId.QuitTheLuxorDream => "Leave for Luxor",
            EndingId.CrimeKingpin => "Deepen criminal power",
            _ => throw new ArgumentOutOfRangeException(nameof(endingId), endingId, "Only non-failure endings can be chosen.")
        };
    }

    public static string GetChoiceRequirements(EndingId endingId)
    {
        return endingId switch
        {
            EndingId.StabilityHonestWork => "30 days, 6 honest shifts, 180 LE earned, and five clean days.",
            EndingId.NetworkShelter => "30 days, 140 combined support trust, and 120 LE saved.",
            EndingId.QuitTheLuxorDream => "30 days, 550 LE for the train and first weeks, low crime, and a healthy mother.",
            EndingId.CrimeKingpin => "1,000 LE in crime earnings, 8 crimes, faction control, and standing above 50.",
            _ => throw new ArgumentOutOfRangeException(nameof(endingId), endingId, "Only non-failure endings can be chosen.")
        };
    }

    private static bool CanChooseHonestStability(GameSession gameState)
    {
        return gameState.DaysSurvived >= 30 &&
            gameState.TotalHonestWorkEarnings >= 180 &&
            gameState.HonestShiftsCompleted >= 6 &&
            gameState.Player.Household.MotherAlive &&
            gameState.PolicePressure < 60 &&
            HasBeenCleanForFiveDays(gameState);
    }

    private static bool CanChooseNetworkShelter(GameSession gameState)
    {
        return gameState.DaysSurvived >= 30 &&
            GetNetworkTrust(gameState) >= 140 &&
            gameState.Player.Household.MotherAlive &&
            gameState.Player.Stats.Money >= 120;
    }

    private static bool CanChooseLuxor(GameSession gameState)
    {
        return gameState.DaysSurvived >= 30 &&
            gameState.Player.Stats.Money >= 550 &&
            gameState.CrimesCommitted <= 3 &&
            gameState.Player.Household.MotherHealth > 60 &&
            HasBeenCleanForFiveDays(gameState);
    }

    private static bool CanChooseCrimeKingpin(GameSession gameState)
    {
        var imbabaStanding = gameState.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation;
        var controlsTerritory = gameState.Territory.Districts.Values.Any(control => control.ControllingFaction == FactionId.ImbabaCrew);
        return gameState.TotalCrimeEarnings >= 1000 && gameState.CrimesCommitted >= 8 && imbabaStanding > 50 && controlsTerritory;
    }

    private static int GetNetworkTrust(GameSession gameState)
    {
        return gameState.Relationships.GetNpcRelationship(NpcId.NeighborMona).Trust +
            gameState.Relationships.GetNpcRelationship(NpcId.NurseSalma).Trust +
            gameState.Relationships.GetNpcRelationship(NpcId.CafeOwnerNadia).Trust +
            gameState.Relationships.GetNpcRelationship(NpcId.FenceHanan).Trust;
    }

    private static bool HasBeenCleanForFiveDays(GameSession gameState)
    {
        var elapsedDays = Math.Max(gameState.DaysSurvived, gameState.Clock.Day);
        return gameState.LastCrimeDay == 0 || elapsedDays - gameState.LastCrimeDay >= 5;
    }

    public static string GetMessage(EndingId endingId)
    {
        return endingId switch
        {
            EndingId.MotherDied => "Your mother is gone. The flat is suddenly unbearable.",
            EndingId.Arrested => "The pressure finally breaks. A police van door closes on your future.",
            EndingId.Eviction => "Seven days behind on rent. The landlord throws you and your mother onto the street.",
            EndingId.Destitution => "Destitution. Cairo keeps moving, but it leaves you behind.",
            EndingId.StabilityHonestWork => "Against the odds, you carve out a narrow honest stability.",
            EndingId.CrimeKingpin => "You climb the ladder, but every rung belongs to someone dangerous.",
            EndingId.QuitTheLuxorDream => "You choose distance, family, and the possibility of a softer life in Luxor.",
            EndingId.NetworkShelter => "You never get rich, but people keep you from falling alone. In Cairo that becomes a kind of victory.",
            _ => throw new ArgumentOutOfRangeException(nameof(endingId))
        };
    }

    public static string GetInkKnot(EndingId endingId)
    {
        return EndingKnotCatalog.GetDefault(endingId);
    }

    public static string GetInkKnot(GameSession gameState, EndingId endingId)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        return endingId switch
        {
            EndingId.StabilityHonestWork => GetStabilityKnot(gameState.Player.BackgroundType),
            EndingId.NetworkShelter => GetNetworkShelterKnot(gameState),
            EndingId.QuitTheLuxorDream => EndingKnotCatalog.GetLuxorKnot(gameState.Player.BackgroundType),
            _ => GetInkKnot(endingId)
        };
    }

    private static string GetStabilityKnot(BackgroundType backgroundType)
    {
        return EndingKnotCatalog.GetStabilityKnot(backgroundType);
    }

    private static string GetNetworkShelterKnot(GameSession gameState)
    {
        var rankedContacts = new[]
        {
            NpcId.NeighborMona,
            NpcId.NurseSalma,
            NpcId.CafeOwnerNadia,
            NpcId.FenceHanan
        };

        var strongestSupport = rankedContacts
            .OrderByDescending(npcId => gameState.Relationships.GetNpcRelationship(npcId).Trust)
            .First();

        return EndingKnotCatalog.GetNetworkShelterKnot(strongestSupport);
    }
}
