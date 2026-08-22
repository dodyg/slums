using FluentAssertions;
using Slums.Core.Relationships;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Relationships;

internal sealed class NpcDistrictRegistryTests
{
    [Test]
    public void Registry_ShouldMatchCanonicalWorkshopAndRumorDistricts()
    {
        NpcDistrictRegistry.GetDistrict(NpcId.WorkshopBossAbuSamir).Should().Be(DistrictId.ArdAlLiwa);
        NpcRegistry.GetNpcsInDistrict(DistrictId.ArdAlLiwa).Should().Contain(NpcId.WorkshopBossAbuSamir);
        NpcRegistry.GetNpcsInDistrict(DistrictId.BulaqAlDakrour).Should().NotContain(NpcId.WorkshopBossAbuSamir);
    }

    [Test]
    public void Registry_ShouldAssignEveryRecurringNpcExactlyOnce()
    {
        var npcs = Enum.GetValues<NpcId>();
        var mapped = npcs.Select(NpcDistrictRegistry.GetDistrict).ToArray();

        mapped.Should().HaveSameCount(npcs);
        NpcDistrictRegistry.GetNpcsInDistrict(DistrictId.Imbaba)
            .Should().Contain([NpcId.LandlordHajjMahmoud, NpcId.NeighborMona]);
    }
}
