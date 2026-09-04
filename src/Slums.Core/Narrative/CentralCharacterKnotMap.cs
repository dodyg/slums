namespace Slums.Core.Narrative;

/// <summary>Maps authored central-character knot prefixes to their typed arc identities.</summary>
internal static class CentralCharacterKnotMap
{
    internal static CentralCharacterId? ResolveCharacter(string knotName)
    {
        ArgumentNullException.ThrowIfNull(knotName);

        return knotName switch
        {
            var knot when knot.StartsWith("central_mother_", StringComparison.Ordinal) => CentralCharacterId.Mother,
            var knot when knot.StartsWith("central_mona_", StringComparison.Ordinal) => CentralCharacterId.NeighborMona,
            var knot when knot.StartsWith("central_salma_", StringComparison.Ordinal) => CentralCharacterId.NurseSalma,
            var knot when knot.StartsWith("central_mahmoud_", StringComparison.Ordinal) => CentralCharacterId.HajjMahmoud,
            var knot when knot.StartsWith("central_ummkarim_", StringComparison.Ordinal) => CentralCharacterId.UmmKarim,
            _ => null
        };
    }
}
