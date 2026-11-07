using WhatToCook.Api.Infrastructure.Validation;

namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class UnicodeTextLengthTests
{
    [Theory]
    [InlineData(null, 200, false)]
    [InlineData("", 200, false)]
    [InlineData("   ", 0, false)]
    [InlineData("abc", 3, false)]
    [InlineData("abc", 2, true)]
    [InlineData("  abc  ", 3, false)]
    [InlineData("a😀", 2, false)]
    [InlineData("a😀", 1, true)]
    [InlineData("e\u0301", 1, true)]
    public void MatchesPostgresUnicodeScalarLimits(string? value, int maximum, bool exceeded)
    {
        Assert.Equal(exceeded, UnicodeTextLength.ExceedsMax(value, maximum));
    }

    [Fact]
    public void CanValidateUntrimmedValuesLikeSourceUrls()
    {
        Assert.True(UnicodeTextLength.ExceedsMax(" a ", 1, trim: false));
        Assert.False(UnicodeTextLength.ExceedsMax(" a ", 1));
    }

    [Fact]
    public void StopsAtTheFirstExcessScalar()
    {
        Assert.False(UnicodeTextLength.ExceedsMax(string.Concat(Enumerable.Repeat("😀", 200)), 200));
        Assert.True(UnicodeTextLength.ExceedsMax(string.Concat(Enumerable.Repeat("😀", 201)), 200));
    }
}
