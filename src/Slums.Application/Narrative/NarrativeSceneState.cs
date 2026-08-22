using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Application.Narrative;

public sealed record NarrativeSceneState(
    int Money,
    int Health,
    int Energy,
    int Hunger,
    int Stress,
    int MotherHealth,
    int FoodStockpile,
    int Day,
    string? Background,
    string? Gender)
{
    /// <summary>Current canonical district.</summary>
    public string District { get; init; } = string.Empty;

    /// <summary>Current weather type.</summary>
    public string Weather { get; init; } = string.Empty;

    /// <summary>Current season.</summary>
    public string Season { get; init; } = string.Empty;

    /// <summary>Active holiday or an empty string.</summary>
    public string Holiday { get; init; } = string.Empty;

    public bool IsRamadan { get; init; }
    public bool IsRamadanFasting { get; init; }
    public int UnpaidRentDays { get; init; }
    public int RentDebt { get; init; }
    public int RentGraceDays { get; init; }
    public int PolicePressure { get; init; }
    public IReadOnlyDictionary<string, int> DebtBalances { get; init; } = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> RelationshipTrust { get; init; } = new Dictionary<string, int>();
    public IReadOnlyList<string> OperationalRobots { get; init; } = [];
    public IReadOnlyList<string> ActiveNews { get; init; } = [];
    public IReadOnlyDictionary<string, string> Infrastructure { get; init; } = new Dictionary<string, string>();

    public static NarrativeSceneState Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var holiday = gameSession.GetActiveHolidayState();
        var sceneState = new NarrativeSceneState(
            gameSession.Player.Stats.Money,
            gameSession.Player.Stats.Health,
            gameSession.Player.Stats.Energy,
            gameSession.Player.Stats.Hunger,
            gameSession.Player.Stats.Stress,
            gameSession.Player.Household.MotherHealth,
            gameSession.Player.Household.FoodStockpile,
            gameSession.Clock.Day,
            gameSession.Player.Background?.Type.ToString(),
            gameSession.Player.Gender == Slums.Core.Characters.Gender.Male ? "male" : "female")
        {
            District = DistrictInfo.GetName(gameSession.World.CurrentDistrict),
            Weather = gameSession.CurrentWeather.Type.ToString(),
            Season = gameSession.GetCurrentSeason().ToString(),
            Holiday = holiday.IsActive ? holiday.Name ?? string.Empty : string.Empty,
            IsRamadan = holiday.IsRamadan,
            IsRamadanFasting = gameSession.RamadanState.PlayerIsFasting,
            UnpaidRentDays = gameSession.UnpaidRentDays,
            RentDebt = gameSession.AccumulatedRentDebt,
            RentGraceDays = gameSession.RentGraceDaysRemaining,
            PolicePressure = gameSession.PolicePressure,
            DebtBalances = gameSession.PlayerDebts.Debts.ToDictionary(static debt => debt.Source.ToString(), static debt => debt.AmountOwed),
            RelationshipTrust = gameSession.Relationships.NpcRelationships.ToDictionary(static pair => pair.Key.ToString(), static pair => pair.Value.Trust),
            OperationalRobots = gameSession.Player.Robotics.Robots.Where(static robot => robot.IsOperational).Select(static robot => robot.Type.ToString()).ToArray(),
            ActiveNews = gameSession.GetActiveNewsDefinitions().Select(static news => news.Id).ToArray(),
            Infrastructure = gameSession.Infrastructure.Services
                .Where(static service => service.IsActive)
                .ToDictionary(static service => $"{service.District}:{service.Service}", static service => service.Severity.ToString())
        };

        return sceneState;
    }
}
