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

        var plantedOn = new DateOnly(2026, 5, 22);
        var lastWateredAt = DateTimeOffset.UtcNow;

        var response = new PublicPlantResponse(plantId, "Basil", "Green", "Loose mix", "photo", plantedOn, lastWateredAt, location);

        response.Id.Should().Be(plantId);
        response.Name.Should().Be("Basil");
        response.Description.Should().Be("Green");
        response.Soil.Should().Be("Loose mix");
        response.PhotoDataUrl.Should().Be("photo");
        response.PlantedOn.Should().Be(plantedOn);
        response.LastWateredAt.Should().Be(lastWateredAt);
        response.Location.Should().Be(location);
    }
}
