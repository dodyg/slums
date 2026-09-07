using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Narrative.Ink;
using TUnit.Core;

namespace Slums.Narrative.Ink.Tests.Coverage;

internal sealed class NarrativeInfrastructureCoverageTests
{
    [Test]
    public void NarrativeOutcomeMerger_ShouldAccumulateEffectsFlagsAndMessages()
    {
        var existing = new NarrativeOutcome
        {
            MoneyChange = 10,
            SetFlag = "first_flag",
            Message = "First result",
            Effects = [new PolicePressureEffect(2)]
        };
        var next = new NarrativeOutcome
        {
            MoneyChange = -4,
            SetFlags = ["second_flag"],
            Message = "Second result",
            Effects = [new PolicePressureEffect(3)]
        };

        var merged = NarrativeOutcomeMerger.MergeOutcome(existing, next);

        merged.MoneyChange.Should().Be(6);
        merged.SetFlags.Should().Equal("first_flag", "second_flag");
        merged.Message.Should().Be("First result Second result");
        merged.Effects.Should().HaveCount(2);
    }

    [Test]
    public void NarrativeOutcomeMerger_ShouldReturnTheNextOutcomeWhenNoExistingOutcomeExists()
    {
        var next = new NarrativeOutcome { Message = "Only result" };

        NarrativeOutcomeMerger.MergeOutcome(null, next).Should().BeSameAs(next);
    }

    [Test]
    public void InkChoiceAuditor_ShouldFindNoDuplicateChoicesInTheCompiledStory()
    {
        var audits = InkChoiceAuditor.Audit(InkStoryLoader.LoadStoryJson());

        audits.Should().NotBeEmpty();
        audits.Should().OnlyContain(static audit => !audit.HasDuplicateChoiceText);
    }
}
