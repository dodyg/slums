using Slums.Core.State;

namespace Slums.Core.Events;

public sealed record RandomEvent(
    string Id,
    string Description,
    RandomEventEffect Effect,
    int MinDay,
    int Weight,
    Func<GameSession, bool>? Condition);