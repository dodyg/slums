using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Slums.Core.Tests")]
[assembly: InternalsVisibleTo("Slums.Infrastructure")]
[assembly: InternalsVisibleTo("Slums.Application.Tests")]
[assembly: InternalsVisibleTo("Slums.Infrastructure.Tests")]
[assembly: InternalsVisibleTo("Slums.Narrative.Ink.Tests")]

// Single documented exception to CA5394 for this assembly: every gameplay roll intentionally
// uses the non-cryptographic GameRandom (xoshiro256**) whose full state is captured in save
// files so runs reproduce exactly. This assembly contains no security-sensitive randomness.
// GameRandom.FromEntropy is the only source of unseeded gameplay randomness.
[assembly: SuppressMessage(
    "Security",
    "CA5394:Do not use insecure randomness",
    Justification = "Gameplay randomness is intentionally non-cryptographic and persisted with the session for reproducibility.")]
