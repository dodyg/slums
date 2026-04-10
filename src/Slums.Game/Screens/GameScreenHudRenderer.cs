using SadConsole;
using SadConsole.UI;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Application.HouseholdAssets;
using Slums.Application.Investments;
using Slums.Core.Characters;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Game.Screens;

internal static class GameScreenHudRenderer
{
    public static void RenderHud(ScreenSurface surface, GameStatusContext statusContext)
    {
        surface.Surface.Print(0, 0, "=== SLUMS - Cairo Survival ===", Color.Yellow);
        RenderStat(surface, "Energy", statusContext.Player.Stats.Energy, 100, GetStatColor(statusContext.Player.Stats.Energy));
        RenderStat(surface, "Hunger", statusContext.Player.Stats.Hunger, 100, GetStatColor(statusContext.Player.Stats.Hunger));
        RenderStat(surface, "Health", statusContext.Player.Stats.Health, 100, GetStatColor(statusContext.Player.Stats.Health));
        RenderStat(surface, "Stress", statusContext.Player.Stats.Stress, 100, GetStressColor(statusContext.Player.Stats.Stress));
        RenderStat(surface, "Mother Health", statusContext.Player.Household.MotherHealth, 100, GetMotherHealthColor(statusContext.Player.Household.MotherCondition));
    }

    public static void RenderActions(ScreenSurface surface, IReadOnlyList<GameAction> actions, int selectedAction)
    {
        surface.Surface.Print(0, GameScreenLayout.GetActionHeaderY(surface.Surface.Height), "--- Actions ---", Color.Cyan);
        var actionListStartY = GameScreenLayout.GetActionListStartY(surface.Surface.Height);

        for (var i = 0; i < actions.Count; i++)
        {
            var prefix = i == selectedAction ? "> " : "  ";
            var color = i == selectedAction ? Color.Cyan : Color.White;
            surface.Surface.Print(GameScreenLayout.ActionListX, actionListStartY + i, prefix + actions[i].Label, color);
        }

        var hintY = actionListStartY + actions.Count + 1;
        if (hintY < GameScreenLayout.GetActionHeaderY(surface.Surface.Height) + 14)
        {
            surface.Surface.Print(GameScreenLayout.ActionListX, hintY, "Tab=status | T=travel | P=save", Color.DarkGray);
            surface.Surface.Print(GameScreenLayout.ActionListX, hintY + 1, "L=event log | Esc=menu", Color.DarkGray);
        }
    }

    public static void RenderStat(ScreenSurface surface, string name, int value, int max, Color color)
    {
        const int barWidth = 10;
        var filled = (int)((double)value / max * barWidth);
        var bar = new string('#', filled) + new string('-', barWidth - filled);
        surface.Surface.Print(0, GameScreenLayout.GetStatRowY(surface.Surface.Height, GetStatLineOffset(name)), $"{name}: [{bar}] {value}", color);
    }

    public static string BuildRentOverviewText(GameStatusContext statusContext)
    {
        return statusContext.UnpaidRentDays > 0
            ? $"Rent debt: {statusContext.AccumulatedRentDebt} LE ({statusContext.UnpaidRentDays}d)"
            : $"Rent: {statusContext.RentCost} LE due today";
    }

    public static string TrimToWidth(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : $"{text[..(maxLength - 3)]}...";
    }

    private static int GetStatLineOffset(string name) => name switch
    {
        "Hunger" => 0,
        "Energy" => 1,
        "Health" => 2,
        "Stress" => 3,
        "Mother Health" => 4,
        _ => 0
    };

    private static Color GetStatColor(int value) => value switch
    {
        < 20 => Color.Red,
        < 50 => Color.Orange,
        _ => Color.Green
    };

    private static Color GetStressColor(int value) => value switch
    {
        > 80 => Color.Red,
        > 50 => Color.Orange,
        _ => Color.Green
    };

    private static Color GetMotherHealthColor(MotherCondition condition) => condition switch
    {
        MotherCondition.Crisis => Color.Red,
        MotherCondition.Fragile => Color.Orange,
        _ => Color.Green
    };

    internal static IEnumerable<string> WrapText(string text, int maxWidth)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var current = string.Empty;

        foreach (var word in words)
        {
            var candidate = string.IsNullOrEmpty(current) ? word : $"{current} {word}";
            if (candidate.Length > maxWidth && current.Length > 0)
            {
                yield return current;
                current = word;
            }
            else
            {
                current = candidate;
            }
        }

        if (current.Length > 0)
        {
            yield return current;
        }
    }
}
