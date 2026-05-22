namespace Models.Tests;

public sealed class PlantNoteTextTests
{
    [Fact(DisplayName = "From trims valid text")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsValidTrimsValue() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsValidTrimsValue(value => PlantNoteText.From(value).Value);

    [Fact(DisplayName = "From throws when value is null")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsNullThrowsArgumentNullException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsNullThrowsArgumentNullException(value => PlantNoteText.From(value).Value);

    [Fact(DisplayName = "From throws when value is whitespace")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsWhitespaceThrowsArgumentException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsWhitespaceThrowsArgumentException(value => PlantNoteText.From(value).Value);

    [Fact(DisplayName = "From throws when value is too long")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsTooLongThrowsArgumentException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsTooLongThrowsArgumentException(
            value => PlantNoteText.From(value).Value,
            PlantNoteText.MaxLength);
}
