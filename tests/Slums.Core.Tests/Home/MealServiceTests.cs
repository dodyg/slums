using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Home;

internal sealed class MealServiceTests
{
    [Test]
    public void EatAtHome_WhenFoodIsUnavailable_ReturnsFalseAndRaisesExpectedEvent()
    {
        var session = new GameSession();
        session.Player.Household.SetFoodStockpile(0);

        var result = session.EatAtHome();

        result.Should().BeFalse();
        session.EventJournal.Entries.Should().Contain(entry => entry.Message == "There is not enough food at home.");
    }

    [Test]
    public void EatAtHome_WithCookingHerb_AppliesBonusAndRaisesBothEvents()
    {
        var session = new GameSession();
        session.Player.HouseholdAssets.BuyPlant(PlantType.Mint, session.Clock.Day, session.CurrentWeek);
        session.Player.Stats.SetStress(20);

        var result = session.EatAtHome();

        result.Should().BeTrue();
        session.Player.Stats.Stress.Should().Be(19);
        session.EventJournal.Entries.Should().Contain(entry => entry.Message == "You eat a simple meal at home and make sure your mother eats too.");
        session.EventJournal.Entries.Should().Contain(entry => entry.Message == "Fresh herbs soften the meal a little. Stress -1.");
    }

    [Test]
    public void EatAtHome_WithoutCookingHerb_DoesNotRaiseCookingBonusEvent()
    {
        var session = new GameSession();

        var result = session.EatAtHome();

        result.Should().BeTrue();
        session.EventJournal.Entries.Should().NotContain(entry => entry.Message.StartsWith("Fresh herbs", StringComparison.Ordinal));
    }

    [Test]
    public void EatStreetFood_WhenMoneyIsInsufficient_ReturnsFalseAndRaisesExpectedEvent()
    {
        var session = new GameSession();
        session.Player.Stats.SetMoney(0);
        var cost = session.GetStreetFoodCost();

        var result = session.EatStreetFood();

        result.Should().BeFalse();
        session.EventJournal.Entries.Should().Contain(entry => entry.Message == $"You do not have enough money for street food. It costs {cost} LE here.");
    }

    [Test]
    public void EatStreetFood_WhenMoneyIsAvailable_ReturnsTrueAndRaisesExpectedEvent()
    {
        var session = new GameSession();
        var cost = session.GetStreetFoodCost();

        var result = session.EatStreetFood();

        result.Should().BeTrue();
        session.EventJournal.Entries.Should().Contain(entry => entry.Message == $"You grab a cheap meal from the street for {cost} LE.");
    }
}
