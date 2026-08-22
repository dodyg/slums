using FluentAssertions;
using SadConsole.Input;
using Slums.Game.Input;
using TUnit.Core;

namespace Slums.Game.Tests.Input;

internal sealed class NumberKeyMapperTests
{
    [Test]
    public void GetNumberIndex_ShouldReturnTopRowNumberIndex()
    {
        NumberKeyMapper.GetNumberIndex(Keys.D2, maxCount: 3).Should().Be(1);
    }

    [Test]
    public void GetNumberIndex_ShouldReturnNumPadNumberIndex()
    {
        NumberKeyMapper.GetNumberIndex(Keys.NumPad3, maxCount: 3).Should().Be(2);
    }

    [Test]
    public void GetNumberIndex_ShouldIgnoreNumbersOutsideMaxCount()
    {
        NumberKeyMapper.GetNumberIndex(Keys.D4, maxCount: 3).Should().BeNull();
    }
}
