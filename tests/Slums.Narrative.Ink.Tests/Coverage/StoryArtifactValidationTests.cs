using System.Text.Json;
using System.Security.Cryptography;
using FluentAssertions;
using InkStoryException = Ink.Runtime.StoryException;
using Slums.Core.Endings;
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
    public async Task CompiledArtifact_DeclaresAllSynchronizedGameplayGlobals()
    {
        var story = StoryTraversalHelper.LoadStory();

        foreach (var requiredGlobal in InkStoryCatalog.RequiredGlobals)
        {
            story.variablesState.GlobalVariableExistsWithName(requiredGlobal.Key).Should().BeTrue();
            story.variablesState[requiredGlobal.Key]!.GetType().Should().Be(requiredGlobal.Value);
        }
    }

    [Test]
    public void InkSources_HaveFreshnessManifest()
    {
        var sourceDirectory = ResolveInkSourceDirectory();
        var manifestPath = Path.Combine(sourceDirectory, "source-manifest.sha256");
        File.Exists(manifestPath).Should().BeTrue("the manifest must be committed with the compiled artifact");

        var expected = File.ReadAllLines(manifestPath)
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .Select(static line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToDictionary(static parts => parts[1], static parts => parts[0].ToUpperInvariant(), StringComparer.Ordinal);
        var actual = Directory.EnumerateFiles(sourceDirectory, "*.ink", SearchOption.TopDirectoryOnly)
            .Select(static path => (Name: Path.GetFileName(path), Hash: Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))))
            .ToDictionary(static item => item.Name, static item => item.Hash, StringComparer.Ordinal);

        actual.Should().BeEquivalentTo(expected, "editing an Ink source requires recompiling and updating the committed source manifest");
    }

    [Test]
    public async Task CompiledArtifact_HasNoOrphanEndingKnots()
    {
        var knotNames = StoryTraversalHelper.GetAllKnotNames().ToHashSet(StringComparer.Ordinal);

        var act = () => EndingKnotCatalog.ValidateKnownKnots(knotNames);

        act.Should().NotThrow();
    }

    [Test]
    public void InkValidator_RejectsDuplicateStatTagsWithinChoice()
    {
        const string invalidStory = "{\"root\":[{\"c-0\":[\"^STRESS:1\",\"^STRESS:-1\"]}]}";

        var act = () => InkStoryValidator.Validate(invalidStory);

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("more than one STRESS effect tag");
    }

    [Test]
    public void InkValidator_RejectsUnknownEffectTags()
    {
        const string invalidStory = "{\"root\":[\"#\",\"^UNDECLARED_EFFECT:1\",\"/#\"]}";

        var act = () => InkStoryValidator.Validate(invalidStory);

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("unknown effect tag key");
    }

    [Test]
    public void InkValidator_RejectsEffectTagsWithoutColon()
    {
        const string invalidStory = "{\"root\":[\"#\",\"^MONEY 10\",\"/#\"]}";

        var act = () => InkStoryValidator.Validate(invalidStory);

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("missing ':' separator");
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
            catch (Exception exception) when (exception is InkStoryException or InvalidOperationException or ArgumentException)
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

    private static string ResolveInkSourceDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "content", "ink");
            if (File.Exists(Path.Combine(candidate, "source-manifest.sha256")))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate content/ink for freshness validation.");
    }
}
