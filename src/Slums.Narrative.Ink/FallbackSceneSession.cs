using Slums.Application.Narrative;
using Slums.Core.State;

namespace Slums.Narrative.Ink;

internal sealed class FallbackSceneSession
{
    private readonly FallbackSceneDefinition _definition;
    private readonly GameState _gameState;
    private FallbackNode _currentNode;

    public FallbackSceneSession(FallbackSceneDefinition definition, GameState gameState)
    {
        _definition = definition;
        _gameState = gameState;
        _currentNode = definition.Nodes[definition.StartNodeId];
    }

    public string CurrentText => _currentNode.TextFactory(_gameState);

    public IReadOnlyList<string> CurrentChoices => _currentNode.Choices.Select(static choice => choice.Text).ToArray();

    public NarrativeOutcome? SelectChoice(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= _currentNode.Choices.Count)
        {
            return null;
        }

        var choice = _currentNode.Choices[choiceIndex];
        if (!string.IsNullOrWhiteSpace(choice.NextNodeId) && _definition.Nodes.TryGetValue(choice.NextNodeId, out var nextNode))
        {
            _currentNode = nextNode;
        }

        return choice.Outcome;
    }
}