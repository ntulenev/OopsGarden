using FluentAssertions;

namespace Models.Tests;

public sealed class LanguageCodeTests
{
    [Theory(DisplayName = "From normalizes supported value or defaults to English")]
    [Trait("Category", "Unit")]
    [InlineData(null, "en")]
    [InlineData("", "en")]
    [InlineData(" ", "en")]
    [InlineData("EN", "en")]
    [InlineData("ru", "ru")]
    [InlineData("de", "en")]
    public void FromWhenValueIsReadNormalizesValue(string? value, string expected)
    {
        var language = LanguageCode.From(value);

        language.Value.Should().Be(expected);
    }

    [Fact(DisplayName = "Constants return supported codes")]
    [Trait("Category", "Unit")]
    public void ConstantsWhenReadReturnsSupportedCodes()
    {
        LanguageCode.English.Should().Be("en");
        LanguageCode.Russian.Should().Be("ru");
    }
}
