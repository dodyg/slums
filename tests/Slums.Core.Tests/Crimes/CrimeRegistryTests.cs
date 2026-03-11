using FluentAssertions;
using Slums.Core.Crimes;
using Slums.Core.Relationships;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Crimes;

internal sealed class CrimeRegistryTests
{
    [Test]
    public void GetCrimeOpportunityStatuses_ShouldShowStreetRepBlockReason_ForRobbery()
    {
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Market);
        var relationships = new RelationshipState();

        var statuses = CrimeRegistry.GetCrimeOpportunityStatuses(location, relationships);

        var robbery = statuses.Single(static status => status.Attempt.Type == CrimeType.Robbery);
        robbery.IsAvailable.Should().BeFalse();
        robbery.BlockReason.Should().Contain("street rep 10");
    }

    [Test]
    public void GetCrimeOpportunityStatuses_ShouldShowTrustBlockReason_ForHananRoute()
    {
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Market);
        var relationships = new RelationshipState();

        var statuses = CrimeRegistry.GetCrimeOpportunityStatuses(location, relationships);

        var fencing = statuses.Single(static status => status.Attempt.Type == CrimeType.MarketFencing);
        fencing.IsAvailable.Should().BeFalse();
        fencing.BlockReason.Should().Contain("Hanan trust 10");
    }

    [Test]
    public void GetCrimeOpportunityStatuses_ShouldShowTrustOrRepBlockReason_ForDokkiDrop()
    {
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Square);
        var relationships = new RelationshipState();

        var statuses = CrimeRegistry.GetCrimeOpportunityStatuses(location, relationships);

        var dokkiDrop = statuses.Single(static status => status.Attempt.Type == CrimeType.DokkiDrop);
        dokkiDrop.IsAvailable.Should().BeFalse();
        dokkiDrop.BlockReason.Should().Contain("Youssef trust 15 or Dokki standing 15");
    }

    [Test]
    public void GetAvailableCrimes_ShouldUnlockHananFencingRoute_WhenHananTrustIsHigh()
    {
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Market);
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.FenceHanan, 10, 1);

        var crimes = CrimeRegistry.GetAvailableCrimes(location, relationships);

        crimes.Select(static attempt => attempt.Type).Should().Contain(CrimeType.MarketFencing);
    }

    [Test]
    public void GetAvailableCrimes_ShouldUnlockDokkiDropRoute_WhenYoussefTrustIsHigh()
    {
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Square);
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.RunnerYoussef, 15, 1);
        relationships.SetFactionStanding(FactionId.DokkiThugs, 15);

        var crimes = CrimeRegistry.GetAvailableCrimes(location, relationships);

        crimes.Select(static attempt => attempt.Type).Should().Contain(CrimeType.DokkiDrop);
    }

    [Test]
    public void GetAvailableCrimes_ShouldUnlockUmmKarimNetworkErrand_WhenTrustAndImbabaRepAreHigh()
    {
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Market);
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.FixerUmmKarim, 12, 1);
        relationships.SetFactionStanding(FactionId.ImbabaCrew, 15);

        var crimes = CrimeRegistry.GetAvailableCrimes(location, relationships);

        crimes.Select(static attempt => attempt.Type).Should().Contain(CrimeType.NetworkErrand);
    }

    [Test]
    public void GetAvailableCrimes_ShouldUseDokkiStanding_WhenFilteringStreetRep()
    {
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Square);
        var relationships = new RelationshipState();
        relationships.SetFactionStanding(FactionId.ImbabaCrew, 0);
        relationships.SetFactionStanding(FactionId.DokkiThugs, 10);

        var crimes = CrimeRegistry.GetAvailableCrimes(location, relationships);

        crimes.Select(static attempt => attempt.Type).Should().Contain(CrimeType.Robbery);
    }
}