using FluentAssertions;

namespace Models.Tests;

public sealed class PlantSoilTests
{
    [Theory(DisplayName = "From normalizes optional value")]
    [Trait("Category", "Unit")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("  loose mix  ", "loose mix")]
    public void FromWhenValueIsOptionalNormalizesValue(string? value, string expected)
    {
        var soil = PlantSoil.From(value);

        soil.Value.Should().Be(expected);
    }

    [Fact(DisplayName = "From throws when value is too long")]
    [Trait("Category", "Unit")]
    public void FromWhenValueIsTooLongThrowsArgumentException()
    {
        Action act = () => _ = PlantSoil.From(new string('x', PlantSoil.MaxLength + 1));

        act.Should().Throw<ArgumentException>();
    }
}
