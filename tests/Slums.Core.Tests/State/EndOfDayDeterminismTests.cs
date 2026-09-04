using Slums.Core.Characters;
using Slums.Core.Randomness;
using Slums.Core.State;
using Slums.Core.Weather;
using TUnit;

namespace Slums.Core.Tests.State;

internal sealed class EndOfDayDeterminismTests
{
    [Test]
    public async Task EndDay_WithGoldenSeed_ProducesTheRecordedDayTwoState()
    {
        var session = new GameSession(new GameRandom(20260904));
        session.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);

        session.EndDay(session.SharedRandom);

        await Assert.That(session.Clock.Day).IsEqualTo(2);
        await Assert.That(session.DaysSurvived).IsEqualTo(1);
        await Assert.That(session.Player.Stats.Money).IsEqualTo(30);
        await Assert.That(session.Player.Stats.Hunger).IsEqualTo(55);
        await Assert.That(session.Player.Stats.Energy).IsEqualTo(65);
        await Assert.That(session.Player.Stats.Health).IsEqualTo(90);
        await Assert.That(session.Player.Stats.Stress).IsEqualTo(50);
        await Assert.That(session.Player.Household.MotherHealth).IsEqualTo(62);
        await Assert.That(session.PolicePressure).IsEqualTo(10);
        await Assert.That(session.CurrentWeather.Type).IsEqualTo(WeatherType.Clear);
    }

    [Test]
    public async Task EndDay_WithTheSameSeed_IsRepeatableAcrossSessions()
    {
        var first = new GameSession(new GameRandom(20260904));
        first.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);
        first.EndDay(first.SharedRandom);

        var second = new GameSession(new GameRandom(20260904));
        second.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);
        second.EndDay(second.SharedRandom);

        await Assert.That(second.RandomState).IsEqualTo(first.RandomState);
        await Assert.That(second.Player.Stats.Money).IsEqualTo(first.Player.Stats.Money);
        await Assert.That(second.Player.Stats.Stress).IsEqualTo(first.Player.Stats.Stress);
        await Assert.That(second.CurrentWeather.Type).IsEqualTo(first.CurrentWeather.Type);
    }
}
