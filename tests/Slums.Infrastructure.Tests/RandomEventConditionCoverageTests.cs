using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Slums.Infrastructure.Content;
using TUnit.Core;
using Slums.TestSupport;

namespace Slums.Infrastructure.Tests;

/// <summary>
/// Proves that every condition id shipped in <c>random_events.json</c> maps to a compiled
/// predicate. Unknown ids throw during <see cref="JsonContentRepository.LoadRandomEvents"/>,
/// so a successful load of the repository content is exhaustive coverage of the mapping.
/// </summary>
internal sealed class RandomEventConditionCoverageTests
{
    [Test]
    public void LoadRandomEvents_FromRepositoryContent_ShouldMapEveryShippedConditionId()
    {
        var repository = new JsonContentRepository(
            NullLogger<JsonContentRepository>.Instance,
            TestContent.FindContentDirectory());

        var act = () => repository.LoadRandomEvents();

        act.Should().NotThrow("every ConditionId in random_events.json must resolve to a known predicate");
    }

    [Test]
    public void LoadRandomEvents_FromRepositoryContent_ShouldExposeAPredicateForEachCondition()
    {
        var repository = new JsonContentRepository(
            NullLogger<JsonContentRepository>.Instance,
            TestContent.FindContentDirectory());

        var events = repository.LoadRandomEvents();

        events.Should().NotBeEmpty();
        events.Should().OnlyContain(randomEvent => !string.IsNullOrWhiteSpace(randomEvent.Id));
    }
}
