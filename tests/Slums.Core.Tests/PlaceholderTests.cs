using FluentAssertions;
using TUnit.Core;
using TUnit.Core.Interfaces;

namespace Slums.Core.Tests;

public class PlaceholderTests
{
    [Test]
    public async Task Test1()
    {
        await Assert.That(true).IsTrue();
    }
}
