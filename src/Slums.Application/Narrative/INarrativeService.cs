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
