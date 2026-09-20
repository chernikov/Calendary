using Calendary.Common;
using Xunit;

namespace Calendary.Common.Tests;

public class UkrainianPhoneNumberTests
{
    [Theory]
    [InlineData("+380671234567", "+380671234567")]
    [InlineData("380671234567", "+380671234567")]
    [InlineData("0671234567", "+380671234567")]
    [InlineData("067 123 45 67", "+380671234567")]
    [InlineData("+38 (067) 123-45-67", "+380671234567")]
    [InlineData("671234567", "+380671234567")]
    public void Normalize_accepts_common_ways_of_typing_a_ukrainian_number(string raw, string expected)
    {
        Assert.Equal(expected, UkrainianPhoneNumber.Normalize(raw));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("+1 555 123 4567")]
    [InlineData("+380671234567890")]
    [InlineData("not a phone number")]
    public void Normalize_rejects_input_that_does_not_fit_the_ukrainian_shape(string? raw)
    {
        Assert.Null(UkrainianPhoneNumber.Normalize(raw));
    }
}
