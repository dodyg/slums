using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Slums.Core.Characters;
using Slums.Core.World;
using Slums.Infrastructure.Content;
using TUnit.Core;

namespace Slums.Infrastructure.Tests;

internal sealed class JsonContentRepositoryTests
{
    [Test]
    public void LoadBackgrounds_ShouldReturnConfiguredBackgrounds()
    {
        var contentDirectory = CreateTempDirectory();
        try
        {
            File.WriteAllText(Path.Combine(contentDirectory, "backgrounds.json"), """
            [
              {
                "Type": "MedicalSchoolDropout",
                "Name": "Medical School Dropout",
                "Description": "desc",
                "StoryIntro": "intro",
                "StartingMoney": 80,
                "StartingHealth": 100,
                "StartingEnergy": 70,
                "StartingHunger": 75,
                "StartingStress": 35,
                "MotherStartingHealth": 60,
                "FoodStockpile": 2,
                "InkIntroKnot": "intro_medical"
              }
            ]
            """);

            var repository = new JsonContentRepository(NullLogger<JsonContentRepository>.Instance, contentDirectory);
            var backgrounds = repository.LoadBackgrounds();

            backgrounds.Should().HaveCount(1);
            backgrounds[0].Name.Should().Be("Medical School Dropout");
        }
        finally
        {
            DeleteDirectory(contentDirectory);
        }
    }

    [Test]
    public void LoadBackgrounds_ShouldThrow_WhenFileIsMissing()
    {
        var contentDirectory = CreateTempDirectory();
        try
        {
            var repository = new JsonContentRepository(NullLogger<JsonContentRepository>.Instance, contentDirectory);

            var act = () => repository.LoadBackgrounds();

            act.Should().Throw<ContentLoadException>();
        }
        finally
        {
            DeleteDirectory(contentDirectory);
        }
    }

    [Test]
    public void LoadBackgrounds_ShouldThrow_WhenJsonIsInvalid()
    {
        var contentDirectory = CreateTempDirectory();
        try
        {
            File.WriteAllText(Path.Combine(contentDirectory, "backgrounds.json"), "not json");
            var repository = new JsonContentRepository(NullLogger<JsonContentRepository>.Instance, contentDirectory);

            var act = () => repository.LoadBackgrounds();

            act.Should().Throw<ContentLoadException>();
        }
        finally
        {
            DeleteDirectory(contentDirectory);
        }
    }

    [Test]
    public void LoadLocations_ShouldDeserializeClinicMetadata()
    {
        var contentDirectory = CreateTempDirectory();
        try
        {
            File.WriteAllText(Path.Combine(contentDirectory, "locations.json"), """
            [
              {
                "Id": { "Value": "clinic" },
                "Name": "Rahma Clinic",
                "Description": "desc",
                "District": "ArdAlLiwa",
                "HasJobOpportunities": true,
                "HasCrimeOpportunities": false,
                "HasClinicServices": true,
                "ClinicVisitBaseCost": 35,
                "ClinicOpenDays": ["Saturday", "Sunday", "Thursday"],
                "TravelTimeMinutes": 25
              }
            ]
            """);

            var repository = new JsonContentRepository(NullLogger<JsonContentRepository>.Instance, contentDirectory);

            var locations = repository.LoadLocations();

            locations.Should().HaveCount(1);
            locations[0].HasClinicServices.Should().BeTrue();
            locations[0].ClinicVisitBaseCost.Should().Be(35);
            locations[0].ClinicOpenDays.Should().Contain(DayOfWeek.Thursday);
        }
        finally
        {
            DeleteDirectory(contentDirectory);
        }
    }

