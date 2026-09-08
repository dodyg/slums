using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Diagnostics;
using Slums.Core.Expenses;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;
using Slums.TestSupport;

namespace Slums.Core.Tests.Expenses;

internal sealed class FoodPreservationTests
{
    [Test]
    public void Preview_ShouldExposeTheSameBoundedConversionUsedByCommit()
    {
        var session = CreateSession(food: 5);

        var preview = session.PreviewFoodPreservation();

        preview.RequiredSkillLevel.Should().Be(SkillThresholds.HighLevel);
        preview.InputFoodUnits.Should().Be(2);
        preview.OutputMealUnits.Should().Be(1);
        preview.TimeCostMinutes.Should().Be(60);
        preview.EnergyCost.Should().Be(8);
        preview.CanPerform.Should().BeTrue();
    }

    [Test]
    public void PreserveFood_ShouldConvertStaplesAndRecordTheFoodMutation()
    {
        var session = CreateSession(food: 5);
        var beforeEnergy = session.Player.Stats.Energy;

        var result = session.PreserveFood();

        result.Should().BeTrue();
        session.Player.Household.FoodStockpile.Should().Be(3);
        session.Player.Household.PreservedMealUnits.Should().Be(1);
        session.Player.Stats.Energy.Should().Be(beforeEnergy - 8);
        session.Mutations[^1].Category.Should().Be(MutationCategories.Food);
        session.Mutations[^1].Action.Should().Be("PreserveFood");
        session.EventJournal.Entries[^1].Message.Should().Contain("preserve");
    }

    [Test]
    public void EatAtHome_ShouldConsumePreservedMealBeforeFreshStaples()
    {
        var session = CreateSession(food: 5);
        session.Player.Household.SetPreservedMealUnits(1);
        var plan = session.GetProvisioningMealPlan();

        plan.FoodUnitsRequired.Should().Be(0);
        plan.UsesPreservedFood.Should().BeTrue();
        session.EatAtHome().Should().BeTrue();

        session.Player.Household.PreservedMealUnits.Should().Be(0);
        session.Player.Household.FoodStockpile.Should().Be(5);
    }

    [Test]
    public void PreserveFood_ShouldExplainMissingSkillAndSupplies()
    {
        var unskilled = CreateSession(skill: 4, food: 5);
        var noFood = CreateSession(skill: 6, food: 1);

        unskilled.PreviewFoodPreservation().UnavailabilityReason.Should().Be("Reach Provisioning 6.");
        noFood.PreviewFoodPreservation().UnavailabilityReason.Should().Be("Requires 2 staple units.");
    }

    private static GameSession CreateSession(int skill = 6, int food = 3)
    {
        var session = TestSessions.Create();
        session.Player.ApplyBackground(TestContent.Catalog.GetBackground(BackgroundType.MedicalSchoolDropout));
        session.Player.Skills.SetLevel(SkillId.Provisioning, skill);
        session.Player.Household.SetFoodStockpile(food);
        session.Player.Stats.SetEnergy(100);
        session.Clock.SetTime(1, 12, 0);
        session.World.TravelTo(LocationId.Home);
        return session;
    }
}
