using Ink.Runtime;

namespace Slums.Narrative.Ink;

public static class InkChoiceAuditor
{
    public static IReadOnlyList<InkChoiceAudit> Audit(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var template = InkStoryFactory.Create(json);
        var audits = new List<InkChoiceAudit>();
        foreach (var knotName in template.mainContentContainer.namedOnlyContent.Keys
                     .Where(static knot => knot != "global decl")
                     .Order(StringComparer.Ordinal))
        {
            var story = InkStoryFactory.Create(json);
            story.ChoosePathString(knotName);
            while (story.canContinue)
            {
                story.Continue();
            }

            var choices = story.currentChoices.Select(static choice => choice.text).ToArray();
            audits.Add(new InkChoiceAudit(
                knotName,
                choices.Length,
                choices,
                choices.GroupBy(static text => text, StringComparer.Ordinal).Any(static group => group.Count() > 1)));
        }

        return audits;
    }
}
