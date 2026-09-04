using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Slums.Application.Narrative;
using TUnit;

namespace Slums.Narrative.Ink.Tests;

[NotInParallel]
internal sealed class InkStoryLoaderTests
{
    [Test]
    public void LoadStoryJson_ShouldIgnoreProcessWorkingDirectory()
    {
        var originalDirectory = Environment.CurrentDirectory;
        var temporaryDirectory = Directory.CreateTempSubdirectory("slums-ink-cwd-");

        try
        {
            Environment.CurrentDirectory = temporaryDirectory.FullName;

            var service = new InkNarrativeService(NullLogger<InkNarrativeService>.Instance);

            service.StartScene("intro_medical", NarrativeSceneState.Create(new Slums.Core.State.GameSession()));

            service.CurrentText.Should().Contain("Cairo, 2060.");
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            temporaryDirectory.Delete(recursive: true);
        }
    }
}
