namespace Slums.Core.Narrative;

/// <summary>
/// Defines the naming contract for authored player-visible Ink entry knots.
/// </summary>
public static class NarrativeEntryKnotCatalog
{
    // These knots are entered by a recurring scene's choice or by an older authored route;
    // they are support content, not independent player-triggered scenes.
    private static readonly HashSet<string> SupportOnlyKnots = new(StringComparer.Ordinal)
    {
        "fixer_double_life",
        "fixer_first_contact",
        "hanan_fence",
        "landlord_rent_broke",
        "landlord_rent_negotiation",
        "mariam_pharmacy_urgent",
        "neighbor_mona_heat",
        "nurse_salma",
        "nurse_salma_debt",
        "safaa_depot_regular"
    };
    private static readonly HashSet<string> RequiredKnots = typeof(NarrativeKnots)
        .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
        .Where(static field => field.FieldType == typeof(string))
        .Select(static field => (string)field.GetValue(null)!)
        .ToHashSet(StringComparer.Ordinal);

    /// <summary>Gets the explicit Core-triggered entry knots.</summary>
    public static IReadOnlySet<string> RequiredEntryKnots => RequiredKnots;

    /// <summary>
    /// Returns whether a top-level knot follows a player-visible entry naming convention.
    /// Ordinary continuation knots remain inside their parent scene and are not entries.
    /// </summary>
    public static bool IsPlayerVisible(string knotName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(knotName);

        return RequiredKnots.Contains(knotName)
            || knotName.StartsWith("intro_", StringComparison.Ordinal)
            || knotName.StartsWith("ending_", StringComparison.Ordinal)
            || knotName.StartsWith("event_", StringComparison.Ordinal)
            || knotName.StartsWith("weather_", StringComparison.Ordinal)
            || knotName.StartsWith("season_", StringComparison.Ordinal)
            || knotName.StartsWith("community_", StringComparison.Ordinal)
            || knotName.StartsWith("debt_", StringComparison.Ordinal)
            || knotName.StartsWith("recurring_", StringComparison.Ordinal)
            || knotName.StartsWith("central_", StringComparison.Ordinal)
            || knotName.StartsWith("crime_", StringComparison.Ordinal)
            || IsRecurringConversation(knotName);
    }

    /// <summary>Finds authored top-level knots that have no declared entry classification.</summary>
    public static IReadOnlyList<string> GetUnclassified(IEnumerable<string> knotNames)
    {
        ArgumentNullException.ThrowIfNull(knotNames);

        return knotNames
            .Where(static knot => !string.Equals(knot, "global decl", StringComparison.Ordinal))
            .Where(static knot => !IsPlayerVisible(knot) && !SupportOnlyKnots.Contains(knot))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsRecurringConversation(string knotName)
    {
        var separator = knotName.LastIndexOf('_');
        return separator > 0 && int.TryParse(knotName[(separator + 1)..], out _);
    }
}
