using Slums.Application.Narrative;
using Slums.Core.Economy;
using Slums.Core.Endings;
using Slums.Core.Narrative;
using Slums.Core.Relationships;

namespace Slums.Narrative.Ink;

/// <summary>Parses the authored Ink tag effect language.</summary>
internal static class InkTagEffectParser
{
    internal static NarrativeEffect? Parse(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (!TrySplit(tag, out var key, out var value))
        {
            return null;
        }

        return key switch
        {
            "NPC_TRUST" => ParseNpcTrustEffect(tag, value),
            "FACTION_REP" => ParseFactionReputationEffect(tag, value),
            "FAVOR" => new FavorEffect(ParseNpcTarget(tag, value)),
            "REFUSAL" => new RefusalEffect(ParseNpcTarget(tag, value)),
            "DEBT" => ParseDebtEffect(tag, value),
            "EMBARRASSED" => ParseBoolStateEffect(tag, value, static (npc, state) => new EmbarrassedEffect(npc, state)),
            "HELPED" => ParseBoolStateEffect(tag, value, static (npc, state) => new HelpedEffect(npc, state)),
            "RENT_PAYMENT" => new RentPaymentEffect(ParseIntEffect(tag, value)),
            "RENT_GRACE_DAYS" => new RentGraceDaysEffect(ParseIntEffect(tag, value)),
            "DEBT_PAYMENT" => ParseDebtAmountEffect(tag, value, static (source, amount) => new DebtPaymentEffect(source, amount)),
            "DEBT_DUE_EXTENSION" => ParseDebtAmountEffect(tag, value, static (source, amount) => new DebtDueExtensionEffect(source, amount)),
            "RAMADAN_FASTING" => ParseRamadanFasting(tag, value),
            "CRISIS_EVIDENCE" => new CrisisEvidenceEffect(ParsePositiveIntEffect(tag, value)),
            "CRISIS_RESOURCES" => new CrisisResourcesEffect(ParsePositiveIntEffect(tag, value)),
            "CRISIS_DECISION" => ParseCrisisDecision(tag, value),
            "CRISIS_RESOLUTION" => ParseCrisisResolution(tag, value),
            "POLICE" => new PolicePressureEffect(ParseIntEffect(tag, value)),
            "ENDING_COMMIT" => ParseEndingCommitment(tag, value),
            "CENTRAL_DECISION" => ParseCentralDecision(tag, value),
            _ => null
        };
    }

    internal static NarrativeOutcome? ParseOutcome(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (!TrySplit(tag, out var key, out var value))
        {
            return null;
        }

        return key switch
        {
            "FLAG" => new NarrativeOutcome { SetFlag = value, SetFlags = [value] },
            "MESSAGE" => new NarrativeOutcome { Message = value },
            "MONEY" => new NarrativeOutcome { MoneyChange = ParseIntEffect(tag, value) },
            "HEALTH" => new NarrativeOutcome { HealthChange = ParseIntEffect(tag, value) },
            "ENERGY" => new NarrativeOutcome { EnergyChange = ParseIntEffect(tag, value) },
            "HUNGER" => new NarrativeOutcome { HungerChange = ParseIntEffect(tag, value) },
            "STRESS" => new NarrativeOutcome { StressChange = ParseIntEffect(tag, value) },
            "MOTHER_HEALTH" => new NarrativeOutcome { MotherHealthChange = ParseIntEffect(tag, value) },
            "FOOD" => new NarrativeOutcome { FoodChange = ParseIntEffect(tag, value) },
            _ => Parse(tag) is { } effect ? new NarrativeOutcome { Effects = [effect] } : null
        };
    }

    private static bool TrySplit(string tag, out string key, out string value)
    {
        var parts = tag.Split(':', 2);
        if (parts.Length != 2)
        {
            key = string.Empty;
            value = string.Empty;
            return false;
        }

        key = parts[0].Trim().ToUpperInvariant();
        value = parts[1].Trim();
        return true;
    }

    private static int ParseIntEffect(string tag, string valueStr)
    {
        if (!int.TryParse(valueStr, out var value))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected an integer value.");
        }

