using FluentAssertions;

namespace Models.Tests;

public sealed class AppUserTests
{
    [Fact(DisplayName = "AppUser create sets defaults")]
    [Trait("Category", "Unit")]
    public void AppUserCreateWhenArgumentsAreValidSetsDefaults()
    {
        // Act
        var createdAt = DateTimeOffset.UtcNow;
        var user = ModelTestUsers.Create(createdAt);

        // Assert
        user.Id.Value.Should().NotBe(Guid.Empty);
        user.Email.Value.Should().Be("USER@EXAMPLE.COM");
        user.DisplayName.Value.Should().Be("User");
        user.PasswordHash.Value.Should().Be("hash");
        user.Language.Value.Should().Be("en");
        user.IsBlocked.Should().BeFalse();
        user.IsGardenPublic.Should().BeFalse();
        user.AvatarDataUrl.Should().BeNull();
        user.CreatedAt.Should().Be(createdAt);
    }

    [Fact(DisplayName = "AppUser update settings changes editable values")]
    [Trait("Category", "Unit")]
    public void AppUserUpdateSettingsWhenValuesAreValidUpdatesUser()
    {
        // Arrange
        var user = ModelTestUsers.Create();
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
}
