using Slums.Core.State;

namespace Slums.Application.Narrative;

public static class NarrativeSceneExtensions
{
    public static void ApplyOutcome(this INarrativeOutcomeTarget state, NarrativeOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(outcome);

        if (outcome.MoneyChange != 0)
        {
            state.AdjustMoney(outcome.MoneyChange);
        }

        if (outcome.HealthChange != 0)
        {
            state.AdjustHealth(outcome.HealthChange);
        }

        if (outcome.EnergyChange != 0)
        {
            state.AdjustEnergy(outcome.EnergyChange);
        }

        if (outcome.HungerChange != 0)
        {
            state.AdjustHunger(outcome.HungerChange);
        }

        if (outcome.StressChange != 0)
        {
            state.AdjustStress(outcome.StressChange);
        }

        if (outcome.MotherHealthChange != 0)
        {
            state.AdjustMotherHealth(outcome.MotherHealthChange);
        }

        if (outcome.FoodChange != 0)
        {
            state.AdjustFoodStockpile(outcome.FoodChange);
        }

        if (!string.IsNullOrWhiteSpace(outcome.SetFlag))
        {
            state.SetStoryFlag(outcome.SetFlag);
        }

        if (outcome.NpcTrustTarget is not null && outcome.NpcTrustChange != 0)
        {
            state.ModifyNpcTrust(outcome.NpcTrustTarget.Value, outcome.NpcTrustChange);
        }

        if (outcome.FavorTarget is not null)
        {
            state.RecordFavor(outcome.FavorTarget.Value, outcome.DebtState == true);
        }

        if (outcome.RefusalTarget is not null)
        {
            state.RecordRefusal(outcome.RefusalTarget.Value);
        }

        if (outcome.DebtTarget is not null && outcome.DebtState is not null)
        {
            state.SetDebtState(outcome.DebtTarget.Value, outcome.DebtState.Value);
        }

        if (outcome.EmbarrassedTarget is not null && outcome.EmbarrassedState is not null)
        {
            state.SetEmbarrassedState(outcome.EmbarrassedTarget.Value, outcome.EmbarrassedState.Value);
        }

        if (outcome.HelpedTarget is not null && outcome.HelpedState is not null)
        {
            state.SetHelpedState(outcome.HelpedTarget.Value, outcome.HelpedState.Value);
        }

        if (outcome.FactionTarget is not null && outcome.FactionReputationChange != 0)
        {
            state.ModifyFactionReputation(outcome.FactionTarget.Value, outcome.FactionReputationChange);
        }

        if (!string.IsNullOrWhiteSpace(outcome.Message))
        {
            state.AddEventMessage(outcome.Message);
        }
    }
}
