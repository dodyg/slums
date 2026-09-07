namespace Slums.Application.Activities;

public sealed class CommunityActionMenuQuery
{
    public IReadOnlyList<CommunityActionMenuStatus> GetStatuses(CommunityActionMenuContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Actions
            .Select(preview => new CommunityActionMenuStatus(preview, preview.CanPerform, preview.UnavailabilityReason))
            .ToArray();
    }
}
