using FluentAssertions;

namespace Transport.Tests;

public sealed class PlantRequestTests
{
    [Fact(DisplayName = "Constructor stores plant values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var locationId = Guid.NewGuid();
        var plantedOn = new DateOnly(2026, 5, 22);
        var lastWateredOn = new DateOnly(2026, 5, 23);

        var request = new PlantRequest("Basil", "Green", "Loose mix", locationId, plantedOn, lastWateredOn, "data:image/png;base64,abc");

        request.Name.Should().Be("Basil");
        request.Description.Should().Be("Green");
        request.Soil.Should().Be("Loose mix");
        request.LocationId.Should().Be(locationId);
        request.PlantedOn.Should().Be(plantedOn);
        request.LastWateredOn.Should().Be(lastWateredOn);
        request.PhotoDataUrl.Should().Be("data:image/png;base64,abc");
    }
}
