using Slums.Core.Economy;

namespace Slums.Application.Narrative;

/// <summary>Extends one specific debt's due date.</summary>
public sealed record DebtDueExtensionEffect(DebtSource Source, int Days) : NarrativeEffect;
