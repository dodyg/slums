namespace Slums.Core.Narrative;

/// <summary>Remembers concrete central-character decisions separately from affinity signals.</summary>
public sealed class CentralCharacterArcState
{
    private readonly Dictionary<CentralCharacterId, int> _beats = [];
    private readonly Dictionary<CentralCharacterId, CentralArcDecision> _decisions = [];

    public IReadOnlyDictionary<CentralCharacterId, int> Beats => _beats;
    public IReadOnlyDictionary<CentralCharacterId, CentralArcDecision> Decisions => _decisions;

    public int GetBeat(CentralCharacterId character) => _beats.GetValueOrDefault(character);

    public bool HasDecision(CentralCharacterId character) => _decisions.ContainsKey(character);

    public bool RecordDecision(CentralCharacterId character, CentralArcDecision decision)
    {
        if (!IsDecisionForCharacter(character, decision))
        {
            return false;
        }

        _decisions[character] = decision;
        _beats[character] = Math.Max(2, GetBeat(character));
        return true;
    }

    public void MarkBeat(CentralCharacterId character)
    {
        _beats[character] = Math.Min(6, GetBeat(character) + 1);
    }

    public CentralArcDecision? GetDecision(CentralCharacterId character)
    {
        return _decisions.TryGetValue(character, out var decision) ? decision : null;
    }

    public void Restore(IReadOnlyDictionary<string, int>? beats, IReadOnlyDictionary<string, string>? decisions)
    {
        _beats.Clear();
        _decisions.Clear();
        if (beats is not null)
        {
            foreach (var pair in beats)
            {
                if (Enum.TryParse<CentralCharacterId>(pair.Key, out var character))
                {
                    _beats[character] = Math.Clamp(pair.Value, 0, 6);
                }
            }
        }

        if (decisions is not null)
        {
            foreach (var pair in decisions)
            {
                if (Enum.TryParse<CentralCharacterId>(pair.Key, out var character)
                    && Enum.TryParse<CentralArcDecision>(pair.Value, out var decision)
                    && IsDecisionForCharacter(character, decision))
                {
                    _decisions[character] = decision;
                }
            }
        }
    }

    private static bool IsDecisionForCharacter(CentralCharacterId character, CentralArcDecision decision)
    {
        return character switch
        {
            CentralCharacterId.Mother => decision is CentralArcDecision.MotherAcceptCare or CentralArcDecision.MotherKeepPrivate,
            CentralCharacterId.NeighborMona => decision is CentralArcDecision.MonaShareRota or CentralArcDecision.MonaKeepReserve,
            CentralCharacterId.NurseSalma => decision is CentralArcDecision.SalmaPublishEvidence or CentralArcDecision.SalmaProtectPatient,
            CentralCharacterId.HajjMahmoud => decision is CentralArcDecision.MahmoudOpenLedger or CentralArcDecision.MahmoudProtectReputation,
            CentralCharacterId.UmmKarim => decision is CentralArcDecision.UmmKarimShareWarning or CentralArcDecision.UmmKarimLimitExposure,
            _ => false
        };
    }
}
