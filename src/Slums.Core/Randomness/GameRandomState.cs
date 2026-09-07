namespace Slums.Core.Randomness;

/// <summary>Serializable internal state of a <see cref="GameRandom"/>.</summary>
/// <param name="S0">xoshiro256** state word 0.</param>
/// <param name="S1">xoshiro256** state word 1.</param>
/// <param name="S2">xoshiro256** state word 2.</param>
/// <param name="S3">xoshiro256** state word 3.</param>
public sealed record GameRandomState(ulong S0, ulong S1, ulong S2, ulong S3);
