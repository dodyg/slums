using Slums.Core.Endings;

namespace Slums.Application.Endings;

public static class EndingChoiceMenuQuery
{
    public static IReadOnlyList<EndingChoiceOption> GetOptions(EndingChoiceMenuContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.AvailableEndings
            .Select(endingId => new EndingChoiceOption(
                endingId,
                EndingService.GetChoiceLabel(endingId),
                EndingService.GetChoiceRequirements(endingId)))
            .ToArray();
    }
}
