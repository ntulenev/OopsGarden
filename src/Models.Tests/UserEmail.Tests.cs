using FluentAssertions;

namespace Models.Tests;

public sealed class UserEmailTests
{
    [Fact(DisplayName = "From trims valid value")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsValidTrimsValue()
    {
        var email = UserEmail.From("  user@example.com  ");

        email.Value.Should().Be("USER@EXAMPLE.COM");
    }

    [Fact(DisplayName = "From throws when value is null")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsNullThrowsArgumentNullException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsNullThrowsArgumentNullException(value => UserEmail.From(value).Value);

    [Fact(DisplayName = "From throws when value is whitespace")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsWhitespaceThrowsArgumentException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsWhitespaceThrowsArgumentException(value => UserEmail.From(value).Value);

    [Fact(DisplayName = "From throws when value is too long")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsTooLongThrowsArgumentException() =>
        RequiredTextValueObjectAssertions.FromWhenValueIsTooLongThrowsArgumentException(value => UserEmail.From(value).Value, UserEmail.MaxLength);


    [Fact(DisplayName = "From throws when value has no at sign")]
    [Trait("Category", "Unit")]
    public void FromWhenValueHasNoAtSignThrowsArgumentException()
    {
        Action act = () => _ = UserEmail.From("user.example.com");

        act.Should().Throw<ArgumentException>();
    }

}
