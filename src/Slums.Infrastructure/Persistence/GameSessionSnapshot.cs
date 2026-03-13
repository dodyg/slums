using Slums.Core.Characters;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionSnapshot
{
    public GameSessionClockSnapshot Clock { get; init; } = new();

    public GameSessionPlayerSnapshot Player { get; init; } = new();

    public GameSessionWorldSnapshot World { get; init; } = new();

    public GameSessionRelationshipSnapshot Relationships { get; init; } = new();

    public GameSessionJobProgressSnapshot JobProgress { get; init; } = new();

    public GameSessionCrimeSnapshot Crime { get; init; } = new();

    public GameSessionWorkSnapshot Work { get; init; } = new();

    public GameSessionRunSnapshot Run { get; init; } = new();

    public GameSessionNarrativeSnapshot Narrative { get; init; } = new();

    public static GameSessionSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionSnapshot
        {
            Clock = GameSessionClockSnapshot.Capture(gameSession),
            Player = GameSessionPlayerSnapshot.Capture(gameSession),
            World = GameSessionWorldSnapshot.Capture(gameSession),
            Relationships = GameSessionRelationshipSnapshot.Capture(gameSession),
            JobProgress = GameSessionJobProgressSnapshot.Capture(gameSession),
            Crime = GameSessionCrimeSnapshot.Capture(gameSession),
            Work = GameSessionWorkSnapshot.Capture(gameSession),
            Run = GameSessionRunSnapshot.Capture(gameSession),
            Narrative = GameSessionNarrativeSnapshot.Capture(gameSession)
        };
    }

    public GameSession Restore()
    {
        var gameSession = new GameSession();
        try
        {
            gameSession.Player.ApplyBackground(BackgroundRegistry.GetByType(Player.BackgroundType));
            gameSession.Player.Stats.SetMoney(Player.Money);
            gameSession.Player.Nutrition.SetSatiety(Player.Satiety);
            gameSession.Player.Nutrition.SetDaysUndereating(Player.DaysUndereating);
            gameSession.Player.Stats.SetHunger(gameSession.Player.Nutrition.Satiety);
            gameSession.Player.Stats.SetEnergy(Player.Energy);
            gameSession.Player.Stats.SetHealth(Player.Health);
            gameSession.Player.Stats.SetStress(Player.Stress);
            gameSession.Player.Household.SetMotherHealth(Player.MotherHealth);
            gameSession.Player.Household.SetFoodStockpile(Player.FoodStockpile);
            gameSession.Player.Household.SetMedicineStock(Player.MedicineStock);
            gameSession.Player.Skills.Restore(Player.EnumerateSkillLevels());
            gameSession.Clock.SetTime(Clock.Day, Clock.Hour, Clock.Minute);
            gameSession.World.TravelTo(new LocationId(World.CurrentLocationId));

            gameSession.RestoreCrimeState(
                Crime.PolicePressure,
                Crime.TotalCrimeEarnings,
                Crime.CrimesCommitted,
                Crime.LastCrimeDay,
                Crime.HasCrimeCommittedToday);

            gameSession.RestoreWorkState(
                Work.TotalHonestWorkEarnings,
                Work.HonestShiftsCompleted,
                Work.LastHonestWorkDay,
                Work.LastPublicFacingWorkDay);

            gameSession.RestoreRunState(
                Run.RunId,
                Run.DaysSurvived,
                Run.IsGameOver,
                Run.GameOverReason,
                Run.EndingId,
                Run.PendingEndingKnot);

            gameSession.RestoreNarrativeState(
                Narrative.StoryFlags,
                Narrative.RandomEventHistory,
                Narrative.PendingNarrativeScenes);

            foreach (var npcId in Enum.GetValues<NpcId>())
            {
                var relationship = Relationships.GetNpcSnapshot(npcId);
                gameSession.Relationships.SetNpcRelationship(npcId, relationship.Trust, relationship.LastSeenDay);
                gameSession.Relationships.SetNpcRelationshipMemory(
                    npcId,
                    relationship.LastFavorDay,
                    relationship.LastRefusalDay,
                    relationship.HasUnpaidDebt,
                    relationship.WasEmbarrassed,
                    relationship.WasHelped,
                    relationship.RecentContactCount);
                gameSession.Relationships.RestoreConversationHistory(npcId, relationship.SeenConversationKnots);
            }

            foreach (var factionId in Enum.GetValues<FactionId>())
            {
                gameSession.Relationships.SetFactionStanding(factionId, Relationships.GetFactionReputation(factionId));
            }

            foreach (var jobType in Enum.GetValues<JobType>())
            {
                var track = JobProgress.GetTrackSnapshot(jobType);
                gameSession.RestoreJobTrack(jobType, track.Reliability, track.ShiftsCompleted, track.LockoutUntilDay);
            }

            return gameSession;
        }
        catch
        {
            gameSession.Dispose();
            throw;
        }
    }
}
