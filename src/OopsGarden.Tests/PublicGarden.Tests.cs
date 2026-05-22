using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class PublicGardenTests
{
    [Fact(DisplayName = "Constructor stores public garden values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = UserId.New();
        var plant = new PublicGardenPlant(PlantId.New(), "Basil", "Green", "photo", null, null, null);

        var value = new PublicGarden(id, "User", "avatar", [plant]);

        value.Id.Should().Be(id);
        value.Name.Should().Be("User");
        value.AvatarData.Should().Be("avatar");
        value.Plants.Should().ContainSingle().Which.Should().Be(plant);
    }
}
