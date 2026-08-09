using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Narrative.Ink.Tests.Helpers;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Simulation;

internal sealed class NarrativeOutcomeIntegrationTests
{
    [Test]
    public async Task Outcome_MoneyTag_CreatesCorrectOutcome()
    {
        var outcome = new NarrativeOutcome
        {
            MoneyChange = 50,
            Message = "player earned money"
        };

        outcome.MoneyChange.Should().Be(50);
        outcome.Message.Should().Be("player earned money");
    }

    [Test]
    public async Task Outcome_HealthTag_CreatesCorrectOutcome()
    {
        var outcome = new NarrativeOutcome
        {
            HealthChange = -10,
            Message = "player lost health"
        };

        outcome.HealthChange.Should().Be(-10);
    }

    [Test]
    public async Task Outcome_EnergyTag_CreatesCorrectOutcome()
    {
        var outcome = new NarrativeOutcome
        {
            EnergyChange = -15,
            Message = "activity cost energy"
        };

        outcome.EnergyChange.Should().Be(-15);
    }

    [Test]
    public async Task Outcome_StressTag_CreatesCorrectOutcome()
    {
        var outcome = new NarrativeOutcome
        {
            StressChange = 20,
            Message = "stressful situation"
        };

        outcome.StressChange.Should().Be(20);
    }

    [Test]
    public async Task Outcome_MotherHealthTag_CreatesCorrectOutcome()
    {
        var outcome = new NarrativeOutcome
        {
            MotherHealthChange = -5,
            Message = "mother's condition worsened"
        };

        outcome.MotherHealthChange.Should().Be(-5);
    }

    [Test]
    public async Task Outcome_FoodTag_CreatesCorrectOutcome()
    {
        var outcome = new NarrativeOutcome
        {
            FoodChange = 3,
            Message = "bought food supplies"
        };

        outcome.FoodChange.Should().Be(3);
    }

    [Test]
    public async Task Outcome_FlagTag_SetsFlag()
    {
        var outcome = new NarrativeOutcome
        {
            SetFlag = "crime_first_success",
            Message = "first successful crime"
        };

        outcome.SetFlag.Should().Be("crime_first_success");
    }

    [Test]
    public async Task Outcome_NpcTrustTag_CreatesCorrectOutcome()
    {
        var outcome = new NarrativeOutcome
        {
            Effects = [new NpcTrustEffect(NpcId.NurseSalma, 15)],
            Message = "gained trust"
        };

        outcome.Effects.OfType<NpcTrustEffect>().Should().ContainSingle(effect => effect.Npc == NpcId.NurseSalma && effect.Change == 15);
    }

    [Test]
    public async Task Outcome_FactionRepTag_CreatesCorrectOutcome()
    {
        var outcome = new NarrativeOutcome
        {
            Effects = [new FactionReputationEffect(FactionId.ImbabaCrew, 10)],
            Message = "gained faction reputation"
        };

        outcome.Effects.OfType<FactionReputationEffect>().Should().ContainSingle(effect => effect.Faction == FactionId.ImbabaCrew && effect.Change == 10);
    }

    [Test]
    public async Task Outcome_MessageTag_ContainsText()
    {
        var outcome = new NarrativeOutcome
        {
            Message = "The market is unusually quiet today."
        };

        outcome.Message.Should().Contain("quiet");
    }

    [Test]
    public async Task Outcome_MultipleOutcomes_AppliedInOrder()
    {
        var session = new GameStateBuilder()
            .WithMoney(100)
            .WithHealth(80)
            .WithEnergy(60)
            .Build();

        session.AdjustMoney(50);
        session.AdjustHealth(-10);
        session.AdjustEnergy(-15);

        session.Player.Stats.Money.Should().Be(150);
        session.Player.Stats.Health.Should().Be(70);
        session.Player.Stats.Energy.Should().Be(45);
    }

    [Test]
    public async Task Outcome_NpcTrust_UpdatesRelationship()
    {
        var session = new GameStateBuilder()
            .WithNpcTrust(NpcId.NurseSalma, 10)
            .Build();

        session.ModifyNpcTrust(NpcId.NurseSalma, 5);

        var relationship = session.Relationships.GetNpcRelationship(NpcId.NurseSalma);
        relationship.Trust.Should().Be(15);
    }

