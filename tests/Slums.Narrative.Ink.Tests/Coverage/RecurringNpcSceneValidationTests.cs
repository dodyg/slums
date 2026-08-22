using System.Text.RegularExpressions;
using FluentAssertions;
using TUnit.Core;

namespace Slums.Narrative.Ink.Tests.Coverage;

internal sealed class RecurringNpcSceneValidationTests
{
    [Test]
    public void RecurringNpcScenes_ShouldHaveDistinctBodiesWithinEachContext()
    {
        var source = File.ReadAllText(ResolveSourcePath());
        var scenes = Regex.Matches(source, "^=== (?<name>[A-Za-z0-9_]+) ===\\n(?<body>.*?)(?=^=== |\\z)", RegexOptions.Multiline | RegexOptions.Singleline)
            .Cast<Match>()
            .Select(match => new
            {
                Name = match.Groups["name"].Value,
                Body = Normalize(match.Groups["body"].Value)
            })
            .Where(static scene => Regex.IsMatch(scene.Name, "_[1-4]$"))
            .ToArray();

        scenes.Should().NotBeEmpty();

        var duplicates = scenes
            .GroupBy(scene => GetContextPrefix(scene.Name), StringComparer.Ordinal)
            .SelectMany(group => group
                .GroupBy(scene => scene.Body, StringComparer.Ordinal)
                .Where(static bodyGroup => bodyGroup.Count() > 1)
                .Select(bodyGroup => $"{group.Key}: {string.Join(", ", bodyGroup.Select(static scene => scene.Name))}"))
            .ToArray();

        duplicates.Should().BeEmpty("a four-scene context should contain four authored scenes, not copied bodies");
    }

    private static string ResolveSourcePath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "content", "ink", "npcs.ink");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Recurring NPC Ink source was not found.", Path.Combine("content", "ink", "npcs.ink"));
    }

    private static string GetContextPrefix(string knotName)
    {
        return knotName[..knotName.LastIndexOf('_')];
    }

    private static string Normalize(string body)
    {
        return Regex.Replace(body, "\\s+", " ").Trim();
    }
}
