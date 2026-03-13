using Slums.Core.Relationships;

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
    public void AddEventMessage(string message);
}
