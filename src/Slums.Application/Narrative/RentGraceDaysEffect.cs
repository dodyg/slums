namespace Slums.Application.Narrative;

/// <summary>Extends the rent deadline by granting a bounded number of grace days.</summary>
public sealed record RentGraceDaysEffect(int Days) : NarrativeEffect;
