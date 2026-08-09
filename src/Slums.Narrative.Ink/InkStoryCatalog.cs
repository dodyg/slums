namespace Slums.Narrative.Ink;

/// <summary>
/// Enumerates the authored knots of the compiled Ink story. Used by bootstrap validation to
/// reject repo-owned content that references knots the story does not declare.
/// </summary>
public static class InkStoryCatalog
{
    public static IReadOnlySet<string> GetKnotNames()
    {
        var story = InkStoryFactory.Create(InkStoryLoader.LoadStoryJson());
        return story.mainContentContainer.namedOnlyContent.Keys.ToHashSet(StringComparer.Ordinal);
    }
}
