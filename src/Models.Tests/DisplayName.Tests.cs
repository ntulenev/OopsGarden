namespace Models.Tests;

public sealed class DisplayNameTests
{
    [Fact(DisplayName = "From trims valid value")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsValidTrimsValue() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsValidTrimsValue(value => DisplayName.From(value).Value);

    [Fact(DisplayName = "From throws when value is null")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsNullThrowsArgumentNullException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsNullThrowsArgumentNullException(value => DisplayName.From(value).Value);

    [Fact(DisplayName = "From throws when value is whitespace")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsWhitespaceThrowsArgumentException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsWhitespaceThrowsArgumentException(value => DisplayName.From(value).Value);

    [Fact(DisplayName = "From throws when value is too long")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsTooLongThrowsArgumentException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsTooLongThrowsArgumentException(value => DisplayName.From(value).Value, DisplayName.MaxLength);

}
