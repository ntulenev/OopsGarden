namespace Models.Tests;

public sealed class PlantNameTests
{
    [Fact(DisplayName = "From trims valid value")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsValidTrimsValue() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsValidTrimsValue(value => PlantName.From(value).Value);

    [Fact(DisplayName = "From throws when value is null")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsNullThrowsArgumentNullException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsNullThrowsArgumentNullException(value => PlantName.From(value).Value);

    [Fact(DisplayName = "From throws when value is whitespace")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsWhitespaceThrowsArgumentException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsWhitespaceThrowsArgumentException(value => PlantName.From(value).Value);

    [Fact(DisplayName = "From throws when value is too long")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsTooLongThrowsArgumentException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsTooLongThrowsArgumentException(value => PlantName.From(value).Value, PlantName.MaxLength);

}
