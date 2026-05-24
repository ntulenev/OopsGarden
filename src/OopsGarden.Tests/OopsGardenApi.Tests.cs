using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

using Transport;

namespace OopsGarden.Tests;

public sealed class OopsGardenApiTests
{
    [Fact(DisplayName = "GET /api/me returns anonymous session")]
    [Trait("Category", "Integration")]
    public async Task GetMeWhenAnonymousReturnsAnonymousSession()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/me");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.GetProperty("authenticated").GetBoolean().Should().BeFalse();
    }

    [Fact(DisplayName = "Admin can create invite and user can register")]
    [Trait("Category", "Integration")]
    public async Task RegisterWhenInviteIsCreatedByAdminSignsInUser()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/auth/admin-login", new LoginRequest("admin", "secret"));
        var inviteResponse = await client.PostAsync("/api/admin/invites", null);
        var invite = await inviteResponse.Content.ReadFromJsonAsync<JsonElement>();
        await client.PostAsync("/api/auth/logout", null);

        // Act
        var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(
                invite.GetProperty("code").GetString()!,
                "User",
                "user@example.com",
                "password",
                "en"));
        var meResponse = await client.GetAsync("/api/me");
        var me = await meResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        inviteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        me.GetProperty("authenticated").GetBoolean().Should().BeTrue();
        me.GetProperty("name").GetString().Should().Be("User");
        me.GetProperty("role").GetString().Should().Be("User");
    }

    [Fact(DisplayName = "User can log out and log back in with password")]
    [Trait("Category", "Integration")]
    public async Task LoginWhenRegisteredUserProvidesPasswordSignsInUser()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await RegisterUserAsync(client, "user@example.com");
        await client.PostAsync("/api/auth/logout", null);

        // Act
        var failedLoginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("user@example.com", "wrong-password"));
        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("user@example.com", "password"));
        var login = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var meResponse = await client.GetAsync("/api/me");
        var me = await meResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        failedLoginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        login.GetProperty("email").GetString().Should().Be("USER@EXAMPLE.COM");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        me.GetProperty("authenticated").GetBoolean().Should().BeTrue();
        me.GetProperty("name").GetString().Should().Be("User");
    }

    [Fact(DisplayName = "Revoked invite cannot be used for registration")]
    [Trait("Category", "Integration")]
    public async Task RegisterWhenInviteWasRevokedReturnsBadRequest()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/admin-login", new LoginRequest("admin", "secret"));
        var (inviteId, inviteCode) = await CreateInviteAsync(client);

        // Act
        var revokeResponse = await client.PostAsync($"/api/admin/invites/{inviteId}/revoke", null);
        await client.PostAsync("/api/auth/logout", null);
        var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(inviteCode, "User", "user@example.com", "password", "en"));
        var payload = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        revokeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        payload.GetProperty("error").GetString().Should().Be("Invalid invite.");
    }

    [Fact(DisplayName = "Garden endpoints require authenticated user")]
    [Trait("Category", "Integration")]
    public async Task GardenEndpointsWhenAnonymousReturnUnauthorized()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync("/api/garden/plants");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "User can manage garden and expose public garden")]
    [Trait("Category", "Integration")]
    public async Task GardenWorkflowWhenUserIsAuthenticatedManagesGarden()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await RegisterUserAsync(client);

        // Act
        var settingsResponse = await client.PostAsJsonAsync(
            "/api/auth/settings",
            new SettingsRequest("Gardener", "ru", "data:image/png;base64,abc", true));
        var settings = await settingsResponse.Content.ReadFromJsonAsync<JsonElement>();
        var userId = settings.GetProperty("id").GetGuid();

        var locationResponse = await client.PostAsJsonAsync("/api/garden/locations", new LocationRequest("Kitchen"));
        var location = await locationResponse.Content.ReadFromJsonAsync<JsonElement>();
        var locationId = location.GetProperty("id").GetGuid();

        var createPlantResponse = await client.PostAsJsonAsync(
            "/api/garden/plants",
            new PlantRequest(
                "Basil",
                "Green",
                locationId,
                new DateOnly(2026, 5, 22),
                null,
                "data:image/png;base64,plant"));
        var createdPlant = await createPlantResponse.Content.ReadFromJsonAsync<JsonElement>();
        var plantId = createdPlant.GetProperty("id").GetGuid();

        var waterResponse = await client.PostAsync($"/api/garden/plants/{plantId}/water", null);
        var plantsResponse = await client.GetAsync("/api/garden/plants");
        var plants = await plantsResponse.Content.ReadFromJsonAsync<JsonElement>();
        var publicResponse = await client.GetAsync($"/api/public/gardens/{userId}");
        var publicGarden = await publicResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        settingsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        locationResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        createPlantResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        waterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        plantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        plants.EnumerateArray().Should().ContainSingle();
        publicGarden.GetProperty("name").GetString().Should().Be("Gardener");
        publicGarden.GetProperty("plants").EnumerateArray().Should().ContainSingle();
    }

    [Fact(DisplayName = "User can update and delete garden entries")]
    [Trait("Category", "Integration")]
    public async Task GardenWorkflowWhenUserUpdatesAndDeletesEntriesReflectsChanges()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await RegisterUserAsync(client);
        var locationId = await CreateLocationAsync(client, "Kitchen");
        var plantId = await CreatePlantAsync(client, locationId, "Basil");

        // Act
        var renameResponse = await client.PutAsJsonAsync(
            $"/api/garden/locations/{locationId}",
            new LocationRequest("Window"));
        var updatePlantResponse = await client.PutAsJsonAsync(
            $"/api/garden/plants/{plantId}",
            new PlantRequest(
                "Mint",
                "Fresh",
                locationId,
                new DateOnly(2026, 5, 1),
                new DateOnly(2026, 5, 20),
                "data:image/png;base64,mint"));
        var locationsResponse = await client.GetAsync("/api/garden/locations");
        var locations = await locationsResponse.Content.ReadFromJsonAsync<JsonElement>();
        var plantsResponse = await client.GetAsync("/api/garden/summary");
        var plants = await plantsResponse.Content.ReadFromJsonAsync<JsonElement>();
        var deleteLocationResponse = await client.DeleteAsync($"/api/garden/locations/{locationId}");
        var plantsAfterLocationDeleteResponse = await client.GetAsync("/api/garden/plants");
        var plantsAfterLocationDelete = await plantsAfterLocationDeleteResponse.Content.ReadFromJsonAsync<JsonElement>();
        var deletePlantResponse = await client.DeleteAsync($"/api/garden/plants/{plantId}");
        var plantsAfterPlantDeleteResponse = await client.GetAsync("/api/garden/plants");
        var plantsAfterPlantDelete = await plantsAfterPlantDeleteResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        renameResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updatePlantResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        locationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        locations.EnumerateArray().Should().ContainSingle(location =>
            location.GetProperty("name").GetString() == "Window"
            && location.GetProperty("plants").GetInt32() == 1);
        plantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        plants.EnumerateArray().Should().ContainSingle(plant =>
            plant.GetProperty("name").GetString() == "Mint"
            && plant.GetProperty("location").GetProperty("name").GetString() == "Window"
            && plant.GetProperty("lastWateredAt").ValueKind == JsonValueKind.String);
        deleteLocationResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        plantsAfterLocationDeleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        plantsAfterLocationDelete.EnumerateArray().Should().ContainSingle(plant =>
            plant.GetProperty("location").ValueKind == JsonValueKind.Null);
        deletePlantResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        plantsAfterPlantDeleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        plantsAfterPlantDelete.EnumerateArray().Should().BeEmpty();
    }

    [Fact(DisplayName = "User can keep paged notes for a plant")]
    [Trait("Category", "Integration")]
    public async Task PlantNotesWorkflowWhenUserManagesNotesPaginatesAndDeletes()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await RegisterUserAsync(client);
        var locationId = await CreateLocationAsync(client, "Kitchen");
        var plantId = await CreatePlantAsync(client, locationId, "Basil");
        var createdNoteIds = new List<Guid>();

        // Act
        for (var i = 1; i <= 7; i++)
        {
            var createResponse = await client.PostAsJsonAsync(
                $"/api/garden/plants/{plantId}/notes",
                new PlantNoteRequest($"Observation {i}"));
            var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            createdNoteIds.Add(created.GetProperty("id").GetGuid());
        }

        var firstPageResponse = await client.GetAsync($"/api/garden/plants/{plantId}/notes?page=1&pageSize=5");
        var firstPage = await firstPageResponse.Content.ReadFromJsonAsync<JsonElement>();
        var secondPageResponse = await client.GetAsync($"/api/garden/plants/{plantId}/notes?page=2&pageSize=5");
        var secondPage = await secondPageResponse.Content.ReadFromJsonAsync<JsonElement>();
        var deleteResponse = await client.DeleteAsync($"/api/garden/plants/{plantId}/notes/{createdNoteIds[0]}");
        var deleteMissingResponse = await client.DeleteAsync($"/api/garden/plants/{plantId}/notes/{Guid.NewGuid()}");
        var afterDeleteResponse = await client.GetAsync($"/api/garden/plants/{plantId}/notes?page=1&pageSize=10");
        var afterDelete = await afterDeleteResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        firstPageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstPage.GetProperty("items").EnumerateArray().Should().HaveCount(5);
        firstPage.GetProperty("total").GetInt32().Should().Be(7);
        firstPage.GetProperty("hasNext").GetBoolean().Should().BeTrue();
        secondPageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondPage.GetProperty("items").EnumerateArray().Should().HaveCount(2);
        secondPage.GetProperty("hasPrevious").GetBoolean().Should().BeTrue();
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        deleteMissingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        afterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        afterDelete.GetProperty("total").GetInt32().Should().Be(6);
        afterDelete.GetProperty("items").EnumerateArray().Should().NotContain(note =>
            note.GetProperty("id").GetGuid() == createdNoteIds[0]);
    }

    [Fact(DisplayName = "Garden mutations return not found for missing resources")]
    [Trait("Category", "Integration")]
    public async Task GardenMutationsWhenResourcesAreMissingReturnNotFound()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await RegisterUserAsync(client);
        var missingId = Guid.NewGuid();

        // Act
        var renameLocationResponse = await client.PutAsJsonAsync(
            $"/api/garden/locations/{missingId}",
            new LocationRequest("Window"));
        var deleteLocationResponse = await client.DeleteAsync($"/api/garden/locations/{missingId}");
        var waterPlantResponse = await client.PostAsync($"/api/garden/plants/{missingId}/water", null);
        var updatePlantResponse = await client.PutAsJsonAsync(
            $"/api/garden/plants/{missingId}",
            new PlantRequest("Mint", "Fresh", null, null, null, null));
        var deletePlantResponse = await client.DeleteAsync($"/api/garden/plants/{missingId}");

        // Assert
        renameLocationResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        deleteLocationResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        waterPlantResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        updatePlantResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        deletePlantResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Public garden is hidden until user enables sharing")]
    [Trait("Category", "Integration")]
    public async Task GetPublicGardenWhenSharingIsDisabledReturnsNotFound()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var userId = await RegisterUserAsync(client);

        // Act
        var publicResponse = await client.GetAsync($"/api/public/gardens/{userId}");

        // Assert
        publicResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Public plant history is available only for shared garden plant")]
    [Trait("Category", "Integration")]
    public async Task PublicPlantHistoryWhenGardenIsSharedReturnsPlantHistory()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var userId = await RegisterUserAsync(client);
        var locationId = await CreateLocationAsync(client, "Kitchen");
        var plantId = await CreatePlantAsync(client, locationId, "Basil");

        var privateResponse = await client.GetAsync($"/api/public/gardens/{userId}/plants/{plantId}/history");

        await client.PostAsJsonAsync(
            "/api/auth/settings",
            new SettingsRequest("Gardener", "en", null, true));
        var waterResponse = await client.PostAsync($"/api/garden/plants/{plantId}/water", null);
        var noteResponse = await client.PostAsJsonAsync(
            $"/api/garden/plants/{plantId}/notes",
            new PlantNoteRequest("Sprouted"));

        // Act
        var publicResponse = await client.GetAsync($"/api/public/gardens/{userId}/plants/{plantId}/history");
        var missingPlantResponse = await client.GetAsync($"/api/public/gardens/{userId}/plants/{Guid.NewGuid()}/history");
        var history = await publicResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        privateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        waterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        noteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        missingPlantResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        history.EnumerateArray().Should().Contain(item =>
            item.GetProperty("type").GetString() == "watering"
            && item.GetProperty("text").ValueKind == JsonValueKind.Null);
        history.EnumerateArray().Should().Contain(item =>
            item.GetProperty("type").GetString() == "note"
            && item.GetProperty("text").GetString() == "Sprouted");
    }

    [Fact(DisplayName = "Plant creation rejects missing location")]
    [Trait("Category", "Integration")]
    public async Task CreatePlantWhenLocationIsMissingReturnsBadRequest()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await RegisterUserAsync(client);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/garden/plants",
            new PlantRequest("Basil", "Green", Guid.NewGuid(), null, null, null));
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        payload.GetProperty("error").GetString().Should().Be("Invalid location.");
    }

    [Fact(DisplayName = "Admin can list block and delete users")]
    [Trait("Category", "Integration")]
    public async Task AdminWorkflowWhenAdminIsAuthenticatedManagesUsers()
    {
        // Arrange
        using var factory = CreateFactory();
        using var userClient = factory.CreateClient();
        var userId = await RegisterUserAsync(userClient);

        using var adminClient = factory.CreateClient();
        await adminClient.PostAsJsonAsync("/api/auth/admin-login", new LoginRequest("admin", "secret"));

        // Act
        var listResponse = await adminClient.GetAsync("/api/admin/users");
        var list = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        var blockResponse = await adminClient.PostAsJsonAsync(
            $"/api/admin/users/{userId}/block",
            new BlockUserRequest(true));
        var deleteResponse = await adminClient.DeleteAsync($"/api/admin/users/{userId}");

        // Assert
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        list.EnumerateArray().Should().ContainSingle();
        blockResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Admin can list revoke and delete invites")]
    [Trait("Category", "Integration")]
    public async Task AdminInviteWorkflowWhenAdminIsAuthenticatedManagesInvites()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/admin-login", new LoginRequest("admin", "secret"));
        var (inviteToRevokeId, _) = await CreateInviteAsync(client);
        var (inviteToDeleteId, _) = await CreateInviteAsync(client);
        var missingId = Guid.NewGuid();

        // Act
        var listResponse = await client.GetAsync("/api/admin/invites");
        var list = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        var revokeResponse = await client.PostAsync($"/api/admin/invites/{inviteToRevokeId}/revoke", null);
        var deleteResponse = await client.DeleteAsync($"/api/admin/invites/{inviteToDeleteId}");
        var revokeMissingResponse = await client.PostAsync($"/api/admin/invites/{missingId}/revoke", null);
        var deleteMissingResponse = await client.DeleteAsync($"/api/admin/invites/{missingId}");

        // Assert
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        list.EnumerateArray().Should().HaveCount(2);
        revokeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        revokeMissingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        deleteMissingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Used invite cannot be deleted")]
    [Trait("Category", "Integration")]
    public async Task DeleteInviteWhenInviteWasUsedReturnsBadRequest()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/admin-login", new LoginRequest("admin", "secret"));
        var (inviteId, inviteCode) = await CreateInviteAsync(client);
        await client.PostAsync("/api/auth/logout", null);
        await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(inviteCode, "User", "user@example.com", "password", "en"));
        await client.PostAsync("/api/auth/logout", null);
        await client.PostAsJsonAsync("/api/auth/admin-login", new LoginRequest("admin", "secret"));

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/admin/invites/{inviteId}");
        var payload = await deleteResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        payload.GetProperty("error").GetString().Should().Be("Used invite cannot be deleted.");
    }

    [Fact(DisplayName = "Role protected endpoints reject wrong role")]
    [Trait("Category", "Integration")]
    public async Task RoleProtectedEndpointsWhenUserHasWrongRoleReturnForbidden()
    {
        // Arrange
        using var factory = CreateFactory();
        using var userClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        await RegisterUserAsync(userClient);

        using var adminClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        await adminClient.PostAsJsonAsync("/api/auth/admin-login", new LoginRequest("admin", "secret"));

        // Act
        var adminEndpointAsUserResponse = await userClient.GetAsync("/api/admin/users");
        var gardenEndpointAsAdminResponse = await adminClient.GetAsync("/api/garden/plants");

        // Assert
        adminEndpointAsUserResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        gardenEndpointAsAdminResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static OopsGardenAppFactory CreateFactory() => new();

    private static async Task<(Guid Id, string Code)> CreateInviteAsync(HttpClient client)
    {
        var inviteResponse = await client.PostAsync("/api/admin/invites", null);
        var invite = await inviteResponse.Content.ReadFromJsonAsync<JsonElement>();
        inviteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return (invite.GetProperty("id").GetGuid(), invite.GetProperty("code").GetString()!);
    }

    private static async Task<Guid> CreateLocationAsync(HttpClient client, string name)
    {
        var locationResponse = await client.PostAsJsonAsync("/api/garden/locations", new LocationRequest(name));
        var location = await locationResponse.Content.ReadFromJsonAsync<JsonElement>();
        locationResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return location.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreatePlantAsync(HttpClient client, Guid locationId, string name)
    {
        var createPlantResponse = await client.PostAsJsonAsync(
            "/api/garden/plants",
            new PlantRequest(name, "Green", locationId, null, null, null));
        var createdPlant = await createPlantResponse.Content.ReadFromJsonAsync<JsonElement>();
        createPlantResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return createdPlant.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> RegisterUserAsync(HttpClient client, string? email = null)
    {
        await client.PostAsJsonAsync("/api/auth/admin-login", new LoginRequest("admin", "secret"));
        var inviteResponse = await client.PostAsync("/api/admin/invites", null);
        var invite = await inviteResponse.Content.ReadFromJsonAsync<JsonElement>();
        await client.PostAsync("/api/auth/logout", null);

        var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(
                invite.GetProperty("code").GetString()!,
                "User",
                email ?? $"user-{Guid.NewGuid():N}@example.com",
                "password",
                "en"));
        var registered = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
        return registered.GetProperty("id").GetGuid();
    }
}
