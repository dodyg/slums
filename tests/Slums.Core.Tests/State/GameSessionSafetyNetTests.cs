using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Slums.Core.Characters;
using Slums.Core.Community;
using Slums.Core.Crimes;
using Slums.Core.Economy;
using Slums.Core.Entertainment;
using Slums.Core.Home;
using Slums.Core.Investments;
using Slums.Core.Jobs;
using Slums.Core.Randomness;
using Slums.Core.Relationships;
using Slums.Core.Robotics;
using Slums.Core.State;
using Slums.Core.Training;
using Slums.Core.World;
using TUnit;

namespace Slums.Core.Tests.State;

internal sealed class GameSessionSafetyNetTests
{
    private static readonly string[] ApprovedPublicMembers =
    [
        "field:ConversationDurationMinutes",
        "field:EmergencySupportDurationMinutes",
        "property:ActiveInvestments",
        "property:ActiveNews",
        "property:AccumulatedRentDebt",
        "property:CanRequestEmergencySupport",
        "property:CityCrisis",
        "property:CommunityAdaptation",
        "property:Clock",
        "property:CrimesCommitted",
        "property:CurrentDay",
        "property:CurrentWeather",
        "property:CurrentWeek",
        "property:DaysSurvived",
        "property:DistrictHeat",
        "property:EndingId",
        "property:EventAttendance",
        "property:EventJournal",
        "property:FinalSacrifice",
        "property:FinalWarningGiven",
        "property:FirstWarningGiven",
        "property:GameOverReason",
        "property:HasClaimedEmergencySupport",
        "property:HasCrimeCommittedToday",
        "property:HomeUpgrades",
        "property:HonestShiftsCompleted",
        "property:Infrastructure",
        "property:Inventory",
        "property:IsGameOver",
        "property:JobProgress",
        "property:Jobs",
        "property:LastCrimeDay",
        "property:LastHonestWorkDay",
        "property:LastPublicFacingWorkDay",
        "property:Mutations",
        "property:News",
        "property:NpcEconomies",
        "property:CentralCharacterArcs",
        "property:PendingEndingId",
        "property:PendingEndingKnot",
        "property:PendingNarrativeScenes",
        "property:Phone",
        "property:PhoneMessages",
        "property:Player",
        "property:PlayerDebts",
        "property:PolicePressure",
        "property:RandomEventHistory",
        "property:RandomState",
        "property:Relationships",
        "property:RamadanState",
        "property:RentGraceDaysRemaining",
        "property:Rumors",
        "property:RunId",
        "property:SharedRandom",
        "property:StoryFlags",
        "property:Technology",
        "property:Tips",
        "property:TotalCrimeEarnings",
        "property:TotalHerbEarnings",
        "property:TotalHonestWorkEarnings",
        "property:TotalInvestmentEarnings",
        "property:TrainedSkillsToday",
        "property:Territory",
        "property:UnpaidRentDays",
        "property:World",
        "event:GameEvent",
        "event:MutationRecorded",
        "method:AddEventMessage",
        "method:AdjustEnergy",
        "method:AdjustFoodStockpile",
        "method:AdjustHealth",
        "method:AdjustHunger",
        "method:AdjustMoney",
        "method:AdjustMotherHealth",
        "method:AdjustPolicePressure",
        "method:AdjustStress",
        "method:AdoptStreetCat",
        "method:AdvanceTime",
        "method:ApplyDebtPayment",
        "method:ApplyGenderRelationshipModifiers",
        "method:ApplyNarrativeOutcome",
        "method:ApplyRentPayment",
        "method:AttendCommunityEvent",
        "method:AcknowledgeTip",
        "method:BuyFishTank",
        "method:BuyFood",
        "method:BuyMedicine",
        "method:BuyPlant",
        "method:BuyRobot",
        "method:BuyRobotParts",
        "method:CanAffordTravel",
        "method:CanUseHouseholdAssets",
        "method:CheckInvestmentEligibility",
        "method:CheckOnMother",
        "method:ChooseCrisisDecision",
        "method:CollectCrisisEvidence",
        "method:CommitCrisisResources",
        "method:CommitCrime",
        "method:CommitEnding",
        "method:EndDay",
        "method:EatAtHome",
        "method:EatStreetFood",
        "method:ExtendDebtDueDate",
        "method:GiveMotherMedicine",
        "method:GetActiveDistrictConditionDefinition",
        "method:GetActiveHolidayState",
        "method:GetActiveNewsDefinitions",
        "method:GetAvailableCommunityEvents",
        "method:GetAvailableCrimes",
        "method:GetAvailableEndingChoices",
        "method:GetAvailableEntertainmentActivities",
        "method:GetAvailableHomeUpgrades",
        "method:GetAvailableInvestments",
        "method:GetAvailableJobs",
        "method:GetAvailableTrainingActivities",
        "method:GetClinicLocations",
        "method:GetClinicTravelOption",
        "method:GetCommunityActionPreviews",
        "method:GetCrimeBlockReason",
        "method:GetCurrentInvestmentOpportunities",
        "method:GetCurrentLocationClinicStatus",
        "method:GetCurrentSchedule",
        "method:GetCurrentSeason",
        "method:GetCurrentSeasonModifiers",
        "method:GetDailyDistrictConditions",
        "method:GetEffectiveRandomEventWeight",
        "method:GetEventCount",
        "method:GetFoodCost",
        "method:GetMedicineCost",
        "method:GetProvisioningMealPlan",
        "method:GetNpcAvailability",
        "method:GetReachableNpcs",
        "method:GetStatusSummary",
        "method:GetStreetFoodCost",
        "method:GetTravelConditionSummary",
        "method:GetTravelCost",
        "method:GetTravelTimeMinutes",
        "method:GetWalkTimeMinutes",
        "method:GrantRentGraceDays",
        "method:HasStoryFlag",
        "method:IgnoreMessage",
        "method:IgnoreTipAction",
        "method:MakeInvestment",
        "method:MarkCrisisCallbackQueued",
        "method:ModifyFactionReputation",
        "method:ModifyNpcTrust",
        "method:PayPetCare",
        "method:PayPlantCare",
        "method:PerformCommunityAction",
        "method:PreviewCommunityAction",
        "method:GetTechnicalRepairPreviews",
        "method:PerformTechnicalRepair",
        "method:PreviewTechnicalRepair",
        "method:GetDigitalServicePreviews",
        "method:PerformDigitalService",
        "method:PreviewDigitalService",
        "method:PreviewCrime",
        "method:PreviewJob",
        "method:QueueNarrativeScene",
        "method:RecordCentralCharacterDecision",
        "method:RecordEventHistory",
        "method:RecordFavor",
        "method:RecordRefusal",
        "method:RefillPhoneCredit",
        "method:RefuseNpcLoan",
        "method:RepairRobot",
        "method:RepayDebt",
        "method:ReplacePhone",
        "method:RequestEmergencySupport",
        "method:RespondToMessage",
        "method:ResolveCityCrisis",
        "method:ResolveWeeklyInvestments",
        "method:RestAtHome",
        "method:RestoreFromSnapshot",
        "method:SetDebtState",
        "method:SetEmbarrassedState",
        "method:SetHelpedState",
        "method:SetRamadanFasting",
        "method:SetStoryFlag",
        "method:TakeMotherToClinic",
        "method:TravelAndTakeMotherToClinic",
        "method:TryChooseEnding",
        "method:TryDequeueNarrativeScene",
        "method:TryPerformEntertainment",
        "method:TryPerformTraining",
        "method:TryPurchaseHomeUpgrade",
        "method:TryTakePendingEndingKnot",
        "method:TryTravelTo",
        "method:TryWalkTo",
        "method:TryBorrowFromLandlord",
        "method:TryBorrowFromLoanShark",
        "method:TryBorrowFromNpc",
        "method:TryLendToNpc",
        "method:UpgradeFishTank",
        "method:UpgradePlant",
        "method:WorkJob"
    ];

