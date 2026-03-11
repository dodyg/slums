using FluentAssertions;
using Slums.Core.Crimes;
using Slums.Core.Relationships;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Crimes;

internal sealed class CrimeRegistryTests
{
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