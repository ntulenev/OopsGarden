using FluentAssertions;

namespace Transport.Tests;

public sealed class PublicGardenResponseTests
{
    [Fact(DisplayName = "Constructor stores nested plants")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresNestedPlants()
    {
        var plant = new PublicPlantResponse(Guid.NewGuid(), "Basil", "Green", "Loose mix", "photo", null, null, null);
        var gardenId = Guid.NewGuid();

        var response = new PublicGardenResponse(gardenId, "User", "avatar", [plant]);

        response.Id.Should().Be(gardenId);
        response.Name.Should().Be("User");
        response.Avatar.Should().Be("avatar");
        response.Plants.Should().ContainSingle().Which.Should().Be(plant);
    }
}
