using FluentAssertions;

namespace Models.Tests;

public sealed class WateringEventTests
{
    [Fact(DisplayName = "WateringEvent create sets id and current time")]
    [Trait("Category", "Unit")]
    public void WateringEventCreateWhenPlantIdIsValidSetsDefaults()
    {
        // Arrange
        var plantId = PlantId.New();

        // Act
        var watering = WateringEvent.Create(plantId);

        // Assert
        watering.Id.Value.Should().NotBe(Guid.Empty);
        watering.PlantId.Should().Be(plantId);
        watering.Plant.Should().BeNull();
        watering.WateredAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "WateringEvent restore rehydrates persisted values")]
    [Trait("Category", "Unit")]
    public void WateringEventRestoreWhenArgumentsAreValidCreatesWateringEvent()
    {
        // Arrange
        var id = WateringEventId.New();
        var plantId = PlantId.New();
        var wateredAt = DateTimeOffset.UtcNow.AddDays(-1);

        // Act
        var watering = WateringEvent.Restore(id, plantId, wateredAt);

        // Assert
        watering.Id.Should().Be(id);
        watering.PlantId.Should().Be(plantId);
        watering.WateredAt.Should().Be(wateredAt);
    }
}
