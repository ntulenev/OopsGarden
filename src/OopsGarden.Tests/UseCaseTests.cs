using System.Security.Claims;

using Abstractions;

using FluentAssertions;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Models;

using OopsGarden.Configuration;
using OopsGarden.UseCases;

namespace OopsGarden.Tests;

public sealed class UseCaseTests
{
    [Fact(DisplayName = "Login returns authenticated user for valid credentials")]
    [Trait("Category", "Unit")]
    public async Task LoginWhenCredentialsAreValidReturnsUser()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var hasher = new PasswordHasher<AppUser>();
        var user = CreateUser("user@example.com");
        user.ChangePasswordHash(PasswordHash.From(hasher.HashPassword(user, "secret")));
        unitOfWork.Users.Users.Add(user);
        var useCase = new LoginUseCase(unitOfWork, hasher);

        // Act
        var result = await useCase.ExecuteAsync(new LoginCommand("USER@example.com", "secret"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("USER@EXAMPLE.COM");
    }

    [Theory(DisplayName = "Login returns null when credentials cannot authenticate")]
    [Trait("Category", "Unit")]
    [InlineData("missing@example.com", "secret")]
    [InlineData("user@example.com", "bad")]
    public async Task LoginWhenCredentialsAreInvalidReturnsNull(string email, string password)
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var hasher = new PasswordHasher<AppUser>();
        var user = CreateUser("user@example.com");
        user.ChangePasswordHash(PasswordHash.From(hasher.HashPassword(user, "secret")));
        unitOfWork.Users.Users.Add(user);
        var useCase = new LoginUseCase(unitOfWork, hasher);

        // Act
        var result = await useCase.ExecuteAsync(new LoginCommand(email, password), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "Admin login matches configured credentials case-insensitively")]
    [Trait("Category", "Unit")]
    public void AdminLoginWhenCredentialsAreValidReturnsAdmin()
    {
        // Arrange
        var options = Options.Create(new AdminOptions());
        options.Value.Users.Add(new AdminCredential { UserName = "Admin", Password = "secret" });
        var useCase = new AdminLoginUseCase(options);

        // Act
        var result = useCase.Execute(new LoginCommand(" admin ", "secret"));

        // Assert
        result.Should().Be(new AdminLogin("Admin", "Admin"));
    }

    [Fact(DisplayName = "Register consumes invite and creates user")]
    [Trait("Category", "Unit")]
    public async Task RegisterWhenInviteIsValidCreatesUser()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var invite = InviteLink.Create(InviteCode.From("invite"), AdminName.From("admin"));
        unitOfWork.Invites.Invites.Add(invite);
        var useCase = new RegisterUseCase(unitOfWork, new PasswordHasher<AppUser>());

        // Act
        var result = await useCase.ExecuteAsync(
            new RegisterCommand("invite", "User", "user@example.com", "secret", "en"),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.User.Should().NotBeNull();
        unitOfWork.Users.Users.Should().ContainSingle();
        invite.CanBeUsed.Should().BeFalse();
        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Register returns error when invite is invalid")]
    [Trait("Category", "Unit")]
    public async Task RegisterWhenInviteIsInvalidReturnsError()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var useCase = new RegisterUseCase(unitOfWork, new PasswordHasher<AppUser>());

        // Act
        var result = await useCase.ExecuteAsync(
            new RegisterCommand("missing", "User", "user@example.com", "secret", "en"),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid invite.");
        unitOfWork.Users.Users.Should().BeEmpty();
    }

    [Fact(DisplayName = "Register returns error when email exists")]
    [Trait("Category", "Unit")]
    public async Task RegisterWhenEmailExistsReturnsError()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        unitOfWork.Invites.Invites.Add(InviteLink.Create(InviteCode.From("invite"), AdminName.From("admin")));
        unitOfWork.Users.Users.Add(CreateUser("user@example.com"));
        var useCase = new RegisterUseCase(unitOfWork, new PasswordHasher<AppUser>());

        // Act
        var result = await useCase.ExecuteAsync(
            new RegisterCommand("invite", "User", "USER@example.com", "secret", "en"),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email already registered.");
    }

    [Fact(DisplayName = "Update settings updates active user")]
    [Trait("Category", "Unit")]
    public async Task UpdateSettingsWhenUserExistsUpdatesUser()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var user = CreateUser("user@example.com");
        unitOfWork.Users.Users.Add(user);
        var useCase = new UpdateSettingsUseCase(unitOfWork);

        // Act
        var result = await useCase.ExecuteAsync(
            user.Id,
            new SettingsCommand("New", "ru", "data:image/png;base64,abc", true),
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("New");
        result.Language.Should().Be("ru");
        result.AvatarData.Should().Be("data:image/png;base64,abc");
        result.IsGardenPublic.Should().BeTrue();
        user.DisplayName.Value.Should().Be("New");
    }

    [Fact(DisplayName = "GetMe returns current user for authenticated user principal")]
    [Trait("Category", "Unit")]
    public async Task GetMeWhenPrincipalIsUserReturnsCurrentUser()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var user = CreateUser("user@example.com");
        unitOfWork.Users.Users.Add(user);
        var principal = CreatePrincipal(user.Id, "User");
        var useCase = new GetMeUseCase(unitOfWork);

        // Act
        var result = await useCase.ExecuteAsync(principal, CancellationToken.None);

        // Assert
        result.Authenticated.Should().BeTrue();
        result.Id.Should().Be(user.Id);
        result.Name.Should().Be("User");
        result.Role.Should().Be("User");
    }

