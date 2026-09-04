using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Core.Economy;
using Slums.Core.Endings;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using TUnit;

namespace Slums.Narrative.Ink.Tests;

internal sealed class InkNarrativeTagParserTests
{
    [Test]
    public void Parse_ShouldHandleEveryEffectTag()
    {
        var cases = new Dictionary<string, Type>
        {
            ["NPC_TRUST:OfficerKhalid,2"] = typeof(NpcTrustEffect),
            ["FACTION_REP:ImbabaCrew,3"] = typeof(FactionReputationEffect),
            ["FAVOR:OfficerKhalid"] = typeof(FavorEffect),
            ["REFUSAL:OfficerKhalid"] = typeof(RefusalEffect),
            ["DEBT:FixerUmmKarim,true"] = typeof(DebtEffect),
            ["EMBARRASSED:NeighborMona,true"] = typeof(EmbarrassedEffect),
            ["HELPED:NeighborMona,false"] = typeof(HelpedEffect),
            ["RENT_PAYMENT:20"] = typeof(RentPaymentEffect),
            ["RENT_GRACE_DAYS:2"] = typeof(RentGraceDaysEffect),
            ["DEBT_PAYMENT:LoanShark,10"] = typeof(DebtPaymentEffect),
            ["DEBT_DUE_EXTENSION:LandlordAdvance,3"] = typeof(DebtDueExtensionEffect),
            ["RAMADAN_FASTING:true"] = typeof(RamadanFastingEffect),
            ["CRISIS_EVIDENCE:2"] = typeof(CrisisEvidenceEffect),
            ["CRISIS_RESOURCES:3"] = typeof(CrisisResourcesEffect),
            ["CRISIS_DECISION:MutualAid"] = typeof(CrisisDecisionEffect),
            ["CRISIS_RESOLUTION:SharedEmergencyPlan"] = typeof(CrisisResolutionEffect),
            ["POLICE:4"] = typeof(PolicePressureEffect),
            ["ENDING_COMMIT:StabilityHonestWork,keep_the_shop"] = typeof(EndingCommitmentEffect),
            ["CENTRAL_DECISION:Mother,MotherAcceptCare"] = typeof(CentralCharacterDecisionEffect)
        };

        foreach (var (tag, expectedType) in cases)
        {
            InkTagEffectParser.Parse(tag).Should().BeOfType(expectedType);
        }
    }

    [Test]
    public void ParseOutcome_ShouldHandleScalarOutcomeTags()
    {
        InkTagEffectParser.ParseOutcome("FLAG:scene_seen")!.SetFlags.Should().ContainSingle("scene_seen");
        InkTagEffectParser.ParseOutcome("MESSAGE:Street remembers")!.Message.Should().Be("Street remembers");
        InkTagEffectParser.ParseOutcome("MONEY:20")!.MoneyChange.Should().Be(20);
        InkTagEffectParser.ParseOutcome("HEALTH:-2")!.HealthChange.Should().Be(-2);
        InkTagEffectParser.ParseOutcome("ENERGY:-3")!.EnergyChange.Should().Be(-3);
        InkTagEffectParser.ParseOutcome("HUNGER:4")!.HungerChange.Should().Be(4);
        InkTagEffectParser.ParseOutcome("STRESS:5")!.StressChange.Should().Be(5);
        InkTagEffectParser.ParseOutcome("MOTHER_HEALTH:6")!.MotherHealthChange.Should().Be(6);
        InkTagEffectParser.ParseOutcome("FOOD:1")!.FoodChange.Should().Be(1);
    }

    [Test]
    [Arguments("FAVOR:999")]
    [Arguments("NPC_TRUST:999,-5")]
    [Arguments("FACTION_REP:999,3")]
    [Arguments("DEBT:999,true")]
    [Arguments("EMBARRASSED:999,true")]
    [Arguments("HELPED:999,false")]
    [Arguments("DEBT_PAYMENT:999,10")]
    [Arguments("DEBT_DUE_EXTENSION:999,3")]
    [Arguments("MONEY:not_an_integer")]
    [Arguments("CRISIS_EVIDENCE:0")]
    [Arguments("RAMADAN_FASTING:maybe")]
    [Arguments("CRISIS_DECISION:None")]
    [Arguments("CRISIS_RESOLUTION:Unresolved")]
    [Arguments("ENDING_COMMIT:StabilityHonestWork,")]
    [Arguments("CENTRAL_DECISION:Mother,unknown")]
    public void Parse_ShouldRejectMalformedEffectValues(string tag)
    {
        var act = () => InkTagEffectParser.ParseOutcome(tag);

        act.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain(tag);
    }

    [Test]
    public void Parse_ShouldIgnoreNonEffectMarkers()
    {
        InkTagEffectParser.Parse("weather:rain").Should().BeNull();
        InkTagEffectParser.Parse("not_an_effect").Should().BeNull();
    }
}
