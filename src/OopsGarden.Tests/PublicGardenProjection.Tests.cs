using Abstractions;

using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class PublicGardenProjectionTests
{
    [Fact(DisplayName = "Constructor stores public garden projection values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = UserId.New();
        var plant = new PublicGardenPlantProjection(PlantId.New(), "Basil", "Green", "photo", null, null, null);

        var value = new PublicGardenProjection(id, "User", "avatar", [plant]);

        value.Id.Should().Be(id);
        value.Name.Should().Be("User");
        value.Avatar.Should().Be("avatar");
        value.Plants.Should().ContainSingle().Which.Should().Be(plant);
    }
}
