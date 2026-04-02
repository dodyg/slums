using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Characters;
using Slums.Core.Entertainment;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class EntertainmentMenuQueryTests
{
    private static EntertainmentActivity MakeActivity(int cost = 5, int energyCost = 3) =>
        new(EntertainmentActivityType.Coffee, "Coffee", "Test", cost, 30, 3, energyCost, true, false, false);

    private static PlayerCharacter MakePlayer(int money = 50, int energy = 50)
    {
        var player = new PlayerCharacter();
        player.Stats.SetMoney(money);
        player.Stats.SetEnergy(energy);
        return player;
    }

    [Test]
    public void GetStatuses_ShouldThrow_WhenContextIsNull()
    {
        var query = new EntertainmentMenuQuery();
        var act = () => query.GetStatuses(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetStatuses_CanPerform_WhenMoneyAndEnergySufficient()
    {
        var query = new EntertainmentMenuQuery();
        var activities = new[] { MakeActivity(5, 3) };
        var context = new EntertainmentMenuContext(MakePlayer(50, 50), activities, "Test");

        var statuses = query.GetStatuses(context);

        statuses.Should().HaveCount(1);
        statuses[0].CanAfford.Should().BeTrue();
        statuses[0].HasEnergy.Should().BeTrue();
        statuses[0].CanPerform.Should().BeTrue();
        statuses[0].UnavailabilityReason.Should().BeNull();
    }

    [Test]
    public void GetStatuses_BlockedByMoney_WhenInsufficient()
    {
        var query = new EntertainmentMenuQuery();
        var activities = new[] { MakeActivity(10, 3) };
        var context = new EntertainmentMenuContext(MakePlayer(5, 50), activities, "Test");

        var statuses = query.GetStatuses(context);

        statuses[0].CanAfford.Should().BeFalse();
        statuses[0].CanPerform.Should().BeFalse();
        statuses[0].UnavailabilityReason.Should().Contain("10");
    }

    [Test]
    public void GetStatuses_BlockedByEnergy_WhenInsufficient()
    {
        var query = new EntertainmentMenuQuery();
        var activities = new[] { MakeActivity(5, 20) };
        var context = new EntertainmentMenuContext(MakePlayer(50, 5), activities, "Test");

        var statuses = query.GetStatuses(context);

        statuses[0].HasEnergy.Should().BeFalse();
        statuses[0].CanPerform.Should().BeFalse();
        statuses[0].UnavailabilityReason.Should().Contain("20");
    }

    [Test]
    public void GetStatuses_BlockedByBoth_WhenMoneyAndEnergyInsufficient()
    {
        var query = new EntertainmentMenuQuery();
        var activities = new[] { MakeActivity(10, 20) };
        var context = new EntertainmentMenuContext(MakePlayer(2, 2), activities, "Test");

        var statuses = query.GetStatuses(context);

        statuses[0].CanPerform.Should().BeFalse();
        statuses[0].UnavailabilityReason.Should().Contain("money or energy");
    }

    [Test]
    public void GetStatuses_HandlesMultipleActivities()
    {
        var query = new EntertainmentMenuQuery();
        var activities = new[]
        {
            MakeActivity(5, 3),
            new EntertainmentActivity(EntertainmentActivityType.Billiards, "Billiards", "Test", 8, 45, 5, 10, false, false, true)
        };
        var context = new EntertainmentMenuContext(MakePlayer(6, 50), activities, "Test");

        var statuses = query.GetStatuses(context);

        statuses.Should().HaveCount(2);
        statuses[0].CanPerform.Should().BeTrue();
        statuses[1].CanPerform.Should().BeFalse();
    }
}
