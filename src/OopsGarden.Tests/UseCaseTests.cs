using System.Security.Claims;

using Abstractions;

using FluentAssertions;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Models;

using Moq;

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
        var cancellationToken = new CancellationToken();
        var user = CreateUser("user@example.com");
        var hasher = new PasswordHasher<AppUser>();
        user.ChangePasswordHash(PasswordHash.From(hasher.HashPassword(user, "secret")));

        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(usersMock.Object);
        var findCalls = 0;

        usersMock
            .Setup(repo => repo.FindByEmailAsync(UserEmail.From("USER@example.com"), cancellationToken))
            .Callback(() => findCalls++)
            .ReturnsAsync(user);

        var useCase = new LoginUseCase(unitOfWorkMock.Object, hasher);

        // Act
        var result = await useCase.ExecuteAsync(new LoginCommand("USER@example.com", "secret"), cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("USER@EXAMPLE.COM");
        findCalls.Should().Be(1);
    }

    [Theory(DisplayName = "Login returns null when credentials cannot authenticate")]
    [Trait("Category", "Unit")]
    [InlineData("missing@example.com", "secret")]
    [InlineData("user@example.com", "bad")]
    public async Task LoginWhenCredentialsAreInvalidReturnsNull(string email, string password)
    {
        // Arrange
        ArgumentNullException.ThrowIfNull(email);
        var cancellationToken = new CancellationToken();
        var hasher = new PasswordHasher<AppUser>();
        var user = CreateUser("user@example.com");
        user.ChangePasswordHash(PasswordHash.From(hasher.HashPassword(user, "secret")));
        var found = email.StartsWith("missing", StringComparison.Ordinal) ? null : user;

        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(usersMock.Object);
        var findCalls = 0;

        usersMock
            .Setup(repo => repo.FindByEmailAsync(UserEmail.From(email), cancellationToken))
            .Callback(() => findCalls++)
            .ReturnsAsync(found);

        var useCase = new LoginUseCase(unitOfWorkMock.Object, hasher);

        // Act
        var result = await useCase.ExecuteAsync(new LoginCommand(email, password), cancellationToken);

        // Assert
        result.Should().BeNull();
        findCalls.Should().Be(1);
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

    [Fact(DisplayName = "Admin login returns null for invalid credentials")]
    [Trait("Category", "Unit")]
    public void AdminLoginWhenCredentialsAreInvalidReturnsNull()
    {
        // Arrange
        var options = Options.Create(new AdminOptions());
        options.Value.Users.Add(new AdminCredential { UserName = "Admin", Password = "secret" });
        var useCase = new AdminLoginUseCase(options);

        // Act
        var result = useCase.Execute(new LoginCommand("admin", "bad"));

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "Register consumes invite and creates user")]
    [Trait("Category", "Unit")]
    public async Task RegisterWhenInviteIsValidCreatesUser()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var invite = InviteLink.Create(InviteCode.From("invite"), AdminName.From("admin"));
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(usersMock.Object, invitesMock.Object);
        var findInviteCalls = 0;
        var emailExistsCalls = 0;
        var addUserCalls = 0;
        var saveCalls = 0;
        AppUser? addedUser = null;

        invitesMock
            .Setup(repo => repo.FindByCodeAsync(InviteCode.From("invite"), cancellationToken))
            .Callback(() => findInviteCalls++)
            .ReturnsAsync(invite);
        usersMock
            .Setup(repo => repo.ExistsByEmailAsync(UserEmail.From("user@example.com"), cancellationToken))
            .Callback(() => emailExistsCalls++)
            .ReturnsAsync(false);
        usersMock
            .Setup(repo => repo.AddAsync(It.IsAny<AppUser>(), cancellationToken))
            .Callback<AppUser, CancellationToken>((user, _) =>
            {
                addUserCalls++;
                addedUser = user;
            })
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new RegisterUseCase(unitOfWorkMock.Object, new PasswordHasher<AppUser>());

        // Act
        var result = await useCase.ExecuteAsync(
            new RegisterCommand("invite", "User", "user@example.com", "secret", "en"),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.User.Should().NotBeNull();
        addedUser.Should().NotBeNull();
        invite.CanBeUsed.Should().BeFalse();
        findInviteCalls.Should().Be(1);
        emailExistsCalls.Should().Be(1);
        addUserCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Register returns error when invite is invalid")]
    [Trait("Category", "Unit")]
    public async Task RegisterWhenInviteIsInvalidReturnsError()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(invites: invitesMock.Object);
        var findInviteCalls = 0;

        invitesMock
            .Setup(repo => repo.FindByCodeAsync(InviteCode.From("missing"), cancellationToken))
            .Callback(() => findInviteCalls++)
            .ReturnsAsync((InviteLink?)null);

        var useCase = new RegisterUseCase(unitOfWorkMock.Object, new PasswordHasher<AppUser>());

        // Act
        var result = await useCase.ExecuteAsync(
            new RegisterCommand("missing", "User", "user@example.com", "secret", "en"),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid invite.");
        findInviteCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Register returns error when email exists")]
    [Trait("Category", "Unit")]
    public async Task RegisterWhenEmailExistsReturnsError()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var invite = InviteLink.Create(InviteCode.From("invite"), AdminName.From("admin"));
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(usersMock.Object, invitesMock.Object);
        var emailExistsCalls = 0;

        invitesMock
            .Setup(repo => repo.FindByCodeAsync(InviteCode.From("invite"), cancellationToken))
            .ReturnsAsync(invite);
        usersMock
            .Setup(repo => repo.ExistsByEmailAsync(UserEmail.From("USER@example.com"), cancellationToken))
            .Callback(() => emailExistsCalls++)
            .ReturnsAsync(true);

        var useCase = new RegisterUseCase(unitOfWorkMock.Object, new PasswordHasher<AppUser>());

        // Act
        var result = await useCase.ExecuteAsync(
            new RegisterCommand("invite", "User", "USER@example.com", "secret", "en"),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email already registered.");
        emailExistsCalls.Should().Be(1);
        invite.CanBeUsed.Should().BeTrue();
    }

    [Fact(DisplayName = "Update settings updates active user")]
    [Trait("Category", "Unit")]
    public async Task UpdateSettingsWhenUserExistsUpdatesUser()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var user = CreateUser("user@example.com");
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(usersMock.Object);
        var findCalls = 0;
        var saveCalls = 0;

        usersMock
            .Setup(repo => repo.FindByIdAsync(user.Id, cancellationToken))
            .Callback(() => findCalls++)
            .ReturnsAsync(user);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new UpdateSettingsUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            user.Id,
            new SettingsCommand("New", "ru", "data:image/png;base64,abc", true),
            cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("New");
        result.Language.Should().Be("ru");
        result.AvatarData.Should().Be("data:image/png;base64,abc");
        result.IsGardenPublic.Should().BeTrue();
        user.DisplayName.Value.Should().Be("New");
        findCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Update settings returns null for blocked user")]
    [Trait("Category", "Unit")]
    public async Task UpdateSettingsWhenUserIsBlockedReturnsNull()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var user = CreateUser("user@example.com");
        user.Block();
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(usersMock.Object);
        usersMock.Setup(repo => repo.FindByIdAsync(user.Id, cancellationToken)).ReturnsAsync(user);
        var useCase = new UpdateSettingsUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            user.Id,
            new SettingsCommand("New", "ru", null, true),
            cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "GetMe returns anonymous current user for anonymous principal")]
    [Trait("Category", "Unit")]
    public async Task GetMeWhenPrincipalIsAnonymousReturnsAnonymousUser()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var unitOfWorkMock = CreateUnitOfWork();
        var useCase = new GetMeUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(new ClaimsPrincipal(), cancellationToken);

        // Assert
        result.Authenticated.Should().BeFalse();
    }

    [Fact(DisplayName = "GetMe returns current admin for admin principal")]
    [Trait("Category", "Unit")]
    public async Task GetMeWhenPrincipalIsAdminReturnsCurrentAdmin()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var unitOfWorkMock = CreateUnitOfWork();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin")
            ],
            "Test"));
        var useCase = new GetMeUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(principal, cancellationToken);

        // Assert
        result.Authenticated.Should().BeTrue();
        result.Name.Should().Be("admin");
        result.Role.Should().Be("Admin");
        result.Language.Should().Be("en");
    }

    [Fact(DisplayName = "GetMe returns current user for authenticated user principal")]
    [Trait("Category", "Unit")]
    public async Task GetMeWhenPrincipalIsUserReturnsCurrentUser()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var user = CreateUser("user@example.com");
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(usersMock.Object);
        var findCalls = 0;

        usersMock
            .Setup(repo => repo.FindByIdAsync(user.Id, cancellationToken))
            .Callback(() => findCalls++)
            .ReturnsAsync(user);

        var principal = CreatePrincipal(user.Id, "User");
        var useCase = new GetMeUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(principal, cancellationToken);

        // Assert
        result.Authenticated.Should().BeTrue();
        result.Id.Should().Be(user.Id);
        result.Name.Should().Be("User");
        result.Role.Should().Be("User");
        findCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Get public garden maps projection")]
    [Trait("Category", "Unit")]
    public async Task GetPublicGardenWhenProjectionExistsMapsGarden()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var locationId = LocationId.New();
        var plantId = PlantId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        var gardenCalls = 0;

        gardenMock
            .Setup(repo => repo.GetPublicGardenAsync(userId, cancellationToken))
            .Callback(() => gardenCalls++)
            .ReturnsAsync(new PublicGardenProjection(
                userId,
                "User",
                "avatar",
                [new PublicGardenPlantProjection(
                    plantId,
                    "Basil",
                    "Green",
                    "photo",
                    new GardenPlantLocationProjection(locationId, "Kitchen"))]));

        var useCase = new GetPublicGardenUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId.Value, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Plants.Should().ContainSingle();
        result.Plants[0].Location!.Name.Should().Be("Kitchen");
        gardenCalls.Should().Be(1);
    }

    [Fact(DisplayName = "List garden plants maps projections")]
    [Trait("Category", "Unit")]
    public async Task ListGardenPlantsWhenPlantsExistMapsPlants()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        var listCalls = 0;

        gardenMock
            .Setup(repo => repo.ListPlantsAsync(userId, cancellationToken))
            .Callback(() => listCalls++)
            .ReturnsAsync([
                new GardenPlantProjection(
                    PlantId.New(),
                    "Basil",
                    "Green",
                    null,
                    null,
                    null,
                    DateTimeOffset.UtcNow)
            ]);

        var useCase = new ListGardenPlantsUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, cancellationToken);

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("Basil");
        listCalls.Should().Be(1);
    }

    [Fact(DisplayName = "List garden locations maps projections")]
    [Trait("Category", "Unit")]
    public async Task ListGardenLocationsWhenLocationsExistMapsLocations()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        var listCalls = 0;

        gardenMock
            .Setup(repo => repo.ListLocationsAsync(userId, cancellationToken))
            .Callback(() => listCalls++)
            .ReturnsAsync([new GardenLocationProjection(LocationId.New(), "Kitchen", 2)]);

        var useCase = new ListGardenLocationsUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, cancellationToken);

        // Assert
        result.Should().ContainSingle();
        result[0].Plants.Should().Be(2);
        listCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Create location persists location")]
    [Trait("Category", "Unit")]
    public async Task CreateLocationWhenCommandIsValidPersistsLocation()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        var addCalls = 0;
        var saveCalls = 0;

        gardenMock
            .Setup(repo => repo.AddLocationAsync(It.Is<Location>(location => location.UserId == userId), cancellationToken))
            .Callback(() => addCalls++)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new CreateLocationUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, new LocationCommand("Kitchen"), cancellationToken);

        // Assert
        result.Name.Should().Be("Kitchen");
        addCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Rename location updates existing location")]
    [Trait("Category", "Unit")]
    public async Task RenameLocationWhenLocationExistsUpdatesLocation()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var location = Location.Create(userId, LocationName.From("Kitchen"));
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        var findCalls = 0;
        var saveCalls = 0;

        gardenMock
            .Setup(repo => repo.FindLocationAsync(userId, location.Id, cancellationToken))
            .Callback(() => findCalls++)
            .ReturnsAsync(location);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new RenameLocationUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, location.Id.Value, new LocationCommand("Window"), cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Window");
        location.Name.Value.Should().Be("Window");
        findCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Rename location returns null for missing location")]
    [Trait("Category", "Unit")]
    public async Task RenameLocationWhenLocationIsMissingReturnsNull()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var locationId = LocationId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        gardenMock.Setup(repo => repo.FindLocationAsync(userId, locationId, cancellationToken)).ReturnsAsync((Location?)null);
        var useCase = new RenameLocationUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, locationId.Value, new LocationCommand("Window"), cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "Delete location clears plants removes location and saves")]
    [Trait("Category", "Unit")]
    public async Task DeleteLocationWhenLocationExistsClearsPlantsAndRemovesLocation()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var location = Location.Create(userId, LocationName.From("Kitchen"));
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        var clearCalls = 0;
        var removeCalls = 0;
        var saveCalls = 0;

        gardenMock.Setup(repo => repo.FindLocationAsync(userId, location.Id, cancellationToken)).ReturnsAsync(location);
        gardenMock
            .Setup(repo => repo.ClearPlantLocationAsync(userId, location.Id, cancellationToken))
            .Callback(() => clearCalls++)
            .Returns(Task.CompletedTask);
        gardenMock
            .Setup(repo => repo.RemoveLocation(location))
            .Callback(() => removeCalls++);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new DeleteLocationUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, location.Id.Value, cancellationToken);

        // Assert
        result.Should().BeTrue();
        clearCalls.Should().Be(1);
        removeCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Delete location returns false for missing location")]
    [Trait("Category", "Unit")]
    public async Task DeleteLocationWhenLocationIsMissingReturnsFalse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var locationId = LocationId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        gardenMock.Setup(repo => repo.FindLocationAsync(userId, locationId, cancellationToken)).ReturnsAsync((Location?)null);
        var useCase = new DeleteLocationUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, locationId.Value, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Create plant returns invalid location error")]
    [Trait("Category", "Unit")]
    public async Task CreatePlantWhenLocationDoesNotExistReturnsError()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var locationId = LocationId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        var locationExistsCalls = 0;

        gardenMock
            .Setup(repo => repo.LocationExistsAsync(userId, locationId, cancellationToken))
            .Callback(() => locationExistsCalls++)
            .ReturnsAsync(false);

        var useCase = new CreatePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            new PlantCommand("Basil", "Green", locationId.Value, null, null, null),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid location.");
        locationExistsCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Create plant persists plant when command is valid")]
    [Trait("Category", "Unit")]
    public async Task CreatePlantWhenCommandIsValidPersistsPlant()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        var addCalls = 0;
        var saveCalls = 0;

        gardenMock
            .Setup(repo => repo.AddPlantAsync(It.Is<Plant>(plant => plant.UserId == userId), cancellationToken))
            .Callback(() => addCalls++)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new CreatePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            new PlantCommand("Basil", "Green", null, null, null, null),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Id.Should().NotBeNull();
        addCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Update plant returns not found for missing plant")]
    [Trait("Category", "Unit")]
    public async Task UpdatePlantWhenPlantIsMissingReturnsNotFound()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        gardenMock.Setup(repo => repo.FindPlantAsync(userId, plantId, cancellationToken)).ReturnsAsync((Plant?)null);
        var useCase = new UpdatePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plantId.Value,
            new PlantCommand("Basil", "Green", null, null, null, null),
            cancellationToken);

        // Assert
        result.Status.Should().Be(UpdatePlantStatus.NotFound);
    }

    [Fact(DisplayName = "Update plant returns invalid for missing location")]
    [Trait("Category", "Unit")]
    public async Task UpdatePlantWhenLocationIsMissingReturnsInvalid()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plant = Plant.Create(userId, PlantName.From("Basil"), PlantDescription.From(null), null, null, null);
        var locationId = LocationId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);

        gardenMock.Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken)).ReturnsAsync(plant);
        gardenMock.Setup(repo => repo.LocationExistsAsync(userId, locationId, cancellationToken)).ReturnsAsync(false);
        var useCase = new UpdatePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plant.Id.Value,
            new PlantCommand("Basil", "Green", locationId.Value, null, null, null),
            cancellationToken);

        // Assert
        result.Status.Should().Be(UpdatePlantStatus.Invalid);
        result.Error.Should().Be("Invalid location.");
    }

    [Fact(DisplayName = "Update plant updates details and watering history")]
    [Trait("Category", "Unit")]
    public async Task UpdatePlantWhenCommandIsValidUpdatesDetailsAndWateringHistory()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plant = Plant.Create(userId, PlantName.From("Basil"), PlantDescription.From(null), null, null, null);
        var lastWateredOn = new DateOnly(2026, 5, 22);
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        var replaceCalls = 0;
        var saveCalls = 0;

        gardenMock.Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken)).ReturnsAsync(plant);
        gardenMock
            .Setup(repo => repo.ReplaceWateringHistoryAsync(plant.Id, lastWateredOn, cancellationToken))
            .Callback(() => replaceCalls++)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(cancellationToken))
            .Callback(() => saveCalls++)
            .Returns(Task.CompletedTask);

        var useCase = new UpdatePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            userId,
            plant.Id.Value,
            new PlantCommand("Mint", "Fresh", null, null, lastWateredOn, null),
            cancellationToken);

        // Assert
        result.Status.Should().Be(UpdatePlantStatus.Updated);
        plant.Name.Value.Should().Be("Mint");
        replaceCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Delete plant removes existing plant")]
    [Trait("Category", "Unit")]
    public async Task DeletePlantWhenPlantExistsRemovesPlant()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plant = Plant.Create(userId, PlantName.From("Basil"), PlantDescription.From(null), null, null, null);
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        var removeCalls = 0;
        var saveCalls = 0;

        gardenMock.Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken)).ReturnsAsync(plant);
        gardenMock.Setup(repo => repo.RemovePlant(plant)).Callback(() => removeCalls++);
        unitOfWorkMock.Setup(work => work.SaveChangesAsync(cancellationToken)).Callback(() => saveCalls++).Returns(Task.CompletedTask);

        var useCase = new DeletePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plant.Id.Value, cancellationToken);

        // Assert
        result.Should().BeTrue();
        removeCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Delete plant returns false for missing plant")]
    [Trait("Category", "Unit")]
    public async Task DeletePlantWhenPlantIsMissingReturnsFalse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        gardenMock.Setup(repo => repo.FindPlantAsync(userId, plantId, cancellationToken)).ReturnsAsync((Plant?)null);
        var useCase = new DeletePlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId.Value, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Water plant adds watering event")]
    [Trait("Category", "Unit")]
    public async Task WaterPlantWhenPlantExistsAddsWateringEvent()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plant = Plant.Create(userId, PlantName.From("Basil"), PlantDescription.From(null), null, null, null);
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        var wateringCalls = 0;
        var saveCalls = 0;

        gardenMock.Setup(repo => repo.FindPlantAsync(userId, plant.Id, cancellationToken)).ReturnsAsync(plant);
        gardenMock
            .Setup(repo => repo.AddWateringEventAsync(It.Is<WateringEvent>(watering => watering.PlantId == plant.Id), cancellationToken))
            .Callback(() => wateringCalls++)
            .Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(work => work.SaveChangesAsync(cancellationToken)).Callback(() => saveCalls++).Returns(Task.CompletedTask);
        var useCase = new WaterPlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plant.Id.Value, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        wateringCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Water plant returns null for missing plant")]
    [Trait("Category", "Unit")]
    public async Task WaterPlantWhenPlantIsMissingReturnsNull()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var plantId = PlantId.New();
        var gardenMock = new Mock<IGardenRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(garden: gardenMock.Object);
        gardenMock.Setup(repo => repo.FindPlantAsync(userId, plantId, cancellationToken)).ReturnsAsync((Plant?)null);
        var useCase = new WaterPlantUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId, plantId.Value, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "List invites maps projections")]
    [Trait("Category", "Unit")]
    public async Task ListInvitesWhenInvitesExistMapsInvites()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(invites: invitesMock.Object);
        var listCalls = 0;

        invitesMock
            .Setup(repo => repo.ListAsync(cancellationToken))
            .Callback(() => listCalls++)
            .ReturnsAsync([new AdminInviteProjection(
                InviteId.New(),
                "code",
                DateTimeOffset.UtcNow,
                "admin",
                null,
                null,
                false)]);

        var useCase = new ListInvitesUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(cancellationToken);

        // Assert
        result.Should().ContainSingle();
        result[0].Code.Should().Be("code");
        listCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Admin invite use cases create revoke and delete invites")]
    [Trait("Category", "Unit")]
    public async Task AdminInviteUseCasesWhenInvitesAreManagedUpdateRepository()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")], "Test"));
        var invites = new List<InviteLink>();
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(invites: invitesMock.Object);
        var addCalls = 0;
        var revokeFindCalls = 0;
        var removeCalls = 0;
        var saveCalls = 0;

        invitesMock
            .Setup(repo => repo.AddAsync(It.IsAny<InviteLink>(), cancellationToken))
            .Callback<InviteLink, CancellationToken>((invite, _) =>
            {
                addCalls++;
                invites.Add(invite);
            })
            .Returns(Task.CompletedTask);
        invitesMock
            .Setup(repo => repo.FindByIdAsync(It.IsAny<InviteId>(), cancellationToken))
            .Callback(() => revokeFindCalls++)
            .ReturnsAsync((InviteId id, CancellationToken _) => invites.Single(invite => invite.Id == id));
        invitesMock
            .Setup(repo => repo.Remove(It.IsAny<InviteLink>()))
            .Callback<InviteLink>(invite =>
            {
                removeCalls++;
                invites.Remove(invite);
            });
        unitOfWorkMock.Setup(work => work.SaveChangesAsync(cancellationToken)).Callback(() => saveCalls++).Returns(Task.CompletedTask);

        var create = new CreateInviteUseCase(unitOfWorkMock.Object);

        // Act
        var created = await create.ExecuteAsync(principal, cancellationToken);
        var revoked = await new RevokeInviteUseCase(unitOfWorkMock.Object).ExecuteAsync(created.Id.Value, cancellationToken);
        var deleteResult = await new DeleteInviteUseCase(unitOfWorkMock.Object).ExecuteAsync(created.Id.Value, cancellationToken);

        // Assert
        created.Code.Should().NotBeNullOrWhiteSpace();
        revoked.Should().BeTrue();
        deleteResult.Status.Should().Be(DeleteInviteStatus.Deleted);
        addCalls.Should().Be(1);
        revokeFindCalls.Should().Be(2);
        removeCalls.Should().Be(1);
        saveCalls.Should().Be(3);
        invites.Should().BeEmpty();
    }

    [Fact(DisplayName = "Revoke invite returns false for missing invite")]
    [Trait("Category", "Unit")]
    public async Task RevokeInviteWhenInviteIsMissingReturnsFalse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var inviteId = InviteId.New();
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(invites: invitesMock.Object);
        invitesMock.Setup(repo => repo.FindByIdAsync(inviteId, cancellationToken)).ReturnsAsync((InviteLink?)null);
        var useCase = new RevokeInviteUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(inviteId.Value, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Delete invite returns invalid when invite is used")]
    [Trait("Category", "Unit")]
    public async Task DeleteInviteWhenInviteIsUsedReturnsInvalid()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var invite = InviteLink.Create(InviteCode.From("code"), AdminName.From("admin"));
        invite.MarkUsed(UserId.New());
        var invitesMock = new Mock<IInviteRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(invites: invitesMock.Object);
        invitesMock.Setup(repo => repo.FindByIdAsync(invite.Id, cancellationToken)).ReturnsAsync(invite);
        var useCase = new DeleteInviteUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(invite.Id.Value, cancellationToken);

        // Assert
        result.Status.Should().Be(DeleteInviteStatus.Invalid);
        result.Error.Should().Be("Used invite cannot be deleted.");
    }

    [Fact(DisplayName = "List users maps projections")]
    [Trait("Category", "Unit")]
    public async Task ListUsersWhenUsersExistMapsUsers()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(usersMock.Object);
        var listCalls = 0;

        usersMock
            .Setup(repo => repo.ListAdminUsersAsync(cancellationToken))
            .Callback(() => listCalls++)
            .ReturnsAsync([new AdminUserProjection(
                UserId.New(),
                "User",
                "USER@EXAMPLE.COM",
                false,
                "en",
                DateTimeOffset.UtcNow,
                3)]);

        var useCase = new ListUsersUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(cancellationToken);

        // Assert
        result.Should().ContainSingle();
        result[0].Plants.Should().Be(3);
        listCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Block user updates blocked state")]
    [Trait("Category", "Unit")]
    public async Task BlockUserWhenUserExistsUpdatesBlockedState()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var user = CreateUser("user@example.com");
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(usersMock.Object);
        var findCalls = 0;
        var saveCalls = 0;

        usersMock
            .Setup(repo => repo.FindByIdAsync(user.Id, cancellationToken))
            .Callback(() => findCalls++)
            .ReturnsAsync(user);
        unitOfWorkMock.Setup(work => work.SaveChangesAsync(cancellationToken)).Callback(() => saveCalls++).Returns(Task.CompletedTask);

        // Act
        var blocked = await new BlockUserUseCase(unitOfWorkMock.Object).ExecuteAsync(user.Id.Value, true, cancellationToken);
        var unblocked = await new BlockUserUseCase(unitOfWorkMock.Object).ExecuteAsync(user.Id.Value, false, cancellationToken);

        // Assert
        blocked.Should().BeTrue();
        unblocked.Should().BeTrue();
        user.IsBlocked.Should().BeFalse();
        findCalls.Should().Be(2);
        saveCalls.Should().Be(2);
    }

    [Fact(DisplayName = "Block user returns false for missing user")]
    [Trait("Category", "Unit")]
    public async Task BlockUserWhenUserIsMissingReturnsFalse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(usersMock.Object);
        usersMock.Setup(repo => repo.FindByIdAsync(userId, cancellationToken)).ReturnsAsync((AppUser?)null);

        // Act
        var result = await new BlockUserUseCase(unitOfWorkMock.Object).ExecuteAsync(userId.Value, true, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Delete user removes existing user")]
    [Trait("Category", "Unit")]
    public async Task DeleteUserWhenUserExistsRemovesUser()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var user = CreateUser("user@example.com");
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(usersMock.Object);
        var removeCalls = 0;
        var saveCalls = 0;

        usersMock.Setup(repo => repo.FindByIdAsync(user.Id, cancellationToken)).ReturnsAsync(user);
        usersMock.Setup(repo => repo.Remove(user)).Callback(() => removeCalls++);
        unitOfWorkMock.Setup(work => work.SaveChangesAsync(cancellationToken)).Callback(() => saveCalls++).Returns(Task.CompletedTask);
        var useCase = new DeleteUserUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(user.Id.Value, cancellationToken);

        // Assert
        result.Should().BeTrue();
        removeCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    [Fact(DisplayName = "Delete user returns false for missing user")]
    [Trait("Category", "Unit")]
    public async Task DeleteUserWhenUserIsMissingReturnsFalse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var userId = UserId.New();
        var usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWorkMock = CreateUnitOfWork(usersMock.Object);
        usersMock.Setup(repo => repo.FindByIdAsync(userId, cancellationToken)).ReturnsAsync((AppUser?)null);
        var useCase = new DeleteUserUseCase(unitOfWorkMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId.Value, cancellationToken);

        // Assert
        result.Should().BeFalse();
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

    private static Mock<IUnitOfWork> CreateUnitOfWork(
        IUserRepository? users = null,
        IInviteRepository? invites = null,
        IGardenRepository? garden = null)
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        if (users is not null)
        {
            unitOfWorkMock.SetupGet(work => work.Users).Returns(users);
        }

        if (invites is not null)
        {
            unitOfWorkMock.SetupGet(work => work.Invites).Returns(invites);
        }

        if (garden is not null)
        {
            unitOfWorkMock.SetupGet(work => work.Garden).Returns(garden);
        }

        return unitOfWorkMock;
    }
}