    [Fact(DisplayName = "Create plant returns invalid location error")]
    [Trait("Category", "Unit")]
    public async Task CreatePlantWhenLocationDoesNotExistReturnsError()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var useCase = new CreatePlantUseCase(unitOfWork);

        // Act
        var result = await useCase.ExecuteAsync(
            UserId.New(),
            new PlantCommand("Basil", "Green", Guid.NewGuid(), null, null, null),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid location.");
        unitOfWork.Garden.Plants.Should().BeEmpty();
    }

    [Fact(DisplayName = "Create plant persists plant when command is valid")]
    [Trait("Category", "Unit")]
    public async Task CreatePlantWhenCommandIsValidPersistsPlant()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var userId = UserId.New();
        var location = Location.Create(userId, LocationName.From("Kitchen"));
        unitOfWork.Garden.Locations.Add(location);
        var useCase = new CreatePlantUseCase(unitOfWork);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            new PlantCommand("Basil", "Green", location.Id.Value, null, null, null),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Id.Should().NotBeNull();
        unitOfWork.Garden.Plants.Should().ContainSingle();
        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Update plant returns not found for missing plant")]
    [Trait("Category", "Unit")]
    public async Task UpdatePlantWhenPlantIsMissingReturnsNotFound()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var useCase = new UpdatePlantUseCase(unitOfWork);

        // Act
        var result = await useCase.ExecuteAsync(
            UserId.New(),
            Guid.NewGuid(),
            new PlantCommand("Basil", "Green", null, null, null, null),
            CancellationToken.None);

        // Assert
        result.Status.Should().Be(UpdatePlantStatus.NotFound);
    }

    [Fact(DisplayName = "Water plant adds watering event")]
    [Trait("Category", "Unit")]
    public async Task WaterPlantWhenPlantExistsAddsWateringEvent()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var userId = UserId.New();
        var plant = Plant.Create(userId, PlantName.From("Basil"), PlantDescription.From(null), null, null, null);
        unitOfWork.Garden.Plants.Add(plant);
        var useCase = new WaterPlantUseCase(unitOfWork);

        // Act
        var result = await useCase.ExecuteAsync(userId, plant.Id.Value, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        unitOfWork.Garden.WateringEvents.Should().ContainSingle();
    }

    [Fact(DisplayName = "Admin invite use cases create revoke and delete invites")]
    [Trait("Category", "Unit")]
    public async Task AdminInviteUseCasesWhenInvitesAreManagedUpdateRepository()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")], "Test"));
        var create = new CreateInviteUseCase(unitOfWork);

        // Act
        var created = await create.ExecuteAsync(principal, CancellationToken.None);
        unitOfWork.Invites.Invites.Should().ContainSingle();
        var revoked = await new RevokeInviteUseCase(unitOfWork).ExecuteAsync(created.Id.Value, CancellationToken.None);
        var deleteResult = await new DeleteInviteUseCase(unitOfWork).ExecuteAsync(created.Id.Value, CancellationToken.None);

        // Assert
        created.Code.Should().NotBeNullOrWhiteSpace();
        revoked.Should().BeTrue();
        deleteResult.Status.Should().Be(DeleteInviteStatus.Deleted);
        unitOfWork.Invites.Invites.Should().BeEmpty();
    }

    [Fact(DisplayName = "Block user updates blocked state")]
    [Trait("Category", "Unit")]
    public async Task BlockUserWhenUserExistsUpdatesBlockedState()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var user = CreateUser("user@example.com");
        unitOfWork.Users.Users.Add(user);

        // Act
        var blocked = await new BlockUserUseCase(unitOfWork).ExecuteAsync(user.Id.Value, true, CancellationToken.None);
        var unblocked = await new BlockUserUseCase(unitOfWork).ExecuteAsync(user.Id.Value, false, CancellationToken.None);

        // Assert
        blocked.Should().BeTrue();
        unblocked.Should().BeTrue();
        user.IsBlocked.Should().BeFalse();
    }

    private static AppUser CreateUser(string email) =>
        AppUser.Create(
            UserEmail.From(email),
            DisplayName.From("User"),
            PasswordHash.From("hash"),
            LanguageCode.From("en"));

    private static ClaimsPrincipal CreatePrincipal(UserId userId, string role) =>
        new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                new Claim(ClaimTypes.Name, "User"),
                new Claim(ClaimTypes.Role, role)
            ],
            "Test"));
}
