using Slums.Core.Technology;

namespace Slums.Application.Technology;

public sealed class DigitalServiceMenuQuery
{
    public IReadOnlyList<DigitalServiceMenuStatus> GetStatuses(DigitalServiceMenuContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Actions
            .Select(preview => new DigitalServiceMenuStatus(preview, preview.CanPerform, preview.UnavailabilityReason))
            .ToArray();
    }
}
