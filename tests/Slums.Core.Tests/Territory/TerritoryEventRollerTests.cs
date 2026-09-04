using FluentAssertions;
using Slums.Core.State;
using Slums.Core.Territory;
using TUnit.Core;

namespace Slums.Core.Tests.Territory;

internal sealed class TerritoryEventRollerTests
{
    [Test]
    public void Roll_ShouldApplyConflictConsequencesThroughTheSession()
    {
        var session = new GameSession();
        session.Territory.ModifyTension(session.World.CurrentDistrict, 80);
        var initialHealth = session.Player.Stats.Health;

        TerritoryEventRoller.Roll(session, new Random(1));

        session.EventJournal.Entries.Should().NotBeEmpty();
        session.Player.Stats.Health.Should().BeLessThanOrEqualTo(initialHealth);
    }
}
