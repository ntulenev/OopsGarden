using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Storage;

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

    private static OopsGardenAppFactory CreateFactory() => new();

    private static async Task<Guid> RegisterUserAsync(HttpClient client)
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
                $"user-{Guid.NewGuid():N}@example.com",
                "password",
                "en"));
        var registered = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
        return registered.GetProperty("id").GetGuid();
    }

    private sealed class OopsGardenAppFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["Admins:Users:0:UserName"] = "admin",
                    ["Admins:Users:0:Password"] = "secret",
                    ["ConnectionStrings:OopsGarden"] = "Server=(localdb)\\mssqllocaldb;Database=OopsGardenTests;Trusted_Connection=True"
                };

                config.AddInMemoryCollection(settings);
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<GardenDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<GardenDbContext>>();
                services.AddDbContext<GardenDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }
    }
}
