using Ink.Runtime;

namespace Slums.Narrative.Ink;

public sealed record InkChoiceAudit(
    string KnotName,
    int ChoiceCount,
    IReadOnlyList<string> ChoiceTexts,
    bool HasDuplicateChoiceText);
