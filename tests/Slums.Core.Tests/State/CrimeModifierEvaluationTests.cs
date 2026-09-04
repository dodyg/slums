using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.State;
using TUnit;

namespace Slums.Core.Tests.State;

internal sealed class CrimeModifierEvaluationTests
{
    [Test]
    public void EvaluateCrimeModifiers_ShouldEmitThinAlibiSignal_ForSameDayPublicFacingWork()
    {
        var session = new GameSession();
        session.RestoreWorkState(0, 0, 0, session.Clock.Day);

        var evaluation = session.EvaluateCrimeModifiers(CreateAttempt());

        evaluation.Signals.Should().Contain(CrimeModifierSignal.ThinAlibi);
        evaluation.ActiveModifiers.Should().Contain(static modifier => modifier.Contains("thin alibi", StringComparison.Ordinal));
    }

    [Test]
    public void EvaluateCrimeModifiers_ShouldEmitPrisonerScrutinySignal_ForReleasedPrisoner()
    {
        var session = new GameSession();
        session.Player.ApplyBackground(BackgroundRegistry.ReleasedPoliticalPrisoner);

        var evaluation = session.EvaluateCrimeModifiers(CreateAttempt());

        evaluation.Signals.Should().Contain(CrimeModifierSignal.PrisonerScrutiny);
        evaluation.ActiveModifiers.Should().Contain(static modifier => modifier.Contains("political prisoner", StringComparison.Ordinal));
    }

    private static CrimeAttempt CreateAttempt()
    {
        return new CrimeAttempt(CrimeType.PettyTheft, 40, 20, 10, 0, 10);
    }
}
