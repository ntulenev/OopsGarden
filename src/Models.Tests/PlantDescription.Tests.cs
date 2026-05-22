using FluentAssertions;

namespace Models.Tests;

public sealed class PlantDescriptionTests
{
    [Theory(DisplayName = "From normalizes optional value")]
    [Trait("Category", "Unit")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("  green  ", "green")]
    public void FromWhenValueIsOptionalNormalizesValue(string? value, string expected)
    {
        var description = PlantDescription.From(value);

        description.Value.Should().Be(expected);
    }

    [Fact(DisplayName = "From throws when value is too long")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsTooLongThrowsArgumentException()
    {
        Action act = () => _ = PlantDescription.From(new string('x', PlantDescription.MaxLength + 1));

        act.Should().Throw<ArgumentException>();
    }
}
