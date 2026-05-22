using FluentAssertions;

namespace Transport.Tests;

public sealed class RequestContractsTests
{
    [Fact(DisplayName = "LoginRequest stores credentials")]
    [Trait("Category", "Unit")]
    public void LoginRequestWhenCreatedStoresValues()
    {
        // Act
        var request = new LoginRequest("user@example.com", "password");

        // Assert
        request.Email.Should().Be("user@example.com");
        request.Password.Should().Be("password");
    }

    [Fact(DisplayName = "RegisterRequest stores registration values")]
    [Trait("Category", "Unit")]
    public void RegisterRequestWhenCreatedStoresValues()
    {
        // Act
        var request = new RegisterRequest("invite", "User", "user@example.com", "password", "ru");

        // Assert
        request.InviteCode.Should().Be("invite");
        request.DisplayName.Should().Be("User");
        request.Email.Should().Be("user@example.com");
        request.Password.Should().Be("password");
        request.Language.Should().Be("ru");
    }

    [Fact(DisplayName = "SettingsRequest stores settings values")]
    [Trait("Category", "Unit")]
    public void SettingsRequestWhenCreatedStoresValues()
    {
        // Act
        var request = new SettingsRequest("User", "en", "data:image/png;base64,abc", true);

        // Assert
        request.DisplayName.Should().Be("User");
        request.Language.Should().Be("en");
        request.AvatarDataUrl.Should().Be("data:image/png;base64,abc");
        request.IsGardenPublic.Should().BeTrue();
    }

    [Fact(DisplayName = "PlantRequest stores plant values")]
    [Trait("Category", "Unit")]
    public void PlantRequestWhenCreatedStoresValues()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var plantedOn = new DateOnly(2026, 5, 22);
        var lastWateredOn = new DateOnly(2026, 5, 23);

        // Act
        var request = new PlantRequest(
            "Basil",
            "Green",
            locationId,
            plantedOn,
            lastWateredOn,
            "data:image/png;base64,abc");

        // Assert
        request.Name.Should().Be("Basil");
        request.Description.Should().Be("Green");
        request.LocationId.Should().Be(locationId);
        request.PlantedOn.Should().Be(plantedOn);
        request.LastWateredOn.Should().Be(lastWateredOn);
        request.PhotoDataUrl.Should().Be("data:image/png;base64,abc");
    }

    [Fact(DisplayName = "LocationRequest stores location name")]
    [Trait("Category", "Unit")]
    public void LocationRequestWhenCreatedStoresValue()
    {
        // Act
        var request = new LocationRequest("Kitchen");

        // Assert
        request.Name.Should().Be("Kitchen");
    }

    [Fact(DisplayName = "BlockUserRequest stores blocked state")]
    [Trait("Category", "Unit")]
    public void BlockUserRequestWhenCreatedStoresValue()
    {
        // Act
        var request = new BlockUserRequest(true);

        // Assert
        request.IsBlocked.Should().BeTrue();
    }
}
