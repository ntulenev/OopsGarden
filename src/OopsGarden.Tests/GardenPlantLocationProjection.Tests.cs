
using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class GardenPlantLocationProjectionTests
{
    [Fact(DisplayName = "Constructor stores location projection values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = LocationId.New();

        var value = new GardenPlantLocationProjection(id, "Kitchen");

        value.Id.Should().Be(id);
        value.Name.Should().Be("Kitchen");
    }
}
