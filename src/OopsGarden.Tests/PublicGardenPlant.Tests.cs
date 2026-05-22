using Abstractions;

using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class PublicGardenPlantTests
{
    [Fact(DisplayName = "Constructor stores public plant values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = PlantId.New();
        var location = new GardenPlantLocation(LocationId.New(), "Kitchen");

        var plantedOn = new DateOnly(2026, 5, 22);
        var lastWateredAt = DateTimeOffset.UtcNow;

        var value = new PublicGardenPlant(id, "Basil", "Green", "photo", plantedOn, lastWateredAt, location);

        value.Id.Should().Be(id);
        value.Name.Should().Be("Basil");
        value.Description.Should().Be("Green");
        value.PhotoData.Should().Be("photo");
        value.PlantedOn.Should().Be(plantedOn);
        value.LastWateredAt.Should().Be(lastWateredAt);
        value.Location.Should().Be(location);
    }
}
