
using FluentAssertions;


namespace OopsGarden.Tests;

public sealed class GardenPlantLocationTests
{
    [Fact(DisplayName = "Constructor stores location values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = LocationId.New();

        var value = new GardenPlantLocation(id, "Kitchen");

        value.Id.Should().Be(id);
        value.Name.Should().Be("Kitchen");
    }
}
