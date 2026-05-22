
using FluentAssertions;


namespace OopsGarden.Tests;

public sealed class PlantSummaryTests
{
    [Fact(DisplayName = "Constructor stores plant summary values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = PlantId.New();
        var location = new GardenPlantLocation(LocationId.New(), "Kitchen");
        var wateredAt = DateTimeOffset.UtcNow;

        var value = new PlantSummary(id, "Basil", "Green", "photo", new DateOnly(2026, 5, 22), location, wateredAt);

        value.Id.Should().Be(id);
        value.Name.Should().Be("Basil");
        value.Description.Should().Be("Green");
        value.PhotoData.Should().Be("photo");
        value.PlantedOn.Should().Be(new DateOnly(2026, 5, 22));
        value.Location.Should().Be(location);
        value.LastWateredAt.Should().Be(wateredAt);
    }
}
