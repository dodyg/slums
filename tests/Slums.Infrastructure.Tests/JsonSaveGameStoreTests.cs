using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Slums.Application.Narrative;
using Slums.Core.Characters;
using Slums.Core.World;
using Slums.Infrastructure.Persistence;
using TUnit.Core;

namespace Slums.Infrastructure.Tests;

internal sealed class JsonSaveGameStoreTests
{
    [Test]
    public async Task SaveAndLoad_ShouldRoundTripGameState()
    {
        var saveDirectory = CreateTempDirectory("slums-save-tests");
        try
        {
            var store = new JsonSaveGameStore(NullLogger<JsonSaveGameStore>.Instance, saveDirectory);
            var narrative = Substitute.For<INarrativeService>();
            narrative.LastKnot.Returns("crime_warning");

            var state = new Slums.Core.State.GameState();
            state.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);
            state.Player.Stats.SetMoney(222);
            state.Player.Stats.SetHealth(77);
            state.Player.Stats.SetEnergy(55);
            state.Player.Household.SetMotherHealth(63);
            state.World.TravelTo(LocationId.Market);
            state.SetPolicePressure(30);
            state.SetDaysSurvived(7);
            state.SetCrimeCounters(120, 2);
            state.SetStoryFlag("crime_warning");

            await store.SaveAsync(state, narrative, "slot1").ConfigureAwait(false);
            var loaded = await store.LoadAsync("slot1").ConfigureAwait(false);

            loaded.Should().NotBeNull();
            loaded!.LastKnot.Should().Be("crime_warning");
            loaded.GameState.Player.Stats.Money.Should().Be(222);
            loaded.GameState.Player.Stats.Health.Should().Be(77);
            loaded.GameState.World.CurrentLocationId.Should().Be(LocationId.Market);
            loaded.GameState.PolicePressure.Should().Be(30);
            loaded.GameState.CrimesCommitted.Should().Be(2);
            loaded.GameState.StoryFlags.Should().Contain("crime_warning");
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
              "RunId": "00000000-0000-0000-0000-000000000001",
              "CreatedUtc": "2026-03-11T00:00:00+00:00",
              "LastPlayedUtc": "2026-03-11T00:00:00+00:00",
              "CheckpointName": "bad",
              "GameState": {
                "Money": 100,
                "Hunger": 80,
                "Energy": 80,
                "Health": 100,
                "Stress": 20,
                "MotherHealth": 70,
                "FoodStockpile": 3,
                "Day": 1,
                "Hour": 6,
                "Minute": 0,
                "BackgroundType": "MedicalSchoolDropout",
                "CurrentLocationId": "home",
                "PolicePressure": 0,
                "TotalCrimeEarnings": 0,
                "CrimesCommitted": 0,
                "DaysSurvived": 0,
                "SkillLevels": {},
                "NpcTrust": {},
                "NpcLastSeenDay": {},
                "FactionReputation": {},
                "StoryFlags": []
              },
              "NarrativeState": {
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