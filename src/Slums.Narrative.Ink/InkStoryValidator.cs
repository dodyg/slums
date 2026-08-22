using System.Text.Json;
using Slums.Core.Relationships;

namespace Slums.Narrative.Ink;

internal static class InkStoryValidator
{
    private static readonly string[] IntegerTags = ["MONEY", "HEALTH", "ENERGY", "HUNGER", "STRESS", "MOTHER_HEALTH", "FOOD"];

    public static void Validate(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = JsonDocument.Parse(json);
        ValidateElement(document.RootElement);
    }

    private static void ValidateElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    ValidateElement(property.Value);
                }

                break;
            case JsonValueKind.Array:
                foreach (var child in element.EnumerateArray())
                {
                    ValidateElement(child);
                }

                break;
            case JsonValueKind.String:
                ValidateString(element.GetString());
                break;
        }
    }

    private static void ValidateString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith('^'))
        {
            return;
        }

        var tag = value[1..];
        var separator = tag.IndexOf(':', StringComparison.Ordinal);
        if (separator <= 0)
        {
            return;
        }

        var key = tag[..separator].Trim().ToUpperInvariant();
        var payload = tag[(separator + 1)..].Trim();
        switch (key)
        {
            case "NPC_TRUST":
                ValidateNpcDelta(tag, payload);
                break;
            case "FACTION_REP":
                ValidateFactionDelta(tag, payload);
                break;
            case "FAVOR":
            case "REFUSAL":
                ValidateNpc(tag, payload.Split(',', 2)[0]);
                break;
            case "DEBT":
            case "EMBARRASSED":
            case "HELPED":
                ValidateNpcBoolean(tag, payload);
                break;
            default:
                if (IntegerTags.Contains(key, StringComparer.Ordinal) && !int.TryParse(payload, out _))
                {
                    throw InvalidTag(tag, "expected an integer value");
                }

                break;
        }
    }

    private static void ValidateNpcDelta(string tag, string payload)
    {
        var parts = payload.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !int.TryParse(parts[1], out _))
        {
            throw InvalidTag(tag, "expected 'NPC,delta'");
        }

        ValidateNpc(tag, parts[0]);
    }

    private static void ValidateFactionDelta(string tag, string payload)
    {
        var parts = payload.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !int.TryParse(parts[1], out _))
        {
            throw InvalidTag(tag, "expected 'Faction,delta'");
        }

        if (!Enum.TryParse<FactionId>(parts[0], out var faction) || !Enum.IsDefined(faction))
        {
            throw InvalidTag(tag, $"unknown faction '{parts[0]}'");
        }
    }

    private static void ValidateNpcBoolean(string tag, string payload)
    {
        var parts = payload.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !bool.TryParse(parts[1], out _))
        {
            throw InvalidTag(tag, "expected 'NPC,true|false'");
        }

        ValidateNpc(tag, parts[0]);
    }

    private static void ValidateNpc(string tag, string value)
    {
        if (!Enum.TryParse<NpcId>(value, out var npc) || !Enum.IsDefined(npc))
        {
            throw InvalidTag(tag, $"unknown NPC '{value}'");
        }
    }

    private static InvalidOperationException InvalidTag(string tag, string reason)
    {
        return new InvalidOperationException($"Invalid compiled Ink effect tag '{tag}': {reason}.");
    }
}