    [Test]
    public async Task PublicApi_ShouldMatchApprovedMemberNames()
    {
        var actual = typeof(GameSession)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static member => member.MemberType is MemberTypes.Field or MemberTypes.Property or MemberTypes.Event ||
                member is MethodInfo { IsSpecialName: false })
            .Select(static member => $"{GetMemberPrefix(member)}:{member.Name}")
            .OrderBy(static member => member, StringComparer.Ordinal)
            .ToArray();

        Console.WriteLine($"GameSession API: {string.Join(",", actual)}");

        var expected = ApprovedPublicMembers.OrderBy(static member => member, StringComparer.Ordinal).ToArray();

        await Assert.That(actual).IsEquivalentTo(expected);
    }

    [Test]
    public async Task ThirtyFiveDayScript_ShouldKeepAStableDigest()
    {
        var session = CreateSafetyNetSession();

        for (var day = 1; day <= 35; day++)
        {
            PrepareForDay(session);
            ExerciseDomain(session, day);
            session.EndDay(session.SharedRandom);
            if (session.IsGameOver)
            {
                throw new InvalidOperationException($"Safety-net script reached game over on day {day}: {session.EndingId}");
            }
        }

        var digest = BuildDigest(session);
        Console.WriteLine($"GameSession golden digest: {digest}");

        await Assert.That(digest).IsEqualTo("EA3B6256D02A3FFE1BAD0E0BF8B9BAC5567D71D8E8F74518560FA89D0BA19508");
    }

    private static GameSession CreateSafetyNetSession()
    {
        var session = new GameSession(new GameRandom(0x2060CA1UL));
        session.Player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);
        session.Player.Stats.SetMoney(10_000);
        session.Player.Stats.SetHealth(100);
        session.Player.Stats.SetEnergy(100);
        session.Player.Stats.SetHunger(100);
        session.Player.Stats.SetStress(20);
        session.Player.Nutrition.SetSatiety(100);
        session.Player.Household.SetMotherHealth(100);
        session.Player.Household.SetFoodStockpile(100);
        session.Player.Household.AddMedicine(100);
        return session;
    }

    private static void PrepareForDay(GameSession session)
    {
        session.Player.Stats.SetMoney(Math.Max(session.Player.Stats.Money, 10_000));
        session.Player.Stats.SetHealth(100);
        session.Player.Stats.SetEnergy(100);
        session.Player.Stats.SetHunger(100);
        session.Player.Stats.SetStress(20);
        session.Player.Nutrition.SetSatiety(100);
        session.Player.Household.SetMotherHealth(100);
        session.Player.Household.SetFoodStockpile(100);
    }

    private static void ExerciseDomain(GameSession session, int day)
    {
        switch (day)
        {
            case 1:
                session.GetFoodCost();
                session.GetStreetFoodCost();
                session.BuyFood();
                session.EatAtHome();
                session.GetAvailableHomeUpgrades();
                session.TryPurchaseHomeUpgrade(HomeUpgrade.CleanBedding);
                session.RequestEmergencySupport();
                break;
            case 2:
                session.TryTravelTo(LocationId.Pharmacy);
                session.GetMedicineCost();
                session.BuyMedicine();
                session.CheckOnMother();
                session.GiveMotherMedicine();
                session.GetClinicLocations();
                session.GetCurrentLocationClinicStatus();
                session.TakeMotherToClinic();
                session.World.TravelTo(LocationId.Home);
                break;
            case 3:
                session.World.TravelTo(LocationId.Cafe);
                var entertainmentActivities = session.GetAvailableEntertainmentActivities();
                var entertainment = entertainmentActivities.Count > 0 ? entertainmentActivities[0] : null;
                if (entertainment is not null)
                {
                    session.TryPerformEntertainment(entertainment);
                }

                session.World.TravelTo(LocationId.Home);
                break;
            case 4:
                session.Clock.SetTime(session.Clock.Day, 18, 0);
                var trainingActivities = session.GetAvailableTrainingActivities();
                var training = trainingActivities.Count > 0 ? trainingActivities[0] : null;
                if (training is not null)
                {
                    session.TryPerformTraining(training);
                }

                break;
            case 5:
                session.World.TravelTo(LocationId.Bakery);
                var jobs = session.GetAvailableJobs();
                var job = jobs.Count > 0 ? jobs[0] : null;
                if (job is not null)
                {
                    session.WorkJob(job, session.SharedRandom);
                }

                session.World.TravelTo(LocationId.Market);
                session.GetAvailableCrimes();
                session.CommitCrime(new CrimeAttempt(CrimeType.PettyTheft, 1, 0, 0, 0, 1), session.SharedRandom);

                session.World.TravelTo(LocationId.Home);
                break;
            case 6:
                session.GetAvailableCommunityEvents();
                session.AttendCommunityEvent(CommunityEventId.NeighborhoodCleanup, session.SharedRandom);
                session.AdoptStreetCat();
                session.BuyPlant(PlantType.Basil);
                session.CanUseHouseholdAssets();
                break;
            case 7:
                session.TryBorrowFromNpc(NpcId.NeighborMona, 0);
                session.TryBorrowFromLandlord(0);
                session.TryBorrowFromLoanShark(0);
                session.TryLendToNpc(NpcId.NeighborMona, 0);
                session.RefuseNpcLoan(NpcId.NeighborMona);
                session.RepayDebt(DebtSource.NeighborLoan, 100);
                session.GetAvailableInvestments();
                session.GetCurrentInvestmentOpportunities();
                session.MakeInvestment(InvestmentType.FoulCart);
                session.ResolveWeeklyInvestments(session.SharedRandom);
                break;
            case 8:
                session.RefillPhoneCredit();
                session.GetReachableNpcs();
                session.GetNpcAvailability();
                break;
            default:
                session.GetStatusSummary();
                session.GetCurrentSchedule();
                session.GetCurrentSeason();
                session.GetCurrentSeasonModifiers();
                session.GetDailyDistrictConditions();
                session.GetTravelConditionSummary(LocationId.Market);
                session.GetTravelCost(LocationId.Market);
                session.GetTravelTimeMinutes(LocationId.Market);
                session.GetWalkTimeMinutes(LocationId.Market);
                session.CanAffordTravel(LocationId.Market);
                session.EatStreetFood();
                break;
        }
    }

    private static string BuildDigest(GameSession session)
    {
        var journal = string.Join("|", session.EventJournal.Entries.Select(static entry => $"{entry.Day}:{entry.Source}:{entry.Message}"));
        var mutations = string.Join("|", session.Mutations.Select(static mutation => $"{mutation.Category}:{mutation.Action}:{mutation.Reason}"));
        var random = session.RandomState;
        var randomState = random is null ? "null" : $"{random.S0},{random.S1},{random.S2},{random.S3}";
        var stats = $"{session.Clock.Day},{session.Player.Stats.Money},{session.Player.Stats.Hunger},{session.Player.Stats.Energy},{session.Player.Stats.Health},{session.Player.Stats.Stress},{session.Player.Household.MotherHealth},{session.Player.Household.FoodStockpile},{session.Player.Household.MedicineStock},{session.Player.HouseholdAssets.Pets.Count},{session.Player.HouseholdAssets.Plants.Count},{session.ActiveInvestments.Count},{session.PlayerDebts.Debts.Count}";
        var payload = $"journal={journal};mutations={session.Mutations.Count}:{mutations};stats={stats};random={randomState}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }

    private static string GetMemberPrefix(MemberInfo member)
    {
        return member.MemberType switch
        {
            MemberTypes.Field => "field",
            MemberTypes.Property => "property",
            MemberTypes.Method => "method",
            MemberTypes.Event => "event",
            _ => member.MemberType.ToString()
        };
    }
}
