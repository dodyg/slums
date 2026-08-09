using System.Text.Json;
using FluentAssertions;
using Slums.Narrative.Ink.Tests.Helpers;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Coverage;

/// <summary>
/// Build validation for the compiled Ink artifact. These tests keep invalid or incompatible
/// Ink content a hard failure: the checked-in artifact must load and every authored knot must
/// be traversable with the pinned compiler/runtime combination (inkjs 2.4.0 output, inkVersion
/// 21, loaded by Qyl27.Ink.Engine 1.2.0).
/// </summary>
internal sealed class StoryArtifactValidationTests
{
    [Test]
    public async Task CompiledArtifact_IsInkVersion21()
    {
        var json = LoadArtifactText();

        using var document = JsonDocument.Parse(json);
        var inkVersion = document.RootElement.GetProperty("inkVersion").GetInt32();

        inkVersion.Should().Be(21, "inkjs 2.4.0 emits inkVersion 21, which Qyl27.Ink.Engine 1.2.0 supports");
    }

    [Test]
    public async Task CompiledArtifact_LoadsIntoRuntime()
    {
        var story = StoryTraversalHelper.LoadStory();

        story.Should().NotBeNull("the pinned runtime must accept the checked-in compiled artifact");
    }

    [Test]
    public async Task EveryAuthoredKnot_IsTraversable()
    {
        var story = StoryTraversalHelper.LoadStory();
        var knots = story.mainContentContainer.namedOnlyContent.Keys
            .Where(static knot => knot != "global decl")
            .ToList();

        knots.Should().NotBeEmpty("the compiled artifact must declare authored knots");

        var untraversable = new List<string>();

        foreach (var knot in knots)
        {
            try
            {
                story.ChoosePathString(knot);

                var text = new List<string>();
                while (story.canContinue)
                {
                    var content = story.Continue();
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        text.Add(content.Trim());
                    }
                }

                if (text.Count == 0 && story.currentChoices.Count == 0)
                {
                    untraversable.Add($"{knot} (no text or choices produced)");
                }
            }
            catch (Exception exception) when (exception is InvalidOperationException or ArgumentException)
            {
                untraversable.Add($"{knot} ({exception.GetType().Name}: {exception.Message})");
            }
        }

        untraversable.Should().BeEmpty("every authored knot should be enterable and produce content");
    }

    private static string LoadArtifactText()
    {
        var candidate = ResolveArtifactPath();
        if (candidate is not null)
        {
            return File.ReadAllText(candidate);
        }

        throw new FileNotFoundException(
            "Compiled Ink artifact content/ink/main.json not found anywhere above the test output directory; run 'npm run compile-ink' from src/Slums.Game.",
            Path.Combine("content", "ink", "main.json"));
    }

    private static string? ResolveArtifactPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "content", "ink", "main.json");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
