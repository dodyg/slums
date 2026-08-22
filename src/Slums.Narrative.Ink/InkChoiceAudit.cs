using Ink.Runtime;

namespace Slums.Narrative.Ink;

public sealed record InkChoiceAudit(
    string KnotName,
    int ChoiceCount,
    IReadOnlyList<string> ChoiceTexts,
    bool HasDuplicateChoiceText);

public static class InkChoiceAuditor
{
    public static IReadOnlyList<InkChoiceAudit> Audit(Story story)
    {
        ArgumentNullException.ThrowIfNull(story);

        var audits = new List<InkChoiceAudit>();
        foreach (var knotName in story.mainContentContainer.namedOnlyContent.Keys
                     .Where(static knot => knot != "global decl")
                     .Order(StringComparer.Ordinal))
        {
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