    [Test]
    public void LoadPetsAndPlants_ShouldDeserializeConfiguredCatalogs()
    {
        var contentDirectory = CreateTempDirectory();
        try
        {
            File.WriteAllText(Path.Combine(contentDirectory, "pets.json"), """
            [
              {
                "Type": "Cat",
                "Name": "Street Cat",
                "Description": "desc",
                "MaxOwned": 3,
                "OneTimeCost": 0,
                "WeeklyCareCost": 6,
                "PassiveMotherHealthBonus": 1,
                "ActiveCareMotherHealthBonus": 1,
                "RequiresStreetEncounter": true
              }
            ]
            """);
            File.WriteAllText(Path.Combine(contentDirectory, "plants.json"), """
            [
              {
                "Type": "Chamomile",
                "Name": "Chamomile",
                "ArabicName": "babounig",
                "Category": "SellableHerb",
                "Description": "desc",
                "OneTimeCost": 12,
                "WeeklyCareCost": 3,
                "PassiveMotherHealthBonus": 0,
                "ActiveCareMotherHealthBonus": 0,
                "MotherHealthBonusPerUpgrade": 0,
                "CookingBonus": 1,
                "CookingBonusPerUpgrade": 1,
                "IsSellable": true,
                "HarvestCycleDays": 5,
                "HarvestSalePrice": 10,
                "HarvestPriceBonusPerUpgrade": 5,
                "PurchaseLocationId": { "Value": "plant_shop" }
              }
            ]
            """);

            var repository = new JsonContentRepository(NullLogger<JsonContentRepository>.Instance, contentDirectory);

            var pets = repository.LoadPets();
            var plants = repository.LoadPlants();

            pets.Should().ContainSingle();
            pets[0].Type.Should().Be(PetType.Cat);
            plants.Should().ContainSingle();
            plants[0].Type.Should().Be(PlantType.Chamomile);
            plants[0].PurchaseLocationId.Should().Be(LocationId.PlantShop);
        }
        finally
        {
            DeleteDirectory(contentDirectory);
        }
    }

    [Test]
    public void LoadRandomEvents_ShouldMapLocationSpecificConditions()
    {
        var contentDirectory = CreateTempDirectory();
        try
        {
            File.WriteAllText(Path.Combine(contentDirectory, "random_events.json"), """
            [
              {
                "Id": "ClinicOverflow",
                "Description": "desc",
                "MinDay": 4,
                "Weight": 7,
                "ConditionId": "at_clinic",
                "MoneyChange": 18,
                "InkKnot": "event_clinic_overflow"
              }
            ]
            """);

            var repository = new JsonContentRepository(NullLogger<JsonContentRepository>.Instance, contentDirectory);
            using var state = new Slums.Core.State.GameSession();
            state.World.TravelTo(Slums.Core.World.LocationId.Clinic);

            var events = repository.LoadRandomEvents();

            events.Should().HaveCount(1);
            events[0].Condition.Should().NotBeNull();
            events[0].Condition!(state).Should().BeTrue();
        }
        finally
        {
            DeleteDirectory(contentDirectory);
        }
    }

    [Test]
    public void LoadRandomEvents_ShouldMapPolicePressureConditionAndChange()
    {
        var contentDirectory = CreateTempDirectory();
        try
        {
            File.WriteAllText(Path.Combine(contentDirectory, "random_events.json"), """
            [
              {
                "Id": "DokkiCheckpointSweep",
                "Description": "desc",
                "MinDay": 5,
                "Weight": 9,
                "ConditionId": "in_dokki",
                "PolicePressureChange": 6,
                "InkKnot": "event_dokki_checkpoint_sweep"
              }
            ]
            """);

            var repository = new JsonContentRepository(NullLogger<JsonContentRepository>.Instance, contentDirectory);
            using var state = new Slums.Core.State.GameSession();
            state.World.TravelTo(Slums.Core.World.LocationId.CallCenter);

            var events = repository.LoadRandomEvents();

            events.Should().HaveCount(1);
            events[0].Condition!(state).Should().BeTrue();
            events[0].Effect.PolicePressureChange.Should().Be(6);
        }
        finally
        {
            DeleteDirectory(contentDirectory);
        }
    }