        return value;
    }

    private static int ParsePositiveIntEffect(string tag, string valueStr)
    {
        var value = ParseIntEffect(tag, valueStr);
        if (value <= 0)
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected a positive integer value.");
        }

        return value;
    }

    private static NpcId ParseNpcTarget(string tag, string valueStr)
    {
        var npc = valueStr.Split(',', StringSplitOptions.TrimEntries)[0];
        if (!Enum.TryParse<NpcId>(npc, out var npcId) || !Enum.IsDefined(npcId))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': unknown NPC '{npc}'.");
        }

        return npcId;
    }

    private static NpcTrustEffect ParseNpcTrustEffect(string tag, string valueStr)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<NpcId>(parts[0], out var npcId) || !Enum.IsDefined(npcId) || !int.TryParse(parts[1], out var delta))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'NPC,delta'.");
        }

        return new NpcTrustEffect(npcId, delta);
    }

    private static FactionReputationEffect ParseFactionReputationEffect(string tag, string valueStr)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<FactionId>(parts[0], out var factionId) || !Enum.IsDefined(factionId) || !int.TryParse(parts[1], out var delta))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'Faction,delta'.");
        }

        return new FactionReputationEffect(factionId, delta);
    }

    private static DebtEffect ParseDebtEffect(string tag, string valueStr)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<NpcId>(parts[0], out var npcId) || !Enum.IsDefined(npcId) || !bool.TryParse(parts[1], out var debtState))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'NPC,true|false'.");
        }

        return new DebtEffect(npcId, debtState);
    }

    private static TEffect ParseDebtAmountEffect<TEffect>(string tag, string valueStr, Func<DebtSource, int, TEffect> factory)
        where TEffect : NarrativeEffect
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<DebtSource>(parts[0], out var source) || !Enum.IsDefined(source) || !int.TryParse(parts[1], out var amount) || amount <= 0)
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'DebtSource,positiveAmount'.");
        }

        return factory(source, amount);
    }

    private static NarrativeEffect ParseBoolStateEffect(string tag, string valueStr, Func<NpcId, bool, NarrativeEffect> factory)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<NpcId>(parts[0], out var npcId) || !Enum.IsDefined(npcId) || !bool.TryParse(parts[1], out var state))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'NPC,true|false'.");
        }

        return factory(npcId, state);
    }

    private static RamadanFastingEffect ParseRamadanFasting(string tag, string valueStr)
    {
        if (!bool.TryParse(valueStr, out var isFasting))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected true or false.");
        }

        return new RamadanFastingEffect(isFasting);
    }

    private static CrisisDecisionEffect ParseCrisisDecision(string tag, string valueStr)
    {
        if (!Enum.TryParse<CityCrisisDecision>(valueStr, out var decision) || !Enum.IsDefined(decision) || decision == CityCrisisDecision.None)
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': unknown crisis decision '{valueStr}'.");
        }

        return new CrisisDecisionEffect(decision);
    }

    private static CrisisResolutionEffect ParseCrisisResolution(string tag, string valueStr)
    {
        if (!Enum.TryParse<CityCrisisResolution>(valueStr, out var resolution) || !Enum.IsDefined(resolution) || resolution == CityCrisisResolution.Unresolved)
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': unknown crisis resolution '{valueStr}'.");
        }

        return new CrisisResolutionEffect(resolution);
    }

    private static EndingCommitmentEffect ParseEndingCommitment(string tag, string valueStr)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<EndingId>(parts[0], out var ending) || !Enum.IsDefined(ending) || string.IsNullOrWhiteSpace(parts[1]))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'EndingId,sacrifice'.");
        }

        return new EndingCommitmentEffect(ending, parts[1]);
    }

    private static CentralCharacterDecisionEffect ParseCentralDecision(string tag, string valueStr)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2
            || !Enum.TryParse<CentralCharacterId>(parts[0], out var character)
            || !Enum.IsDefined(character)
            || !Enum.TryParse<CentralArcDecision>(parts[1], out var decision)
            || !Enum.IsDefined(decision))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'Character,Decision'.");
        }

        return new CentralCharacterDecisionEffect(character, decision);
    }
}
