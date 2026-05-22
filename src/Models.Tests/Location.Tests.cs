using FluentAssertions;

namespace Models.Tests;

public sealed class LocationTests
{
    [Fact(DisplayName = "Location rename changes name")]
    [Trait("Category", "Unit")]
    public void LocationRenameWhenNameIsValidChangesName()
    {
        // Arrange
        var location = Location.Create(UserId.New(), LocationName.From("Kitchen"));

        // Act
        location.Rename(LocationName.From("Window"));

        // Assert
        location.Name.Value.Should().Be("Window");
    }

    [Fact(DisplayName = "Location create sets owner and empty plants")]
    [Trait("Category", "Unit")]
    public void LocationCreateWhenArgumentsAreValidSetsDefaults()
    {
        // Arrange
        var userId = UserId.New();

        // Act
        var location = Location.Create(userId, LocationName.From("Kitchen"));

        // Assert
        location.Id.Value.Should().NotBe(Guid.Empty);
        location.UserId.Should().Be(userId);
        location.Name.Value.Should().Be("Kitchen");
        location.Plants.Should().BeEmpty();
    }

    [Fact(DisplayName = "Location restore rehydrates persisted values")]
    [Trait("Category", "Unit")]
    public void LocationRestoreWhenArgumentsAreValidCreatesLocation()
    {
        // Arrange
        var id = LocationId.New();
        var userId = UserId.New();

        // Act
        var location = Location.Restore(id, userId, LocationName.From("Kitchen"));

        // Assert
        location.Id.Should().Be(id);
        location.UserId.Should().Be(userId);
        location.Name.Value.Should().Be("Kitchen");
    }
}
