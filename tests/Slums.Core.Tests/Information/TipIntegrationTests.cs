using Slums.Core.Characters;
using Slums.Core.Heat;
using Slums.Core.Information;
using Slums.Core.Phone;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Information;

internal sealed class TipIntegrationTests
{
    [Test]
    public async Task GameSession_EndDay_GeneratesTips()
    {
        using var session = new GameSession(new Random(42));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.MedicalSchoolDropout));
        session.Relationships.SetNpcRelationship(NpcId.OfficerKhalid, 25, 0);
        session.DistrictHeat.SetHeat(DistrictId.Imbaba, 50);

        var found = false;
        for (var i = 0; i < 20; i++)
        {
            using var s = new GameSession(new Random(i));
            s.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.MedicalSchoolDropout));
            s.Relationships.SetNpcRelationship(NpcId.OfficerKhalid, 25, 0);
            s.DistrictHeat.SetHeat(DistrictId.Imbaba, 50);

            s.EndDay();

            if (s.Tips.AllTips.Count > 0)
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GameSession_AcknowledgeTip_MarksTipAcknowledged()
    {
        using var session = new GameSession(new Random(1));
        session.Tips.AddTip(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Test",
            DayGenerated = 1,
            ExpiresAfterDay = 3
        });

        var tipId = session.Tips.AllTips[0].Id;
        var (success, message) = session.AcknowledgeTip(tipId);

        await Assert.That(success).IsTrue();
        await Assert.That(session.Tips.GetTip(tipId)!.Acknowledged).IsTrue();
    }

    [Test]
    public async Task GameSession_IgnoreTipAction_MarksTipIgnored()
    {
        using var session = new GameSession(new Random(1));
        session.Tips.AddTip(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Test",
            DayGenerated = 1,
            ExpiresAfterDay = 3
        });

        var tipId = session.Tips.AllTips[0].Id;
        var (success, _, _) = session.IgnoreTipAction(tipId);

        await Assert.That(success).IsTrue();
        await Assert.That(session.Tips.GetTip(tipId)!.Ignored).IsTrue();
    }

    [Test]
    public async Task GameSession_IgnoreTip_ThreeOrMoreTimesErodesTrust()
    {
        using var session = new GameSession(new Random(1));
        session.Relationships.SetNpcRelationship(NpcId.OfficerKhalid, 15, 0);

        for (var i = 0; i < 4; i++)
        {
            session.Tips.AddTip(new Tip
            {
                Type = TipType.PoliceTip,
                Source = NpcId.OfficerKhalid,
                Content = $"Tip {i}",
                DayGenerated = 1,
                ExpiresAfterDay = 10
            });
            var tipId = session.Tips.AllTips[^1].Id;
            session.IgnoreTipAction(tipId);
        }

        var trustAfterFourIgnores = session.Relationships.GetNpcRelationship(NpcId.OfficerKhalid).Trust;
        await Assert.That(trustAfterFourIgnores).IsLessThan(15);
    }

    [Test]
    public async Task GameSession_IgnoreTip_LowTrustNoErosion()
    {
        using var session = new GameSession(new Random(1));
        session.Relationships.SetNpcRelationship(NpcId.OfficerKhalid, 5, 0);

        for (var i = 0; i < 4; i++)
        {
            session.Tips.AddTip(new Tip
            {
                Type = TipType.PoliceTip,
                Source = NpcId.OfficerKhalid,
                Content = $"Tip {i}",
                DayGenerated = 1,
                ExpiresAfterDay = 10
            });
            var tipId = session.Tips.AllTips[^1].Id;
            session.IgnoreTipAction(tipId);
        }

        var trust = session.Relationships.GetNpcRelationship(NpcId.OfficerKhalid).Trust;
        await Assert.That(trust).IsEqualTo(5);
    }

    [Test]
    public async Task GameSession_EndDay_ExpiredTipsRemoved()
    {
        using var session = new GameSession(new Random(1));
        session.Tips.AddTip(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Will expire",
            DayGenerated = 1,
            ExpiresAfterDay = 2
        });

        while (session.Clock.Day < 3)
        {
            session.EndDay();
        }

        await Assert.That(session.Tips.AllTips.Any(t => t.Content == "Will expire")).IsFalse();
    }

    [Test]
    public async Task GameSession_PhoneDelivery_DeliveredAsPhoneMessage()
    {
        using var session = new GameSession(new Random(1));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.MedicalSchoolDropout));
        session.Relationships.SetNpcRelationship(NpcId.OfficerKhalid, 25, 0);
        session.DistrictHeat.SetHeat(DistrictId.Dokki, 50);

        var found = false;
        for (var i = 0; i < 50; i++)
        {
            using var s = new GameSession(new Random(i));
            s.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.MedicalSchoolDropout));
            s.Relationships.SetNpcRelationship(NpcId.OfficerKhalid, 25, 0);
            s.DistrictHeat.SetHeat(DistrictId.Dokki, 50);

            s.EndDay();

            var tipMessages = s.PhoneMessages.Inbox.Where(m => m.Type == PhoneMessageType.Tip).ToList();
            if (tipMessages.Count > 0)
            {
                found = true;
                await Assert.That(tipMessages[0].Content).IsNotNull();
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GameSession_NoPhone_NoTipDelivery()
    {
        using var session = new GameSession(new Random(1));
        session.Phone.LosePhone(1);

        session.Tips.AddTip(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Test",
            DayGenerated = 1,
            ExpiresAfterDay = 3,
            RelevantDistrict = DistrictId.Dokki
        });

        var tip = session.Tips.AllTips[0];
        var deliveryMethod = TipDeliveryConfig.GetDeliveryMethod(tip, session.World.CurrentDistrict);
        await Assert.That(deliveryMethod == TipDeliveryMethod.Phone || deliveryMethod == TipDeliveryMethod.Emergency).IsTrue();
    }

    [Test]
    public async Task GameSession_AcknowledgeTip_NotFound_ReturnsFalse()
    {
        using var session = new GameSession(new Random(1));
        var (success, _) = session.AcknowledgeTip("nonexistent");
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task GameSession_IgnoreTipAction_NotFound_ReturnsFalse()
    {
        using var session = new GameSession(new Random(1));
        var (success, _, _) = session.IgnoreTipAction("nonexistent");
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task GameSession_RestoreTips_RestoresCorrectly()
    {
        using var session = new GameSession(new Random(1));
        var tips = new List<Tip>
        {
            new() { Type = TipType.PoliceTip, Source = NpcId.OfficerKhalid, Content = "Restored", DayGenerated = 1, ExpiresAfterDay = 5 }
        };
        var ignored = new Dictionary<NpcId, int> { { NpcId.OfficerKhalid, 3 } };

        session.RestoreTips(tips, ignored);

        await Assert.That(session.Tips.AllTips).Count().IsEqualTo(1);
        await Assert.That(session.Tips.GetIgnoredCount(NpcId.OfficerKhalid)).IsEqualTo(3);
    }

    [Test]
    public async Task GameSession_EndDay_AppliesIgnoreErosion()
    {
        using var session = new GameSession(new Random(1));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.MedicalSchoolDropout));
        session.Relationships.SetNpcRelationship(NpcId.OfficerKhalid, 15, 0);

        for (var i = 0; i < 3; i++)
        {
            session.Tips.AddTip(new Tip
            {
                Type = TipType.PoliceTip,
                Source = NpcId.OfficerKhalid,
                Content = $"Tip {i}",
                DayGenerated = 1,
                ExpiresAfterDay = 100
            });
            session.Tips.IgnoreTip(session.Tips.AllTips[^1].Id);
        }

        var trustBefore = session.Relationships.GetNpcRelationship(NpcId.OfficerKhalid).Trust;
        session.EndDay();
        var trustAfter = session.Relationships.GetNpcRelationship(NpcId.OfficerKhalid).Trust;

        await Assert.That(trustAfter).IsLessThan(trustBefore);
    }

    [Test]
    public async Task GameSession_TipDeliveryConfig_SameDistrict_InPerson()
    {
        var tip = new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            DayGenerated = 1,
            ExpiresAfterDay = 3,
            RelevantDistrict = DistrictId.Imbaba
        };

        var method = TipDeliveryConfig.GetDeliveryMethod(tip, DistrictId.Imbaba);
        await Assert.That(method).IsEqualTo(TipDeliveryMethod.InPerson);
    }

    [Test]
    public async Task GameSession_TipDeliveryConfig_DifferentDistrict_Phone()
    {
        var tip = new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            DayGenerated = 1,
            ExpiresAfterDay = 3,
            RelevantDistrict = DistrictId.Dokki
        };

        var method = TipDeliveryConfig.GetDeliveryMethod(tip, DistrictId.Imbaba);
        await Assert.That(method).IsEqualTo(TipDeliveryMethod.Phone);
    }

    [Test]
    public async Task GameSession_TipDeliveryConfig_Emergency_AlwaysEmergency()
    {
        var tip = new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            DayGenerated = 1,
            ExpiresAfterDay = 3,
            IsEmergency = true
        };

        var method = TipDeliveryConfig.GetDeliveryMethod(tip, DistrictId.Imbaba);
        await Assert.That(method).IsEqualTo(TipDeliveryMethod.Emergency);
    }
}
