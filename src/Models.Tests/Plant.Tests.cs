using FluentAssertions;

namespace Models.Tests;

public sealed class PlantTests
{
    [Fact(DisplayName = "Plant create sets editable values")]
    [Trait("Category", "Unit")]
    public void PlantCreateWhenArgumentsAreValidSetsValues()
    {
        // Arrange
        var userId = UserId.New();
        var locationId = LocationId.New();
        var plantedOn = new DateOnly(2026, 5, 22);

        // Act
        var createdAt = DateTimeOffset.UtcNow;
        var plant = Plant.Create(
            userId,
            PlantName.From("Basil"),
            PlantDescription.From("Green"),
            PlantSoil.From("Loose mix"),
            locationId,
            plantedOn,
            "data:image/png;base64,abc",
            createdAt);

        // Assert
        plant.Id.Value.Should().NotBe(Guid.Empty);
        plant.UserId.Should().Be(userId);
        plant.Name.Value.Should().Be("Basil");
        plant.Description.Value.Should().Be("Green");
        plant.Soil.Value.Should().Be("Loose mix");
        plant.LocationId.Should().Be(locationId);
        plant.PlantedOn.Should().Be(plantedOn);
        plant.PhotoDataUrl!.Value.Value.Should().Be("data:image/png;base64,abc");
        plant.CreatedAt.Should().Be(createdAt);
    }

    [Fact(DisplayName = "Plant update details changes editable values")]
    [Trait("Category", "Unit")]
    public void PlantUpdateDetailsWhenArgumentsAreValidChangesValues()
    {
        // Arrange
        var plant = Plant.Create(
            UserId.New(),
            PlantName.From("Basil"),
            PlantDescription.From("Green"),
            null,
            null,
            null);
        var locationId = LocationId.New();

        // Act
        plant.UpdateDetails(
            PlantName.From("Mint"),
            PlantDescription.From("Fresh"),
            PlantSoil.From("Coco peat"),
            locationId,
            new DateOnly(2026, 5, 22),
            "data:image/jpeg;base64,xyz");

        // Assert
        plant.Name.Value.Should().Be("Mint");
        plant.Description.Value.Should().Be("Fresh");
        plant.Soil.Value.Should().Be("Coco peat");
        plant.LocationId.Should().Be(locationId);
        plant.PlantedOn.Should().Be(new DateOnly(2026, 5, 22));
        plant.PhotoDataUrl!.Value.Value.Should().Be("data:image/jpeg;base64,xyz");
    }

    [Fact(DisplayName = "Plant water adds watering event")]
    [Trait("Category", "Unit")]
    public void PlantWaterWhenCalledAddsWateringEvent()
    {
        // Arrange
        var plant = Plant.Create(
            UserId.New(),
            PlantName.From("Basil"),
            PlantDescription.From(null),
            null,
            null,
            null);

        // Act
        var wateredAt = DateTimeOffset.UtcNow;
        var watering = plant.Water(wateredAt);

        // Assert
        watering.PlantId.Should().Be(plant.Id);
        watering.WateredAt.Should().Be(wateredAt);
        plant.WateringEvents.Should().ContainSingle().Which.Should().Be(watering);
    }

    [Fact(DisplayName = "Plant restore rehydrates persisted values")]
    [Trait("Category", "Unit")]
    public void PlantRestoreWhenArgumentsAreValidCreatesPlant()
    {
        // Arrange
        var id = PlantId.New();
        var userId = UserId.New();
        var locationId = LocationId.New();
        var photo = ImageDataUrl.PlantPhoto("data:image/png;base64,abc");
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);

        // Act
        var plant = Plant.Restore(
            id,
            userId,
            PlantName.From("Basil"),
            PlantDescription.From("Green"),
            PlantSoil.From("Loose mix"),
            locationId,
            new DateOnly(2026, 5, 22),
            photo,
            createdAt);

        // Assert
        plant.Id.Should().Be(id);
        plant.UserId.Should().Be(userId);
        plant.Soil.Value.Should().Be("Loose mix");
        plant.LocationId.Should().Be(locationId);
        plant.PhotoDataUrl.Should().Be(photo);
        plant.CreatedAt.Should().Be(createdAt);
    }
}
