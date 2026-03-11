using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Application.Narrative;

public interface INarrativeService
{
    public bool IsSceneActive { get; }
    public string? CurrentText { get; }
    public IReadOnlyList<string> CurrentChoices { get; }
    public string? LastKnot { get; }

    public void StartScene(string knotName, GameState gameState);
    public void SelectChoice(int choiceIndex);
    public void EndScene();

    public NarrativeOutcome? GetPendingOutcome();
    public void ClearPendingOutcome();
}

public sealed record NarrativeOutcome
{
    public int MoneyChange { get; init; }
    public int HealthChange { get; init; }
    public int EnergyChange { get; init; }
    public int HungerChange { get; init; }
    public int StressChange { get; init; }
    public int MotherHealthChange { get; init; }
    public int FoodChange { get; init; }
    public string? SetFlag { get; init; }
    public string Message { get; init; } = string.Empty;
    public NpcId? NpcTrustTarget { get; init; }
    public int NpcTrustChange { get; init; }
    public FactionId? FactionTarget { get; init; }
    public int FactionReputationChange { get; init; }
    public NpcId? FavorTarget { get; init; }
    public NpcId? RefusalTarget { get; init; }
    public NpcId? DebtTarget { get; init; }
    public bool? DebtState { get; init; }
    public NpcId? EmbarrassedTarget { get; init; }
    public bool? EmbarrassedState { get; init; }
    public NpcId? HelpedTarget { get; init; }
    public bool? HelpedState { get; init; }
}
