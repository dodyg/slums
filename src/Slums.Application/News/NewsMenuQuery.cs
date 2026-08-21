using Slums.Core.Inventory;
using Slums.Core.World;

namespace Slums.Application.News;

public sealed class NewsMenuQuery
{
    #pragma warning disable CA1822
    public NewsMenuStatus GetStatus(NewsMenuContext context, IReadOnlyDictionary<string, int>? inventory = null, int money = 0)
    #pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);
        inventory ??= new Dictionary<string, int>();

        var states = context.ActiveStates.ToDictionary(static state => state.DefinitionId, StringComparer.Ordinal);
        var flashes = context.ActiveNews.Select(definition =>
        {
            var state = states[definition.Id];
            var responses = definition.Responses.Select(response =>
            {
                var requirements = new List<string>();
                if (response.MoneyCost > 0)
                {
                    requirements.Add($"{response.MoneyCost} LE");
                }
                if (response.TimeCostMinutes > 0)
                {
                    requirements.Add($"{response.TimeCostMinutes} min");
                }
                if (response.RequiredItemId is not null)
                {
                    var itemName = ItemRegistry.GetById(response.RequiredItemId)?.Name ?? response.RequiredItemId;
                    requirements.Add($"{response.RequiredItemQuantity} {itemName}");
                }

                var hasMoney = money >= response.MoneyCost;
                var hasItem = response.RequiredItemId is null || inventory.GetValueOrDefault(response.RequiredItemId) >= response.RequiredItemQuantity;
                var used = state.UsedResponseId is not null;
                var reason = used
                    ? "A response has already been used for this flash."
                    : !hasMoney
                        ? $"Need {response.MoneyCost} LE."
                        : !hasItem
                            ? $"Need {response.RequiredItemQuantity} {response.RequiredItemId}."
                            : string.Empty;
                return new NewsResponseDisplay(response.Id, response.Label, requirements.Count == 0 ? "No immediate cost" : string.Join(" + ", requirements), !used && hasMoney && hasItem, reason);
            }).ToArray();

            return new NewsFlashDisplay(
                definition.Id,
                definition.Headline,
                definition.Body,
                definition.SourceLabel,
                definition.Reliability.ToString(),
                Math.Max(0, state.ExpiryDay - context.CurrentDay + 1),
                definition.AffectedDistricts.Select(DistrictInfo.GetName).ToArray(),
                responses,
                state.Acknowledged);
        }).ToArray();

        return new NewsMenuStatus(flashes);
    }
}
