using Slums.Core.Characters;
using Slums.Core.Endings;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Endings;

internal sealed class EndingServiceTests
{
    [Test]
    public async Task CheckEndings_ShouldReturnStabilityHonestWork_WhenCriteriaMet()
    {
        using var state = new GameSession();
        state.Player.Stats.SetMoney(250);
        state.SetDaysSurvived(30);
        state.SetPolicePressure(10);
        state.SetWorkCounters(totalHonestWorkEarnings: 400, honestShiftsCompleted: 15, lastHonestWorkDay: 30, lastPublicFacingWorkDay: 30);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.StabilityHonestWork);
    }

    [Test]
    public async Task CheckEndings_ShouldReturnNull_WhenCriteriaAreNotMet()
    {
        using var state = new GameSession();

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsNull();
    }

    [Test]
    public async Task CheckEndings_ShouldReturnArrested_WhenPolicePressureHitsMaximum()
    {
        using var state = new GameSession();
        state.SetPolicePressure(100);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.Arrested);
    }

    [Test]
    public async Task CheckEndings_ShouldReturnNetworkShelter_WhenCommunityTrustIsHigh()
    {
        using var state = new GameSession();
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
        using var state = new GameSession();
        state.SetDaysSurvived(30);
        state.SetPolicePressure(30);
        state.SetCrimeCounters(300, 5);
        state.SetCrimeCounters(300, 5, lastCrimeDay: 25);
        state.SetWorkCounters(totalHonestWorkEarnings: 220, honestShiftsCompleted: 6, lastHonestWorkDay: 30, lastPublicFacingWorkDay: 30);
        state.Clock.SetTime(30, 6, 0);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.LeavingCrime);
    }

    [Test]
    public async Task CheckEndings_ShouldReturnBuriedByHeat_WhenCrimeAndPressureStayHigh()
    {
        using var state = new GameSession();
        state.SetDaysSurvived(30);
        state.SetCrimeCounters(500, 7);
        state.SetPolicePressure(90);
        state.Player.Stats.SetStress(75);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.BuriedByHeat);
    }

    [Test]
    public async Task CheckEndings_ShouldReturnEviction_WhenUnpaidRentDaysReachesThreshold()
    {
        using var state = new GameSession();
        state.RestoreRentState(unpaidRentDays: 7, accumulatedRentDebt: 140, firstWarningGiven: true, finalWarningGiven: true);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.Eviction);
    }

    [Test]
    public async Task GetInkKnot_ShouldUseBackgroundSpecificVariant_ForStabilityAndLeavingCrime()
    {
        using var stabilityState = new GameSession();
        stabilityState.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);

        using var leavingCrimeState = new GameSession();
        leavingCrimeState.Player.ApplyBackground(BackgroundRegistry.ReleasedPoliticalPrisoner);

        await Assert.That(EndingService.GetInkKnot(stabilityState, EndingId.StabilityHonestWork)).IsEqualTo("ending_stability_sudanese");
        await Assert.That(EndingService.GetInkKnot(leavingCrimeState, EndingId.LeavingCrime)).IsEqualTo("ending_leaving_crime_prisoner");
    }

    [Test]
    public async Task GetInkKnot_ShouldUseBackgroundSpecificVariant_ForLuxorEnding()
    {
        using var state = new GameSession();
        state.Player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);

        await Assert.That(EndingService.GetInkKnot(state, EndingId.QuitTheLuxorDream)).IsEqualTo("ending_luxor_medical");
    }

    [Test]
    public async Task GetInkKnot_ShouldUseStrongestSupportContact_ForNetworkShelter()
    {
        using var state = new GameSession();
        state.Relationships.SetNpcRelationship(Slums.Core.Relationships.NpcId.NeighborMona, 20, 1);
        state.Relationships.SetNpcRelationship(Slums.Core.Relationships.NpcId.NurseSalma, 35, 1);
        state.Relationships.SetNpcRelationship(Slums.Core.Relationships.NpcId.CafeOwnerNadia, 22, 1);
        state.Relationships.SetNpcRelationship(Slums.Core.Relationships.NpcId.FenceHanan, 18, 1);

        await Assert.That(EndingService.GetInkKnot(state, EndingId.NetworkShelter)).IsEqualTo("ending_network_shelter_salma");
    }

    [Test]
    public async Task GetInkKnot_ShouldReturnNarrativeScene_ForEveryEnding()
    {
        using var state = new GameSession();

        foreach (var endingId in Enum.GetValues<EndingId>())
        {
            var knotName = EndingService.GetInkKnot(state, endingId);

            await Assert.That(knotName).IsNotNull();
            await Assert.That(knotName.Length).IsGreaterThan(0);
        }
    }
}
