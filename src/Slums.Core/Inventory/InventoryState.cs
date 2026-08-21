namespace Slums.Core.Inventory;

public sealed class InventoryState
{
    private readonly Dictionary<string, int> _quantities = new(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, int> Quantities => _quantities;

    public int GetQuantity(string itemId) => _quantities.GetValueOrDefault(itemId);

    public bool Add(string itemId, int quantity, int maximumQuantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumQuantity, 1);

        var current = GetQuantity(itemId);
        if (current + quantity > maximumQuantity)
        {
            return false;
        }

        _quantities[itemId] = current + quantity;
        return true;
    }

    public bool Remove(string itemId, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        var current = GetQuantity(itemId);
        if (current < quantity)
        {
            return false;
        }

        var remaining = current - quantity;
        if (remaining == 0)
        {
            _quantities.Remove(itemId);
        }
        else
        {
            _quantities[itemId] = remaining;
        }

        return true;
    }

    public void Restore(IEnumerable<KeyValuePair<string, int>> quantities)
    {
        ArgumentNullException.ThrowIfNull(quantities);
        _quantities.Clear();
        foreach (var quantity in quantities)
        {
            if (!string.IsNullOrWhiteSpace(quantity.Key) && quantity.Value > 0)
            {
                _quantities[quantity.Key] = quantity.Value;
            }
        }
    }
}
