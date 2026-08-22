namespace Slums.Application.Narrative;

/// <summary>Records the player's Ramadan fasting choice for the active holiday.</summary>
public sealed record RamadanFastingEffect(bool IsFasting) : NarrativeEffect;
