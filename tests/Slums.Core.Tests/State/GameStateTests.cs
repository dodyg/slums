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
        var state = new GameState();

        await Assert.That(state.RunId).IsNotEqualTo(Guid.Empty);
        await Assert.That(state.Clock.Day).IsEqualTo(1);
        await Assert.That(state.Player).IsNotNull();
        await Assert.That(state.World).IsNotNull();
        await Assert.That(state.IsGameOver).IsFalse();
    }

    [Test]
    public async Task EndDay_ShouldDeductRentFromMoney()
    {
        var state = new GameState();

        state.EndDay();

        await Assert.That(state.Player.Stats.Money).IsEqualTo(80);
    }

    [Test]
    public async Task EatAtHome_ShouldFeedPlayerAndMother()
    {
        var state = new GameState();

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
        var state = new GameState();
        state.Clock.AdvanceHours(10);

        state.EndDay();

        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(6);
    }

    [Test]
    public async Task EndDay_ShouldReturnPlayerHome()
    {
        var state = new GameState();
        state.World.TravelTo(LocationId.Market);

        state.EndDay(new Random(1));

        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Home);
    }

    [Test]
    public async Task EatAtHome_WithoutStaples_ShouldFail()
    {
        var state = new GameState();
        state.Player.Household.SetFoodStockpile(0);

        var result = state.EatAtHome();

        await Assert.That(result).IsFalse();
        await Assert.That(state.Player.Nutrition.AteToday).IsFalse();
        await Assert.That(state.Player.Household.FedMotherToday).IsFalse();
    }

    [Test]
    public async Task EatStreetFood_ShouldCostMoneyAndFeedOnlyPlayer()
    {
        var state = new GameState();

        var result = state.EatStreetFood();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Money).IsEqualTo(92);
        await Assert.That(state.Player.Nutrition.AteToday).IsTrue();
        await Assert.That(state.Player.Household.FedMotherToday).IsFalse();
    }

    [Test]
    public async Task BuyMedicine_ShouldIncreaseMedicineStock()
    {
        var state = new GameState();

        var result = state.BuyMedicine();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Household.MedicineStock).IsEqualTo(2);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(50);
    }

    [Test]
    public async Task GiveMotherMedicine_ShouldConsumeMedicineStock()
    {
        var state = new GameState();
        state.Player.Household.SetMedicineStock(2);

        var result = state.GiveMotherMedicine();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Household.MedicineStock).IsEqualTo(1);
        await Assert.That(state.Player.Household.MedicationGivenToday).IsTrue();
    }

    [Test]
    public async Task RestAtHome_ShouldRestoreEnergyAndAdvanceTime()
    {
        var state = new GameState();

        state.RestAtHome();

        await Assert.That(state.Player.Stats.Energy).IsEqualTo(100);
        await Assert.That(state.Clock.Hour).IsEqualTo(14);
    }

    [Test]
    public async Task RestAtHome_ShouldTriggerEndDayWhenRestPassesCurfew()
    {
        var state = new GameState();
        state.Clock.AdvanceHours(12);

        state.RestAtHome();

        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(10);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(80);
    }

    [Test]
    public async Task TryTravelTo_ShouldSucceedWithEnoughMoney()
    {
        var state = new GameState();

        var result = state.TryTravelTo(LocationId.Market);

        await Assert.That(result).IsTrue();
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Market);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(98);
    }

    [Test]
    public async Task TryTravelTo_ShouldFailWithInsufficientMoney()
    {
        var state = new GameState();
        state.Player.Stats.ModifyMoney(-99);

        var result = state.TryTravelTo(LocationId.CallCenter);

        await Assert.That(result).IsFalse();
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Home);
    }

    [Test]
    public async Task TryTravelTo_ShouldAdvanceTimeByTravelDuration()
    {
        var state = new GameState();

        state.TryTravelTo(LocationId.Market);

        await Assert.That(state.Clock.Minute).IsEqualTo(15);
    }

    [Test]
    public async Task TryTravelTo_ShouldTriggerEndDayWhenTravelPassesCurfew()
    {
        var state = new GameState();
        state.Clock.AdvanceHours(15);
        state.Clock.AdvanceMinutes(50);

        var result = state.TryTravelTo(LocationId.CallCenter);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(6);
        await Assert.That(state.Clock.Minute).IsEqualTo(35);
    }

    [Test]
    public async Task WorkJob_ShouldTriggerEndDayWhenShiftPassesCurfew()
    {
        var state = new GameState();
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
        var state = new GameState();
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
        var state = new GameState();
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
        var state = new GameState();
        state.World.TravelTo(LocationId.Workshop);
        state.SetPolicePressure(70);
        state.SetWorkCounters(0, 0, lastCrimeDay: 1, lastHonestWorkDay: 0, lastPublicFacingWorkDay: 0);
        state.Player.Stats.SetEnergy(35);

        var result = state.WorkJob(state.GetAvailableJobs().Single());

        await Assert.That(result.MistakeMade).IsTrue();
        await Assert.That(state.Relationships.GetNpcRelationship(NpcId.WorkshopBossAbuSamir).WasEmbarrassed).IsTrue();
    }

    [Test]
    public async Task CommitCrime_ShouldUsePublicFacingWorkAsAnAlibi_SameDay()
    {
        var state = new GameState();
        state.World.TravelTo(LocationId.Cafe);
        state.WorkJob(state.GetAvailableJobs().Single());
        state.World.TravelTo(LocationId.Square);
        var attempt = new CrimeAttempt(CrimeType.DokkiDrop, 95, 42, 24, 0, 18);

        var result = state.CommitCrime(attempt, new Random(2));

        await Assert.That(state.LastPublicFacingWorkDay).IsEqualTo(state.Clock.Day);
        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task BuyFood_ShouldGrantExtraStaples_ForSudaneseBackground()
    {
        var state = new GameState();
        state.Player.ApplyBackground(Slums.Core.Characters.BackgroundRegistry.SudaneseRefugee);
        var before = state.Player.Household.FoodStockpile;

        var result = state.BuyFood();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Household.FoodStockpile).IsEqualTo(before + 4);
    }

    [Test]
    public async Task TryTravelTo_ShouldIncreaseStress_ForSudaneseBackground_InDokki()
    {
        var state = new GameState();
        state.Player.ApplyBackground(Slums.Core.Characters.BackgroundRegistry.SudaneseRefugee);
        var before = state.Player.Stats.Stress;

        var result = state.TryTravelTo(LocationId.CallCenter);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Stress).IsEqualTo(before + 2);
    }

    [Test]
    public async Task EndDay_ShouldDecayPressureMoreSlowly_ForReleasedPrisoner()
    {
        var state = new GameState();
        state.Player.ApplyBackground(Slums.Core.Characters.BackgroundRegistry.ReleasedPoliticalPrisoner);
        state.SetPolicePressure(25);

        state.EndDay(new Random(2));

        await Assert.That(state.PolicePressure).IsEqualTo(23);
    }

    [Test]
    public async Task RelationshipMemory_ShouldRecordDebtState_InCore()
    {
        var state = new GameState();
        state.Relationships.RecordFavor(NpcId.NurseSalma, state.Clock.Day, hasUnpaidDebt: true);

        await Assert.That(state.Relationships.GetNpcRelationship(NpcId.NurseSalma).HasUnpaidDebt).IsTrue();
        await Assert.That(state.Relationships.GetNpcRelationship(NpcId.NurseSalma).LastFavorDay).IsEqualTo(state.Clock.Day);
    }

    [Test]
    public async Task ApplyRandomEvent_ShouldRecordEventHistory_WhenDayEnds()
    {
        var state = new GameState();
        state.Clock.SetTime(5, 6, 0);
        state.World.TravelTo(LocationId.CallCenter);
        state.SetPolicePressure(60);

        state.EndDay(new Random(1));

        await Assert.That(state.RandomEventHistory.Count).IsGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task IsGameOver_ShouldBeTrueWhenHealthIsZero()
    {
        var state = new GameState();
        state.Player.Stats.ModifyHealth(-100);

        state.EndDay();

        await Assert.That(state.IsGameOver).IsTrue();
        await Assert.That(state.GameOverReason).Contains("health");
    }

    [Test]
    public async Task GameEvent_ShouldBeRaisedForActions()
    {
        var state = new GameState();
        var events = new List<string>();
        state.GameEvent += (_, e) => events.Add(e.Message);

        state.TryTravelTo(LocationId.Market);

        events.Should().ContainMatch("*Traveled*");
    }

    [Test]
    public async Task CommitCrime_ShouldApplyMoneyEnergyAndPressureChanges()
    {
        var state = new GameState();
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
            var baseline = CreateCrimeState(LocationId.Market);
            var baselineResult = baseline.CommitCrime(attempt, new Random(seed));
            if (!baselineResult.Detected)
            {
                continue;
            }

            var trusted = CreateCrimeState(LocationId.Market);
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
            var baseline = CreateCrimeState(LocationId.Square);
            var baselineResult = baseline.CommitCrime(attempt, new Random(seed));
            if (!baselineResult.Detected)
            {
                continue;
            }

            var trusted = CreateCrimeState(LocationId.Square);
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
            var baseline = CreateCrimeState(LocationId.Market);
            var baselineResult = baseline.CommitCrime(attempt, new Random(seed));
            if (baselineResult.Success || !baselineResult.Detected)
            {
                continue;
            }

            var trusted = CreateCrimeState(LocationId.Market);
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
            var baseline = CreateCrimeState(LocationId.Square);
            var baselineResult = baseline.CommitCrime(attempt, new Random(seed));
            if (baselineResult.Success || !baselineResult.Detected)
            {
                continue;
            }

            var trusted = CreateCrimeState(LocationId.Square);
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
            var state = CreateCrimeState(LocationId.Market);
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
            var state = CreateCrimeState(LocationId.Square);
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
            var state = CreateCrimeState(LocationId.Market);
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
    public async Task EndDay_ShouldDecayPolicePressure_OnCleanDay()
    {
        var state = new GameState();
        state.SetPolicePressure(25);

        state.EndDay(new Random(2));

        await Assert.That(state.PolicePressure).IsEqualTo(20);
    }

    [Test]
    public async Task EndDay_PlayerWithoutMeal_ShouldLoseEnergyAndGainStress()
    {
        var state = new GameState();

        state.EndDay();

        await Assert.That(state.Player.Stats.Energy).IsEqualTo(58);
        await Assert.That(state.Player.Stats.Stress).IsEqualTo(31);
        await Assert.That(state.Player.Stats.Hunger).IsEqualTo(57);
    }

    [Test]
    public async Task EndDay_PlayerUnderfedForTwoDays_ShouldLoseHealth()
    {
        var state = new GameState();
        state.Player.Nutrition.SetDaysUndereating(1);

        state.EndDay();

        await Assert.That(state.Player.Stats.Health).IsEqualTo(95);
    }

    [Test]
    public async Task EndDay_MotherFragileWithoutMedicine_ShouldLoseHealth()
    {
        var state = new GameState();
        state.Player.Household.SetMotherHealth(45);
        state.Player.Household.FeedMother();

        state.EndDay();

        await Assert.That(state.Player.Household.MotherHealth).IsEqualTo(39);
    }

    [Test]
    public async Task EndDay_MotherInCrisisWithoutCheck_ShouldIncreasePlayerStress()
    {
        var state = new GameState();
        state.Player.Household.SetMotherHealth(20);
        state.Player.Household.FeedMother();
        state.Player.Household.SetMedicineStock(1);
        state.Player.Household.GiveMedicine();

        state.EndDay();

        await Assert.That(state.Player.Stats.Stress).IsEqualTo(39);
    }

    [Test]
    public async Task EndDay_ShouldResetNutritionAndCareFlagsForNextDay()
    {
        var state = new GameState();
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
        var state = new GameState();
        state.Player.Household.SetMotherHealth(4);

        state.EndDay();

        await Assert.That(state.IsGameOver).IsTrue();
        await Assert.That(state.GameOverReason).Contains("mother");
    }

    [Test]
    public async Task GetStatusSummary_ShouldReturnCurrentStatus()
    {
        var state = new GameState();

        var summary = state.GetStatusSummary();

        summary.Should().HaveCount(8);
        summary[0].Should().Contain("Day 1");
        summary[2].Should().Contain("Money");
    }

    private static GameState CreateCrimeState(LocationId locationId)
    {
        var state = new GameState();
        state.World.TravelTo(locationId);
        state.SetPolicePressure(60);
        return state;
    }

    private static bool HasPendingNarrativeScene(GameState state, string knotName)
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
}
