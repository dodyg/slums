using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.World;

internal sealed class LocationPricingServiceTests
{
    private readonly LocationPricingService _service = new();

    private static Location MakeClinic() => new()
    {
        Id = LocationId.Clinic,
        District = DistrictId.ArdAlLiwa,
        HasClinicServices = true,
        ClinicVisitBaseCost = 35
    };

    private static Location MakePharmacy() => new()
    {
        Id = LocationId.Pharmacy,
        District = DistrictId.BulaqAlDakrour,
        HasClinicServices = true,
        ClinicVisitBaseCost = 46
    };

    private static RelationshipState MakeRelationships(int salmaTrust = 0, int mariamTrust = 0, int safaaTrust = 0, int imanTrust = 0)
    {
        var rs = new RelationshipState();
        rs.SetNpcRelationship(NpcId.NurseSalma, salmaTrust, 0);
        rs.SetNpcRelationship(NpcId.PharmacistMariam, mariamTrust, 0);
        rs.SetNpcRelationship(NpcId.DispatcherSafaa, safaaTrust, 0);
        rs.SetNpcRelationship(NpcId.LaundryOwnerIman, imanTrust, 0);
        return rs;
    }

    private static SkillState MakeSkills(int medicalLevel = 0)
    {
        var skills = new SkillState();
        if (medicalLevel > 0)
        {
            skills.SetLevel(SkillId.Medical, medicalLevel);
        }
        return skills;
    }

    [Test]
    public async Task GetClinicVisitCost_ReturnsBaseCost_WithNoDiscounts()
    {
        var cost = _service.GetClinicVisitCost(MakeClinic(), new RelationshipState(), new SkillState());
        await Assert.That(cost).IsEqualTo(35);
    }

    [Test]
    public async Task GetClinicVisitCost_ReturnsZero_WhenBaseCostIsZero()
    {
        var location = new Location { Id = LocationId.Clinic, ClinicVisitBaseCost = 0 };
        var cost = _service.GetClinicVisitCost(location, new RelationshipState(), new SkillState());
        await Assert.That(cost).IsEqualTo(0);
    }

    [Test]
    public async Task GetClinicVisitCost_AppliesMedicalSkillDiscount()
    {
        var cost = _service.GetClinicVisitCost(MakeClinic(), new RelationshipState(), MakeSkills(2));
        await Assert.That(cost).IsEqualTo(30);
    }

    [Test]
    public async Task GetClinicVisitCost_MedicalDiscountFloorsAt20()
    {
        var location = new Location { Id = LocationId.Clinic, ClinicVisitBaseCost = 22 };
        var cost = _service.GetClinicVisitCost(location, new RelationshipState(), MakeSkills(3));
        await Assert.That(cost).IsEqualTo(20);
    }

    [Test]
    public async Task GetClinicVisitCost_AppliesSalmaTrustDiscount()
    {
        var cost = _service.GetClinicVisitCost(MakeClinic(), MakeRelationships(salmaTrust: 20), new SkillState());
        await Assert.That(cost).IsEqualTo(29);
    }

    [Test]
    public async Task GetClinicVisitCost_SalmaDiscountFloorsAt18()
    {
        var location = new Location { Id = LocationId.Clinic, ClinicVisitBaseCost = 20, HasClinicServices = true };
        var cost = _service.GetClinicVisitCost(location, MakeRelationships(salmaTrust: 50), new SkillState());
        await Assert.That(cost).IsEqualTo(18);
    }

    [Test]
    public async Task GetClinicVisitCost_AppliesMariamTrustDiscountAtPharmacy()
    {
        var cost = _service.GetClinicVisitCost(MakePharmacy(), MakeRelationships(mariamTrust: 12), new SkillState());
        await Assert.That(cost).IsEqualTo(42);
    }

    [Test]
    public async Task GetClinicVisitCost_MariamDiscountDoesNotApplyAtClinic()
    {
        var cost = _service.GetClinicVisitCost(MakeClinic(), MakeRelationships(mariamTrust: 20), new SkillState());
        await Assert.That(cost).IsEqualTo(35);
    }

