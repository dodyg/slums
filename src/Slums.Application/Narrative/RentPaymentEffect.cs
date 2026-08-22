namespace Slums.Application.Narrative;

/// <summary>Applies a payment to the accumulated rent debt.</summary>
public sealed record RentPaymentEffect(int Amount) : NarrativeEffect;
