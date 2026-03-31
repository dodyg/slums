using Slums.Core.Characters;
using Slums.Core.Economy;
using Slums.Core.Heat;
using Slums.Core.Information;
using Slums.Core.Relationships;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Information;

internal sealed class TipGeneratorTests
{
    [Test]
    public async Task GenerateTips_NoTrustedContacts_ReturnsEmpty()
    {
        var relationships = new RelationshipState();
        var economy = new NpcEconomyState();
        economy.Initialize();

        var tips = TipGenerator.GenerateTips(
            1, relationships, new DistrictHeatState(), economy,
            BackgroundType.MedicalSchoolDropout, 0, 0, new Random(1));

        await Assert.That(tips).Count().IsEqualTo(0);
    }

    [Test]
    public async Task GenerateTips_OfficerKhalidHighTrust_CanGeneratePoliceTip()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.OfficerKhalid, 25, 0);
        var districtHeat = new DistrictHeatState();
        districtHeat.SetHeat(DistrictId.Imbaba, 50);

        var economy = new NpcEconomyState();
        economy.Initialize();

        var found = false;
        for (var i = 0; i < 100; i++)
        {
            var tips = TipGenerator.GenerateTips(
                1, relationships, districtHeat, economy,
                BackgroundType.MedicalSchoolDropout, 0, 0, new Random(i));

            if (tips.Any(t => t.Type == TipType.PoliceTip && t.Source == NpcId.OfficerKhalid))
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GenerateTips_OfficerKhalidLowTrust_NoPoliceTip()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.OfficerKhalid, 5, 0);
        var districtHeat = new DistrictHeatState();
        districtHeat.SetHeat(DistrictId.Imbaba, 50);

        var economy = new NpcEconomyState();
        economy.Initialize();

        for (var i = 0; i < 100; i++)
        {
            var tips = TipGenerator.GenerateTips(
                1, relationships, districtHeat, economy,
                BackgroundType.MedicalSchoolDropout, 0, 0, new Random(i));

            await Assert.That(tips.Any(t => t.Source == NpcId.OfficerKhalid)).IsFalse();
        }
    }

    [Test]
    public async Task GenerateTips_UmmKarimHighTrust_CanGeneratePoliceTip()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.FixerUmmKarim, 20, 0);
        var districtHeat = new DistrictHeatState();
        districtHeat.SetHeat(DistrictId.Imbaba, 50);

        var economy = new NpcEconomyState();
        economy.Initialize();

        var found = false;
        for (var i = 0; i < 100; i++)
        {
            var tips = TipGenerator.GenerateTips(
                1, relationships, districtHeat, economy,
                BackgroundType.MedicalSchoolDropout, 0, 0, new Random(i));

            if (tips.Any(t => t.Type == TipType.PoliceTip && t.Source == NpcId.FixerUmmKarim))
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GenerateTips_EmployerHighTrust_CanGenerateJobLead()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.NurseSalma, 15, 0);

        var economy = new NpcEconomyState();
        economy.Initialize();

        var found = false;
        for (var i = 0; i < 100; i++)
        {
            var tips = TipGenerator.GenerateTips(
                1, relationships, new DistrictHeatState(), economy,
                BackgroundType.MedicalSchoolDropout, 0, 0, new Random(i));

            if (tips.Any(t => t.Type == TipType.JobLead))
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GenerateTips_CriminalContactHighTrust_CanGenerateCrimeWarning()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.FenceHanan, 15, 0);

        var economy = new NpcEconomyState();
        economy.Initialize();

        var found = false;
        for (var i = 0; i < 100; i++)
        {
            var tips = TipGenerator.GenerateTips(
                1, relationships, new DistrictHeatState(), economy,
                BackgroundType.MedicalSchoolDropout, 5, 0, new Random(i));

            if (tips.Any(t => t.Type == TipType.CrimeWarning))
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GenerateTips_CrimeWarning_NoCrimesCommitted_ReturnsNone()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.FenceHanan, 15, 0);

        var economy = new NpcEconomyState();
        economy.Initialize();

        for (var i = 0; i < 100; i++)
        {
            var tips = TipGenerator.GenerateTips(
                1, relationships, new DistrictHeatState(), economy,
                BackgroundType.MedicalSchoolDropout, 0, 0, new Random(i));

            await Assert.That(tips.Any(t => t.Type == TipType.CrimeWarning)).IsFalse();
        }
    }

    [Test]
    public async Task GenerateTips_PoliceTipWithHighHeat_IsEmergency()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.OfficerKhalid, 25, 0);
        var districtHeat = new DistrictHeatState();
        districtHeat.SetHeat(DistrictId.Imbaba, 80);

        var economy = new NpcEconomyState();
        economy.Initialize();

        var found = false;
        for (var i = 0; i < 100; i++)
        {
            var tips = TipGenerator.GenerateTips(
                1, relationships, districtHeat, economy,
                BackgroundType.MedicalSchoolDropout, 0, 0, new Random(i));

            var emergency = tips.FirstOrDefault(t => t.Type == TipType.PoliceTip && t.IsEmergency);
            if (emergency is not null)
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GenerateTips_TipsExpireWithinOneToTwoDays()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.OfficerKhalid, 25, 0);
        var districtHeat = new DistrictHeatState();
        districtHeat.SetHeat(DistrictId.Imbaba, 50);

        var economy = new NpcEconomyState();
        economy.Initialize();

        for (var i = 0; i < 100; i++)
        {
            var tips = TipGenerator.GenerateTips(
                1, relationships, districtHeat, economy,
                BackgroundType.MedicalSchoolDropout, 0, 0, new Random(i));

            foreach (var tip in tips)
            {
                await Assert.That(tip.ExpiresAfterDay - tip.DayGenerated).IsGreaterThanOrEqualTo(1);
                await Assert.That(tip.ExpiresAfterDay - tip.DayGenerated).IsLessThanOrEqualTo(2);
            }
        }
    }

    [Test]
    public async Task GenerateTips_MultipleTipsCanCoexist()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.OfficerKhalid, 25, 0);
        relationships.SetNpcRelationship(NpcId.NurseSalma, 15, 0);
        relationships.SetNpcRelationship(NpcId.FenceHanan, 15, 0);

        var districtHeat = new DistrictHeatState();
        districtHeat.SetHeat(DistrictId.Imbaba, 50);

        var economy = new NpcEconomyState();
        economy.Initialize();

        var foundMultiple = false;
        for (var i = 0; i < 200; i++)
        {
            var tips = TipGenerator.GenerateTips(
                1, relationships, districtHeat, economy,
                BackgroundType.MedicalSchoolDropout, 5, 0, new Random(i));

            if (tips.Count >= 2)
            {
                foundMultiple = true;
                break;
            }
        }

        await Assert.That(foundMultiple).IsTrue();
    }

    [Test]
    public async Task GenerateTips_SudaneseRefugee_BackgroundTip()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.FixerUmmKarim, 15, 0);

        var economy = new NpcEconomyState();
        economy.Initialize();

        var found = false;
        for (var i = 0; i < 200; i++)
        {
            var tips = TipGenerator.GenerateTips(
                1, relationships, new DistrictHeatState(), economy,
                BackgroundType.SudaneseRefugee, 0, 0, new Random(i));

            if (tips.Any(t => t.Content.Contains("Dokki", StringComparison.Ordinal)))
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GenerateTips_MarketIntel_FromPharmacistMariam()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.PharmacistMariam, 10, 0);

        var economy = new NpcEconomyState();
        economy.Initialize();

        var found = false;
        for (var i = 0; i < 200; i++)
        {
            var tips = TipGenerator.GenerateTips(
                1, relationships, new DistrictHeatState(), economy,
                BackgroundType.MedicalSchoolDropout, 0, 0, new Random(i));

            if (tips.Any(t => t.Type == TipType.MarketIntel && t.Source == NpcId.PharmacistMariam))
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }
}
