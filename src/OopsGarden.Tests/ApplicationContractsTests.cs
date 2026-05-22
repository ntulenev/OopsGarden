using Abstractions;

using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class ApplicationContractsTests
{
    [Fact(DisplayName = "Authentication contracts store user values")]
    [Trait("Category", "Unit")]
    public void AuthenticationContractsWhenCreatedStoreValues()
    {
        // Arrange
        var userId = UserId.New();

        // Act
        var login = new LoginCommand("user@example.com", "secret");
        var authenticated = new AuthenticatedUser(userId, "User", "user@example.com", "ru", "avatar", true);
        var current = new CurrentUser(true, userId, "User", "User", "ru", "avatar", true);
        var admin = new AdminLogin("admin", "Admin");

        // Assert
        login.Email.Should().Be("user@example.com");
        login.Password.Should().Be("secret");
        authenticated.Id.Should().Be(userId);
        authenticated.DisplayName.Should().Be("User");
        authenticated.Email.Should().Be("user@example.com");
        authenticated.Language.Should().Be("ru");
        authenticated.AvatarData.Should().Be("avatar");
        authenticated.IsGardenPublic.Should().BeTrue();
        current.Authenticated.Should().BeTrue();
        current.Id.Should().Be(userId);
        current.Name.Should().Be("User");
        current.Role.Should().Be("User");
        current.Language.Should().Be("ru");
        current.AvatarData.Should().Be("avatar");
        current.IsGardenPublic.Should().BeTrue();
        admin.Name.Should().Be("admin");
        admin.Role.Should().Be("Admin");
    }

    [Fact(DisplayName = "Registration and settings contracts store values")]
    [Trait("Category", "Unit")]
    public void RegistrationAndSettingsContractsWhenCreatedStoreValues()
    {
        // Arrange
        var user = new AuthenticatedUser(UserId.New(), "User", "user@example.com", "en", null, false);

        // Act
        var register = new RegisterCommand("invite", "User", "user@example.com", "secret", "en");
        var success = new RegisterResult(user, null);
        var failure = new RegisterResult(null, "Invite is invalid.");
        var settings = new SettingsCommand("User", "ru", "avatar", true);

        // Assert
        register.InviteCode.Should().Be("invite");
        register.DisplayName.Should().Be("User");
        register.Email.Should().Be("user@example.com");
        register.Password.Should().Be("secret");
        register.Language.Should().Be("en");
        success.User.Should().Be(user);
        success.Error.Should().BeNull();
        success.IsSuccess.Should().BeTrue();
        failure.User.Should().BeNull();
        failure.Error.Should().Be("Invite is invalid.");
        failure.IsSuccess.Should().BeFalse();
        settings.DisplayName.Should().Be("User");
        settings.Language.Should().Be("ru");
        settings.AvatarData.Should().Be("avatar");
        settings.IsGardenPublic.Should().BeTrue();
    }

    [Fact(DisplayName = "Garden contracts store nested values")]
    [Trait("Category", "Unit")]
    public void GardenContractsWhenCreatedStoreValues()
    {
        // Arrange
        var userId = UserId.New();
        var plantId = PlantId.New();
        var locationId = LocationId.New();
        var location = new GardenPlantLocation(locationId, "Kitchen");
        var plant = new PublicGardenPlant(plantId, "Basil", "Green", "photo", location);

        // Act
        var garden = new PublicGarden(userId, "User", "avatar", [plant]);
        var locationSummary = new LocationSummary(locationId, "Kitchen", 3);
        var plantSummary = new PlantSummary(
            plantId,
            "Basil",
            "Green",
            "photo",
            new DateOnly(2026, 5, 22),
            location,
            DateTimeOffset.UtcNow);

        // Assert
        garden.Id.Should().Be(userId);
        garden.Name.Should().Be("User");
        garden.AvatarData.Should().Be("avatar");
        garden.Plants.Should().ContainSingle().Which.Should().Be(plant);
        plant.Id.Should().Be(plantId);
        plant.Name.Should().Be("Basil");
        plant.Description.Should().Be("Green");
        plant.PhotoData.Should().Be("photo");
        plant.Location.Should().Be(location);
        location.Id.Should().Be(locationId);
        location.Name.Should().Be("Kitchen");
        locationSummary.Id.Should().Be(locationId);
        locationSummary.Name.Should().Be("Kitchen");
        locationSummary.Plants.Should().Be(3);
        plantSummary.Id.Should().Be(plantId);
        plantSummary.Name.Should().Be("Basil");
        plantSummary.Description.Should().Be("Green");
        plantSummary.PhotoData.Should().Be("photo");
        plantSummary.PlantedOn.Should().Be(new DateOnly(2026, 5, 22));
        plantSummary.Location.Should().Be(location);
        plantSummary.LastWateredAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Plant command and results store values")]
    [Trait("Category", "Unit")]
    public void PlantCommandAndResultsWhenCreatedStoreValues()
    {
        // Arrange
        var plantId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        // Act
        var command = new PlantCommand(
            "Basil",
            "Green",
            locationId,
            new DateOnly(2026, 5, 22),
            new DateOnly(2026, 5, 23),
            "photo");
        var created = new CreatePlantResult(plantId, null);
        var failedCreate = new CreatePlantResult(null, "Location was not found.");
        var updated = new UpdatePlantResult(UpdatePlantStatus.Updated, null);
        var missing = new UpdatePlantResult(UpdatePlantStatus.NotFound, "Plant was not found.");

        // Assert
        command.Name.Should().Be("Basil");
        command.Description.Should().Be("Green");
        command.LocationId.Should().Be(locationId);
        command.PlantedOn.Should().Be(new DateOnly(2026, 5, 22));
        command.LastWateredOn.Should().Be(new DateOnly(2026, 5, 23));
        command.PhotoData.Should().Be("photo");
        created.Id.Should().Be(plantId);
        created.Error.Should().BeNull();
        created.IsSuccess.Should().BeTrue();
        failedCreate.Id.Should().BeNull();
        failedCreate.Error.Should().Be("Location was not found.");
        failedCreate.IsSuccess.Should().BeFalse();
        updated.Status.Should().Be(UpdatePlantStatus.Updated);
        updated.Error.Should().BeNull();
        missing.Status.Should().Be(UpdatePlantStatus.NotFound);
        missing.Error.Should().Be("Plant was not found.");
    }

    [Fact(DisplayName = "Invite and admin contracts store values")]
    [Trait("Category", "Unit")]
    public void InviteAndAdminContractsWhenCreatedStoreValues()
    {
        // Arrange
        var inviteId = InviteId.New();
        var userId = UserId.New();
        var createdAt = DateTimeOffset.UtcNow;
        var usedAt = createdAt.AddMinutes(1);
        var url = new Uri("https://example.com/?invite=code");

        // Act
        var invite = new AdminInvite(inviteId, "code", createdAt, "admin", usedAt, userId, true);
        var createdInvite = new CreatedInvite(inviteId, "code", url);
        var deleted = new DeleteInviteResult(DeleteInviteStatus.Deleted, null);
        var invalid = new DeleteInviteResult(DeleteInviteStatus.Invalid, "Invite was already used.");
        var adminUser = new AdminUser(userId, "User", "user@example.com", true, "ru", createdAt, 5);

        // Assert
        invite.Id.Should().Be(inviteId);
        invite.Code.Should().Be("code");
        invite.CreatedAt.Should().Be(createdAt);
        invite.CreatedBy.Should().Be("admin");
        invite.UsedAt.Should().Be(usedAt);
        invite.UsedByUserId.Should().Be(userId);
        invite.IsRevoked.Should().BeTrue();
        createdInvite.Id.Should().Be(inviteId);
        createdInvite.Code.Should().Be("code");
        createdInvite.Url.Should().Be(url);
        deleted.Status.Should().Be(DeleteInviteStatus.Deleted);
        deleted.Error.Should().BeNull();
        invalid.Status.Should().Be(DeleteInviteStatus.Invalid);
        invalid.Error.Should().Be("Invite was already used.");
        adminUser.Id.Should().Be(userId);
        adminUser.DisplayName.Should().Be("User");
        adminUser.Email.Should().Be("user@example.com");
        adminUser.IsBlocked.Should().BeTrue();
        adminUser.Language.Should().Be("ru");
        adminUser.CreatedAt.Should().Be(createdAt);
        adminUser.Plants.Should().Be(5);
    }

    [Fact(DisplayName = "Projection contracts store values")]
    [Trait("Category", "Unit")]
    public void ProjectionContractsWhenCreatedStoreValues()
    {
        // Arrange
        var inviteId = InviteId.New();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var locationId = LocationId.New();
        var createdAt = DateTimeOffset.UtcNow;
        var plantLocation = new GardenPlantLocationProjection(locationId, "Kitchen");
        var publicPlant = new PublicGardenPlantProjection(plantId, "Basil", "Green", "photo", plantLocation);

        // Act
        var invite = new AdminInviteProjection(inviteId, "code", createdAt, "admin", null, null, false);
        var user = new AdminUserProjection(userId, "User", "user@example.com", false, "en", createdAt, 2);
        var location = new GardenLocationProjection(locationId, "Kitchen", 2);
        var gardenPlant = new GardenPlantProjection(
            plantId,
            "Basil",
            "Green",
            "photo",
            new DateOnly(2026, 5, 22),
            plantLocation,
            createdAt);
        var publicGarden = new PublicGardenProjection(userId, "User", "avatar", [publicPlant]);

        // Assert
        invite.Id.Should().Be(inviteId);
        invite.Code.Should().Be("code");
        invite.CreatedAt.Should().Be(createdAt);
        invite.CreatedBy.Should().Be("admin");
        invite.UsedAt.Should().BeNull();
        invite.UsedByUserId.Should().BeNull();
        invite.IsRevoked.Should().BeFalse();
        user.Id.Should().Be(userId);
        user.DisplayName.Should().Be("User");
        user.Email.Should().Be("user@example.com");
        user.IsBlocked.Should().BeFalse();
        user.Language.Should().Be("en");
        user.CreatedAt.Should().Be(createdAt);
        user.Plants.Should().Be(2);
        location.Id.Should().Be(locationId);
        location.Name.Should().Be("Kitchen");
        location.Plants.Should().Be(2);
        plantLocation.Id.Should().Be(locationId);
        plantLocation.Name.Should().Be("Kitchen");
        gardenPlant.Id.Should().Be(plantId);
        gardenPlant.Name.Should().Be("Basil");
        gardenPlant.Description.Should().Be("Green");
        gardenPlant.PhotoData.Should().Be("photo");
        gardenPlant.PlantedOn.Should().Be(new DateOnly(2026, 5, 22));
        gardenPlant.Location.Should().Be(plantLocation);
        gardenPlant.LastWateredAt.Should().Be(createdAt);
        publicPlant.Id.Should().Be(plantId);
        publicPlant.Name.Should().Be("Basil");
        publicPlant.Description.Should().Be("Green");
        publicPlant.PhotoData.Should().Be("photo");
        publicPlant.Location.Should().Be(plantLocation);
        publicGarden.Id.Should().Be(userId);
        publicGarden.Name.Should().Be("User");
        publicGarden.Avatar.Should().Be("avatar");
        publicGarden.Plants.Should().ContainSingle().Which.Should().Be(publicPlant);
    }
}
