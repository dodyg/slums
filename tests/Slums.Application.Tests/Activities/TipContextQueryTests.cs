using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Information;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class TipContextQueryTests
{
    [Test]
    public void GetCrimeHints_ShouldReturnEmpty_WhenNoRelevantTips()
    {
        var query = new TipContextQuery();
        using var gameState = new GameSession();

        var hints = query.GetCrimeHints(gameState);

        hints.Should().BeEmpty();
    }

    [Test]
    public void GetCrimeHints_ShouldIncludeCrimeWarnings()
    {
        var query = new TipContextQuery();
        using var gameState = new GameSession();
        gameState.Tips.AddTip(new Tip
        {
            Type = TipType.CrimeWarning,
            Source = NpcId.FenceHanan,
            Content = "Gang activity in Dokki",
            DayGenerated = 1,
            ExpiresAfterDay = 3
        });

        var hints = query.GetCrimeHints(gameState);

        hints.Should().HaveCount(1);
        hints[0].Content.Should().Be("Gang activity in Dokki");
        hints[0].IsWarning.Should().BeTrue();
    }

    [Test]
    public void GetCrimeHints_ShouldIncludePoliceTipsWithDistrict()
    {
        var query = new TipContextQuery();
        using var gameState = new GameSession();
        gameState.Tips.AddTip(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Checkpoint in Bulaq",
            DayGenerated = 1,
            ExpiresAfterDay = 3,
            RelevantDistrict = DistrictId.BulaqAlDakrour
        });

        var hints = query.GetCrimeHints(gameState);

        hints.Should().HaveCount(1);
        hints[0].IsWarning.Should().BeTrue();
    }

    [Test]
    public void GetCrimeHints_ShouldSkipIgnoredTips()
    {
        var query = new TipContextQuery();
        using var gameState = new GameSession();
        gameState.Tips.AddTip(new Tip
        {
            Type = TipType.CrimeWarning,
            Source = NpcId.FenceHanan,
            Content = "Gang moving in",
            DayGenerated = 1,
            ExpiresAfterDay = 3,
            Ignored = true
        });

        var hints = query.GetCrimeHints(gameState);

        hints.Should().BeEmpty();
    }

    [Test]
    public void GetCrimeHints_ShouldNotIncludeOtherTipTypes()
    {
        var query = new TipContextQuery();
        using var gameState = new GameSession();
        gameState.Tips.AddTip(new Tip
        {
            Type = TipType.JobLead,
            Source = NpcId.WorkshopBossAbuSamir,
            Content = "New opening",
            DayGenerated = 1,
            ExpiresAfterDay = 3
        });

        var hints = query.GetCrimeHints(gameState);

        hints.Should().BeEmpty();
    }

    [Test]
    public void GetWorkHints_ShouldIncludeJobLeads()
    {
        var query = new TipContextQuery();
        using var gameState = new GameSession();
        gameState.Tips.AddTip(new Tip
        {
            Type = TipType.JobLead,
            Source = NpcId.WorkshopBossAbuSamir,
            Content = "Opening at bakery tomorrow",
            DayGenerated = 1,
            ExpiresAfterDay = 3
        });

        var hints = query.GetWorkHints(gameState);

        hints.Should().HaveCount(1);
        hints[0].IsWarning.Should().BeFalse();
    }

    [Test]
    public void GetWorkHints_ShouldIncludeMarketIntel()
    {
        var query = new TipContextQuery();
        using var gameState = new GameSession();
        gameState.Tips.AddTip(new Tip
        {
            Type = TipType.MarketIntel,
            Source = NpcId.PharmacistMariam,
            Content = "Food prices dropping",
            DayGenerated = 1,
            ExpiresAfterDay = 3
        });

        var hints = query.GetWorkHints(gameState);

        hints.Should().HaveCount(1);
    }

    [Test]
    public void GetTravelHints_ShouldIncludePoliceTipsWithDistrict()
    {
        var query = new TipContextQuery();
        using var gameState = new GameSession();
        gameState.Tips.AddTip(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Checkpoint on bridge",
            DayGenerated = 1,
            ExpiresAfterDay = 3,
            RelevantDistrict = DistrictId.Dokki
        });

        var hints = query.GetTravelHints(gameState);

        hints.Should().HaveCount(1);
        hints[0].IsWarning.Should().BeTrue();
    }

    [Test]
    public void GetTravelHints_ShouldSkipPoliceTipsWithoutDistrict()
    {
        var query = new TipContextQuery();
        using var gameState = new GameSession();
        gameState.Tips.AddTip(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "General police activity",
            DayGenerated = 1,
            ExpiresAfterDay = 3,
            RelevantDistrict = null
        });

        var hints = query.GetTravelHints(gameState);

        hints.Should().BeEmpty();
    }

    [Test]
    public void GetCrimeHints_ShouldMarkEmergencyTips()
    {
        var query = new TipContextQuery();
        using var gameState = new GameSession();
        gameState.Tips.AddTip(new Tip
        {
            Type = TipType.CrimeWarning,
            Source = NpcId.FenceHanan,
            Content = "Raid imminent!",
            DayGenerated = 1,
            ExpiresAfterDay = 2,
            IsEmergency = true
        });

        var hints = query.GetCrimeHints(gameState);

        hints[0].IsEmergency.Should().BeTrue();
    }
}
