namespace Slums.Narrative.Ink;

internal static class InkTagCatalog
{
    internal static IReadOnlySet<string> ValidKeys { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "NPC_TRUST", "FACTION_REP", "FAVOR", "REFUSAL", "DEBT", "EMBARRASSED", "HELPED",
        "RENT_PAYMENT", "RENT_GRACE_DAYS", "DEBT_PAYMENT", "DEBT_DUE_EXTENSION", "RAMADAN_FASTING",
        "CRISIS_EVIDENCE", "CRISIS_RESOURCES", "CRISIS_DECISION", "CRISIS_RESOLUTION", "POLICE",
        "CRIME_LOCK", "ENDING_COMMIT", "CENTRAL_DECISION", "FLAG", "MESSAGE", "MONEY", "HEALTH",
        "ENERGY", "HUNGER", "STRESS", "MOTHER_HEALTH", "FOOD"
    };
}
