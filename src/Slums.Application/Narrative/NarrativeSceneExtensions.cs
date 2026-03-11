using Slums.Core.Characters;
using Slums.Core.State;

namespace Slums.Application.Narrative;

public static class NarrativeSceneExtensions
{
    public static void ApplyOutcome(this GameState state, NarrativeOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(outcome);
        
        if (outcome.MoneyChange != 0)
        {
            state.Player.Stats.ModifyMoney(outcome.MoneyChange);
        }
        
        if (outcome.HealthChange != 0)
        {
            state.Player.Stats.ModifyHealth(outcome.HealthChange);
        }
        
        if (outcome.EnergyChange != 0)
        {
            state.Player.Stats.ModifyEnergy(outcome.EnergyChange);
        }
        
        if (outcome.HungerChange != 0)
        {
            state.Player.Nutrition.ModifySatiety(outcome.HungerChange);
            state.Player.Stats.SetHunger(state.Player.Nutrition.Satiety);
        }
        
        if (outcome.StressChange != 0)
        {
            state.Player.Stats.ModifyStress(outcome.StressChange);
        }
        
        if (outcome.MotherHealthChange != 0)
        {
            state.Player.Household.UpdateMotherHealth(outcome.MotherHealthChange);
        }
        
        if (outcome.FoodChange != 0)
        {
            if (outcome.FoodChange > 0)
            {
                state.Player.Household.AddFood(outcome.FoodChange);
            }
            else
            {
                for (var i = 0; i < -outcome.FoodChange; i++)
                {
                    state.Player.Household.ConsumeFood();
                }
            }
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
            state.Relationships.RecordFavor(outcome.FavorTarget.Value, state.Clock.Day, outcome.DebtState == true);
        }

        if (outcome.RefusalTarget is not null)
        {
            state.Relationships.RecordRefusal(outcome.RefusalTarget.Value, state.Clock.Day);
        }

        if (outcome.DebtTarget is not null && outcome.DebtState is not null)
        {
            state.Relationships.SetDebtState(outcome.DebtTarget.Value, outcome.DebtState.Value);
        }

        if (outcome.EmbarrassedTarget is not null && outcome.EmbarrassedState is not null)
        {
            state.Relationships.SetEmbarrassedState(outcome.EmbarrassedTarget.Value, outcome.EmbarrassedState.Value);
        }

        if (outcome.HelpedTarget is not null && outcome.HelpedState is not null)
        {
            state.Relationships.SetHelpedState(outcome.HelpedTarget.Value, outcome.HelpedState.Value);
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
