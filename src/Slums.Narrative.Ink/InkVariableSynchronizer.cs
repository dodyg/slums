using Ink.Runtime;
using Slums.Application.Narrative;

namespace Slums.Narrative.Ink;

/// <summary>Synchronizes the application scene snapshot into Ink globals.</summary>
internal static class InkVariableSynchronizer
{
    internal static void SyncVariablesToInk(Story story, NarrativeSceneState sceneState)
    {
        ArgumentNullException.ThrowIfNull(story);
        ArgumentNullException.ThrowIfNull(sceneState);

        TrySetGlobalVariable(story, "money", sceneState.Money);
        TrySetGlobalVariable(story, "health", sceneState.Health);
        TrySetGlobalVariable(story, "energy", sceneState.Energy);
        TrySetGlobalVariable(story, "hunger", sceneState.Hunger);
        TrySetGlobalVariable(story, "stress", sceneState.Stress);
        TrySetGlobalVariable(story, "mother_health", sceneState.MotherHealth);
        TrySetGlobalVariable(story, "food_stockpile", sceneState.FoodStockpile);
        TrySetGlobalVariable(story, "day", sceneState.Day);
        TrySetGlobalVariable(story, "district", sceneState.District);
        TrySetGlobalVariable(story, "weather", sceneState.Weather);
        TrySetGlobalVariable(story, "season", sceneState.Season);
        TrySetGlobalVariable(story, "holiday", sceneState.Holiday);
        TrySetGlobalVariable(story, "is_ramadan", sceneState.IsRamadan);
        TrySetGlobalVariable(story, "is_fasting", sceneState.IsRamadanFasting);
        TrySetGlobalVariable(story, "unpaid_rent_days", sceneState.UnpaidRentDays);
        TrySetGlobalVariable(story, "rent_debt", sceneState.RentDebt);
        TrySetGlobalVariable(story, "rent_grace_days", sceneState.RentGraceDays);
        TrySetGlobalVariable(story, "police_pressure", sceneState.PolicePressure);
        TrySetGlobalVariable(story, "operational_robot_count", sceneState.OperationalRobots.Count);
        TrySetGlobalVariable(story, "active_news_count", sceneState.ActiveNews.Count);
        TrySetGlobalVariable(story, "infrastructure_disruption_count", sceneState.Infrastructure.Count);
        TrySetGlobalVariable(story, "mona_trust", sceneState.RelationshipTrust.GetValueOrDefault("NeighborMona"));
        TrySetGlobalVariable(story, "salma_trust", sceneState.RelationshipTrust.GetValueOrDefault("NurseSalma"));
        TrySetGlobalVariable(story, "conversation_variant", sceneState.ConversationVariantId);
        TrySetGlobalVariable(story, "conversation_context", sceneState.ConversationContext);
        TrySetGlobalVariable(story, "conversation_npc", sceneState.ConversationNpc);

        var variantParts = sceneState.ConversationVariantId.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (variantParts.Length >= 2
            && int.TryParse(variantParts[^2], out var opener)
            && int.TryParse(variantParts[^1], out var body))
        {
            TrySetGlobalVariable(story, "conversation_opener", opener);
            TrySetGlobalVariable(story, "conversation_body", body);
        }

        TrySetGlobalVariable(story, "crisis_phase", sceneState.CrisisPhase.ToString());
        TrySetGlobalVariable(story, "crisis_evidence", sceneState.CrisisEvidenceCollected);
        TrySetGlobalVariable(story, "crisis_resources", sceneState.CrisisResourcesCommitted);
        TrySetGlobalVariable(story, "crisis_condition", sceneState.CrisisCooperativeCondition);
        TrySetGlobalVariable(story, "crisis_decision", sceneState.CrisisDecision.ToString());
        TrySetGlobalVariable(story, "crisis_resolution_state", sceneState.CrisisResolution.ToString());
        TrySetGlobalVariable(story, "pending_ending", sceneState.PendingEnding);
        TrySetGlobalVariable(story, "handset_data_exposure", sceneState.HandsetDataExposure);
        TrySetGlobalVariable(story, "microgrid_repair_debt", sceneState.MicrogridRepairDebt);
        TrySetGlobalVariable(story, "microgrid_storage_condition", sceneState.MicrogridStorageCondition);
        TrySetGlobalVariable(story, "transit_permit_review", sceneState.TransitPermitReview);
        TrySetGlobalVariable(story, "biometric_appeal_pending", sceneState.BiometricAppealPending);
        TrySetGlobalVariable(story, "last_telemedicine_triage_day", sceneState.LastTelemedicineTriageDay);
        TrySetGlobalVariable(story, "allocation_model_confidence", sceneState.AllocationModelConfidence);
        TrySetGlobalVariable(story, "mother_arc_decision", sceneState.CentralDecisions.GetValueOrDefault("Mother", string.Empty));
        TrySetGlobalVariable(story, "mona_arc_decision", sceneState.CentralDecisions.GetValueOrDefault("NeighborMona", string.Empty));
        TrySetGlobalVariable(story, "salma_arc_decision", sceneState.CentralDecisions.GetValueOrDefault("NurseSalma", string.Empty));
        TrySetGlobalVariable(story, "mahmoud_arc_decision", sceneState.CentralDecisions.GetValueOrDefault("HajjMahmoud", string.Empty));
        TrySetGlobalVariable(story, "ummkarim_arc_decision", sceneState.CentralDecisions.GetValueOrDefault("UmmKarim", string.Empty));

        if (!string.IsNullOrWhiteSpace(sceneState.Background))
        {
            TrySetGlobalVariable(story, "background", sceneState.Background);
        }

        if (!string.IsNullOrWhiteSpace(sceneState.Gender))
        {
            TrySetGlobalVariable(story, "gender", sceneState.Gender);
        }
    }

    internal static void TrySetGlobalVariable(Story story, string variableName, object value)
    {
        ArgumentNullException.ThrowIfNull(story);
        ArgumentException.ThrowIfNullOrWhiteSpace(variableName);

        if (!story.variablesState.GlobalVariableExistsWithName(variableName))
        {
            return;
        }

        story.variablesState[variableName] = value;
    }
}
