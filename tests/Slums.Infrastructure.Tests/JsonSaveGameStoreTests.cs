using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Slums.Application.Persistence;
using Slums.Core.Characters;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.World;
using Slums.Infrastructure.Persistence;
using TUnit.Core;

namespace Slums.Infrastructure.Tests;

internal sealed class JsonSaveGameStoreTests
{
    [Test]
    public async Task SaveAndLoad_ShouldRoundTripGameSessionSnapshot()
    {
        var saveDirectory = CreateTempDirectory("slums-save-tests");
        try
        {
            var store = new JsonSaveGameStore(NullLogger<JsonSaveGameStore>.Instance, saveDirectory);

            using var gameSession = new Slums.Core.State.GameSession();
            var runId = Guid.NewGuid();
            gameSession.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);
            gameSession.Player.Stats.SetMoney(222);
            gameSession.Player.Nutrition.SetSatiety(41);
            gameSession.Player.Nutrition.SetDaysUndereating(2);
            gameSession.Player.Stats.SetHunger(gameSession.Player.Nutrition.Satiety);
            gameSession.Player.Stats.SetHealth(77);
            gameSession.Player.Stats.SetEnergy(55);
            gameSession.Player.Stats.SetStress(38);
            gameSession.Player.Household.SetMotherHealth(63);
            gameSession.Player.Household.SetFoodStockpile(5);
            gameSession.Player.Household.SetMedicineStock(2);
            gameSession.World.TravelTo(LocationId.Market);
            gameSession.RestoreCrimeState(30, 120, 2, 5, hasCrimeCommittedToday: true);
            gameSession.RestoreWorkState(140, 4, 7, 7);
            gameSession.RestoreRunState(runId, 7, isGameOver: false, gameOverReason: null, endingId: null, pendingEndingKnot: null);
            gameSession.SetStoryFlag("crime_warning");
            gameSession.QueueNarrativeScene("queued_scene");
            gameSession.RestoreJobTrack(JobType.ClinicReception, 74, 5, 9);
            gameSession.Relationships.RecordFavor(NpcId.NurseSalma, 7, hasUnpaidDebt: true);
            gameSession.Relationships.RecordSeenConversation(NpcId.NurseSalma, "nurse_intro_1");
            gameSession.Relationships.SetFactionStanding(FactionId.ExPrisonerNetwork, 11);
            gameSession.RecordEventHistory("DokkiCheckpointSweep", 2);

            await store.SaveAsync(SaveGameRequest.Create(gameSession, "crime_warning"), "slot1").ConfigureAwait(false);
            var loaded = await store.LoadAsync("slot1").ConfigureAwait(false);

            loaded.Should().NotBeNull();
            var loadedSession = loaded!;
            using (loadedSession)
            {
                loadedSession.LastKnot.Should().Be("crime_warning");
                var restoredSession = loadedSession.TakeGameSession();

                try
                {
                    restoredSession.RunId.Should().Be(runId);
                    restoredSession.Player.Stats.Money.Should().Be(222);
                    restoredSession.Player.Nutrition.Satiety.Should().Be(41);
                    restoredSession.Player.Nutrition.DaysUndereating.Should().Be(2);
                    restoredSession.Player.Stats.Health.Should().Be(77);
                    restoredSession.World.CurrentLocationId.Should().Be(LocationId.Market);
                    restoredSession.PolicePressure.Should().Be(30);
                    restoredSession.CrimesCommitted.Should().Be(2);
                    restoredSession.TotalHonestWorkEarnings.Should().Be(140);
                    restoredSession.LastCrimeDay.Should().Be(5);
                    restoredSession.LastHonestWorkDay.Should().Be(7);
                    restoredSession.LastPublicFacingWorkDay.Should().Be(7);
                    restoredSession.HasCrimeCommittedToday.Should().BeTrue();
                    restoredSession.StoryFlags.Should().Contain("crime_warning");
                    restoredSession.PendingNarrativeScenes.Should().ContainSingle().Which.Should().Be("queued_scene");
                    restoredSession.JobProgress.GetTrack(JobType.ClinicReception).Reliability.Should().Be(74);
                    restoredSession.JobProgress.GetTrack(JobType.ClinicReception).ShiftsCompleted.Should().Be(5);
                    restoredSession.JobProgress.GetTrack(JobType.ClinicReception).LockoutUntilDay.Should().Be(9);
                    restoredSession.Relationships.GetNpcRelationship(NpcId.NurseSalma).HasUnpaidDebt.Should().BeTrue();
                    restoredSession.Relationships.HasSeenConversation(NpcId.NurseSalma, "nurse_intro_1").Should().BeTrue();
                    restoredSession.Relationships.GetFactionStanding(FactionId.ExPrisonerNetwork).Reputation.Should().Be(11);
                    restoredSession.GetEventCount("DokkiCheckpointSweep").Should().Be(2);
                }
                finally
                {
                    restoredSession.Dispose();
                }
            }
        }
        finally
        {
            DeleteDirectory(saveDirectory);
        }
    }

    [Test]
    public async Task LoadAsync_ShouldRejectVersionMismatch()
    {
        var saveDirectory = CreateTempDirectory("slums-save-tests");
        try
        {
            var path = Path.Combine(saveDirectory, "slot1.json");
            await File.WriteAllTextAsync(path, """
            {
              "SaveVersion": 999,
              "CreatedUtc": "2026-03-11T00:00:00+00:00",
              "LastPlayedUtc": "2026-03-11T00:00:00+00:00",
              "CheckpointName": "bad",
              "SessionSnapshot": {
                "Clock": {
                  "Day": 1,
                  "Hour": 6,
                  "Minute": 0
                },
                "Player": {
                  "BackgroundType": "MedicalSchoolDropout",
                  "Money": 100,
                  "Satiety": 80,
                  "DaysUndereating": 0,
                  "Energy": 80,
                  "Health": 100,
                  "Stress": 20,
                  "MotherHealth": 70,
                  "FoodStockpile": 3,
                  "MedicineStock": 0,
                  "SkillLevels": {}
                },
                "World": {
                  "CurrentLocationId": "home"
                },
                "Relationships": {
                  "Npcs": {},
                  "Factions": {}
                },
                "JobProgress": {
                  "Tracks": {}
                },
                "Crime": {
                  "PolicePressure": 0,
                  "TotalCrimeEarnings": 0,
                  "CrimesCommitted": 0,
                  "LastCrimeDay": 0,
                  "HasCrimeCommittedToday": false
                },
                "Work": {
                  "TotalHonestWorkEarnings": 0,
                  "HonestShiftsCompleted": 0,
                  "LastHonestWorkDay": 0,
                  "LastPublicFacingWorkDay": 0
                },
                "Run": {
                  "RunId": "00000000-0000-0000-0000-000000000001",
                  "IsGameOver": false,
                  "GameOverReason": null,
                  "EndingId": null,
                  "DaysSurvived": 0,
                  "PendingEndingKnot": null
                },
                "Narrative": {
                  "StoryFlags": [],
                  "RandomEventHistory": {},
                  "PendingNarrativeScenes": []
                }
              },
              "NarrativeProgress": {
                "LastKnot": null
              }
            }
            """).ConfigureAwait(false);

            var store = new JsonSaveGameStore(NullLogger<JsonSaveGameStore>.Instance, saveDirectory);
            var loaded = await store.LoadAsync("slot1").ConfigureAwait(false);

            loaded.Should().BeNull();
        }
        finally
        {
            DeleteDirectory(saveDirectory);
        }
    }

    private static string CreateTempDirectory(string rootName)
    {
        var path = Path.Combine(Path.GetTempPath(), rootName, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
