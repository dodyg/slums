using FluentAssertions;
using Slums.Core.Diagnostics;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Diagnostics;

internal sealed class GameMutationRecordTests
{
    [Test]
    public void Record_ShouldStoreAllProperties()
    {
        var runId = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow;
        var before = new Dictionary<string, object?> { ["Money"] = 100 };
        var after = new Dictionary<string, object?> { ["Money"] = 80 };
        var record = new GameMutationRecord(runId, timestamp, "Work", "WorkJob", before, after, "Completed shift");

        record.RunId.Should().Be(runId);
        record.Timestamp.Should().Be(timestamp);
        record.Category.Should().Be("Work");
        record.Action.Should().Be("WorkJob");
        record.Before.Should().BeSameAs(before);
        record.After.Should().BeSameAs(after);
        record.Reason.Should().Be("Completed shift");
    }
}

internal sealed class MutationCategoriesTests
{
    [Test]
    public void AllCategories_ShouldBeNonEmptyStrings()
    {
        typeof(MutationCategories).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Select(f => f.GetValue(null))
            .Cast<string>()
            .Should()
            .NotBeNullOrEmpty();
    }
}

internal sealed class GameMutationEventArgsTests
{
    [Test]
    public void Ctor_ShouldWrapRecord()
    {
        var before = new Dictionary<string, object?>();
        var after = new Dictionary<string, object?>();
        var record = new GameMutationRecord(Guid.NewGuid(), DateTimeOffset.UtcNow, "Test", "TestAction", before, after, "test");

        var args = new GameMutationEventArgs(record);

        args.Record.Should().BeSameAs(record);
    }
}
