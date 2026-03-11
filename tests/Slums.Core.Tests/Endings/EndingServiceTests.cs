using Slums.Core.Endings;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Endings;

internal sealed class EndingServiceTests
{
    [Test]
    public async Task CheckEndings_ShouldReturnStabilityHonestWork_WhenCriteriaMet()
    {
        var state = new GameState();
        state.Player.Stats.SetMoney(250);
        state.SetDaysSurvived(30);
        state.SetPolicePressure(10);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.StabilityHonestWork);
    }

    [Test]
    public async Task CheckEndings_ShouldReturnNull_WhenCriteriaAreNotMet()
    {
        var state = new GameState();

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsNull();
    }

    [Test]
    public async Task CheckEndings_ShouldReturnArrested_WhenPolicePressureHitsMaximum()
    {
        var state = new GameState();
        state.SetPolicePressure(100);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.Arrested);
    }

    [Test]
    public async Task CheckEndings_ShouldReturnNetworkShelter_WhenCommunityTrustIsHigh()
    {
        var state = new GameState();
        state.SetDaysSurvived(30);
        state.Player.Stats.SetMoney(140);
        state.Relationships.SetNpcRelationship(Slums.Core.Relationships.NpcId.NeighborMona, 40, 1);
        state.Relationships.SetNpcRelationship(Slums.Core.Relationships.NpcId.NurseSalma, 40, 1);
        state.Relationships.SetNpcRelationship(Slums.Core.Relationships.NpcId.CafeOwnerNadia, 35, 1);
        state.Relationships.SetNpcRelationship(Slums.Core.Relationships.NpcId.FenceHanan, 35, 1);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.NetworkShelter);
    }

    [Test]
    public async Task CheckEndings_ShouldReturnLeavingCrime_WhenCrimeStopsAndWorkCarriesYou()
    {
        var state = new GameState();
        state.SetDaysSurvived(30);
        state.SetPolicePressure(30);
        state.SetCrimeCounters(300, 5);
        state.SetWorkCounters(totalHonestWorkEarnings: 220, honestShiftsCompleted: 6, lastCrimeDay: 25, lastHonestWorkDay: 30, lastPublicFacingWorkDay: 30);
        state.Clock.SetTime(30, 6, 0);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.LeavingCrime);
    }

    [Test]
    public async Task CheckEndings_ShouldReturnBuriedByHeat_WhenCrimeAndPressureStayHigh()
    {
        var state = new GameState();
        state.SetDaysSurvived(30);
        state.SetCrimeCounters(500, 7);
        state.SetPolicePressure(90);
        state.Player.Stats.SetStress(75);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.BuriedByHeat);
    }
}