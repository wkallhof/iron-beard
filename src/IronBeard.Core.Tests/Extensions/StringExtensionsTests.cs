using IronBeard.Core.Extensions;

namespace IronBeard.Core.Tests.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void IsSet_NullString_ReturnsFalse()
    {
        string? s = null;
        Assert.False(s.IsSet());
    }

    [Fact]
    public void IsSet_EmptyString_ReturnsFalse()
    {
        Assert.False("".IsSet());
    }

    [Fact]
    public void IsSet_WhitespaceString_ReturnsFalse()
    {
        Assert.False("   ".IsSet());
    }

    [Fact]
    public void IsSet_ValueString_ReturnsTrue()
    {
        Assert.True("hello".IsSet());
    }

    [Fact]
    public void IgnoreCaseEquals_SameCase_ReturnsTrue()
    {
        Assert.True("hello".IgnoreCaseEquals("hello"));
    }

    [Fact]
    public void IgnoreCaseEquals_DifferentCase_ReturnsTrue()
    {
        Assert.True("Hello".IgnoreCaseEquals("hello"));
        Assert.True("HELLO".IgnoreCaseEquals("hello"));
    }

    [Fact]
    public void IgnoreCaseEquals_DifferentStrings_ReturnsFalse()
    {
        Assert.False("hello".IgnoreCaseEquals("world"));
    }
}
