using FluentAssertions;


namespace OopsGarden.Tests;

public sealed class PlantCommandTests
{
    [Fact(DisplayName = "Constructor stores plant command values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var locationId = Guid.NewGuid();

        var value = new PlantCommand("Basil", "Green", locationId, new DateOnly(2026, 5, 22), new DateOnly(2026, 5, 23), "photo");

        value.Name.Should().Be("Basil");
        value.Description.Should().Be("Green");
        value.LocationId.Should().Be(locationId);
        value.PlantedOn.Should().Be(new DateOnly(2026, 5, 22));
        value.LastWateredOn.Should().Be(new DateOnly(2026, 5, 23));
        value.PhotoData.Should().Be("photo");
    }
}