    [Test]
    public void LoadRandomEvents_ShouldMapNewBulaqAndShubraConditions()
    {
        var contentDirectory = CreateTempDirectory();
        try
        {
            File.WriteAllText(Path.Combine(contentDirectory, "random_events.json"), """
            [
                {
                    "Id": "BulaqMedicineQueue",
                    "Description": "desc",
                    "MinDay": 5,
                    "Weight": 8,
                    "ConditionId": "at_pharmacy",
                    "InkKnot": "event_bulaq_medicine_queue"
                },
                {
                    "Id": "ShubraBlockSolidarity",
                    "Description": "desc",
                    "MinDay": 5,
                    "Weight": 7,
                    "ConditionId": "shubra_low_money",
                    "FoodChange": 1,
                    "InkKnot": "event_shubra_block_solidarity"
                }
            ]
            """);

            var repository = new JsonContentRepository(NullLogger<JsonContentRepository>.Instance, contentDirectory);
            using var bulaqState = new Slums.Core.State.GameSession();
            bulaqState.World.TravelTo(Slums.Core.World.LocationId.Pharmacy);
            using var shubraState = new Slums.Core.State.GameSession();
            shubraState.World.TravelTo(Slums.Core.World.LocationId.Laundry);
            shubraState.Player.Stats.ModifyMoney(-10);

            var events = repository.LoadRandomEvents();

            events.Should().HaveCount(2);
            events[0].Condition!(bulaqState).Should().BeTrue();
            events[1].Condition!(shubraState).Should().BeTrue();
        }
        finally
        {
            DeleteDirectory(contentDirectory);
        }
    }

    [Test]
    public void LoadRandomEvents_ShouldThrow_WhenConditionIdIsUnknown()
    {
        var contentDirectory = CreateTempDirectory();
        try
        {
            File.WriteAllText(Path.Combine(contentDirectory, "random_events.json"), """
            [
              {
                "Id": "BadEvent",
                "Description": "desc",
                "MinDay": 1,
                "Weight": 1,
                "ConditionId": "unknown_condition"
              }
            ]
            """);

            var repository = new JsonContentRepository(NullLogger<JsonContentRepository>.Instance, contentDirectory);

            var act = () => repository.LoadRandomEvents();

            act.Should().Throw<ContentLoadException>();
        }
        finally
        {
            DeleteDirectory(contentDirectory);
        }
    }

    [Test]
    public void LoadDistrictConditions_ShouldDeserializeEffectsAndBulletins()
    {
        var contentDirectory = CreateTempDirectory();
        try
        {
            File.WriteAllText(Path.Combine(contentDirectory, "district_conditions.json"), """
            [
              {
                "Id": "dokki_checkpoint_sweep",
                "District": "Dokki",
                "Title": "Checkpoint Sweep",
                "BulletinText": "desc",
                "GameplaySummary": "Travel is slower and crime runs hotter.",
                "MinDay": 2,
                "Weight": 4,
                "MinPolicePressure": 20,
                "Effect": {
                  "TravelTimeMinutesModifier": 10,
                  "CrimeDetectionRiskModifier": 9,
                  "BoostedRandomEventIds": ["DokkiCheckpointSweep"],
                  "SuppressedRandomEventIds": []
                }
              }
            ]
            """);

            var repository = new JsonContentRepository(NullLogger<JsonContentRepository>.Instance, contentDirectory);

            var conditions = repository.LoadDistrictConditions();

            conditions.Should().ContainSingle();
            conditions[0].District.Should().Be(Slums.Core.World.DistrictId.Dokki);
            conditions[0].Effect.TravelTimeMinutesModifier.Should().Be(10);
            conditions[0].Effect.BoostedRandomEventIds.Should().ContainSingle().Which.Should().Be("DokkiCheckpointSweep");
            conditions[0].Effect.SuppressedRandomEventIds.Should().BeEmpty();
        }
        finally
        {
            DeleteDirectory(contentDirectory);
        }
    }

    [Test]
    public void LoadDistrictConditions_FromRepositoryContent_ShouldConfigureDistrictConditionRegistry()
    {
        var repository = new JsonContentRepository(NullLogger<JsonContentRepository>.Instance, GetRepositoryContentDirectory());
        var originalDefinitions = DistrictConditionRegistry.AllDefinitions.ToArray();

        try
        {
            var conditions = repository.LoadDistrictConditions();

            conditions.Should().OnlyContain(static condition =>
                condition.Effect != null
                && condition.Effect.BoostedRandomEventIds != null
                && condition.Effect.SuppressedRandomEventIds != null);

            var act = () => DistrictConditionRegistry.Configure(conditions);

            act.Should().NotThrow();
        }
        finally
        {
            DistrictConditionRegistry.Configure(originalDefinitions);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "slums-content-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string GetRepositoryContentDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "content", "data", "district_conditions.json");
            if (File.Exists(candidate))
            {
                return Path.GetDirectoryName(candidate)!;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository content/data directory.");
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
