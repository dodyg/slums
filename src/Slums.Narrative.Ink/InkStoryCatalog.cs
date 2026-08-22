using Ink.Runtime;

namespace Slums.Narrative.Ink;

/// <summary>
/// Enumerates the authored knots of the compiled Ink story. Used by bootstrap validation to
/// reject repo-owned content that references knots the story does not declare.
/// </summary>
public static class InkStoryCatalog
{
    private static readonly IReadOnlyDictionary<string, Type> RequiredGlobalTypes =
        new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            ["gender"] = typeof(string),
            ["background"] = typeof(string),
            ["money"] = typeof(int),
            ["health"] = typeof(int),
            ["energy"] = typeof(int),
            ["hunger"] = typeof(int),
            ["stress"] = typeof(int),
            ["mother_health"] = typeof(int),
            ["food_stockpile"] = typeof(int),
            ["day"] = typeof(int)
        };

    /// <summary>Gets the gameplay globals that every compiled story must declare.</summary>
    public static IReadOnlyDictionary<string, Type> RequiredGlobals => RequiredGlobalTypes;

    /// <summary>Validates that a compiled story exposes every synchronized gameplay global.</summary>
    public static void ValidateRequiredGlobals(Story story)
    {
        ArgumentNullException.ThrowIfNull(story);

        foreach (var requiredGlobal in RequiredGlobalTypes)
        {
            if (!story.variablesState.GlobalVariableExistsWithName(requiredGlobal.Key))
            {
                throw new InvalidOperationException(
                    $"Compiled Ink story is missing required global '{requiredGlobal.Key}'.");
            }

            var value = story.variablesState[requiredGlobal.Key];
            if (value is null || value.GetType() != requiredGlobal.Value)
            {
                var actualType = value?.GetType().Name ?? "null";
                throw new InvalidOperationException(
                    $"Compiled Ink global '{requiredGlobal.Key}' has type '{actualType}', expected '{requiredGlobal.Value.Name}'.");
            }
        }
    }

    public static IReadOnlySet<string> GetKnotNames()
    {
        var story = InkStoryFactory.Create(InkStoryLoader.LoadStoryJson());
        return story.mainContentContainer.namedOnlyContent.Keys.ToHashSet(StringComparer.Ordinal);
    }
}
