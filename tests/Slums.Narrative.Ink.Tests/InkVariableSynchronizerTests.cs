using FluentAssertions;
using Ink.Runtime;
using Slums.Application.Narrative;
using Slums.Core.Narrative;
using Slums.Narrative.Ink;
using TUnit;

namespace Slums.Narrative.Ink.Tests;

internal sealed class InkVariableSynchronizerTests
{
    [Test]
    public void SyncVariablesToInk_ShouldSetGlobalsOnTheCheckedInStory()
    {
        var story = InkStoryFactory.Create(InkStoryLoader.LoadStoryJson());
        var sceneState = new NarrativeSceneState(37, 81, 62, 44, 29, 53, 4, 12, "SudaneseRefugee", "female")
        {
            District = "Imbaba",
            Weather = "Heatwave",
            Season = "Summer",
            Holiday = "Ramadan",
            IsRamadan = true,
            IsRamadanFasting = true,
            UnpaidRentDays = 2,
            RentDebt = 40,
            RentGraceDays = 3,
            PolicePressure = 27,
            OperationalRobots = ["SalvageCrawler", "CourierDrone"],
            ActiveNews = ["WaterRationing"],
            Infrastructure = new Dictionary<string, string> { ["Imbaba:Water"] = "Severe" },
            RelationshipTrust = new Dictionary<string, int> { ["NeighborMona"] = 14, ["NurseSalma"] = 9 },
            ConversationVariantId = "landlord_default_4_7",
            ConversationContext = "Default",
            ConversationNpc = "LandlordHajjMahmoud",
            CrisisPhase = CityCrisisPhase.Appeal,
            CrisisEvidenceCollected = 2,
            CrisisResourcesCommitted = 3,
            CrisisCooperativeCondition = 71,
            CrisisDecision = CityCrisisDecision.MutualAid,
            CrisisResolution = CityCrisisResolution.SharedEmergencyPlan,
            PendingEnding = "StabilityHonestWork",
            HandsetDataExposure = 8,
            MicrogridRepairDebt = 6,
            MicrogridStorageCondition = 64,
            TransitPermitReview = true,
            BiometricAppealPending = true,
            LastTelemedicineTriageDay = 11,
            AllocationModelConfidence = 46,
            CentralDecisions = new Dictionary<string, string> { ["Mother"] = "MotherAcceptCare" }
        };

        InkVariableSynchronizer.SyncVariablesToInk(story, sceneState);

        GetInt(story, "money").Should().Be(37);
        GetInt(story, "mother_health").Should().Be(53);
        GetInt(story, "day").Should().Be(12);
        GetInt(story, "operational_robot_count").Should().Be(2);
        GetInt(story, "active_news_count").Should().Be(1);
        GetInt(story, "infrastructure_disruption_count").Should().Be(1);
        GetInt(story, "mona_trust").Should().Be(14);
        GetInt(story, "salma_trust").Should().Be(9);
        GetInt(story, "conversation_opener").Should().Be(4);
        GetInt(story, "conversation_body").Should().Be(7);
        GetInt(story, "crisis_evidence").Should().Be(2);
        GetInt(story, "allocation_model_confidence").Should().Be(46);
        GetBool(story, "is_ramadan").Should().BeTrue();
        GetBool(story, "is_fasting").Should().BeTrue();
        story.variablesState["district"]!.ToString().Should().Be("Imbaba");
        story.variablesState["background"]!.ToString().Should().Be("SudaneseRefugee");
        story.variablesState["gender"]!.ToString().Should().Be("female");
        story.variablesState["mother_arc_decision"]!.ToString().Should().Be("MotherAcceptCare");
    }

    private static int GetInt(Story story, string name)
    {
        return Convert.ToInt32(story.variablesState[name], System.Globalization.CultureInfo.InvariantCulture);
    }

    private static bool GetBool(Story story, string name)
    {
        return Convert.ToBoolean(story.variablesState[name], System.Globalization.CultureInfo.InvariantCulture);
    }
}
