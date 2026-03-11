using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
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
    public void LoadBackgrounds_ShouldReturnEmpty_WhenFileIsMissing()
    {
        var contentDirectory = CreateTempDirectory();
        try
        {
            var repository = new JsonContentRepository(NullLogger<JsonContentRepository>.Instance, contentDirectory);

            var backgrounds = repository.LoadBackgrounds();

            backgrounds.Should().BeEmpty();
        }
        finally
        {
            DeleteDirectory(contentDirectory);
        }
    }

    [Test]
    public void LoadBackgrounds_ShouldReturnEmpty_WhenJsonIsInvalid()
    {
        var contentDirectory = CreateTempDirectory();
        try
        {
            File.WriteAllText(Path.Combine(contentDirectory, "backgrounds.json"), "not json");
            var repository = new JsonContentRepository(NullLogger<JsonContentRepository>.Instance, contentDirectory);

            var backgrounds = repository.LoadBackgrounds();

            backgrounds.Should().BeEmpty();
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
            var state = new Slums.Core.State.GameState();
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

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "slums-content-tests", Guid.NewGuid().ToString("N"));
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