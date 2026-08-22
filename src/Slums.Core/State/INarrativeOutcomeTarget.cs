using Slums.Core.Economy;
using Slums.Core.Endings;
using Slums.Core.Relationships;
using Slums.Core.Narrative;

namespace Slums.Core.State;

public interface INarrativeOutcomeTarget
{
    public int CurrentDay { get; }

    public void AdjustMoney(int delta);
    public void AdjustHealth(int delta);
    public void AdjustEnergy(int delta);
    public void AdjustHunger(int delta);
    public void AdjustStress(int delta);
    public void AdjustMotherHealth(int delta);
    public void AdjustFoodStockpile(int delta);
    public void SetStoryFlag(string flag);
    public void ModifyNpcTrust(NpcId npcId, int delta);
    public void RecordFavor(NpcId npcId, bool hasUnpaidDebt);
    public void RecordRefusal(NpcId npcId);
    public void SetDebtState(NpcId npcId, bool hasUnpaidDebt);
    public void SetEmbarrassedState(NpcId npcId, bool value);
    public void SetHelpedState(NpcId npcId, bool value);
    public void ModifyFactionReputation(FactionId factionId, int delta);
    public void ApplyRentPayment(int amount);
    public void GrantRentGraceDays(int days);
    public void ApplyDebtPayment(DebtSource source, int amount);
    public void ExtendDebtDueDate(DebtSource source, int days);
    public void SetRamadanFasting(bool isFasting);
    public bool CollectCrisisEvidence(int amount);
    public bool CommitCrisisResources(int amount);
    public bool ChooseCrisisDecision(CityCrisisDecision decision);
    public bool ResolveCityCrisis(CityCrisisResolution resolution);
    public void AdjustPolicePressure(int delta);
    public void CommitEnding(EndingId endingId, string sacrifice);
    public bool RecordCentralCharacterDecision(CentralCharacterId character, CentralArcDecision decision);
    public void AddEventMessage(string message);
}
