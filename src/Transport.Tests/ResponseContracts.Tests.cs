using FluentAssertions;

namespace Transport.Tests;

public sealed class ResponseContractsTests
{
    [Fact(DisplayName = "AuthenticatedUserResponse stores user values")]
    [Trait("Category", "Unit")]
    public void AuthenticatedUserResponseWhenCreatedStoresValues()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = new AuthenticatedUserResponse(
            id,
            "User",
            "user@example.com",
            "en",
            "data:image/png;base64,abc",
            true);

        // Assert
        response.Id.Should().Be(id);
        response.DisplayName.Should().Be("User");
        response.Email.Should().Be("user@example.com");
        response.Language.Should().Be("en");
        response.AvatarDataUrl.Should().Be("data:image/png;base64,abc");
        response.IsGardenPublic.Should().BeTrue();
    }

    [Fact(DisplayName = "MeResponse defaults optional values")]
    [Trait("Category", "Unit")]
    public void MeResponseWhenOnlyAuthenticatedIsProvidedDefaultsOptionalValues()
    {
        // Act
        var response = new MeResponse(false);

        // Assert
        response.Authenticated.Should().BeFalse();
        response.Id.Should().BeNull();
        response.Name.Should().BeNull();
        response.Role.Should().BeNull();
        response.Language.Should().BeNull();
        response.Avatar.Should().BeNull();
        response.IsGardenPublic.Should().BeFalse();
    }

    [Fact(DisplayName = "Garden responses store nested plant values")]
    [Trait("Category", "Unit")]
    public void GardenResponsesWhenCreatedStoreValues()
    {
        // Arrange
        var plantId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var gardenId = Guid.NewGuid();
        var location = new PlantLocationResponse(locationId, "Kitchen");
        var plant = new PublicPlantResponse(plantId, "Basil", "Green", "photo", location);

        // Act
        var response = new PublicGardenResponse(gardenId, "User", "avatar", [plant]);

        // Assert
        response.Id.Should().Be(gardenId);
        response.Name.Should().Be("User");
        response.Avatar.Should().Be("avatar");
        response.Plants.Should().ContainSingle().Which.Should().Be(plant);
        plant.Location.Should().Be(location);
    }

    [Fact(DisplayName = "PlantSummaryResponse stores summary values")]
    [Trait("Category", "Unit")]
    public void PlantSummaryResponseWhenCreatedStoresValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var location = new PlantLocationResponse(Guid.NewGuid(), "Kitchen");
        var plantedOn = new DateOnly(2026, 5, 22);
        var lastWateredAt = DateTimeOffset.UtcNow;

        // Act
        var response = new PlantSummaryResponse(
            id,
            "Basil",
            "Green",
            "photo",
            plantedOn,
            location,
            lastWateredAt);

        // Assert
        response.Id.Should().Be(id);
        response.Name.Should().Be("Basil");
        response.Description.Should().Be("Green");
        response.PhotoDataUrl.Should().Be("photo");
        response.PlantedOn.Should().Be(plantedOn);
        response.Location.Should().Be(location);
        response.LastWateredAt.Should().Be(lastWateredAt);
    }

    [Fact(DisplayName = "Admin responses store administration values")]
    [Trait("Category", "Unit")]
    public void AdminResponsesWhenCreatedStoreValues()
    {
        // Arrange
        var inviteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        // Act
        var invite = new AdminInviteResponse(inviteId, "code", createdAt, "admin", null, null, false);
        var user = new AdminUserResponse(userId, "User", "user@example.com", false, "en", createdAt, 2);
        var createdInvite = new CreatedInviteResponse(inviteId, "code", "/?invite=code");
        var admin = new AdminLoginResponse("admin", "Admin");

        // Assert
        invite.Id.Should().Be(inviteId);
        invite.Code.Should().Be("code");
        user.Id.Should().Be(userId);
        user.Plants.Should().Be(2);
        createdInvite.Url.Should().Be("/?invite=code");
        admin.Role.Should().Be("Admin");
    }
}
