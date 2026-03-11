using FluentAssertions;
using Slums.Core.Characters;
using TUnit.Core;

namespace Slums.Core.Tests.Characters;

internal sealed class PlayerCharacterTests
{
    [Test]
    public async Task Constructor_ShouldInitializeWithDefaultValues()
    {
        var player = new PlayerCharacter();

        await Assert.That(player.Name).IsEqualTo("Amira");
        await Assert.That(player.Age).IsEqualTo(24);
        await Assert.That(player.BackgroundType).IsEqualTo(BackgroundType.MedicalSchoolDropout);
        await Assert.That(player.HasSelectedBackground).IsFalse();
        await Assert.That(player.Background).IsNull();
        await Assert.That(player.Stats).IsNotNull();
        await Assert.That(player.Household).IsNotNull();
    }

    [Test]
    public async Task ApplyBackground_ShouldSetBackgroundProperties()
    {
        var player = new PlayerCharacter();
        var background = new Background
        {
            Type = BackgroundType.ReleasedPoliticalPrisoner,
            Name = "Released Political Prisoner",
            Description = "You were detained for your activism.",
            StartingMoney = 50,
            StartingHealth = 90,
            StartingEnergy = 70,
            StartingHunger = 70,
            StartingStress = 40,
            MotherStartingHealth = 60,
            FoodStockpile = 2
        };

        player.ApplyBackground(background);

        await Assert.That(player.BackgroundType).IsEqualTo(BackgroundType.ReleasedPoliticalPrisoner);
        await Assert.That(player.Background).IsEqualTo(background);
        await Assert.That(player.HasSelectedBackground).IsTrue();
    }

    [Test]
    public async Task ApplyBackground_ShouldSetPlayerStats()
    {
        var player = new PlayerCharacter();
        var background = new Background
        {
            Type = BackgroundType.SudaneseRefugee,
            StartingMoney = 30,
            StartingHealth = 85,
            StartingEnergy = 60,
            StartingHunger = 50,
            StartingStress = 50
        };

        player.ApplyBackground(background);

        await Assert.That(player.Stats.Money).IsEqualTo(30);
        await Assert.That(player.Stats.Health).IsEqualTo(85);
        await Assert.That(player.Stats.Energy).IsEqualTo(60);
        await Assert.That(player.Stats.Hunger).IsEqualTo(50);
        await Assert.That(player.Stats.Stress).IsEqualTo(50);
    }

    [Test]
    public async Task ApplyBackground_ShouldSetHouseholdState()
    {
        var player = new PlayerCharacter();
        var background = new Background
        {
            Type = BackgroundType.MedicalSchoolDropout,
            MotherStartingHealth = 80,
            FoodStockpile = 5
        };

        player.ApplyBackground(background);

        await Assert.That(player.Household.MotherHealth).IsEqualTo(80);
        await Assert.That(player.Household.FoodStockpile).IsEqualTo(5);
    }

    [Test]
    public async Task ApplyBackground_ShouldThrow_WhenBackgroundIsNull()
    {
        var player = new PlayerCharacter();

        var act = () => player.ApplyBackground(null!);

        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Name_CanBeChanged()
    {
        var player = new PlayerCharacter();

        player.Name = "Fatima";

        await Assert.That(player.Name).IsEqualTo("Fatima");
    }
}

internal sealed class BackgroundTests
{
    [Test]
    public async Task Background_ShouldHaveDefaultValues()
    {
        var background = new Background();

        await Assert.That(background.Type).IsEqualTo(default(BackgroundType));
        await Assert.That(background.Name).IsEmpty();
        await Assert.That(background.Description).IsEmpty();
        await Assert.That(background.StoryIntro).IsEmpty();
        await Assert.That(background.StartingMoney).IsEqualTo(100);
        await Assert.That(background.StartingHealth).IsEqualTo(100);
        await Assert.That(background.StartingEnergy).IsEqualTo(80);
        await Assert.That(background.StartingHunger).IsEqualTo(80);
        await Assert.That(background.StartingStress).IsEqualTo(20);
        await Assert.That(background.MotherStartingHealth).IsEqualTo(70);
        await Assert.That(background.FoodStockpile).IsEqualTo(3);
        await Assert.That(background.InkIntroKnot).IsEmpty();
    }
}

internal sealed class BackgroundRegistryTests
{
    [Test]
    public async Task AllBackgrounds_ShouldContainAllThreeBackgrounds()
    {
        var all = BackgroundRegistry.AllBackgrounds;

        all.Should().HaveCount(3);
        all.Select(b => b.Type).Should().Contain(
            new[] { BackgroundType.MedicalSchoolDropout, BackgroundType.ReleasedPoliticalPrisoner, BackgroundType.SudaneseRefugee });
    }

    [Test]
    public async Task MedicalSchoolDropout_ShouldHaveCorrectValues()
    {
        var bg = BackgroundRegistry.MedicalSchoolDropout;

        await Assert.That(bg.Type).IsEqualTo(BackgroundType.MedicalSchoolDropout);
        await Assert.That(bg.Name).Contains("Medical");
        await Assert.That(bg.StartingMoney).IsEqualTo(80);
        await Assert.That(bg.StartingHealth).IsEqualTo(100);
    }

    [Test]
    public async Task ReleasedPoliticalPrisoner_ShouldHaveCorrectValues()
    {
        var bg = BackgroundRegistry.ReleasedPoliticalPrisoner;

        await Assert.That(bg.Type).IsEqualTo(BackgroundType.ReleasedPoliticalPrisoner);
        await Assert.That(bg.Name).Contains("Prisoner");
        await Assert.That(bg.StartingMoney).IsEqualTo(30);
    }

    [Test]
    public async Task SudaneseRefugee_ShouldHaveCorrectValues()
    {
        var bg = BackgroundRegistry.SudaneseRefugee;

        await Assert.That(bg.Type).IsEqualTo(BackgroundType.SudaneseRefugee);
        await Assert.That(bg.Name).Contains("Refugee");
        await Assert.That(bg.StartingMoney).IsEqualTo(50);
    }

    [Test]
    public async Task GetByType_ShouldReturnCorrectBackground()
    {
        await Assert.That(BackgroundRegistry.GetByType(BackgroundType.MedicalSchoolDropout))
            .IsEqualTo(BackgroundRegistry.MedicalSchoolDropout);
        await Assert.That(BackgroundRegistry.GetByType(BackgroundType.ReleasedPoliticalPrisoner))
            .IsEqualTo(BackgroundRegistry.ReleasedPoliticalPrisoner);
        await Assert.That(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee))
            .IsEqualTo(BackgroundRegistry.SudaneseRefugee);
    }
}
