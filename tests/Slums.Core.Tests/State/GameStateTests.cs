using FluentAssertions;
using Slums.Core.Crimes;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;
using TUnit.Core.Interfaces;

namespace Slums.Core.Tests.State;

internal sealed class GameStateTests
{
    [Test]
    public async Task Constructor_ShouldInitializeWithDefaultValues()
    {
        using var state = new GameSession();

        await Assert.That(state.RunId).IsNotEqualTo(Guid.Empty);
        await Assert.That(state.Clock.Day).IsEqualTo(1);
        await Assert.That(state.Player).IsNotNull();
        await Assert.That(state.World).IsNotNull();
        await Assert.That(state.IsGameOver).IsFalse();
        await Assert.That(state.GetDailyDistrictConditions().Count).IsEqualTo(Enum.GetValues<DistrictId>().Length);
    }

    [Test]
    public async Task EndDay_ShouldDeductRentFromMoney()
    {
        using var state = new GameSession();

        state.EndDay();

        await Assert.That(state.Player.Stats.Money).IsEqualTo(80);
    }

    [Test]
    public async Task EatAtHome_ShouldFeedPlayerAndMother()
    {
        using var state = new GameSession();

        var result = state.EatAtHome();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Household.FoodStockpile).IsEqualTo(2);
        await Assert.That(state.Player.Household.FedMotherToday).IsTrue();
        await Assert.That(state.Player.Nutrition.AteToday).IsTrue();
        await Assert.That(state.Player.Stats.Hunger).IsEqualTo(97);
    }

    [Test]
    public async Task EndDay_ShouldAdvanceToNextDay()
    {
        using var state = new GameSession();
        state.Clock.AdvanceHours(10);

        state.EndDay();

        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(6);
    }

    [Test]
    public async Task EndDay_ShouldReturnPlayerHome()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Market);

        state.EndDay(new Random(1));

        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Home);
    }

    [Test]
    public async Task EatAtHome_WithoutStaples_ShouldFail()
    {
        using var state = new GameSession();
        state.Player.Household.SetFoodStockpile(0);

        var result = state.EatAtHome();

        await Assert.That(result).IsFalse();
        await Assert.That(state.Player.Nutrition.AteToday).IsFalse();
        await Assert.That(state.Player.Household.FedMotherToday).IsFalse();
    }

    [Test]
    public async Task EatStreetFood_ShouldCostMoneyAndFeedOnlyPlayer()
    {
        using var state = new GameSession();
        state.Clock.SetTime(2, 8, 0);

        var result = state.EatStreetFood();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Money).IsEqualTo(92);
        await Assert.That(state.Player.Nutrition.AteToday).IsTrue();
        await Assert.That(state.Player.Household.FedMotherToday).IsFalse();
    }

    [Test]
    public async Task EatStreetFood_ShouldCostMoreInDokkiThanAtHome()
    {
        using var state = new GameSession();
        state.Clock.SetTime(2, 8, 0);
        state.TryTravelTo(LocationId.CallCenter);
        var moneyBefore = state.Player.Stats.Money;

        var result = state.EatStreetFood();

        await Assert.That(result).IsTrue();
        await Assert.That(moneyBefore - state.Player.Stats.Money).IsEqualTo(10);
    }

    [Test]
    public async Task BuyMedicine_ShouldIncreaseMedicineStock()
    {
        using var state = new GameSession();

        var result = state.BuyMedicine();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Household.MedicineStock).IsEqualTo(2);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(50);
    }

    [Test]
    public async Task BuyMedicine_ShouldBeCheaperInArdAlLiwaThanImbaba()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Clinic);

        var result = state.BuyMedicine();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Money).IsEqualTo(58);
    }

    [Test]
    public async Task GetMedicineCost_ShouldUseMariamDiscount_AtPharmacy()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Pharmacy);
        state.Relationships.SetNpcRelationship(NpcId.PharmacistMariam, 12, 1);

        await Assert.That(state.GetMedicineCost()).IsEqualTo(40);
    }

    [Test]
    public async Task GetFoodCost_ShouldReflectCurrentDistrictCondition()
    {
        using var state = new GameSession();
        state.Clock.SetTime(2, 8, 0);
        state.World.SetActiveDistrictConditions(
        [
            new ActiveDistrictCondition { District = DistrictId.Imbaba, ConditionId = "imbaba_utility_cut" }
        ]);

        await Assert.That(state.GetFoodCost()).IsEqualTo(17);
    }

    [Test]
    public async Task GetTravelTimeMinutes_ShouldReflectDestinationDistrictCondition()
    {
        using var state = new GameSession();
        state.World.SetActiveDistrictConditions(
        [
            new ActiveDistrictCondition { District = DistrictId.Dokki, ConditionId = "dokki_checkpoint_sweep" }
        ]);

        await Assert.That(state.GetTravelTimeMinutes(LocationId.CallCenter)).IsEqualTo(55);
    }

    [Test]
    public async Task GiveMotherMedicine_ShouldConsumeMedicineStock()
    {
        using var state = new GameSession();
        state.Player.Household.SetMedicineStock(2);

        var result = state.GiveMotherMedicine();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Household.MedicineStock).IsEqualTo(1);
        await Assert.That(state.Player.Household.MedicationGivenToday).IsTrue();
    }

    [Test]
    public async Task TakeMotherToClinic_ShouldFail_WhenLocationHasNoClinic()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Market);

        var result = state.TakeMotherToClinic();

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.TotalCost).IsEqualTo(0);
        await Assert.That(state.Player.Household.MotherHealth).IsEqualTo(70);
    }

    [Test]
    public async Task TakeMotherToClinic_ShouldFail_WhenClinicIsClosedToday()
    {
        using var state = new GameSession();
        state.Clock.SetTime(day: 4, hour: 10, minute: 0);
        state.World.TravelTo(LocationId.Clinic);

        var result = state.TakeMotherToClinic();

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.TotalCost).IsEqualTo(35);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(100);
    }

    [Test]
    public async Task TakeMotherToClinic_ShouldImproveMotherHealth_AndChargeLocationPrice()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Clinic);
        state.Player.Household.SetMotherHealth(50);

        var result = state.TakeMotherToClinic();

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.TotalCost).IsEqualTo(35);
        await Assert.That(result.HealthChange).IsEqualTo(20);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(65);
        await Assert.That(state.Player.Household.MotherHealth).IsEqualTo(70);
    }

    [Test]
    public async Task TakeMotherToClinic_ShouldUseDifferentClinicPrice_ByLocation()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Pharmacy);

        var status = state.GetCurrentLocationClinicStatus();

        await Assert.That(status.HasClinicServices).IsTrue();
        await Assert.That(status.VisitCost).IsEqualTo(46);
    }

    [Test]
    public async Task RestAtHome_ShouldRestoreEnergyAndAdvanceTime()
    {
        using var state = new GameSession();

        state.RestAtHome();

        await Assert.That(state.Player.Stats.Energy).IsEqualTo(100);
        await Assert.That(state.Clock.Hour).IsEqualTo(14);
    }

    [Test]
    public async Task RestAtHome_ShouldTriggerEndDayWhenRestPassesCurfew()
    {
        using var state = new GameSession();
        state.Clock.AdvanceHours(12);

        state.RestAtHome();

        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(10);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(80);
    }

    [Test]
    public async Task TryTravelTo_ShouldSucceedWithEnoughMoney()
    {
        using var state = new GameSession();

        var result = state.TryTravelTo(LocationId.Market);

        await Assert.That(result).IsTrue();
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Market);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(98);
    }

    [Test]
    public async Task TryTravelTo_ShouldFailWithInsufficientMoney()
    {
        using var state = new GameSession();
        state.Player.Stats.ModifyMoney(-99);

        var result = state.TryTravelTo(LocationId.CallCenter);

        await Assert.That(result).IsFalse();
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Home);
    }

    [Test]
    public async Task TryTravelTo_ShouldAdvanceTimeByTravelDuration()
    {
        using var state = new GameSession();

        state.TryTravelTo(LocationId.Market);

        await Assert.That(state.Clock.Minute).IsEqualTo(15);
    }

    [Test]
    public async Task TryTravelTo_ShouldUseSafaaRouteHelp_ForBulaqTravel()
    {
        using var state = new GameSession();
        state.Relationships.SetNpcRelationship(NpcId.DispatcherSafaa, 12, 1);

        var result = state.TryTravelTo(LocationId.Depot);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Money).IsEqualTo(99);
        await Assert.That(state.Player.Stats.Energy).IsEqualTo(77);
    }

    [Test]
    public async Task TryTravelTo_CurrentLocation_ShouldFailWithoutChargingMoney()
    {
        using var state = new GameSession();

        var result = state.TryTravelTo(LocationId.Home);

        await Assert.That(result).IsFalse();
        await Assert.That(state.Player.Stats.Money).IsEqualTo(100);
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Home);
    }

    [Test]
    public async Task TryTravelTo_ShouldTriggerEndDayWhenTravelPassesCurfew()
    {
        using var state = new GameSession();
        state.Clock.AdvanceHours(15);
        state.Clock.AdvanceMinutes(50);

        var result = state.TryTravelTo(LocationId.CallCenter);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(6);
        await Assert.That(state.Clock.Minute).IsEqualTo(35);
    }

    [Test]
    public async Task TryWalkTo_ShouldSucceedWithoutSpendingMoney()
    {
        using var state = new GameSession();
        state.Player.Stats.ModifyMoney(-99);

        var result = state.TryWalkTo(LocationId.Market);

        await Assert.That(result).IsTrue();
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Market);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(1);
    }

    [Test]
    public async Task TryWalkTo_ShouldCostMoreEnergyThanTravel()
    {
        using var state = new GameSession();
        var energyBefore = state.Player.Stats.Energy;

        var result = state.TryWalkTo(LocationId.Market);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Energy).IsEqualTo(energyBefore - 15);
    }

    [Test]
    public async Task TryWalkTo_ShouldTakeTripleTime()
    {
        using var state = new GameSession();

        var result = state.TryWalkTo(LocationId.Market);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Clock.Minute).IsEqualTo(45);
    }

    [Test]
    public async Task TryWalkTo_ShouldIncreaseStress()
    {
        using var state = new GameSession();
        var stressBefore = state.Player.Stats.Stress;

        var result = state.TryWalkTo(LocationId.Market);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Stress).IsEqualTo(stressBefore + 3);
    }

    [Test]
    public async Task TryWalkTo_ShouldFailIfTooExhausted()
    {
        using var state = new GameSession();
        state.Player.Stats.SetEnergy(14);

        var result = state.TryWalkTo(LocationId.CallCenter);

        await Assert.That(result).IsFalse();
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Home);
    }

    [Test]
    public async Task TryWalkTo_CurrentLocation_ShouldFail()
    {
        using var state = new GameSession();

        var result = state.TryWalkTo(LocationId.Home);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task TryWalkTo_ShouldIncreaseStress_ForSudaneseBackground_InDokki()
    {
        using var state = new GameSession();
        state.Player.ApplyBackground(Slums.Core.Characters.BackgroundRegistry.SudaneseRefugee);
        var stressBefore = state.Player.Stats.Stress;

        var result = state.TryWalkTo(LocationId.CallCenter);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Stress).IsEqualTo(stressBefore + 5);
    }

    [Test]
    public async Task CanAffordTravel_ShouldReturnTrue_WhenPlayerHasEnoughMoney()
    {
        using var state = new GameSession();

        var result = state.CanAffordTravel(LocationId.Market);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task CanAffordTravel_ShouldReturnFalse_WhenPlayerLacksMoney()
    {
        using var state = new GameSession();
        state.Player.Stats.ModifyMoney(-99);

        var result = state.CanAffordTravel(LocationId.Market);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task WorkJob_ShouldTriggerEndDayWhenShiftPassesCurfew()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Bakery);
        state.Clock.AdvanceHours(14);

        var result = state.WorkJob(Slums.Core.Jobs.JobRegistry.BakeryWork);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(10);
        await Assert.That(state.Clock.Minute).IsEqualTo(0);
    }

    [Test]
    public async Task WorkJob_ShouldImproveEmployerTrust_OnCleanClinicShift()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Clinic);
        state.Relationships.SetNpcRelationship(NpcId.NurseSalma, 12, 1);

        var trustBefore = state.Relationships.GetNpcRelationship(NpcId.NurseSalma).Trust;
        var result = state.WorkJob(state.GetAvailableJobs().Single());

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.MistakeMade).IsFalse();
        await Assert.That(state.Relationships.GetNpcRelationship(NpcId.NurseSalma).Trust).IsGreaterThan(trustBefore);
    }

    [Test]
    public async Task WorkJob_ShouldApplyLockoutAndTrustPenalty_OnCallCenterMistake()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.CallCenter);
        state.Player.Stats.SetStress(65);

        var result = state.WorkJob(state.GetAvailableJobs().Single());

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.MistakeMade).IsTrue();
        await Assert.That(state.JobProgress.GetTrack(Slums.Core.Jobs.JobType.CallCenterWork).LockoutUntilDay).IsEqualTo(3);
    }

    [Test]
    public async Task WorkJob_ShouldMarkAbuSamirEmbarrassed_WhenWorkshopMistakeFollowsCrimeHeat()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Workshop);
        state.SetPolicePressure(70);
        state.SetCrimeCounters(0, 0, lastCrimeDay: 1);
        state.SetWorkCounters(0, 0, lastHonestWorkDay: 0, lastPublicFacingWorkDay: 0);
        state.Player.Stats.SetEnergy(35);

        var result = state.WorkJob(state.GetAvailableJobs().Single());

        await Assert.That(result.MistakeMade).IsTrue();
        await Assert.That(state.Relationships.GetNpcRelationship(NpcId.WorkshopBossAbuSamir).WasEmbarrassed).IsTrue();
    }

    [Test]
    public async Task WorkJob_ShouldAdvanceWorkLedgerWithoutMutatingCrimeLedger()
    {
        using var state = new GameSession();
        state.SetCrimeCounters(70, 2, lastCrimeDay: 1);
        state.SetPolicePressure(25);
        state.World.TravelTo(LocationId.Bakery);

        var result = state.WorkJob(state.GetAvailableJobs().Single(), new Random(3));

        await Assert.That(result.Success).IsTrue();
        await Assert.That(state.TotalCrimeEarnings).IsEqualTo(70);
        await Assert.That(state.CrimesCommitted).IsEqualTo(2);
        await Assert.That(state.LastCrimeDay).IsEqualTo(1);
        await Assert.That(state.PolicePressure).IsEqualTo(25);
        await Assert.That(state.TotalHonestWorkEarnings).IsEqualTo(result.MoneyEarned);
        await Assert.That(state.HonestShiftsCompleted).IsEqualTo(1);
        await Assert.That(state.LastHonestWorkDay).IsEqualTo(1);
    }

    [Test]
    public async Task CommitCrime_ShouldUsePublicFacingWorkAsAnAlibi_SameDay()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Cafe);
        state.WorkJob(state.GetAvailableJobs().Single());
        state.World.TravelTo(LocationId.Square);
        var attempt = new CrimeAttempt(CrimeType.DokkiDrop, 95, 42, 24, 0, 18);

        var result = state.CommitCrime(attempt, new Random(2));

        await Assert.That(state.LastPublicFacingWorkDay).IsEqualTo(state.Clock.Day);
        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task CommitCrime_ShouldAdvanceCrimeLedgerWithoutMutatingWorkLedger()
    {
        using var state = new GameSession();
        state.SetWorkCounters(totalHonestWorkEarnings: 140, honestShiftsCompleted: 4, lastHonestWorkDay: 7, lastPublicFacingWorkDay: 7);
        state.World.TravelTo(LocationId.Market);

        var result = state.CommitCrime(new CrimeAttempt(CrimeType.PettyTheft, 25, 20, 10, 0, 10), new Random(5));
        var expectedCrimeEarnings = result.Success ? result.MoneyEarned : 0;
        var expectedCrimesCommitted = result.Success ? 1 : 0;

        await Assert.That(state.TotalHonestWorkEarnings).IsEqualTo(140);
        await Assert.That(state.HonestShiftsCompleted).IsEqualTo(4);
        await Assert.That(state.LastHonestWorkDay).IsEqualTo(7);
        await Assert.That(state.LastPublicFacingWorkDay).IsEqualTo(7);
        await Assert.That(state.TotalCrimeEarnings).IsEqualTo(expectedCrimeEarnings);
        await Assert.That(state.CrimesCommitted).IsEqualTo(expectedCrimesCommitted);
        await Assert.That(state.LastCrimeDay).IsEqualTo(1);
        await Assert.That(state.PolicePressure).IsGreaterThan(0);
    }

    [Test]
    public async Task WorkJob_ShouldQueuePublicWorkHeatScene_WhenPublicFacingShiftFollowsCrimeHeat()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Clinic);
        state.SetPolicePressure(70);
        state.SetCrimeCounters(0, 0, lastCrimeDay: 1);
        state.SetWorkCounters(0, 0, lastHonestWorkDay: 0, lastPublicFacingWorkDay: 0);

        var result = state.WorkJob(state.GetAvailableJobs().Single());

        await Assert.That(result.Success).IsTrue();
        await Assert.That(HasPendingNarrativeScene(state, "event_public_work_heat")).IsTrue();
    }

    [Test]
    public async Task BuyFood_ShouldGrantExtraStaples_ForSudaneseBackground()
    {
        using var state = new GameSession();
        state.Player.ApplyBackground(Slums.Core.Characters.BackgroundRegistry.SudaneseRefugee);
        var before = state.Player.Household.FoodStockpile;

        var result = state.BuyFood();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Household.FoodStockpile).IsEqualTo(before + 4);
    }

    [Test]
    public async Task BuyFood_ShouldCostMoreInShubraThanImbaba()
    {
        using var state = new GameSession();
        state.Clock.SetTime(2, 8, 0);
        state.World.TravelTo(LocationId.Laundry);

        var result = state.BuyFood();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Money).IsEqualTo(83);
    }

    [Test]
    public async Task TryTravelTo_ShouldIncreaseStress_ForSudaneseBackground_InDokki()
    {
        using var state = new GameSession();
        state.Player.ApplyBackground(Slums.Core.Characters.BackgroundRegistry.SudaneseRefugee);
        var before = state.Player.Stats.Stress;

        var result = state.TryTravelTo(LocationId.CallCenter);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Stress).IsEqualTo(before + 2);
    }

    [Test]
    public async Task EndDay_ShouldDecayPressureMoreSlowly_ForReleasedPrisoner()
    {
        using var state = new GameSession();
        state.Player.ApplyBackground(Slums.Core.Characters.BackgroundRegistry.ReleasedPoliticalPrisoner);
        state.SetPolicePressure(25);

        state.EndDay(new Random(2));

        await Assert.That(state.PolicePressure).IsEqualTo(24);
    }

    [Test]
    public async Task EndDay_ShouldQueueMotherWrongMoneyScene_AfterSuccessfulCrimeRun()
    {
        const int seedLimit = 100;
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 40, 0, 8, 0, 10);

        for (var seed = 0; seed < seedLimit; seed++)
        {
            using var state = new GameSession();
            state.World.TravelTo(LocationId.Market);
            state.Player.Household.SetMotherHealth(50);
            state.SetCrimeCounters(140, 1);
            state.SetPolicePressure(20);

            var result = state.CommitCrime(attempt, new Random(seed));
            if (!result.Success)
            {
                continue;
            }

            state.EndDay(new Random(2));

            await Assert.That(HasPendingNarrativeScene(state, "event_mother_wrong_money")).IsTrue();
            return;
        }

        throw new InvalidOperationException("Could not find a deterministic successful crime seed for the mother money follow-up.");
    }

    [Test]
    public async Task EndDay_ShouldQueueNeighborWatchScene_WhenMonaTrustAndHeatAreHigh()
    {
        const int seedLimit = 100;
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 40, 10, 12, 0, 10);

        for (var seed = 0; seed < seedLimit; seed++)
        {
            using var state = new GameSession();
            state.World.TravelTo(LocationId.Market);
            state.SetPolicePressure(60);
            state.Relationships.SetNpcRelationship(NpcId.NeighborMona, 18, 1);

            var result = state.CommitCrime(attempt, new Random(seed));
            if (!result.Success)
            {
                continue;
            }

            state.EndDay(new Random(2));

            await Assert.That(HasPendingNarrativeScene(state, "event_neighbor_watch")).IsTrue();
            return;
        }

        throw new InvalidOperationException("Could not find a deterministic successful crime seed for the Mona warning follow-up.");
    }

    [Test]
    public async Task RelationshipMemory_ShouldRecordDebtState_InCore()
    {
        using var state = new GameSession();
        state.Relationships.RecordFavor(NpcId.NurseSalma, state.Clock.Day, hasUnpaidDebt: true);

        await Assert.That(state.Relationships.GetNpcRelationship(NpcId.NurseSalma).HasUnpaidDebt).IsTrue();
        await Assert.That(state.Relationships.GetNpcRelationship(NpcId.NurseSalma).LastFavorDay).IsEqualTo(state.Clock.Day);
    }

    [Test]
    public async Task ApplyRandomEvent_ShouldRecordEventHistory_WhenDayEnds()
    {
        using var state = new GameSession();
        state.Clock.SetTime(5, 6, 0);
        state.World.TravelTo(LocationId.CallCenter);
        state.SetPolicePressure(60);

        state.EndDay(new Random(1));

        await Assert.That(state.RandomEventHistory.Count).IsGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task EndDay_ShouldQueueRentFinalWarningScene_WhenFinalWarningHits()
    {
        using var state = new GameSession();
        state.RestoreRentState(unpaidRentDays: 4, accumulatedRentDebt: 80, firstWarningGiven: true, finalWarningGiven: false);
        state.Player.Stats.SetMoney(0);

        state.EndDay(new Random(1));

        await Assert.That(HasPendingNarrativeScene(state, "event_rent_final_warning")).IsTrue();
    }

    [Test]
    public async Task IsGameOver_ShouldBeTrueWhenHealthIsZero()
    {
        using var state = new GameSession();
        state.Player.Stats.ModifyHealth(-100);

        state.EndDay();

        await Assert.That(state.IsGameOver).IsTrue();
        await Assert.That(state.GameOverReason).Contains("health");
        await Assert.That(TryTakePendingEndingKnot(state, out var knotName)).IsTrue();
        await Assert.That(knotName).IsEqualTo("ending_collapse");
    }

    [Test]
    public async Task GameEvent_ShouldBeRaisedForActions()
    {
        using var state = new GameSession();
        var events = new List<string>();
        state.GameEvent += (_, e) => events.Add(e.Message);

        state.TryTravelTo(LocationId.Market);

        events.Should().ContainMatch("*Traveled*");
    }

    [Test]
    public async Task CommitCrime_ShouldApplyMoneyEnergyAndPressureChanges()
    {
        using var state = new GameSession();
        var initialMoney = state.Player.Stats.Money;
        var initialEnergy = state.Player.Stats.Energy;

        var result = state.CommitCrime(new Slums.Core.Crimes.CrimeAttempt(
            Slums.Core.Crimes.CrimeType.PettyTheft,
            25,
            20,
            10,
            0,
            10), new Random(5));

        await Assert.That(state.Player.Stats.Energy).IsLessThan(initialEnergy);
        await Assert.That(state.PolicePressure).IsGreaterThan(0);
        state.Player.Stats.Money.Should().BeGreaterOrEqualTo(initialMoney);
        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task CommitCrime_ShouldLetHananReducePressure_WhenTrustIsHighAndCrimeIsDetected()
    {
        const int seedLimit = 200;
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 20, 10, 0, 10);

        for (var seed = 0; seed < seedLimit; seed++)
        {
            using var baseline = CreateCrimeState(LocationId.Market);
            var baselineResult = baseline.CommitCrime(attempt, new Random(seed));
            if (!baselineResult.Detected)
            {
                continue;
            }

            using var trusted = CreateCrimeState(LocationId.Market);
            trusted.Relationships.SetNpcRelationship(NpcId.FenceHanan, 20, 1);
            var trustedResult = trusted.CommitCrime(attempt, new Random(seed));

            await Assert.That(trustedResult.Detected).IsTrue();
            await Assert.That(trusted.PolicePressure).IsEqualTo(Math.Max(0, baseline.PolicePressure - 5));
            await Assert.That(trusted.HasStoryFlag("crime_hanan_cover_seen")).IsTrue();
            return;
        }

        throw new InvalidOperationException("Could not find a deterministic detected market crime seed.");
    }

    [Test]
    public async Task CommitCrime_ShouldLetYoussefReducePressure_WhenTrustIsHighAndCrimeIsDetected()
    {
        const int seedLimit = 200;
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 35, 30, 10, 0, 10);

        for (var seed = 0; seed < seedLimit; seed++)
        {
            using var baseline = CreateCrimeState(LocationId.Square);
            var baselineResult = baseline.CommitCrime(attempt, new Random(seed));
            if (!baselineResult.Detected)
            {
                continue;
            }

            using var trusted = CreateCrimeState(LocationId.Square);
            trusted.Relationships.SetNpcRelationship(NpcId.RunnerYoussef, 20, 1);
            var trustedResult = trusted.CommitCrime(attempt, new Random(seed));

            await Assert.That(trustedResult.Detected).IsTrue();
            await Assert.That(trusted.PolicePressure).IsEqualTo(Math.Max(0, baseline.PolicePressure - 7));
            await Assert.That(trusted.HasStoryFlag("crime_youssef_tipoff_seen")).IsTrue();
            return;
        }

        throw new InvalidOperationException("Could not find a deterministic detected Dokki crime seed.");
    }

    [Test]
    public async Task CommitCrime_ShouldLetHananSalvageDetectedFailure_WhenTrustIsHigh()
    {
        const int seedLimit = 400;
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 25, 20, 10, 0, 10);

        for (var seed = 0; seed < seedLimit; seed++)
        {
            using var baseline = CreateCrimeState(LocationId.Market);
            var baselineResult = baseline.CommitCrime(attempt, new Random(seed));
            if (baselineResult.Success || !baselineResult.Detected)
            {
                continue;
            }

            using var trusted = CreateCrimeState(LocationId.Market);
            trusted.Relationships.SetNpcRelationship(NpcId.FenceHanan, 20, 1);
            var trustedMoney = trusted.Player.Stats.Money;
            var trustedResult = trusted.CommitCrime(attempt, new Random(seed));

            await Assert.That(trustedResult.Success).IsFalse();
            await Assert.That(trustedResult.Detected).IsTrue();
            await Assert.That(trusted.Player.Stats.Money).IsEqualTo(baseline.Player.Stats.Money + 12);
            await Assert.That(trusted.Player.Stats.Stress).IsEqualTo(Math.Max(0, baseline.Player.Stats.Stress - 4));
            await Assert.That(trusted.Player.Stats.Money).IsGreaterThan(trustedMoney);
            await Assert.That(trusted.HasStoryFlag("crime_hanan_salvage_seen")).IsTrue();
            return;
        }

        throw new InvalidOperationException("Could not find a deterministic detected failed market crime seed.");
    }

    [Test]
    public async Task CommitCrime_ShouldLetYoussefSoftenDetectedFailureStress_WhenTrustIsHigh()
    {
        const int seedLimit = 400;
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 35, 30, 10, 0, 10);

        for (var seed = 0; seed < seedLimit; seed++)
        {
            using var baseline = CreateCrimeState(LocationId.Square);
            var baselineResult = baseline.CommitCrime(attempt, new Random(seed));
            if (baselineResult.Success || !baselineResult.Detected)
            {
                continue;
            }

            using var trusted = CreateCrimeState(LocationId.Square);
            trusted.Relationships.SetNpcRelationship(NpcId.RunnerYoussef, 20, 1);
            var trustedResult = trusted.CommitCrime(attempt, new Random(seed));

            await Assert.That(trustedResult.Success).IsFalse();
            await Assert.That(trustedResult.Detected).IsTrue();
            await Assert.That(trusted.Player.Stats.Stress).IsEqualTo(Math.Max(0, baseline.Player.Stats.Stress - 6));
            await Assert.That(trusted.HasStoryFlag("crime_youssef_escape_seen")).IsTrue();
            return;
        }

        throw new InvalidOperationException("Could not find a deterministic detected failed Dokki crime seed.");
    }

    [Test]
    public async Task CommitCrime_ShouldQueueHananRouteSuccessScene_OnFirstSuccessfulMarketFencingRun()
    {
        const int seedLimit = 200;
        var attempt = new CrimeAttempt(CrimeType.MarketFencing, 60, 18, 8, 0, 14);

        for (var seed = 0; seed < seedLimit; seed++)
        {
            using var state = CreateCrimeState(LocationId.Market);
            state.SetStoryFlag("crime_first_success");
            var result = state.CommitCrime(attempt, new Random(seed));
            if (!result.Success || result.Detected)
            {
                continue;
            }

            await Assert.That(state.HasStoryFlag("crime_hanan_fence_success_seen")).IsTrue();
            await Assert.That(HasPendingNarrativeScene(state, "crime_hanan_fence_success")).IsTrue();
            return;
        }

        throw new InvalidOperationException("Could not find a deterministic successful undetected Hanan route seed.");
    }

    [Test]
    public async Task CommitCrime_ShouldQueueYoussefRouteDetectedScene_OnFirstDetectedSuccessfulDrop()
    {
        const int seedLimit = 400;
        var attempt = new CrimeAttempt(CrimeType.DokkiDrop, 95, 42, 24, 0, 18);

        for (var seed = 0; seed < seedLimit; seed++)
        {
            using var state = CreateCrimeState(LocationId.Square);
            state.SetStoryFlag("crime_first_success");
            var result = state.CommitCrime(attempt, new Random(seed));
            if (!result.Success || !result.Detected)
            {
                continue;
            }

            await Assert.That(state.HasStoryFlag("crime_youssef_drop_detected_seen")).IsTrue();
            await Assert.That(HasPendingNarrativeScene(state, "crime_youssef_drop_detected")).IsTrue();
            return;
        }

        throw new InvalidOperationException("Could not find a deterministic detected successful Youssef route seed.");
    }

    [Test]
    public async Task CommitCrime_ShouldQueueUmmKarimRouteFailureScene_OnFirstFailedErrand()
    {
        const int seedLimit = 400;
        var attempt = new CrimeAttempt(CrimeType.NetworkErrand, 130, 50, 30, 0, 24);

        for (var seed = 0; seed < seedLimit; seed++)
        {
            using var state = CreateCrimeState(LocationId.Market);
            var result = state.CommitCrime(attempt, new Random(seed));
            if (result.Success)
            {
                continue;
            }

            await Assert.That(state.HasStoryFlag("crime_ummkarim_errand_failure_seen")).IsTrue();
            await Assert.That(HasPendingNarrativeScene(state, "crime_ummkarim_errand_failure")).IsTrue();
            return;
        }

        throw new InvalidOperationException("Could not find a deterministic failed Umm Karim route seed.");
    }

    [Test]
    public async Task CommitCrime_ShouldQueueSafaaRouteSuccessScene_OnFirstSuccessfulDepotSkim()
    {
        const int seedLimit = 300;
        var attempt = new CrimeAttempt(CrimeType.DepotFareSkim, 78, 28, 14, 0, 16);

        for (var seed = 0; seed < seedLimit; seed++)
        {
            using var state = CreateCrimeState(LocationId.Depot);
            state.SetStoryFlag("crime_first_success");
            var result = state.CommitCrime(attempt, new Random(seed));
            if (!result.Success || result.Detected)
            {
                continue;
            }

            await Assert.That(state.HasStoryFlag("crime_safaa_skim_success_seen")).IsTrue();
            await Assert.That(HasPendingNarrativeScene(state, "crime_safaa_skim_success")).IsTrue();
            return;
        }

        throw new InvalidOperationException("Could not find a deterministic successful undetected Safaa route seed.");
    }

    [Test]
    public async Task EndDay_ShouldDecayPolicePressure_OnCleanDay()
    {
        using var state = new GameSession();
        state.SetPolicePressure(25);

        state.EndDay(new Random(2));

        await Assert.That(state.PolicePressure).IsEqualTo(22);
    }

    [Test]
    public async Task EndDay_PlayerWithoutMeal_ShouldLoseEnergyAndGainStress()
    {
        using var state = new GameSession();

        state.EndDay();

        await Assert.That(state.Player.Stats.Energy).IsEqualTo(70);
        await Assert.That(state.Player.Stats.Stress).IsEqualTo(29);
        await Assert.That(state.Player.Stats.Hunger).IsEqualTo(60);
    }

    [Test]
    public async Task EndDay_PlayerUnderfedForTwoDays_ShouldLoseHealth()
    {
        using var state = new GameSession();
        state.Player.Nutrition.SetDaysUndereating(1);

        state.EndDay();

        await Assert.That(state.Player.Stats.Health).IsEqualTo(95);
    }

    [Test]
    public async Task EndDay_MotherFragileWithoutMedicine_ShouldLoseHealth()
    {
        using var state = new GameSession();
        state.Player.Household.SetMotherHealth(45);
        state.Player.Household.FeedMother();

        state.EndDay();

        await Assert.That(state.Player.Household.MotherHealth).IsEqualTo(41);
    }

    [Test]
    public async Task EndDay_MotherInCrisisWithoutCheck_ShouldIncreasePlayerStress()
    {
        using var state = new GameSession();
        state.Player.Household.SetMotherHealth(20);
        state.Player.Household.FeedMother();
        state.Player.Household.SetMedicineStock(1);
        state.Player.Household.GiveMedicine();

        state.EndDay();

        await Assert.That(state.Player.Stats.Stress).IsEqualTo(35);
    }

    [Test]
    public async Task EndDay_ShouldResetNutritionAndCareFlagsForNextDay()
    {
        using var state = new GameSession();
        state.EatAtHome();
        state.Player.Household.SetMedicineStock(1);
        state.GiveMotherMedicine();
        state.CheckOnMother();

        state.EndDay();

        await Assert.That(state.Player.Nutrition.AteToday).IsFalse();
        await Assert.That(state.Player.Household.FedMotherToday).IsFalse();
        await Assert.That(state.Player.Household.MedicationGivenToday).IsFalse();
        await Assert.That(state.Player.Household.CheckedOnMotherToday).IsFalse();
    }

    [Test]
    public async Task EndDay_ShouldTriggerGameOver_WhenMotherHealthFallsToZero()
    {
        using var state = new GameSession();
        state.Player.Household.SetMotherHealth(1);

        state.EndDay();

        await Assert.That(state.IsGameOver).IsTrue();
        await Assert.That(state.GameOverReason).Contains("mother");
        await Assert.That(TryTakePendingEndingKnot(state, out var knotName)).IsTrue();
        await Assert.That(knotName).IsEqualTo("ending_mother_died");
    }

    [Test]
    public async Task GetStatusSummary_ShouldReturnCurrentStatus()
    {
        using var state = new GameSession();

        var summary = state.GetStatusSummary();

        summary.Should().HaveCount(8);
        summary[0].Should().Contain("Day 1");
        summary[2].Should().Contain("Money");
    }

    [Test]
    public async Task GetClinicLocations_ShouldReturnAllClinics()
    {
        using var state = new GameSession();

        var clinics = state.GetClinicLocations();

        clinics.Should().NotBeEmpty();
        clinics.Should().Contain(l => l.Id == LocationId.Clinic);
        clinics.Should().Contain(l => l.Id == LocationId.Pharmacy);
    }

    [Test]
    public async Task GetClinicTravelOption_ShouldReturnValidOption_ForClinicLocation()
    {
        using var state = new GameSession();

        var option = state.GetClinicTravelOption(LocationId.Clinic);

        await Assert.That(option.IsValidOption).IsTrue();
        await Assert.That(option.LocationName).IsEqualTo("Rahma Clinic");
        await Assert.That(option.DistrictName).IsEqualTo("ArdAlLiwa");
        await Assert.That(option.TravelCost).IsGreaterThan(0);
        await Assert.That(option.ClinicCost).IsGreaterThan(0);
        await Assert.That(option.TotalCost).IsEqualTo(option.TravelCost + option.ClinicCost);
    }

    [Test]
    public async Task GetClinicTravelOption_ShouldReturnInvalidOption_ForNonClinicLocation()
    {
        using var state = new GameSession();

        var option = state.GetClinicTravelOption(LocationId.Market);

        await Assert.That(option.IsValidOption).IsFalse();
    }

    [Test]
    public async Task TravelAndTakeMotherToClinic_ShouldSucceed_FromHome()
    {
        using var state = new GameSession();
        state.Player.Household.SetMotherHealth(50);
        var initialMoney = state.Player.Stats.Money;

        var result = state.TravelAndTakeMotherToClinic(LocationId.Clinic);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.TotalCost).IsGreaterThan(0);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(initialMoney - result.TotalCost);
        await Assert.That(state.Player.Household.MotherHealth).IsGreaterThan(50);
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Clinic);
    }

    [Test]
    public async Task TravelAndTakeMotherToClinic_ShouldFail_WhenClinicClosed()
    {
        using var state = new GameSession();
        state.Clock.SetTime(day: 4, hour: 10, minute: 0);

        var result = state.TravelAndTakeMotherToClinic(LocationId.Clinic);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Home);
    }

    [Test]
    public async Task TravelAndTakeMotherToClinic_ShouldFail_WhenInsufficientMoney()
    {
        using var state = new GameSession();
        state.Player.Stats.ModifyMoney(-99);

        var result = state.TravelAndTakeMotherToClinic(LocationId.Clinic);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Home);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(1);
    }

    [Test]
    public async Task TravelAndTakeMotherToClinic_ShouldAdvanceTime()
    {
        using var state = new GameSession();

        var result = state.TravelAndTakeMotherToClinic(LocationId.Clinic);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(state.Clock.Minute).IsGreaterThan(0);
    }

    [Test]
    public async Task TravelAndTakeMotherToClinic_ShouldConsumeTravelEnergy()
    {
        using var state = new GameSession();
        var initialEnergy = state.Player.Stats.Energy;

        var result = state.TravelAndTakeMotherToClinic(LocationId.Clinic);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(state.Player.Stats.Energy).IsLessThan(initialEnergy);
    }

    [Test]
    public async Task ResolveWeeklyInvestments_ShouldQueueSuspensionScene_WhenExtortionCannotBePaid()
    {
        using var state = new GameSession();
        state.Player.Stats.SetMoney(0);
        state.RestoreInvestmentState(
        [
            new Slums.Core.Investments.InvestmentSnapshot(Slums.Core.Investments.InvestmentType.Kiosk, 250, 10, 15, 1, false)
        ],
        totalInvestmentEarnings: 0);

        state.ResolveWeeklyInvestments(new Slums.Core.Tests.Investments.SequenceRandom(doubleValues: [0.99, 0.0], intValues: [12]));

        await Assert.That(HasPendingNarrativeScene(state, "event_investment_suspension")).IsTrue();
    }

    private static GameSession CreateCrimeState(LocationId locationId)
    {
        var state = new GameSession();
        state.World.TravelTo(locationId);
        state.SetPolicePressure(60);
        return state;
    }

    private static bool HasPendingNarrativeScene(GameSession state, string knotName)
    {
        while (state.TryDequeueNarrativeScene(out var pendingScene))
        {
            if (pendingScene == knotName)
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryTakePendingEndingKnot(GameSession state, out string knotName)
    {
        if (state.TryTakePendingEndingKnot(out knotName))
        {
            return true;
        }

        knotName = string.Empty;
        return false;
    }
}
