using Slums.Core.Entertainment;
using Slums.Core.State;
using Slums.Core.World;
using TUnit;

namespace Slums.Core.Tests.Entertainment;

internal sealed class EntertainmentServiceTests
{
    [Test]
    public async Task GetAvailableActivities_ShouldResolveFromTheSessionLocation()
    {
        var session = new GameSession();
        session.World.TravelTo(LocationId.Cafe);

        var activities = EntertainmentService.GetAvailableActivities(session);

        await Assert.That(activities.Select(static activity => activity.Type))
            .Contains(EntertainmentActivityType.Coffee);
    }

    [Test]
    public async Task Perform_ShouldPreserveEntertainmentMutationAndStateChanges()
    {
        var session = new GameSession();
        session.World.TravelTo(LocationId.Cafe);
        var activity = EntertainmentRegistry.AllActivities.Single(static candidate => candidate.Type == EntertainmentActivityType.Coffee);
        session.Player.Stats.SetStress(50);
        var moneyBefore = session.Player.Stats.Money;

        var result = EntertainmentService.Perform(session, activity);

        await Assert.That(result).IsTrue();
        await Assert.That(session.Player.Stats.Money).IsEqualTo(moneyBefore - activity.BaseCost);
        await Assert.That(session.Player.Stats.Stress).IsEqualTo(50 - activity.StressReduction);
        await Assert.That(session.Mutations[^1].Action).IsEqualTo("TryPerformEntertainment");
        await Assert.That(session.EventJournal.Entries[^1].Message).IsEqualTo("The coffee is strong and bitter. You feel a little lighter.");
    }
}
