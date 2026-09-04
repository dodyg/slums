using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.Narrative;

namespace Slums.Core.State;

public sealed partial class GameSession
{
    public bool CollectCrisisEvidence(int amount = 1) => CityCrisis.CollectEvidence(amount);

    public bool CommitCrisisResources(int amount)
    {
        var committed = CityCrisis.CommitResources(amount);
        if (committed)
        {
            Technology.RecordMicrogridRepair(amount);
        }

        return committed;
    }

    public bool ChooseCrisisDecision(CityCrisisDecision decision) => CityCrisis.ChooseDecision(decision, Clock.Day);

    public bool MarkCrisisCallbackQueued()
    {
        if (!CityCrisis.HasDueCallback(Clock.Day))
        {
            return false;
        }

        CityCrisis.MarkCallbackQueued();
        return true;
    }

    public bool ResolveCityCrisis(CityCrisisResolution resolution) => CityCrisis.Resolve(resolution);

    public bool RecordCentralCharacterDecision(CentralCharacterId character, CentralArcDecision decision)
    {
        return CentralCharacterArcs.RecordDecision(character, decision);
    }

    public void AdjustPolicePressure(int delta)
        => CrimeSessionService.AdjustPolicePressure(this, delta);
}
