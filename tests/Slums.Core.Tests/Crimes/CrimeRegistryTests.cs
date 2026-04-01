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
    public void GetCrimeOpportunityStatuses_ShouldShowTrustOrRepBlockReason_ForDepotFareSkim()
    {
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Depot);
        var relationships = new RelationshipState();

        var statuses = CrimeRegistry.GetCrimeOpportunityStatuses(location, relationships);

        var fareSkim = statuses.Single(static status => status.Attempt.Type == CrimeType.DepotFareSkim);
        fareSkim.IsAvailable.Should().BeFalse();
        fareSkim.BlockReason.Should().Contain("Safaa trust 10 or Imbaba standing 12");
    }

    [Test]
    public void GetAvailableCrimes_ShouldUnlockDepotFareSkim_WhenSafaaTrustIsHigh()
    {
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Depot);
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.DispatcherSafaa, 10, 1);

        var crimes = CrimeRegistry.GetAvailableCrimes(location, relationships);

        crimes.Select(static attempt => attempt.Type).Should().Contain(CrimeType.DepotFareSkim);
    }

    [Test]
    public void GetAvailableCrimes_ShouldUnlockShubraBundleLift_WhenImanTrustIsHigh()
    {
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Laundry);
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.LaundryOwnerIman, 10, 1);

        var crimes = CrimeRegistry.GetAvailableCrimes(location, relationships);

        crimes.Select(static attempt => attempt.Type).Should().Contain(CrimeType.ShubraBundleLift);
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

    [Test]
    public void GetAvailableCrimes_ShouldReturnBaseCrimes_ForArdAlLiwa()
    {
        var location = WorldState.AllLocations.First(static l => l.Id == LocationId.Workshop);
        var relationships = new RelationshipState();
        relationships.SetFactionStanding(FactionId.ExPrisonerNetwork, 20);

        var crimes = CrimeRegistry.GetAvailableCrimes(location, relationships);

        crimes.Select(c => c.Type).Should().Contain([CrimeType.PettyTheft, CrimeType.HashishTrade]);
    }

    [Test]
    public void GetAvailableCrimes_ShouldUnlockWorkshopContraband_WhenAbuSamirTrustAndExPrisonerRepAreHigh()
    {
        var location = WorldState.AllLocations.First(static l => l.Id == LocationId.Workshop);
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.WorkshopBossAbuSamir, 15, 1);
        relationships.SetFactionStanding(FactionId.ExPrisonerNetwork, 20);

        var crimes = CrimeRegistry.GetAvailableCrimes(location, relationships);

        crimes.Select(c => c.Type).Should().Contain(CrimeType.WorkshopContraband);
    }

    [Test]
    public void GetAvailableCrimes_ShouldNotUnlockWorkshopContraband_WhenOnlyTrustIsHigh()
    {
        var location = WorldState.AllLocations.First(static l => l.Id == LocationId.Workshop);
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.WorkshopBossAbuSamir, 15, 1);
        relationships.SetFactionStanding(FactionId.ExPrisonerNetwork, 5);

        var crimes = CrimeRegistry.GetAvailableCrimes(location, relationships);

        crimes.Select(c => c.Type).Should().NotContain(CrimeType.WorkshopContraband);
    }

    [Test]
    public void GetAvailableCrimes_ShouldUnlockBulaqProtection_WhenSafaaTrustIsHigh()
    {
        var location = WorldState.AllLocations.First(static l => l.Id == LocationId.Depot);
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.DispatcherSafaa, 15, 1);
        relationships.SetFactionStanding(FactionId.ImbabaCrew, 20);

        var crimes = CrimeRegistry.GetAvailableCrimes(location, relationships);

        crimes.Select(c => c.Type).Should().Contain(CrimeType.BulaqProtectionRacket);
    }

    [Test]
    public void GetCrimeOpportunityStatuses_ShouldShowBlockReason_ForWorkshopContraband()
    {
        var location = WorldState.AllLocations.First(static l => l.Id == LocationId.Workshop);
        var relationships = new RelationshipState();

        var statuses = CrimeRegistry.GetCrimeOpportunityStatuses(location, relationships);

        var contraband = statuses.FirstOrDefault(s => s.Attempt.Type == CrimeType.WorkshopContraband);
        contraband.Should().NotBeNull();
        contraband!.IsAvailable.Should().BeFalse();
    }

    [Test]
    public void GetCrimeOpportunityStatuses_ShouldShowBlockReason_ForBulaqProtection()
    {
        var location = WorldState.AllLocations.First(static l => l.Id == LocationId.Depot);
        var relationships = new RelationshipState();

        var statuses = CrimeRegistry.GetCrimeOpportunityStatuses(location, relationships);

        var protection = statuses.FirstOrDefault(s => s.Attempt.Type == CrimeType.BulaqProtectionRacket);
        protection.Should().NotBeNull();
        protection!.IsAvailable.Should().BeFalse();
    }
}