using Abstractions;

using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class PublicGardenPlantProjectionTests
{
    [Fact(DisplayName = "Constructor stores public plant projection values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = PlantId.New();
        var location = new GardenPlantLocationProjection(LocationId.New(), "Kitchen");

        var value = new PublicGardenPlantProjection(id, "Basil", "Green", "photo", location);

        value.Id.Should().Be(id);
        value.Name.Should().Be("Basil");
        value.Description.Should().Be("Green");
        value.PhotoData.Should().Be("photo");
        value.Location.Should().Be(location);
    }
}
