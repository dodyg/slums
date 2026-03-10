using FluentAssertions;
using TUnit.Core;

namespace Slums.Infrastructure.Tests;

public class PlaceholderTests
{
    [Test]
    public async Task Placeholder_ShouldBeTrue()
    {
        await Assert.That(true).IsTrue();
    }
}
