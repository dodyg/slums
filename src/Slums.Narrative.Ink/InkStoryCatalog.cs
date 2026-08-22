using Ink.Runtime;
using Slums.Core.Narrative;

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
            , ["district"] = typeof(string)
            , ["weather"] = typeof(string)
            , ["season"] = typeof(string)
            , ["holiday"] = typeof(string)
            , ["is_ramadan"] = typeof(bool)
            , ["is_fasting"] = typeof(bool)
            , ["unpaid_rent_days"] = typeof(int)
            , ["rent_debt"] = typeof(int)
            , ["rent_grace_days"] = typeof(int)
            , ["police_pressure"] = typeof(int)
            , ["operational_robot_count"] = typeof(int)
            , ["active_news_count"] = typeof(int)
            , ["infrastructure_disruption_count"] = typeof(int)
            , ["mona_trust"] = typeof(int)
            , ["salma_trust"] = typeof(int)
            , ["conversation_variant"] = typeof(string)
            , ["conversation_context"] = typeof(string)
            , ["conversation_npc"] = typeof(string)
            , ["conversation_opener"] = typeof(int)
            , ["conversation_body"] = typeof(int)
            , ["crisis_phase"] = typeof(string)
            , ["crisis_evidence"] = typeof(int)
            , ["crisis_resources"] = typeof(int)
            , ["crisis_condition"] = typeof(int)
            , ["crisis_decision"] = typeof(string)
            , ["crisis_resolution_state"] = typeof(string)
            , ["pending_ending"] = typeof(string)
            , ["handset_data_exposure"] = typeof(int)
            , ["microgrid_repair_debt"] = typeof(int)
            , ["microgrid_storage_condition"] = typeof(int)
            , ["transit_permit_review"] = typeof(bool)
            , ["biometric_appeal_pending"] = typeof(bool)
            , ["last_telemedicine_triage_day"] = typeof(int)
            , ["allocation_model_confidence"] = typeof(int)
            , ["mother_arc_decision"] = typeof(string)
            , ["mona_arc_decision"] = typeof(string)
            , ["salma_arc_decision"] = typeof(string)
            , ["mahmoud_arc_decision"] = typeof(string)
            , ["ummkarim_arc_decision"] = typeof(string)
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

    /// <summary>Validates the explicit Core entry-knot contract against a compiled story.</summary>
    public static void ValidateEntryKnots(Story story)
    {
        ArgumentNullException.ThrowIfNull(story);

        var names = story.mainContentContainer.namedOnlyContent.Keys.ToHashSet(StringComparer.Ordinal);
        var missing = NarrativeEntryKnotCatalog.RequiredEntryKnots.Where(knot => !names.Contains(knot)).Order(StringComparer.Ordinal).ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidOperationException($"Compiled Ink story is missing declared entry knots: {string.Join(", ", missing)}.");
        }

        var unclassified = NarrativeEntryKnotCatalog.GetUnclassified(names);
        if (unclassified.Count > 0)
        {
            throw new InvalidOperationException($"Compiled Ink story contains unclassified top-level knots: {string.Join(", ", unclassified)}.");
        }
    }

    public static IReadOnlyList<InkChoiceAudit> GetChoiceAudit()
    {
        return InkChoiceAuditor.Audit(InkStoryFactory.Create(InkStoryLoader.LoadStoryJson()));
    }
}
