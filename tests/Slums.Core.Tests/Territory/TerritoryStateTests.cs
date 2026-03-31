using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.Territory;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Territory;

internal sealed class TerritoryStateTests
{
    [Test]
    public async Task Initialize_SetsUpAllDistricts()
    {
        var territory = new TerritoryState();

        territory.Initialize(BackgroundType.SudaneseRefugee);

        foreach (DistrictId district in Enum.GetValues<DistrictId>())
        {
            var control = territory.GetControl(district);
            await Assert.That(control.FactionInfluence).IsNotEmpty();
        }
    }

    [Test]
    public async Task Initialize_OnlyRunsOnce()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        var before = territory.GetControl(DistrictId.Imbaba).FactionInfluence[FactionId.ImbabaCrew];
        territory.Initialize(BackgroundType.SudaneseRefugee);
        var after = territory.GetControl(DistrictId.Imbaba).FactionInfluence[FactionId.ImbabaCrew];

        await Assert.That(after).IsEqualTo(before);
    }

    [Test]
    public async Task Initialize_ExPrisonerNetwork_GetsBonusInfluence()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.ReleasedPoliticalPrisoner);

        var imbabaNetwork = territory.GetControl(DistrictId.Imbaba).FactionInfluence[FactionId.ExPrisonerNetwork];

        var normalTerritory = new TerritoryState();
        normalTerritory.Initialize(BackgroundType.SudaneseRefugee);
        var normalImbabaNetwork = normalTerritory.GetControl(DistrictId.Imbaba).FactionInfluence[FactionId.ExPrisonerNetwork];

        await Assert.That(imbabaNetwork).IsGreaterThan(normalImbabaNetwork);
    }

    [Test]
    public async Task ModifyTension_IncreasesTension()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        var before = territory.GetControl(DistrictId.Imbaba).Tension;
        territory.ModifyTension(DistrictId.Imbaba, 20);
        var after = territory.GetControl(DistrictId.Imbaba).Tension;

        await Assert.That(after).IsEqualTo(before + 20);
    }

    [Test]
    public async Task ModifyTension_ClampsToHundred()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        territory.ModifyTension(DistrictId.Imbaba, 200);

        await Assert.That(territory.GetControl(DistrictId.Imbaba).Tension).IsEqualTo(100);
    }

    [Test]
    public async Task ModifyTension_ClampsToZero()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        territory.ModifyTension(DistrictId.Imbaba, -200);

        await Assert.That(territory.GetControl(DistrictId.Imbaba).Tension).IsEqualTo(0);
    }

    [Test]
    public async Task ModifyInfluence_IncreasesInfluence()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        var before = territory.GetControl(DistrictId.Imbaba).FactionInfluence[FactionId.ImbabaCrew];
        territory.ModifyInfluence(DistrictId.Imbaba, FactionId.ImbabaCrew, 15);
        var after = territory.GetControl(DistrictId.Imbaba).FactionInfluence[FactionId.ImbabaCrew];

        await Assert.That(after).IsEqualTo(before + 15);
    }

    [Test]
    public async Task ModifyInfluence_ClampsToHundred()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        territory.ModifyInfluence(DistrictId.Imbaba, FactionId.ImbabaCrew, 200);

        await Assert.That(territory.GetControl(DistrictId.Imbaba).FactionInfluence[FactionId.ImbabaCrew]).IsEqualTo(100);
    }

    [Test]
    public async Task RestoreEntry_OverwritesDistrict()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        var newInfluence = new Dictionary<FactionId, int>
        {
            [FactionId.ImbabaCrew] = 10,
            [FactionId.DokkiThugs] = 80,
            [FactionId.ExPrisonerNetwork] = 5
        };
        territory.RestoreEntry(DistrictId.Imbaba, newInfluence, 50, 10);

        var control = territory.GetControl(DistrictId.Imbaba);
        await Assert.That(control.FactionInfluence[FactionId.DokkiThugs]).IsEqualTo(80);
        await Assert.That(control.Tension).IsEqualTo(50);
        await Assert.That(control.LastConflictDay).IsEqualTo(10);
    }

    [Test]
    public async Task ControllingFaction_ReturnsFaction_WhenAboveThreshold()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        var control = territory.GetControl(DistrictId.Imbaba);

        await Assert.That(control.ControllingFaction).IsEqualTo(FactionId.ImbabaCrew);
    }

    [Test]
    public async Task ControllingFaction_ReturnsNull_WhenNoFactionAboveThreshold()
    {
        var territory = new TerritoryState();
        territory.Initialize(BackgroundType.SudaneseRefugee);

        var control = territory.GetControl(DistrictId.Shubra);

        await Assert.That(control.ControllingFaction).IsNull();
    }

    [Test]
    public async Task TensionLevel_IsNormal_WhenBelowThirtyOne()
    {
        var control = new TerritoryControl(
            DistrictId.Imbaba,
            new Dictionary<FactionId, int> { [FactionId.ImbabaCrew] = 60 },
            30, 0);

        await Assert.That(control.TensionLevel).IsEqualTo(TensionLevel.Normal);
    }

    [Test]
    public async Task TensionLevel_IsElevated_WhenBetweenThirtyOneAndFifty()
    {
        var control = new TerritoryControl(
            DistrictId.Imbaba,
            new Dictionary<FactionId, int> { [FactionId.ImbabaCrew] = 60 },
            45, 0);

        await Assert.That(control.TensionLevel).IsEqualTo(TensionLevel.Elevated);
    }

    [Test]
    public async Task TensionLevel_IsHigh_WhenBetweenFiftyOneAndSeventy()
    {
        var control = new TerritoryControl(
            DistrictId.Imbaba,
            new Dictionary<FactionId, int> { [FactionId.ImbabaCrew] = 60 },
            65, 0);

        await Assert.That(control.TensionLevel).IsEqualTo(TensionLevel.High);
    }

    [Test]
    public async Task TensionLevel_IsDangerous_WhenAboveSeventy()
    {
        var control = new TerritoryControl(
            DistrictId.Imbaba,
            new Dictionary<FactionId, int> { [FactionId.ImbabaCrew] = 60 },
            80, 0);

        await Assert.That(control.TensionLevel).IsEqualTo(TensionLevel.Dangerous);
    }
}