    [Test]
    public async Task Outcome_FactionRep_UpdatesStanding()
    {
        var session = new GameStateBuilder()
            .WithFactionReputation(FactionId.ImbabaCrew, 20)
            .Build();

        session.ModifyFactionReputation(FactionId.ImbabaCrew, 5);

        var standing = session.Relationships.GetFactionStanding(FactionId.ImbabaCrew);
        standing.Reputation.Should().Be(25);
    }

    [Test]
    public async Task Outcome_StoryFlag_CanBeChecked()
    {
        var session = new GameStateBuilder().Build();

        session.HasStoryFlag("test_flag").Should().BeFalse();

        session.SetStoryFlag("test_flag");

        session.HasStoryFlag("test_flag").Should().BeTrue();
    }

    [Test]
    public async Task Outcome_MotherHealth_ClampsToValidRange()
    {
        var session = new GameStateBuilder()
            .WithMotherHealth(50)
            .Build();

        session.Player.Household.UpdateMotherHealth(60);
        session.Player.Household.MotherHealth.Should().Be(100);

        session.Player.Household.UpdateMotherHealth(-200);
        session.Player.Household.MotherHealth.Should().Be(0);
    }

    [Test]
    public async Task Outcome_PolicePressure_ClampsToValidRange()
    {
        var session = new GameStateBuilder().Build();

        session.SetPolicePressure(150);
        session.PolicePressure.Should().Be(100);

        session.SetPolicePressure(-10);
        session.PolicePressure.Should().Be(0);
    }

    [Test]
    public async Task Outcome_MultipleTargetedEffects_AllAppliedInOrder()
    {
        var session = new GameStateBuilder()
            .WithMoney(100)
            .Build();

        var outcome = new NarrativeOutcome
        {
            Effects =
            [
                new FavorEffect(NpcId.NurseSalma),
                new DebtEffect(NpcId.NurseSalma, true),
                new NpcTrustEffect(NpcId.NeighborMona, 2),
                new NpcTrustEffect(NpcId.LandlordHajjMahmoud, 1),
                new FactionReputationEffect(FactionId.ImbabaCrew, 10),
                new EmbarrassedEffect(NpcId.WorkshopBossAbuSamir, true),
                new HelpedEffect(NpcId.CafeOwnerNadia, true),
                new RefusalEffect(NpcId.RunnerYoussef)
            ]
        };

        session.ApplyOutcome(outcome);

        session.Relationships.GetNpcRelationship(NpcId.NurseSalma).LastFavorDay.Should().BeGreaterThan(0);
        session.Relationships.GetNpcRelationship(NpcId.NurseSalma).HasUnpaidDebt.Should().BeTrue();
        session.Relationships.GetNpcRelationship(NpcId.NeighborMona).Trust.Should().Be(2);
        session.Relationships.GetNpcRelationship(NpcId.LandlordHajjMahmoud).Trust.Should().Be(1);
        session.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation.Should().Be(10);
        session.Relationships.GetNpcRelationship(NpcId.WorkshopBossAbuSamir).WasEmbarrassed.Should().BeTrue();
        session.Relationships.GetNpcRelationship(NpcId.CafeOwnerNadia).WasHelped.Should().BeTrue();
        session.Relationships.GetNpcRelationship(NpcId.RunnerYoussef).LastRefusalDay.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task Outcome_FavorDebt_TracksState()
    {
        var outcome = new NarrativeOutcome
        {
            Effects =
            [
                new FavorEffect(NpcId.NurseSalma),
                new DebtEffect(NpcId.NurseSalma, true)
            ]
        };

        outcome.Effects.OfType<FavorEffect>().Should().ContainSingle(effect => effect.Npc == NpcId.NurseSalma);
        outcome.Effects.OfType<DebtEffect>().Should().ContainSingle(effect => effect.Npc == NpcId.NurseSalma && effect.HasUnpaidDebt);
    }

    [Test]
    public async Task Outcome_Embarrassment_TracksState()
    {
        var outcome = new NarrativeOutcome
        {
            Effects = [new EmbarrassedEffect(NpcId.WorkshopBossAbuSamir, true)]
        };

        outcome.Effects.OfType<EmbarrassedEffect>().Should().ContainSingle(effect => effect.Npc == NpcId.WorkshopBossAbuSamir && effect.Value);
    }
}