    [Test]
    public async Task GetClinicVisitCost_SalmaDiscountDoesNotApplyAtPharmacy()
    {
        var cost = _service.GetClinicVisitCost(MakePharmacy(), MakeRelationships(salmaTrust: 30), new SkillState());
        await Assert.That(cost).IsEqualTo(46);
    }

    [Test]
    public async Task GetClinicVisitCost_StacksMedicalAndTrustDiscounts()
    {
        var cost = _service.GetClinicVisitCost(MakeClinic(), MakeRelationships(salmaTrust: 25), MakeSkills(2));
        await Assert.That(cost).IsEqualTo(24);
    }

    [Test]
    public async Task GetClinicVisitCost_StackedDiscountsStillFloorsAt18()
    {
        var location = new Location { Id = LocationId.Clinic, ClinicVisitBaseCost = 22, HasClinicServices = true };
        var cost = _service.GetClinicVisitCost(location, MakeRelationships(salmaTrust: 50), MakeSkills(3));
        await Assert.That(cost).IsEqualTo(18);
    }

    [Test]
    public async Task GetFoodCost_ReturnsCorrectCostPerDistrict()
    {
        await Assert.That(_service.GetFoodCost(DistrictId.Dokki)).IsEqualTo(20);
        await Assert.That(_service.GetFoodCost(DistrictId.Imbaba)).IsEqualTo(15);
        await Assert.That(_service.GetFoodCost(DistrictId.ArdAlLiwa)).IsEqualTo(13);
        await Assert.That(_service.GetFoodCost(DistrictId.BulaqAlDakrour)).IsEqualTo(14);
        await Assert.That(_service.GetFoodCost(DistrictId.Shubra)).IsEqualTo(17);
    }

    [Test]
    public async Task GetStreetFoodCost_ReturnsCorrectCostPerDistrict()
    {
        await Assert.That(_service.GetStreetFoodCost(DistrictId.Dokki)).IsEqualTo(10);
        await Assert.That(_service.GetStreetFoodCost(DistrictId.Imbaba)).IsEqualTo(8);
        await Assert.That(_service.GetStreetFoodCost(DistrictId.ArdAlLiwa)).IsEqualTo(7);
        await Assert.That(_service.GetStreetFoodCost(DistrictId.BulaqAlDakrour)).IsEqualTo(7);
        await Assert.That(_service.GetStreetFoodCost(DistrictId.Shubra)).IsEqualTo(9);
    }

    [Test]
    public async Task GetMedicineCost_ReturnsCorrectBasePerDistrict()
    {
        var rs = new RelationshipState();
        var skills = new SkillState();

        await Assert.That(_service.GetMedicineCost(DistrictId.Dokki, LocationId.CallCenter, rs, skills)).IsEqualTo(58);
        await Assert.That(_service.GetMedicineCost(DistrictId.Imbaba, LocationId.Home, rs, skills)).IsEqualTo(50);
        await Assert.That(_service.GetMedicineCost(DistrictId.ArdAlLiwa, LocationId.Clinic, rs, skills)).IsEqualTo(42);
        await Assert.That(_service.GetMedicineCost(DistrictId.BulaqAlDakrour, LocationId.Pharmacy, rs, skills)).IsEqualTo(46);
        await Assert.That(_service.GetMedicineCost(DistrictId.Shubra, LocationId.Laundry, rs, skills)).IsEqualTo(52);
    }

    [Test]
    public async Task GetMedicineCost_AppliesMariamTrustDiscountAtPharmacy()
    {
        var cost = _service.GetMedicineCost(DistrictId.BulaqAlDakrour, LocationId.Pharmacy, MakeRelationships(mariamTrust: 12), new SkillState());
        await Assert.That(cost).IsEqualTo(40);
    }

    [Test]
    public async Task GetMedicineCost_MariamDiscountCannotGoBelowFloor()
    {
        var cost = _service.GetMedicineCost(DistrictId.ArdAlLiwa, LocationId.Pharmacy, MakeRelationships(mariamTrust: 50), new SkillState());
        await Assert.That(cost).IsEqualTo(36);
    }

