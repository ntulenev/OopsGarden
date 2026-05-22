namespace Models.Tests;

public sealed class AdminNameTests
{
    [Fact(DisplayName = "From trims valid value")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsValidTrimsValue() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsValidTrimsValue(value => AdminName.From(value).Value);

    [Fact(DisplayName = "From throws when value is null")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsNullThrowsArgumentNullException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsNullThrowsArgumentNullException(value => AdminName.From(value).Value);

    [Fact(DisplayName = "From throws when value is whitespace")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsWhitespaceThrowsArgumentException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsWhitespaceThrowsArgumentException(value => AdminName.From(value).Value);
}
