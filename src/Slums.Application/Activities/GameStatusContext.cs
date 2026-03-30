using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Expenses;
using Slums.Core.Investments;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.Weather;
using Slums.Core.World;

namespace Slums.Application.Activities;

public sealed record GameStatusContext(
    GameClock Clock,
    PlayerCharacter Player,
    WorldState World,
    RelationshipState Relationships,
    int PolicePressure,
    int DaysSurvived,
    int HonestShiftsCompleted,
    int TotalHonestWorkEarnings,
    int CrimesCommitted,
    int TotalCrimeEarnings,
    int LastCrimeDay,
    int LastHonestWorkDay,
    int FoodCost,
    int StreetFoodCost,
    int MedicineCost,
    int RentCost,
    int UnpaidRentDays,
    int AccumulatedRentDebt,
    bool HasClinicServices,
    bool ClinicOpenToday,
    int ClinicVisitCost,
    string ClinicOpenDaysSummary,
    DistrictConditionDefinition? CurrentDistrictCondition,
    IReadOnlyList<DistrictConditionDefinition> DailyDistrictConditions,
    IReadOnlyList<Investment> ActiveInvestments,
    int TotalInvestmentEarnings,
    string SeasonName,
    string WeatherName,
    IReadOnlySet<string> StoryFlags)
{
    public static GameStatusContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var clinicStatus = gameSession.GetCurrentLocationClinicStatus();

        return new GameStatusContext(
            gameSession.Clock,
            gameSession.Player,
            gameSession.World,
            gameSession.Relationships,
            gameSession.PolicePressure,
            gameSession.DaysSurvived,
            gameSession.HonestShiftsCompleted,
            gameSession.TotalHonestWorkEarnings,
            gameSession.CrimesCommitted,
            gameSession.TotalCrimeEarnings,
            gameSession.LastCrimeDay,
            gameSession.LastHonestWorkDay,
            gameSession.GetFoodCost(),
            gameSession.GetStreetFoodCost(),
            gameSession.GetMedicineCost(),
            RecurringExpenses.DailyRentCost,
            gameSession.UnpaidRentDays,
            gameSession.AccumulatedRentDebt,
            clinicStatus.HasClinicServices,
            clinicStatus.IsOpenToday,
            clinicStatus.VisitCost,
            clinicStatus.OpenDaysSummary,
            gameSession.GetActiveDistrictConditionDefinition(gameSession.World.CurrentDistrict),
            gameSession.GetDailyDistrictConditions(),
            gameSession.ActiveInvestments,
            gameSession.TotalInvestmentEarnings,
    GameCalendar.GetSeasonName(gameSession.GetCurrentSeason()),
    WeatherModifiers.GetDisplayName(gameSession.CurrentWeather.Type),
    gameSession.StoryFlags.ToHashSet(StringComparer.Ordinal));
    }

    public bool HasStoryFlag(string flag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(flag);
        return StoryFlags.Contains(flag);
    }
}