    [Test]
    public async Task GetMedicineCost_MariamDiscountHits30Floor()
    {
        var cost = _service.GetMedicineCost(DistrictId.ArdAlLiwa, LocationId.Pharmacy, MakeRelationships(mariamTrust: 50), new SkillState());
        await Assert.That(cost).IsEqualTo(36);
    }

    [Test]
    public async Task GetMedicineCost_AppliesMedicalSkillDiscount()
    {
        var cost = _service.GetMedicineCost(DistrictId.Imbaba, LocationId.Home, new RelationshipState(), MakeSkills(3));
        await Assert.That(cost).IsEqualTo(42);
    }

    [Test]
    public async Task GetMedicineCost_MedicalDiscountFloorsAt32()
    {
        var cost = _service.GetMedicineCost(DistrictId.ArdAlLiwa, LocationId.Clinic, new RelationshipState(), MakeSkills(4));
        await Assert.That(cost).IsEqualTo(34);
    }

    [Test]
    public async Task GetMedicineCost_StacksTrustAndSkillDiscounts()
    {
        var cost = _service.GetMedicineCost(DistrictId.BulaqAlDakrour, LocationId.Pharmacy, MakeRelationships(mariamTrust: 15), MakeSkills(3));
        await Assert.That(cost).IsEqualTo(32);
    }

    [Test]
    public async Task GetTravelCost_ReturnsDefaultCost()
    {
        var dest = new Location { District = DistrictId.Dokki };
        var cost = _service.GetTravelCost(dest, new RelationshipState());
        await Assert.That(cost).IsEqualTo(2);
    }

    [Test]
    public async Task GetTravelCost_AppliesSafaaDiscountAtBulaq()
    {
        var dest = new Location { District = DistrictId.BulaqAlDakrour };
        var cost = _service.GetTravelCost(dest, MakeRelationships(safaaTrust: 12));
        await Assert.That(cost).IsEqualTo(1);
    }

    [Test]
    public async Task GetTravelCost_SafaaDiscountFloorsAt1()
    {
        var dest = new Location { District = DistrictId.BulaqAlDakrour };
        var cost = _service.GetTravelCost(dest, MakeRelationships(safaaTrust: 50));
        await Assert.That(cost).IsEqualTo(1);
    }

    [Test]
    public async Task GetTravelCost_SafaaDiscountBelow12DoesNotApply()
    {
        var dest = new Location { District = DistrictId.BulaqAlDakrour };
        var cost = _service.GetTravelCost(dest, MakeRelationships(safaaTrust: 11));
        await Assert.That(cost).IsEqualTo(2);
    }

    [Test]
    public async Task GetTravelEnergyCost_ReturnsDefaultCost()
    {
        var dest = new Location { District = DistrictId.Dokki };
        var cost = _service.GetTravelEnergyCost(dest, new RelationshipState());
        await Assert.That(cost).IsEqualTo(5);
    }

    [Test]
    public async Task GetTravelEnergyCost_AppliesSafaaDiscountAtBulaq()
    {
        var dest = new Location { District = DistrictId.BulaqAlDakrour };
        var cost = _service.GetTravelEnergyCost(dest, MakeRelationships(safaaTrust: 12));
        await Assert.That(cost).IsEqualTo(3);
    }

    [Test]
    public async Task GetTravelEnergyCost_AppliesImanDiscountAtShubra()
    {
        var dest = new Location { District = DistrictId.Shubra };
        var cost = _service.GetTravelEnergyCost(dest, MakeRelationships(imanTrust: 12));
        await Assert.That(cost).IsEqualTo(4);
    }

    [Test]
    public async Task GetTravelEnergyCost_StacksSafaaAndImanForBulaq()
    {
        var dest = new Location { District = DistrictId.BulaqAlDakrour };
        var cost = _service.GetTravelEnergyCost(dest, MakeRelationships(safaaTrust: 12, imanTrust: 12));
        await Assert.That(cost).IsEqualTo(3);
    }

    [Test]
    public async Task GetTravelEnergyCost_ImanDiscountFloorsAt2()
    {
        var dest = new Location { District = DistrictId.Shubra };
        var cost = _service.GetTravelEnergyCost(dest, MakeRelationships(imanTrust: 50));
        await Assert.That(cost).IsEqualTo(4);
    }
}
