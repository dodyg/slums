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