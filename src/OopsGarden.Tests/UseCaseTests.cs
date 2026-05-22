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

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public FakeUnitOfWork()
        {
            Users = new FakeUserRepository();
            Invites = new FakeInviteRepository();
            Garden = new FakeGardenRepository();
        }

        public FakeUserRepository Users { get; }

        public FakeInviteRepository Invites { get; }

        public FakeGardenRepository Garden { get; }

        IUserRepository IUnitOfWork.Users => Users;

        IInviteRepository IUnitOfWork.Invites => Invites;

        IGardenRepository IUnitOfWork.Garden => Garden;

        public int SaveChangesCalls { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<AppUser> Users { get; } = [];

        public Task<AppUser?> FindByEmailAsync(UserEmail email, CancellationToken cancellationToken) =>
            Task.FromResult(Users.SingleOrDefault(user => user.Email == email));

        public Task<AppUser?> FindByIdAsync(UserId id, CancellationToken cancellationToken) =>
            Task.FromResult(Users.SingleOrDefault(user => user.Id == id));

        public Task<bool> ExistsByEmailAsync(UserEmail email, CancellationToken cancellationToken) =>
            Task.FromResult(Users.Exists(user => user.Email == email));

        public Task AddAsync(AppUser user, CancellationToken cancellationToken)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AdminUserProjection>> ListAdminUsersAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<AdminUserProjection>>([.. Users.Select(user => new AdminUserProjection(
                user.Id,
                user.DisplayName.Value,
                user.Email.Value,
                user.IsBlocked,
                user.Language.Value,
                user.CreatedAt,
                user.Plants.Count))]);

        public void Remove(AppUser user) => Users.Remove(user);
    }

    private sealed class FakeInviteRepository : IInviteRepository
    {
        public List<InviteLink> Invites { get; } = [];

        public Task<InviteLink?> FindByCodeAsync(InviteCode code, CancellationToken cancellationToken) =>
            Task.FromResult(Invites.SingleOrDefault(invite => invite.Code == code));

        public Task<InviteLink?> FindByIdAsync(InviteId id, CancellationToken cancellationToken) =>
            Task.FromResult(Invites.SingleOrDefault(invite => invite.Id == id));

        public Task<IReadOnlyList<AdminInviteProjection>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<AdminInviteProjection>>([.. Invites.Select(invite => new AdminInviteProjection(
                invite.Id,
                invite.Code.Value,
                invite.CreatedAt,
                invite.CreatedBy.Value,
                invite.UsedAt,
                invite.UsedByUserId,
                invite.IsRevoked))]);

        public Task AddAsync(InviteLink invite, CancellationToken cancellationToken)
        {
            Invites.Add(invite);
            return Task.CompletedTask;
        }

        public void Remove(InviteLink invite) => Invites.Remove(invite);
    }

    private sealed class FakeGardenRepository : IGardenRepository
    {
        public List<Location> Locations { get; } = [];

        public List<Plant> Plants { get; } = [];

        public List<WateringEvent> WateringEvents { get; } = [];

        public Task<PublicGardenProjection?> GetPublicGardenAsync(UserId userId, CancellationToken cancellationToken) =>
            Task.FromResult<PublicGardenProjection?>(new PublicGardenProjection(userId, "User", null, []));

        public Task<IReadOnlyList<GardenPlantProjection>> ListPlantsAsync(UserId userId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<GardenPlantProjection>>([.. Plants
                .Where(plant => plant.UserId == userId)
                .Select(plant => new GardenPlantProjection(
                    plant.Id,
                    plant.Name.Value,
                    plant.Description.Value,
                    plant.PhotoDataUrl?.Value,
                    plant.PlantedOn,
                    null,
                    WateringEvents.LastOrDefault(watering => watering.PlantId == plant.Id)?.WateredAt))]);

        public Task<IReadOnlyList<GardenLocationProjection>> ListLocationsAsync(UserId userId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<GardenLocationProjection>>([.. Locations
                .Where(location => location.UserId == userId)
                .Select(location => new GardenLocationProjection(location.Id, location.Name.Value, location.Plants.Count))]);

        public Task<Plant?> FindPlantAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken) =>
            Task.FromResult(Plants.SingleOrDefault(plant => plant.UserId == userId && plant.Id == plantId));

        public Task<Location?> FindLocationAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken) =>
            Task.FromResult(Locations.SingleOrDefault(location => location.UserId == userId && location.Id == locationId));

        public Task<bool> LocationExistsAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken) =>
            Task.FromResult(Locations.Exists(location => location.UserId == userId && location.Id == locationId));

        public Task AddPlantAsync(Plant plant, CancellationToken cancellationToken)
        {
            Plants.Add(plant);
            return Task.CompletedTask;
        }

        public Task AddLocationAsync(Location location, CancellationToken cancellationToken)
        {
            Locations.Add(location);
            return Task.CompletedTask;
        }

        public Task AddWateringEventAsync(WateringEvent watering, CancellationToken cancellationToken)
        {
            WateringEvents.Add(watering);
            return Task.CompletedTask;
        }

        public void RemovePlant(Plant plant) => Plants.Remove(plant);

        public void RemoveLocation(Location location) => Locations.Remove(location);

        public Task ClearPlantLocationAsync(UserId userId, LocationId locationId, CancellationToken cancellationToken)
        {
            foreach (var plant in Plants.Where(plant => plant.UserId == userId && plant.LocationId == locationId))
            {
                plant.UpdateDetails(plant.Name, plant.Description, null, plant.PlantedOn, plant.PhotoDataUrl?.Value);
            }

            return Task.CompletedTask;
        }

        public Task ReplaceWateringHistoryAsync(PlantId plantId, DateOnly? lastWateredOn, CancellationToken cancellationToken)
        {
            WateringEvents.RemoveAll(watering => watering.PlantId == plantId);
            if (lastWateredOn.HasValue)
            {
                WateringEvents.Add(WateringEvent.Restore(
                    WateringEventId.New(),
                    plantId,
                    new DateTimeOffset(lastWateredOn.Value.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero)));
            }

            return Task.CompletedTask;
        }
    }
}
