using FluentAssertions;

namespace Transport.Tests;

public sealed class PublicPlantResponseTests
{
    [Fact(DisplayName = "Constructor stores plant values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var plantId = Guid.NewGuid();
        var location = new PlantLocationResponse(Guid.NewGuid(), "Kitchen");

        var response = new PublicPlantResponse(plantId, "Basil", "Green", "photo", location);

        response.Id.Should().Be(plantId);
        response.Name.Should().Be("Basil");
        response.Description.Should().Be("Green");
        response.PhotoDataUrl.Should().Be("photo");
        response.Location.Should().Be(location);
    }
}
