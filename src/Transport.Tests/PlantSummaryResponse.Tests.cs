using FluentAssertions;

namespace Transport.Tests;

public sealed class PlantSummaryResponseTests
{
    [Fact(DisplayName = "Constructor stores summary values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = Guid.NewGuid();
        var location = new PlantLocationResponse(Guid.NewGuid(), "Kitchen");
        var plantedOn = new DateOnly(2026, 5, 22);
        var lastWateredAt = DateTimeOffset.UtcNow;

        var response = new PlantSummaryResponse(id, "Basil", "Green", "Loose mix", "photo", plantedOn, location, lastWateredAt);

        response.Id.Should().Be(id);
        response.Name.Should().Be("Basil");
        response.Description.Should().Be("Green");
        response.Soil.Should().Be("Loose mix");
        response.PhotoDataUrl.Should().Be("photo");
        response.PlantedOn.Should().Be(plantedOn);
        response.Location.Should().Be(location);
        response.LastWateredAt.Should().Be(lastWateredAt);
    }
}
