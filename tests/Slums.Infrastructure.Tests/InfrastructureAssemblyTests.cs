using FluentAssertions;
using TUnit.Core;

namespace Slums.Infrastructure.Tests;

internal sealed class InfrastructureAssemblyTests
{
    [Test]
    public void InfrastructureAssembly_ShouldBeLoadable()
    {
        var assembly = System.Reflection.Assembly.Load("Slums.Infrastructure");

        assembly.GetName().Name.Should().Be("Slums.Infrastructure");
    }
}
