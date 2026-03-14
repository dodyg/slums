using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Investments;
using Slums.Core.Relationships;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Investments;

internal sealed class InvestmentEligibilityEvaluatorTests
{
    [Test]
    public void Evaluate_ShouldRequireTrustForFoulCart_WhenLocationAndCashAlreadyMatch()
    {
        var relationships = new RelationshipState();
        var definition = InvestmentRegistry.GetByType(InvestmentType.FoulCart);

        definition.Should().NotBeNull();

        var context = new InvestmentEligibilityContext(
            CurrentMoney: 200,
            CurrentLocationId: LocationId.Home,
            ReachableNpcs: new HashSet<NpcId> { NpcId.LandlordHajjMahmoud },
            OwnedInvestmentTypes: new HashSet<InvestmentType>(),
            Relationships: relationships,
            TotalCrimeEarnings: 0,
            StreetSmartsLevel: 0,
            BackgroundType: BackgroundType.MedicalSchoolDropout);

        var eligibility = InvestmentEligibilityEvaluator.Evaluate(definition!, context);

        eligibility.IsEligible.Should().BeFalse();
        eligibility.FailureReasons.Should().Contain(reason => reason.Contains("30 trust with Hajj Mahmoud", StringComparison.Ordinal));
    }
}
