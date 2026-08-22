using FluentAssertions;
using Slums.Narrative.Ink;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Coverage;

internal sealed class ChoiceAuditTests
{
    [Test]
    public void CompiledStory_ShouldExposeAStableChoiceAudit()
    {
        var audits = InkStoryCatalog.GetChoiceAudit();

        audits.Should().NotBeEmpty();
        audits.Should().Contain(audit => audit.KnotName == "crisis_appeal" && audit.ChoiceCount == 2);
        audits.Should().OnlyContain(static audit => audit.ChoiceTexts.Distinct(StringComparer.Ordinal).Count() == audit.ChoiceTexts.Count);
    }
}
