using FluentAssertions;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.World;

internal sealed class DistrictConditionRegistryTests
{
    [Test]
    public void Configure_ShouldThrowClearError_WhenEffectIsNull()
    {
        var definition = CreateDefinition("imbaba_null_effect", DistrictId.Imbaba) with
        {
            Effect = null!,
        };

        var act = () => DistrictConditionRegistry.Configure(CreateAllDistrictDefinitions(definition));

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("District condition 'imbaba_null_effect' must provide an effect.");
    }

    [Test]
    public void Configure_ShouldThrowClearError_WhenBoostedRandomEventIdsIsNull()
    {
        var definition = CreateDefinition("imbaba_null_boosted_events", DistrictId.Imbaba) with
        {
            Effect = new DistrictConditionEffect
            {
                BoostedRandomEventIds = null!,
            },
        };

        var act = () => DistrictConditionRegistry.Configure(CreateAllDistrictDefinitions(definition));

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("District condition 'imbaba_null_boosted_events' must provide a boosted random event id list.");
    }

    [Test]
    public void Configure_ShouldThrowClearError_WhenSuppressedRandomEventIdsIsNull()
    {
        var definition = CreateDefinition("imbaba_null_suppressed_events", DistrictId.Imbaba) with
        {
            Effect = new DistrictConditionEffect
            {
                SuppressedRandomEventIds = null!,
            },
        };

        var act = () => DistrictConditionRegistry.Configure(CreateAllDistrictDefinitions(definition));

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("District condition 'imbaba_null_suppressed_events' must provide a suppressed random event id list.");
    }

    private static DistrictConditionDefinition[] CreateAllDistrictDefinitions(DistrictConditionDefinition firstDefinition)
    {
        return
        [
            firstDefinition,
            CreateDefinition("dokki_test_condition", DistrictId.Dokki),
            CreateDefinition("ardalliwa_test_condition", DistrictId.ArdAlLiwa),
            CreateDefinition("bulaq_test_condition", DistrictId.BulaqAlDakrour),
            CreateDefinition("shubra_test_condition", DistrictId.Shubra),
        ];
    }

    private static DistrictConditionDefinition CreateDefinition(string id, DistrictId district)
    {
        return new DistrictConditionDefinition
        {
            Id = id,
            District = district,
            Title = "Test condition",
            BulletinText = "Test bulletin",
            GameplaySummary = "Test summary",
            Weight = 1,
        };
    }
}
