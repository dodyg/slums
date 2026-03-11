using FluentAssertions;
using Slums.Core.Crimes;
using Slums.Core.Relationships;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Crimes;

internal sealed class CrimeRegistryTests
{
    [Test]
    public void GetAvailableCrimes_ShouldImproveMarketCrimeTerms_WhenHananTrustIsHigh()
    {
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Market);
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.FenceHanan, 15, 1);
        relationships.SetFactionStanding(FactionId.ImbabaCrew, 10);

        var crimes = CrimeRegistry.GetAvailableCrimes(location, relationships);
        var pettyTheft = crimes.Single(static attempt => attempt.Type == CrimeType.PettyTheft);
        var hashishTrade = crimes.Single(static attempt => attempt.Type == CrimeType.HashishTrade);

        pettyTheft.BaseReward.Should().Be(35);
        pettyTheft.DetectionRisk.Should().Be(15);
        hashishTrade.BaseReward.Should().Be(60);
        hashishTrade.DetectionRisk.Should().Be(30);
    }

    [Test]
    public void GetAvailableCrimes_ShouldUnlockDokkiTrade_WhenYoussefTrustIsHigh()
    {
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Square);
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.RunnerYoussef, 12, 1);
        relationships.SetFactionStanding(FactionId.ImbabaCrew, 10);

        var crimes = CrimeRegistry.GetAvailableCrimes(location, relationships);

        crimes.Select(static attempt => attempt.Type).Should().Contain(CrimeType.HashishTrade);
    }

    [Test]
    public void GetAvailableCrimes_ShouldLowerDokkiRisk_WhenYoussefTrustIsVeryHigh()
    {
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Square);
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.RunnerYoussef, 20, 1);
        relationships.SetFactionStanding(FactionId.ImbabaCrew, 10);

        var crimes = CrimeRegistry.GetAvailableCrimes(location, relationships);
        var pettyTheft = crimes.Single(static attempt => attempt.Type == CrimeType.PettyTheft);
        var robbery = crimes.Single(static attempt => attempt.Type == CrimeType.Robbery);

        pettyTheft.DetectionRisk.Should().Be(25);
        robbery.DetectionRisk.Should().Be(60);
    }
}