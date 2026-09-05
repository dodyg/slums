using Slums.Core.State;
using Slums.Core.Training;
using Slums.Core.World;

namespace Slums.Application.Activities;

public sealed record GameActionMenuContext(
    Location? CurrentLocation,
    bool IsAtHome,
    bool HasReachableNpcs,
    bool HasInvestmentOpportunities,
    bool HasHouseholdAssetsAccess,
    bool HasTrainingAvailable,
    bool HasHomeUpgradesAvailable,
    bool HasCommunityEventAvailable,
    bool HasCommunityAdaptationAvailable,
    bool HasTechnicalRepairAvailable,
    bool HasDigitalServiceAvailable,
    bool PhoneIsOperational,
    bool HasPhoneMessages,
    bool PhoneNeedsCredit,
    bool HasUndeliveredTips,
    bool HasEndingChoices,
    bool HasEmergencySupport)
{
    public static GameActionMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameActionMenuContext(
            gameSession.World.GetCurrentLocation(),
            gameSession.World.CurrentLocationId == LocationId.Home,
            gameSession.GetReachableNpcs().Count > 0,
            gameSession.GetCurrentInvestmentOpportunities().Count > 0,
            gameSession.CanUseHouseholdAssets(),
            TrainingRegistry.AllActivities.Any(activity => gameSession.Player.Skills.GetLevel(activity.Skill) < 10),
            gameSession.World.CurrentLocationId == LocationId.Home && gameSession.GetAvailableHomeUpgrades().Count > 0,
            gameSession.GetAvailableCommunityEvents().Count > 0,
            gameSession.GetCommunityActionPreviews().Any(preview => preview.HasSkill),
            gameSession.GetTechnicalRepairPreviews().Any(preview => preview.AtRequiredLocation),
            gameSession.GetDigitalServicePreviews().Any(preview => preview.AtRequiredLocation),
            gameSession.Phone.IsOperational(),
            gameSession.PhoneMessages.GetUnrespondedCount(gameSession.Clock.Day) > 0,
            gameSession.Phone.IsOperational() == false && gameSession.Phone.HasPhone && !gameSession.Phone.PhoneLost,
            gameSession.Tips.GetUndeliveredTips(gameSession.Clock.Day).Count > 0,
            gameSession.GetAvailableEndingChoices().Count > 0,
            gameSession.World.CurrentLocationId == LocationId.Home && gameSession.CanRequestEmergencySupport);
    }
}
