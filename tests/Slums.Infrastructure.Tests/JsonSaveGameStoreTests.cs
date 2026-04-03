using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Slums.Application.Persistence;
using Slums.Core.Characters;
using Slums.Core.Investments;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.Training;
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
            gameSession.World.SetActiveDistrictConditions(
            [
                new ActiveDistrictCondition { District = DistrictId.Imbaba, ConditionId = "imbaba_market_crackdown" },
                new ActiveDistrictCondition { District = DistrictId.Dokki, ConditionId = "dokki_checkpoint_sweep" }
            ]);
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
            gameSession.RestoreHouseholdAssetsState(
            [
                OwnedPet.Restore(PetType.Cat, acquiredOnDay: 3, lastUpkeepPaidWeek: 2, hasBetterFilter: false, hasHeater: false, decorationsPaidWeek: 0, waterConditionerPaidWeek: 0)
            ],
            [
                OwnedPlant.Restore(Guid.Parse("11111111-1111-1111-1111-111111111111"), PlantType.Hibiscus, 4, 2, 11, hasBiggerPot: true, hasWindowPlacement: false, fertilizerPaidWeek: 2, irrigationPaidWeek: 0)
            ],
            hasStreetCatEncounter: true,
            lastStreetCatEncounterDay: 7,
            totalHerbEarnings: 25);
            gameSession.RestoreInvestmentState(
            [
                new InvestmentSnapshot(InvestmentType.FoulCart, 150, 8, 12, 3, false)
            ],
            totalInvestmentEarnings: 27);
            gameSession.Player.Stats.SetEnergy(100);
            gameSession.Player.Skills.SetLevel(Slums.Core.Skills.SkillId.Physical, 2);
            gameSession.RestoreTrainedSkillsToday(
                new Dictionary<Slums.Core.Skills.SkillId, bool> { { Slums.Core.Skills.SkillId.Physical, true } });

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
                    restoredSession.GetDailyDistrictConditions().Should().ContainSingle(static definition => definition.Id == "imbaba_market_crackdown");
                    restoredSession.GetDailyDistrictConditions().Should().ContainSingle(static definition => definition.Id == "dokki_checkpoint_sweep");
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
                    restoredSession.Player.HouseholdAssets.Pets.Should().ContainSingle();
                    restoredSession.Player.HouseholdAssets.Pets[0].Type.Should().Be(PetType.Cat);
                    restoredSession.Player.HouseholdAssets.Plants.Should().ContainSingle();
                    restoredSession.Player.HouseholdAssets.Plants[0].Type.Should().Be(PlantType.Hibiscus);
                    restoredSession.Player.HouseholdAssets.Plants[0].HasActiveUpgrade(PlantUpgradeType.BiggerPot, restoredSession.CurrentWeek).Should().BeTrue();
                    restoredSession.Player.HouseholdAssets.HasStreetCatEncounter.Should().BeTrue();
                    restoredSession.Player.HouseholdAssets.TotalHerbEarnings.Should().Be(25);
                    restoredSession.TotalInvestmentEarnings.Should().Be(27);
                    restoredSession.ActiveInvestments.Should().ContainSingle();
                    restoredSession.ActiveInvestments[0].Type.Should().Be(InvestmentType.FoulCart);
                    restoredSession.ActiveInvestments[0].WeeksActive.Should().Be(3);
                    restoredSession.TrainedSkillsToday.Should().ContainKey(SkillId.Physical);
                    restoredSession.Player.Skills.GetLevel(SkillId.Physical).Should().BeGreaterThan(0);
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
