using Slums.Core.Narrative;
using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionCrisisSnapshot
{
    public int BeatIndex { get; init; }
    public int EvidenceCollected { get; init; }
    public int ResourcesCommitted { get; init; }
    public int CooperativeCondition { get; init; } = 70;
    public CityCrisisDecision Decision { get; init; }
    public CityCrisisResolution Resolution { get; init; }

    public static GameSessionCrisisSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionCrisisSnapshot
        {
            BeatIndex = gameSession.CityCrisis.BeatIndex,
            EvidenceCollected = gameSession.CityCrisis.EvidenceCollected,
            ResourcesCommitted = gameSession.CityCrisis.ResourcesCommitted,
            CooperativeCondition = gameSession.CityCrisis.CooperativeCondition,
            Decision = gameSession.CityCrisis.Decision,
            Resolution = gameSession.CityCrisis.Resolution
        };
    }

    public void Restore(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        gameSession.RestoreCityCrisisState(BeatIndex, EvidenceCollected, ResourcesCommitted, CooperativeCondition, Decision, Resolution);
    }
}
