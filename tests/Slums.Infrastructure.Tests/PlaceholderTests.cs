using FluentAssertions;
using Xunit;

namespace Slums.Infrastructure.Tests;

public class PlaceholderTests
{
    [Fact]
    public void Test1()
    {
        true.Should().BeTrue();
    }
}
