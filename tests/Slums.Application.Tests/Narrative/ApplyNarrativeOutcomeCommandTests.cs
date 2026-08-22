using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Core.Diagnostics;
using Slums.Core.Endings;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit;

namespace Slums.Application.Tests.Narrative;

internal sealed class ApplyNarrativeOutcomeCommandTests
{
    [Test]
    public void Execute_ShouldApplyAllEffectsAndRecordSourceMutation()
    {
        var session = new GameSession();
        var initialMoney = session.Player.Stats.Money;
        var initialEnergy = session.Player.Stats.Energy;

        ApplyNarrativeOutcomeCommand.Execute(
            session,
            "event_test_scene",
            new NarrativeOutcome
            {
                MoneyChange = 25,
                HealthChange = -5,
                EnergyChange = -10,
                HungerChange = -8,
                StressChange = 12,
                MotherHealthChange = -4,
                FoodChange = 2,
                SetFlags = ["scene_seen", "second_flag"],
                Effects =
                [
                    new NpcTrustEffect(NpcId.NeighborMona, 3),
                    new FactionReputationEffect(FactionId.ImbabaCrew, 2)
                ],
                Message = "The scene is recorded."
            });

        session.Player.Stats.Money.Should().Be(initialMoney + 25);
        session.Player.Stats.Energy.Should().Be(initialEnergy - 10);
        session.HasStoryFlag("scene_seen").Should().BeTrue();
        session.HasStoryFlag("second_flag").Should().BeTrue();
        session.Relationships.GetNpcRelationship(NpcId.NeighborMona).Trust.Should().Be(3);
        session.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation.Should().Be(2);
        session.EventJournal.Entries.Should().Contain(entry => entry.Message == "The scene is recorded.");

        var mutation = session.Mutations.Should().ContainSingle(record => record.Category == MutationCategories.Narrative).Subject;
        mutation.Action.Should().Be("ApplyNarrativeOutcome");
        mutation.Reason.Should().Contain("event_test_scene");
        mutation.Reason.Should().Contain("The scene is recorded.");
        mutation.Before["Money"].Should().Be(initialMoney);
        mutation.After["Money"].Should().Be(initialMoney + 25);
    }

    [Test]
    public void Execute_ShouldTriggerDestitutionWhenNarrativeKillsPlayerHealth()
    {
        var session = new GameSession();
        session.Player.Stats.SetHealth(5);

        ApplyNarrativeOutcomeCommand.Execute(
            session,
            "event_lethal_health",
            new NarrativeOutcome { HealthChange = -10 });

        session.IsGameOver.Should().BeTrue();
        session.EndingId.Should().Be(EndingId.Destitution);
        session.Mutations.Should().Contain(record => record.Category == MutationCategories.EndingTriggered);
        session.Mutations.Should().Contain(record => record.Category == MutationCategories.Narrative);
    }

    [Test]
    public void Execute_ShouldTriggerMotherDiedWhenNarrativeReducesMotherHealthToZero()
    {
        var session = new GameSession();
        session.Player.Household.SetMotherHealth(5);

        ApplyNarrativeOutcomeCommand.Execute(
            session,
            "event_mother_loss",
            new NarrativeOutcome { MotherHealthChange = -10 });

        session.IsGameOver.Should().BeTrue();
        session.EndingId.Should().Be(EndingId.MotherDied);
    }
}
