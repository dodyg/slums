using Slums.Core.Economy;

namespace Slums.Application.Narrative;

/// <summary>Applies a payment to one specific player debt.</summary>
public sealed record DebtPaymentEffect(DebtSource Source, int Amount) : NarrativeEffect;
