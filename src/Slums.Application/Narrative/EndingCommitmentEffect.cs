using Slums.Core.Endings;

namespace Slums.Application.Narrative;

public sealed record EndingCommitmentEffect(EndingId Ending, string Sacrifice) : NarrativeEffect;
