using System.Text.RegularExpressions;

namespace Slums.Application.Persistence;

/// <summary>
/// Validates save slot identifiers. Slots are interpolated into file names, so they are
/// restricted to a safe identifier format to prevent path traversal.
/// </summary>
public static partial class SaveSlotRules
{
    [GeneratedRegex("^[A-Za-z0-9_-]{1,32}$")]
    private static partial Regex SlotPattern();

    public static bool IsValidSlot(string? slot)
    {
        return !string.IsNullOrWhiteSpace(slot) && SlotPattern().IsMatch(slot);
    }

    public static void EnsureValidSlot(string? slot)
    {
        if (!IsValidSlot(slot))
        {
            throw new ArgumentException(
                $"Save slot '{slot}' is invalid. Slots may only contain letters, digits, underscores, and dashes (1-32 characters).",
                nameof(slot));
        }
    }
}
