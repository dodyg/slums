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

        var flags = outcome.SetFlags.Count > 0
            ? outcome.SetFlags
            : outcome.SetFlag is { } legacyFlag ? [legacyFlag] : [];
        foreach (var flag in flags.Where(static flag => !string.IsNullOrWhiteSpace(flag)).Distinct(StringComparer.Ordinal))
        {
            state.SetStoryFlag(flag);
        }

        foreach (var effect in outcome.Effects)
        {
            switch (effect)
            {
                case NpcTrustEffect trust:
                    state.ModifyNpcTrust(trust.Npc, trust.Change);
                    break;
                case FactionReputationEffect reputation:
                    state.ModifyFactionReputation(reputation.Faction, reputation.Change);
                    break;
                case FavorEffect favor:
                    state.RecordFavor(favor.Npc, hasUnpaidDebt: false);
                    break;
                case RefusalEffect refusal:
                    state.RecordRefusal(refusal.Npc);
                    break;
                case DebtEffect debt:
                    state.SetDebtState(debt.Npc, debt.HasUnpaidDebt);
                    break;
                case EmbarrassedEffect embarrassed:
                    state.SetEmbarrassedState(embarrassed.Npc, embarrassed.Value);
                    break;
                case HelpedEffect helped:
                    state.SetHelpedState(helped.Npc, helped.Value);
                    break;
                case RentPaymentEffect rentPayment:
                    state.ApplyRentPayment(rentPayment.Amount);
                    break;
                case RentGraceDaysEffect rentGrace:
                    state.GrantRentGraceDays(rentGrace.Days);
                    break;
                case DebtPaymentEffect debtPayment:
                    state.ApplyDebtPayment(debtPayment.Source, debtPayment.Amount);
                    break;
                case DebtDueExtensionEffect debtExtension:
                    state.ExtendDebtDueDate(debtExtension.Source, debtExtension.Days);
                    break;
                case RamadanFastingEffect fasting:
                    state.SetRamadanFasting(fasting.IsFasting);
                    break;
                case CrisisEvidenceEffect evidence:
                    state.CollectCrisisEvidence(evidence.Amount);
                    break;
                case CrisisResourcesEffect resources:
                    state.CommitCrisisResources(resources.Amount);
                    break;
                case CrisisDecisionEffect decision:
                    state.ChooseCrisisDecision(decision.Decision);
                    break;
                case CrisisResolutionEffect resolution:
                    state.ResolveCityCrisis(resolution.Resolution);
                    break;
                case PolicePressureEffect police:
                    state.AdjustPolicePressure(police.Change);
                    break;
                case EndingCommitmentEffect ending:
                    state.CommitEnding(ending.Ending, ending.Sacrifice);
                    break;
                case CentralCharacterDecisionEffect decision:
                    if (!state.RecordCentralCharacterDecision(decision.Character, decision.Decision))
                    {
                        throw new InvalidOperationException($"Central decision '{decision.Decision}' does not belong to '{decision.Character}'.");
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown narrative effect type: {effect.GetType().Name}");
            }
        }

        if (!string.IsNullOrWhiteSpace(outcome.Message))
        {
            state.AddEventMessage(outcome.Message);
        }
    }
}
