using Slums.Core.Characters;
using Slums.Core.Relationships;

namespace Slums.Application.Narrative;

public interface INarrativeService
{
    public bool IsSceneActive { get; }
    public string? CurrentText { get; }
    public IReadOnlyList<string> CurrentChoices { get; }
    public string? LastKnot { get; }

    public void StartScene(string knotName, NarrativeSceneState sceneState);
    public void RestoreProgress(string? lastKnot);
    public void SelectChoice(int choiceIndex);
    public void EndScene();

    public NarrativeOutcome? GetPendingOutcome();
    public void ClearPendingOutcome();
}

/// <summary>
/// A single typed narrative effect targeting an NPC or faction, applied to the game state in
/// the order the effects were produced. Multiple effects in one scene are preserved
/// individually so every target receives its own change.
/// </summary>
public abstract record NarrativeEffect;

public sealed record NarrativeOutcome
{
    public int MoneyChange { get; init; }
    public int HealthChange { get; init; }
    public int EnergyChange { get; init; }
    public int HungerChange { get; init; }
    public int StressChange { get; init; }
    public int MotherHealthChange { get; init; }
    public int FoodChange { get; init; }
    /// <summary>All story flags emitted by the scene, in authored order.</summary>
    public IReadOnlyList<string> SetFlags { get; init; } = [];

    /// <summary>
    /// Compatibility accessor for callers that only expect one flag. New code should use
    /// <see cref="SetFlags"/> so no authored flags are lost.
    /// </summary>
    public string? SetFlag { get; init; }
    public string Message { get; init; } = string.Empty;

    /// <summary>NPC/faction-targeted effects in the order they were produced.</summary>
    public IReadOnlyList<NarrativeEffect> Effects { get; init; } = [];
}
