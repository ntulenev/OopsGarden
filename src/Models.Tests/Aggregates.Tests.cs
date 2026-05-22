using FluentAssertions;

namespace Models.Tests;

public sealed class AggregatesTests
{
    [Fact(DisplayName = "AppUser create sets defaults")]
    [Trait("Category", "Unit")]
    public void AppUserCreateWhenArgumentsAreValidSetsDefaults()
    {
        // Act
        var user = CreateUser();

        // Assert
        user.Id.Value.Should().NotBe(Guid.Empty);
        user.Email.Value.Should().Be("USER@EXAMPLE.COM");
        user.DisplayName.Value.Should().Be("User");
        user.PasswordHash.Value.Should().Be("hash");
        user.Language.Value.Should().Be("en");
        user.IsBlocked.Should().BeFalse();
        user.IsGardenPublic.Should().BeFalse();
        user.AvatarDataUrl.Should().BeNull();
        user.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "AppUser update settings changes editable values")]
    [Trait("Category", "Unit")]
    public void AppUserUpdateSettingsWhenValuesAreValidUpdatesUser()
    {
        // Arrange
        var user = CreateUser();
        var avatar = ImageDataUrl.Avatar("data:image/png;base64,abc");

        // Act
        user.UpdateSettings(DisplayName.From("New"), LanguageCode.From("ru"), avatar, true);

        // Assert
        user.DisplayName.Value.Should().Be("New");
        user.Language.Value.Should().Be("ru");
        user.AvatarDataUrl.Should().Be(avatar);
        user.IsGardenPublic.Should().BeTrue();
    }

    [Fact(DisplayName = "AppUser restore rehydrates persisted values")]
    [Trait("Category", "Unit")]
    public void AppUserRestoreWhenArgumentsAreValidCreatesUser()
    {
        // Arrange
        var id = UserId.New();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var avatar = ImageDataUrl.Avatar("data:image/png;base64,abc");

        // Act
        var user = AppUser.Restore(
            id,
            UserEmail.From("user@example.com"),
            DisplayName.From("User"),
            PasswordHash.From("hash"),
            LanguageCode.From("ru"),
            avatar,
            isGardenPublic: true,
            isBlocked: true,
            createdAt);

        // Assert
        user.Id.Should().Be(id);
        user.Language.Value.Should().Be("ru");
        user.AvatarDataUrl.Should().Be(avatar);
        user.IsGardenPublic.Should().BeTrue();
        user.IsBlocked.Should().BeTrue();
        user.CreatedAt.Should().Be(createdAt);
    }

    [Fact(DisplayName = "InviteLink create makes usable invite")]
    [Trait("Category", "Unit")]
    public void InviteLinkCreateWhenArgumentsAreValidCreatesUsableInvite()
    {
        // Act
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));

        // Assert
        invite.Id.Value.Should().NotBe(Guid.Empty);
        invite.Code.Value.Should().Be("code");
        invite.CreatedBy.Value.Should().Be("admin");
        invite.CanBeUsed.Should().BeTrue();
        invite.UsedAt.Should().BeNull();
        invite.UsedByUserId.Should().BeNull();
        invite.IsRevoked.Should().BeFalse();
    }

    [Fact(DisplayName = "InviteLink mark used consumes invite")]
    [Trait("Category", "Unit")]
    public void InviteLinkMarkUsedWhenInviteCanBeUsedConsumesInvite()
    {
        // Arrange
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));
        var userId = UserId.New();

        // Act
        invite.MarkUsed(userId);

        // Assert
        invite.CanBeUsed.Should().BeFalse();
        invite.UsedAt.Should().NotBeNull();
        invite.UsedByUserId.Should().Be(userId);
    }

    [Fact(DisplayName = "InviteLink mark used throws when invite is revoked")]
    [Trait("Category", "Unit")]
    public void InviteLinkMarkUsedWhenInviteIsRevokedThrowsInvalidOperationException()
    {
        // Arrange
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));
        invite.Revoke();

        // Act
        Action act = () => invite.MarkUsed(UserId.New());

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact(DisplayName = "InviteLink revoke throws when invite is used")]
    [Trait("Category", "Unit")]
    public void InviteLinkRevokeWhenInviteIsUsedThrowsInvalidOperationException()
    {
        // Arrange
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));
        invite.MarkUsed(UserId.New());

        // Act
        Action act = invite.Revoke;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

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

    [Fact(DisplayName = "Plant create sets editable values")]
    [Trait("Category", "Unit")]
    public void PlantCreateWhenArgumentsAreValidSetsValues()
    {
        // Arrange
        var userId = UserId.New();
        var locationId = LocationId.New();
        var plantedOn = new DateOnly(2026, 5, 22);

        // Act
        var plant = Plant.Create(
            userId,
            PlantName.From("Basil"),
            PlantDescription.From("Green"),
            locationId,
            plantedOn,
            "data:image/png;base64,abc");

        // Assert
        plant.Id.Value.Should().NotBe(Guid.Empty);
        plant.UserId.Should().Be(userId);
        plant.Name.Value.Should().Be("Basil");
        plant.Description.Value.Should().Be("Green");
        plant.LocationId.Should().Be(locationId);
        plant.PlantedOn.Should().Be(plantedOn);
        plant.PhotoDataUrl!.Value.Value.Should().Be("data:image/png;base64,abc");
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
            locationId,
            new DateOnly(2026, 5, 22),
            "data:image/jpeg;base64,xyz");

        // Assert
        plant.Name.Value.Should().Be("Mint");
        plant.Description.Value.Should().Be("Fresh");
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
        var watering = plant.Water();

        // Assert
        watering.PlantId.Should().Be(plant.Id);
        watering.WateredAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        plant.WateringEvents.Should().ContainSingle().Which.Should().Be(watering);
    }

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
            locationId,
            new DateOnly(2026, 5, 22),
            photo,
            createdAt);

        // Assert
        plant.Id.Should().Be(id);
        plant.UserId.Should().Be(userId);
        plant.LocationId.Should().Be(locationId);
        plant.PhotoDataUrl.Should().Be(photo);
        plant.CreatedAt.Should().Be(createdAt);
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

    private static AppUser CreateUser() =>
        AppUser.Create(
            UserEmail.From("user@example.com"),
            DisplayName.From("User"),
            PasswordHash.From("hash"),
            LanguageCode.From("en"));
}
