using FluentAssertions;
using Slums.Core.Crimes;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.State;

internal sealed class CrimeFactionOutcomeTests
{
    [Test]
    public void SuccessfulCrime_ShouldRaiseTheFactionControllingTheCurrentDistrict()
    {
        var session = new GameSession();
        session.World.TravelTo(LocationId.Workshop);
        var attempt = session.GetAvailableCrimes().First(crime => crime.Type == CrimeType.PettyTheft);
        var beforeExPrisoner = session.Relationships.GetFactionStanding(FactionId.ExPrisonerNetwork).Reputation;
        var beforeImbaba = session.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation;

        session.CommitCrime(attempt with { DetectionRisk = 0 }, new Random(1));

        session.Relationships.GetFactionStanding(FactionId.ExPrisonerNetwork).Reputation.Should().Be(beforeExPrisoner + 4);
        session.Relationships.GetFactionStanding(FactionId.ImbabaCrew).Reputation.Should().Be(beforeImbaba);
    }
}
