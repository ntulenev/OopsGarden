namespace Models.Tests;

public sealed class InviteCodeTests
{
    [Fact(DisplayName = "From trims valid value")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsValidTrimsValue() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsValidTrimsValue(value => InviteCode.From(value).Value);

    [Fact(DisplayName = "From throws when value is null")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsNullThrowsArgumentNullException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsNullThrowsArgumentNullException(value => InviteCode.From(value).Value);

    [Fact(DisplayName = "From throws when value is whitespace")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsWhitespaceThrowsArgumentException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsWhitespaceThrowsArgumentException(value => InviteCode.From(value).Value);

    [Fact(DisplayName = "From throws when value is too long")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsTooLongThrowsArgumentException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsTooLongThrowsArgumentException(value => InviteCode.From(value).Value, InviteCode.MaxLength);

}
