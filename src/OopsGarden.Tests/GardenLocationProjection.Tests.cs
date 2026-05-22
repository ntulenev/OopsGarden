using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class GardenLocationProjectionTests
{
    [Fact(DisplayName = "Constructor stores location projection values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = LocationId.New();

        var value = new GardenLocationProjection(id, "Kitchen", 2);

        value.Id.Should().Be(id);
        value.Name.Should().Be("Kitchen");
        value.Plants.Should().Be(2);
    }
}
