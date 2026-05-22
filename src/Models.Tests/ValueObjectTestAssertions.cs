using FluentAssertions;

namespace Models.Tests;

internal static class RequiredTextValueObjectAssertions
{
    public static void FromWhenValueIsValidTrimsValue(Func<string, string> create)
    {
        var value = create("  valid  ");

        value.Should().Be("valid");
    }

    public static void FromWhenValueIsNullThrowsArgumentNullException(Func<string, string> create)
    {
        string value = null!;

        Action act = () => _ = create(value);

        act.Should().Throw<ArgumentNullException>();
    }

    public static void FromWhenValueIsWhitespaceThrowsArgumentException(Func<string, string> create)
    {
        Action act = () => _ = create("   ");

        act.Should().Throw<ArgumentException>();
    }

    public static void FromWhenValueIsTooLongThrowsArgumentException(Func<string, string> create, int maxLength)
    {
        Action act = () => _ = create(new string('x', maxLength + 1));

        act.Should().Throw<ArgumentException>();
    }
}

internal static class StrongIdAssertions
{
    public static void FromWhenValueIsEmptyThrowsArgumentException(Action create)
    {
        var act = create;

        act.Should().Throw<ArgumentException>();
    }

    public static void NewWhenCalledCreatesNonEmptyValue(Func<Guid> create)
    {
        var value = create();

        value.Should().NotBe(Guid.Empty);
    }
}
