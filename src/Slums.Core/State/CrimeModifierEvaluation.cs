using Slums.Core.Crimes;

namespace Slums.Core.State;

internal sealed record CrimeModifierEvaluation(CrimeAttempt Attempt, IReadOnlyList<string> ActiveModifiers);
