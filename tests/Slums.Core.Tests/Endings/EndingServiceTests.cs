using Slums.Core.Characters;
using Slums.Core.Economy;
using Slums.Core.Endings;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Endings;

internal sealed class EndingServiceTests
{
    [Test]
    public async Task CheckEndings_ShouldReturnStabilityHonestWork_WhenCriteriaMet()
    {
        var state = new GameSession();
        state.Player.Stats.SetMoney(250);
        state.SetDaysSurvived(30);
        state.SetPolicePressure(10);
        state.RestoreWorkState(totalHonestWorkEarnings: 400, honestShiftsCompleted: 15, lastHonestWorkDay: 30, lastPublicFacingWorkDay: 30);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.StabilityHonestWork);
    }

    [Test]
    public async Task CheckEndings_ShouldReturnNull_WhenCriteriaAreNotMet()
    {
        var state = new GameSession();

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsNull();
    }

    [Test]
    public async Task CheckEndings_ShouldReturnArrested_WhenPolicePressureHitsMaximum()
    {
        var state = new GameSession();
        state.SetPolicePressure(100);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.Arrested);
    }

    [Test]
    public async Task CheckEndings_ShouldReturnNetworkShelter_WhenCommunityTrustIsHigh()
    {
        var state = new GameSession();
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
    public async Task CheckEndings_ShouldReturnNetworkShelter_WhenCommunityAdaptationBuildsAnOrganizedRoute()
    {
        var state = new GameSession();
        state.SetDaysSurvived(30);
        state.Player.Stats.SetMoney(120);
        state.Player.Skills.SetLevel(Slums.Core.Skills.SkillId.CommunityOrganizing, 4);
        state.EventAttendance.TotalAttended = 2;
        state.CommunityAdaptation.RecordSuccessfulAction(3);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.NetworkShelter);
    }

    [Test]
    public async Task CheckEndings_ShouldReturnStabilityHonestWork_WhenCrimeStopsAndWorkCarriesYou()
    {
        var state = new GameSession();
        state.SetDaysSurvived(30);
        state.SetPolicePressure(30);
        state.SetCrimeCounters(300, 5);
        state.SetCrimeCounters(300, 5, lastCrimeDay: 25);
        state.RestoreWorkState(totalHonestWorkEarnings: 220, honestShiftsCompleted: 6, lastHonestWorkDay: 30, lastPublicFacingWorkDay: 30);
        state.Clock.SetTime(30, 6, 0);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.StabilityHonestWork);
    }

    [Test]
    public async Task CheckEndings_ShouldReturnArrested_WhenCrimeAndPressureStayHigh()
    {
        var state = new GameSession();
        state.SetDaysSurvived(30);
        state.SetCrimeCounters(500, 7);
        state.SetPolicePressure(90);
        state.Player.Stats.SetStress(75);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.Arrested);
    }

    [Test]
    public async Task CheckEndings_ShouldReturnEviction_WhenUnpaidRentDaysReachesThreshold()
    {
        var state = new GameSession();
        state.RestoreRentState(unpaidRentDays: 7, accumulatedRentDebt: 140, firstWarningGiven: true, finalWarningGiven: true);

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.Eviction);
    }

    [Test]
    public async Task GetInkKnot_ShouldUseBackgroundSpecificVariant_ForStability()
    {
        var stabilityState = new GameSession();
        stabilityState.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);

        await Assert.That(EndingService.GetInkKnot(stabilityState, EndingId.StabilityHonestWork)).IsEqualTo("ending_stability_sudanese");
    }

    [Test]
    public async Task GetInkKnot_ShouldUseBackgroundSpecificVariant_ForLuxorEnding()
    {
        var state = new GameSession();
        state.Player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);

        await Assert.That(EndingService.GetInkKnot(state, EndingId.QuitTheLuxorDream)).IsEqualTo("ending_luxor_medical");
    }

    [Test]
    public async Task GetInkKnot_ShouldUseStrongestSupportContact_ForNetworkShelter()
    {
        var state = new GameSession();
        state.Relationships.SetNpcRelationship(Slums.Core.Relationships.NpcId.NeighborMona, 20, 1);
        state.Relationships.SetNpcRelationship(Slums.Core.Relationships.NpcId.NurseSalma, 35, 1);
        state.Relationships.SetNpcRelationship(Slums.Core.Relationships.NpcId.CafeOwnerNadia, 22, 1);
        state.Relationships.SetNpcRelationship(Slums.Core.Relationships.NpcId.FenceHanan, 18, 1);

        await Assert.That(EndingService.GetInkKnot(state, EndingId.NetworkShelter)).IsEqualTo("ending_network_shelter_salma");
    }

    [Test]
    public async Task GetInkKnot_ShouldReturnNarrativeScene_ForEveryEnding()
    {
        var state = new GameSession();

        foreach (var endingId in Enum.GetValues<EndingId>())
        {
            var knotName = EndingService.GetInkKnot(state, endingId);

            await Assert.That(knotName).IsNotNull();
            await Assert.That(knotName.Length).IsGreaterThan(0);
        }
    }

    [Test]
    public async Task CheckEndings_ShouldReturnDestitution_WhenLoanSharkDebtIsCritical()
    {
        var state = new GameSession();
        state.SetDaysSurvived(15);
        state.Clock.SetTime(15, 8, 0);
        state.PlayerDebts.AddDebt(new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 300,
            DueDay = 1,
            OriginDay = 1,
            CreditorNpcId = (int)Slums.Core.Relationships.NpcId.FixerUmmKarim,
            InterestWeeklyBasisPoints = 500,
            CollectionState = DebtCollectionState.Critical
        });

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsEqualTo(EndingId.Destitution);
    }

    [Test]
    public async Task CheckEndings_ShouldNotReturnDestitution_WhenDebtIsCurrent()
    {
        var state = new GameSession();
        state.SetDaysSurvived(15);
        state.Clock.SetTime(15, 8, 0);
        state.PlayerDebts.AddDebt(new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 100,
            DueDay = 20,
            OriginDay = 10,
            CreditorNpcId = (int)Slums.Core.Relationships.NpcId.FixerUmmKarim,
            InterestWeeklyBasisPoints = 500,
            CollectionState = DebtCollectionState.Current
        });

        var ending = EndingService.CheckEndings(state);

        await Assert.That(ending).IsNull();
    }
}
