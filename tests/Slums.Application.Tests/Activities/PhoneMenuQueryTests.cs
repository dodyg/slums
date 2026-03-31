using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Information;
using Slums.Core.Phone;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class PhoneMenuQueryTests
{
    [Test]
    public void GetStatus_ShouldReturnEmpty_WhenNoMessagesOrTips()
    {
        var query = new PhoneMenuQuery();
        using var gameState = new GameSession();
        var context = PhoneMenuContext.Create(gameState);

        var status = query.GetStatus(context);

        status.Entries.Should().BeEmpty();
        status.CreditRemaining.Should().Be(7);
        status.PhoneLost.Should().BeFalse();
    }

    [Test]
    public void GetStatus_ShouldIncludeUndeliveredTips()
    {
        var query = new PhoneMenuQuery();
        using var gameState = new GameSession();
        gameState.Tips.AddTip(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Sweep planned for Bulaq",
            DayGenerated = 1,
            ExpiresAfterDay = 3,
            Delivered = false
        });
        var context = PhoneMenuContext.Create(gameState);

        var status = query.GetStatus(context);

        status.Entries.Should().HaveCount(1);
        status.Entries[0].IsTip.Should().BeTrue();
        status.Entries[0].Label.Should().Contain("Police intel");
        status.Entries[0].Content.Should().Be("Sweep planned for Bulaq");
    }

    [Test]
    public void GetStatus_ShouldIncludeActiveMessages()
    {
        var query = new PhoneMenuQuery();
        using var gameState = new GameSession();
        gameState.PhoneMessages.AddMessage(new PhoneMessage
        {
            Type = PhoneMessageType.Opportunity,
            Sender = "Test Sender",
            Content = "Job opening at market",
            DayReceived = 1,
            RequiresResponse = true
        });
        var context = PhoneMenuContext.Create(gameState);

        var status = query.GetStatus(context);

        status.Entries.Should().HaveCount(1);
        status.Entries[0].IsTip.Should().BeFalse();
        status.Entries[0].RequiresResponse.Should().BeTrue();
    }

    [Test]
    public void GetStatus_ShouldSkipRespondedAndIgnoredMessages()
    {
        var query = new PhoneMenuQuery();
        using var gameState = new GameSession();
        gameState.PhoneMessages.AddMessage(new PhoneMessage
        {
            Type = PhoneMessageType.Opportunity,
            Sender = "Sender1",
            Content = "Responded msg",
            DayReceived = 1,
            Responded = true
        });
        gameState.PhoneMessages.AddMessage(new PhoneMessage
        {
            Type = PhoneMessageType.Warning,
            Sender = "Sender2",
            Content = "Ignored msg",
            DayReceived = 1,
            Ignored = true
        });
        var context = PhoneMenuContext.Create(gameState);

        var status = query.GetStatus(context);

        status.Entries.Should().BeEmpty();
    }

    [Test]
    public void GetStatus_ShouldMarkEmergencyTips()
    {
        var query = new PhoneMenuQuery();
        using var gameState = new GameSession();
        gameState.Tips.AddTip(new Tip
        {
            Type = TipType.CrimeWarning,
            Source = NpcId.FenceHanan,
            Content = "Raid imminent!",
            DayGenerated = 1,
            ExpiresAfterDay = 2,
            Delivered = false,
            IsEmergency = true
        });
        var context = PhoneMenuContext.Create(gameState);

        var status = query.GetStatus(context);

        status.Entries[0].IsEmergency.Should().BeTrue();
        status.Entries[0].Label.Should().Contain("URGENT");
    }

    [Test]
    public void GetStatus_ShouldComputeDaysUntilExpiry()
    {
        var query = new PhoneMenuQuery();
        using var gameState = new GameSession();
        gameState.Tips.AddTip(new Tip
        {
            Type = TipType.JobLead,
            Source = NpcId.WorkshopBossAbuSamir,
            Content = "Opening tomorrow",
            DayGenerated = 1,
            ExpiresAfterDay = 3,
            Delivered = false
        });
        var context = PhoneMenuContext.Create(gameState);

        var status = query.GetStatus(context);

        status.Entries[0].DaysUntilExpiry.Should().Be(2);
    }

    [Test]
    public void GetStatus_ShouldIncludeBothTipsAndMessages()
    {
        var query = new PhoneMenuQuery();
        using var gameState = new GameSession();
        gameState.Tips.AddTip(new Tip
        {
            Type = TipType.JobLead,
            Source = NpcId.WorkshopBossAbuSamir,
            Content = "Opening",
            DayGenerated = 1,
            ExpiresAfterDay = 3,
            Delivered = false
        });
        gameState.PhoneMessages.AddMessage(new PhoneMessage
        {
            Type = PhoneMessageType.FamilyAlert,
            Sender = "Family",
            Content = "Mother needs medicine",
            DayReceived = 1,
            RequiresResponse = true
        });
        var context = PhoneMenuContext.Create(gameState);

        var status = query.GetStatus(context);

        status.Entries.Should().HaveCount(2);
        status.Entries.Count(e => e.IsTip).Should().Be(1);
        status.Entries.Count(e => !e.IsTip).Should().Be(1);
    }

    [Test]
    public void PhoneMenuContext_Create_ShouldCapturePhoneState()
    {
        using var gameState = new GameSession();
        gameState.Phone.LosePhone(1);

        var context = PhoneMenuContext.Create(gameState);

        context.PhoneOperational.Should().BeFalse();
        context.PhoneLost.Should().BeTrue();
    }

    [Test]
    public void PhoneMenuContext_Create_ShouldCaptureCreditInfo()
    {
        using var gameState = new GameSession();

        var context = PhoneMenuContext.Create(gameState);

        context.CreditRemaining.Should().Be(7);
        context.CreditWeekCost.Should().Be(5);
    }
}
