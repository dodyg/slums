namespace Slums.Application.Activities;

public sealed class CommunityActionMenuQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<CommunityActionMenuStatus> GetStatuses(CommunityActionMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Actions
            .Select(preview => new CommunityActionMenuStatus(preview, preview.CanPerform, preview.UnavailabilityReason))
            .ToArray();
    }
}
