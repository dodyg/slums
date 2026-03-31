using Slums.Core.Characters;
using Slums.Core.Heat;
using Slums.Core.Phone;
using Slums.Core.Relationships;
using TUnit.Core;

namespace Slums.Core.Tests.Phone;

internal sealed class PhoneMessageGeneratorTests
{
    [Test]
    public async Task GenerateMessages_NoTrustedContacts_ReturnsEmpty()
    {
        var relationships = new RelationshipState();
        var messages = PhoneMessageGenerator.GenerateMessages(
            1, relationships, 0, 70, new DistrictHeatState(),
            BackgroundType.MedicalSchoolDropout, new Random(1));

        await Assert.That(messages).Count().IsEqualTo(0);
    }

    [Test]
    public async Task GenerateMessages_CriminalWithHighTrust_CanGenerateOpportunity()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.FenceHanan, 15, 0);

        var found = false;
        for (var i = 0; i < 50; i++)
        {
            var messages = PhoneMessageGenerator.GenerateMessages(
                1, relationships, 0, 70, new DistrictHeatState(),
                BackgroundType.MedicalSchoolDropout, new Random(i));

            if (messages.Any(m => m.Type == PhoneMessageType.Opportunity))
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GenerateMessages_LowMotherHealth_CanGenerateFamilyAlert()
    {
        var relationships = new RelationshipState();

        var found = false;
        for (var i = 0; i < 50; i++)
        {
            var messages = PhoneMessageGenerator.GenerateMessages(
                1, relationships, 0, 20, new DistrictHeatState(),
                BackgroundType.MedicalSchoolDropout, new Random(i));

            if (messages.Any(m => m.Type == PhoneMessageType.FamilyAlert))
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GenerateMessages_HighMotherHealth_NoFamilyAlert()
    {
        var relationships = new RelationshipState();

        for (var i = 0; i < 20; i++)
        {
            var messages = PhoneMessageGenerator.GenerateMessages(
                1, relationships, 0, 80, new DistrictHeatState(),
                BackgroundType.MedicalSchoolDropout, new Random(i));

            await Assert.That(messages.Any(m => m.Type == PhoneMessageType.FamilyAlert)).IsFalse();
        }
    }

    [Test]
    public async Task GenerateMessages_HighFactionRep_CanGenerateNetworkRequest()
    {
        var relationships = new RelationshipState();
        relationships.SetFactionStanding(FactionId.ImbabaCrew, 8);

        var found = false;
        for (var i = 0; i < 50; i++)
        {
            var messages = PhoneMessageGenerator.GenerateMessages(
                1, relationships, 0, 70, new DistrictHeatState(),
                BackgroundType.MedicalSchoolDropout, new Random(i));

            if (messages.Any(m => m.Type == PhoneMessageType.NetworkRequest))
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GenerateMessages_SudaneseRefugee_CanGenerateBackgroundMessage()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 8, 0);

        var found = false;
        for (var i = 0; i < 50; i++)
        {
            var messages = PhoneMessageGenerator.GenerateMessages(
                1, relationships, 0, 70, new DistrictHeatState(),
                BackgroundType.SudaneseRefugee, new Random(i));

            if (messages.Any(m => m.Type == PhoneMessageType.Background))
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GenerateMessages_ReleasedPrisoner_CanGenerateBackgroundMessage()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.RunnerYoussef, 8, 0);

        var found = false;
        for (var i = 0; i < 50; i++)
        {
            var messages = PhoneMessageGenerator.GenerateMessages(
                1, relationships, 0, 70, new DistrictHeatState(),
                BackgroundType.ReleasedPoliticalPrisoner, new Random(i));

            if (messages.Any(m => m.Type == PhoneMessageType.Background && m.SenderNpcId == "RunnerYoussef"))
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GenerateMessages_MedicalDropout_CanGenerateBackgroundMessage()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.NurseSalma, 8, 0);

        var found = false;
        for (var i = 0; i < 50; i++)
        {
            var messages = PhoneMessageGenerator.GenerateMessages(
                1, relationships, 0, 70, new DistrictHeatState(),
                BackgroundType.MedicalSchoolDropout, new Random(i));

            if (messages.Any(m => m.Type == PhoneMessageType.Background && m.SenderNpcId == "NurseSalma"))
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GenerateMessages_LowTrustCriminal_NoOpportunity()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.FenceHanan, 3, 0);

        for (var i = 0; i < 20; i++)
        {
            var messages = PhoneMessageGenerator.GenerateMessages(
                1, relationships, 0, 70, new DistrictHeatState(),
                BackgroundType.MedicalSchoolDropout, new Random(i));

            await Assert.That(messages.Any(m => m.Type == PhoneMessageType.Opportunity)).IsFalse();
        }
    }
}
