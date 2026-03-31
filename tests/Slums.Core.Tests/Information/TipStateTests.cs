using Slums.Core.Heat;
using Slums.Core.Information;
using Slums.Core.Relationships;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Information;

internal sealed class TipStateTests
{
    [Test]
    public async Task TipState_AddTip_TracksCorrectly()
    {
        var state = new TipState();
        state.AddTip(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Test tip",
            DayGenerated = 1,
            ExpiresAfterDay = 3
        });

        await Assert.That(state.AllTips).Count().IsEqualTo(1);
    }

    [Test]
    public async Task TipState_GetActiveTips_ExcludesExpired()
    {
        var state = new TipState();
        state.AddTip(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Expired",
            DayGenerated = 1,
            ExpiresAfterDay = 2
        });
        state.AddTip(new Tip
        {
            Type = TipType.JobLead,
            Source = NpcId.NurseSalma,
            Content = "Active",
            DayGenerated = 1,
            ExpiresAfterDay = 5
        });

        var active = state.GetActiveTips(3);
        await Assert.That(active).Count().IsEqualTo(1);
        await Assert.That(active[0].Type).IsEqualTo(TipType.JobLead);
    }

    [Test]
    public async Task TipState_AcknowledgeTip_MarksAcknowledged()
    {
        var state = new TipState();
        var tip = new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Test",
            DayGenerated = 1,
            ExpiresAfterDay = 3
        };
        state.AddTip(tip);

        var result = state.AcknowledgeTip(tip.Id);

        await Assert.That(result).IsTrue();
        await Assert.That(state.GetTip(tip.Id)!.Acknowledged).IsTrue();
    }

    [Test]
    public async Task TipState_AcknowledgeTip_AlreadyAcknowledged_ReturnsFalse()
    {
        var state = new TipState();
        var tip = new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Test",
            DayGenerated = 1,
            ExpiresAfterDay = 3
        };
        state.AddTip(tip);
        state.AcknowledgeTip(tip.Id);

        var result = state.AcknowledgeTip(tip.Id);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task TipState_IgnoreTip_TracksCount()
    {
        var state = new TipState();
        var tip1 = new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Tip 1",
            DayGenerated = 1,
            ExpiresAfterDay = 3
        };
        var tip2 = new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Tip 2",
            DayGenerated = 1,
            ExpiresAfterDay = 3
        };
        state.AddTip(tip1);
        state.AddTip(tip2);

        state.IgnoreTip(tip1.Id);
        state.IgnoreTip(tip2.Id);

        await Assert.That(state.GetIgnoredCount(NpcId.OfficerKhalid)).IsEqualTo(2);
    }

    [Test]
    public async Task TipState_IgnoreTip_AlreadyAcknowledged_ReturnsZero()
    {
        var state = new TipState();
        var tip = new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Test",
            DayGenerated = 1,
            ExpiresAfterDay = 3
        };
        state.AddTip(tip);
        state.AcknowledgeTip(tip.Id);

        var result = state.IgnoreTip(tip.Id);
        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    public async Task TipState_RemoveExpired_RemovesExpiredTips()
    {
        var state = new TipState();
        state.AddTip(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Expired",
            DayGenerated = 1,
            ExpiresAfterDay = 2
        });
        state.AddTip(new Tip
        {
            Type = TipType.JobLead,
            Source = NpcId.NurseSalma,
            Content = "Active",
            DayGenerated = 1,
            ExpiresAfterDay = 5
        });

        var removed = state.RemoveExpired(3);
        await Assert.That(removed).IsEqualTo(1);
        await Assert.That(state.AllTips).Count().IsEqualTo(1);
    }

    [Test]
    public async Task TipState_GetTipsByType_ReturnsCorrectType()
    {
        var state = new TipState();
        state.AddTip(new Tip { Type = TipType.PoliceTip, Source = NpcId.OfficerKhalid, DayGenerated = 1, ExpiresAfterDay = 3 });
        state.AddTip(new Tip { Type = TipType.JobLead, Source = NpcId.NurseSalma, DayGenerated = 1, ExpiresAfterDay = 3 });
        state.AddTip(new Tip { Type = TipType.PoliceTip, Source = NpcId.FixerUmmKarim, DayGenerated = 1, ExpiresAfterDay = 3 });

        var policeTips = state.GetTipsByType(TipType.PoliceTip);
        await Assert.That(policeTips).Count().IsEqualTo(2);
    }

    [Test]
    public async Task TipState_GetTipsFromNpc_ReturnsCorrectNpc()
    {
        var state = new TipState();
        state.AddTip(new Tip { Type = TipType.PoliceTip, Source = NpcId.OfficerKhalid, DayGenerated = 1, ExpiresAfterDay = 3 });
        state.AddTip(new Tip { Type = TipType.JobLead, Source = NpcId.NurseSalma, DayGenerated = 1, ExpiresAfterDay = 3 });

        var khalidTips = state.GetTipsFromNpc(NpcId.OfficerKhalid);
        await Assert.That(khalidTips).Count().IsEqualTo(1);
    }

    [Test]
    public async Task TipState_GetUndeliveredTips_ReturnsOnlyUndelivered()
    {
        var state = new TipState();
        state.AddTip(new Tip { Type = TipType.PoliceTip, Source = NpcId.OfficerKhalid, DayGenerated = 1, ExpiresAfterDay = 3, Delivered = true });
        state.AddTip(new Tip { Type = TipType.JobLead, Source = NpcId.NurseSalma, DayGenerated = 1, ExpiresAfterDay = 3, Delivered = false });

        var undelivered = state.GetUndeliveredTips(2);
        await Assert.That(undelivered).Count().IsEqualTo(1);
        await Assert.That(undelivered[0].Type).IsEqualTo(TipType.JobLead);
    }

    [Test]
    public async Task TipState_MarkAsDelivered_MarksTip()
    {
        var state = new TipState();
        var tip = new Tip { Type = TipType.PoliceTip, Source = NpcId.OfficerKhalid, DayGenerated = 1, ExpiresAfterDay = 3 };
        state.AddTip(tip);

        state.MarkAsDelivered(tip.Id);

        await Assert.That(state.GetTip(tip.Id)!.Delivered).IsTrue();
    }

    [Test]
    public async Task TipState_RestoreTips_RestoresState()
    {
        var state = new TipState();
        var tips = new List<Tip>
        {
            new() { Type = TipType.PoliceTip, Source = NpcId.OfficerKhalid, DayGenerated = 1, ExpiresAfterDay = 3 }
        };
        var ignored = new Dictionary<NpcId, int> { { NpcId.OfficerKhalid, 2 } };

        state.RestoreTips(tips, ignored);

        await Assert.That(state.AllTips).Count().IsEqualTo(1);
        await Assert.That(state.GetIgnoredCount(NpcId.OfficerKhalid)).IsEqualTo(2);
    }

    [Test]
    public async Task TipState_EmergencyTip_TrackedCorrectly()
    {
        var state = new TipState();
        state.AddTip(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            DayGenerated = 1,
            ExpiresAfterDay = 2,
            IsEmergency = true
        });

        await Assert.That(state.AllTips[0].IsEmergency).IsTrue();
    }
}
