using FluentAssertions;
using Slums.Core.Narrative;
using TUnit.Core;

namespace Slums.Core.Tests.Narrative;

internal sealed class CentralCharacterKnotMapTests
{
    [Test]
    [Arguments("central_mother_arc", CentralCharacterId.Mother)]
    [Arguments("central_mona_transaction", CentralCharacterId.NeighborMona)]
    [Arguments("central_salma_outcome", CentralCharacterId.NurseSalma)]
    [Arguments("central_mahmoud_conflict", CentralCharacterId.HajjMahmoud)]
    [Arguments("central_ummkarim_vulnerability", CentralCharacterId.UmmKarim)]
    public void ResolveCharacter_MapsEveryAuthoredPrefix(string knotName, CentralCharacterId expected)
    {
        CentralCharacterKnotMap.ResolveCharacter(knotName).Should().Be(expected);
    }

    [Test]
    public void ResolveCharacter_ReturnsNullForUnknownPrefix()
    {
        CentralCharacterKnotMap.ResolveCharacter("central_unknown_arc").Should().BeNull();
    }
}
