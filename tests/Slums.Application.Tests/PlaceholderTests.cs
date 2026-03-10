using FluentAssertions;
using TUnit.Core;

namespace Slums.Application.Tests;

public class PlaceholderTests
{
    [Test]
    public async Task Placeholder_ShouldBeTrue()
    {
        await Assert.That(true).IsTrue();
    }
}
