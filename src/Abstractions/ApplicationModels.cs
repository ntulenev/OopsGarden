using Models;

namespace Abstractions;

/// <summary>
/// Represents credentials used for user or admin login.
/// </summary>
/// <param name="Email">The login email address.</param>
/// <param name="Password">The login password.</param>
public sealed record LoginCommand(string Email, string Password);

/// <summary>
/// Represents registration input.
/// </summary>
/// <param name="InviteCode">The registration invite code.</param>
/// <param name="DisplayName">The requested display name.</param>
/// <param name="Email">The registration email address.</param>
/// <param name="Password">The registration password.</param>
/// <param name="Language">The preferred UI language code.</param>
public sealed record RegisterCommand(string InviteCode, string DisplayName, string Email, string Password, string Language);

/// <summary>
/// Represents profile settings input.
/// </summary>
/// <param name="DisplayName">The new display name.</param>
/// <param name="Language">The preferred UI language code.</param>
/// <param name="AvatarData">The optional avatar data URL.</param>
/// <param name="IsGardenPublic">A value indicating whether the garden is public.</param>
public sealed record SettingsCommand(string DisplayName, string Language, string? AvatarData, bool IsGardenPublic);

/// <summary>
/// Represents editable plant input.
/// </summary>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="LocationId">The optional garden location id.</param>
/// <param name="PlantedOn">The optional planting date.</param>
/// <param name="LastWateredOn">The optional last watering date.</param>
/// <param name="PhotoData">The optional plant photo data URL.</param>
public sealed record PlantCommand(
    string Name,
    string Description,
    Guid? LocationId,
    DateOnly? PlantedOn,
    DateOnly? LastWateredOn,
    string? PhotoData);

/// <summary>
/// Represents editable location input.
/// </summary>
/// <param name="Name">The location name.</param>
public sealed record LocationCommand(string Name);

/// <summary>
/// Represents the authenticated user application model.
/// </summary>
/// <param name="Id">The authenticated user id.</param>
/// <param name="DisplayName">The authenticated user's display name.</param>
/// <param name="Email">The authenticated user's email address.</param>
/// <param name="Language">The authenticated user's preferred UI language.</param>
/// <param name="AvatarData">The authenticated user's optional avatar data URL.</param>
/// <param name="IsGardenPublic">A value indicating whether the garden is public.</param>
public sealed record AuthenticatedUser(
    UserId Id,
    string DisplayName,
    string Email,
    string Language,
    string? AvatarData,
    bool IsGardenPublic);

/// <summary>
/// Represents current-session application data.
/// </summary>
/// <param name="Authenticated">A value indicating whether the current principal is authenticated.</param>
/// <param name="Id">The optional current user id.</param>
/// <param name="Name">The optional display or admin name.</param>
/// <param name="Role">The optional current role.</param>
/// <param name="Language">The optional preferred UI language.</param>
/// <param name="AvatarData">The optional avatar data URL.</param>
/// <param name="IsGardenPublic">A value indicating whether the garden is public.</param>
public sealed record CurrentUser(
    bool Authenticated,
    UserId? Id = null,
    string? Name = null,
    string? Role = null,
    string? Language = null,
    string? AvatarData = null,
    bool IsGardenPublic = false);

/// <summary>
/// Represents admin login output.
/// </summary>
/// <param name="Name">The administrator name.</param>
/// <param name="Role">The authenticated administrator role.</param>
public sealed record AdminLogin(string Name, string Role);

/// <summary>
/// Represents a public garden application model.
/// </summary>
/// <param name="Id">The garden owner id.</param>
/// <param name="Name">The garden owner display name.</param>
/// <param name="AvatarData">The optional garden owner avatar data URL.</param>
/// <param name="Plants">The public plants in the garden.</param>
public sealed record PublicGarden(UserId Id, string Name, string? AvatarData, IReadOnlyList<PublicGardenPlant> Plants);

/// <summary>
/// Represents a public plant application model.
/// </summary>
/// <param name="Id">The plant id.</param>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="PhotoData">The optional plant photo data URL.</param>
/// <param name="Location">The optional plant location.</param>
public sealed record PublicGardenPlant(
    PlantId Id,
    string Name,
    string Description,
    string? PhotoData,
    GardenPlantLocation? Location);

/// <summary>
/// Represents a plant summary application model.
/// </summary>
/// <param name="Id">The plant id.</param>
/// <param name="Name">The plant name.</param>
/// <param name="Description">The plant description.</param>
/// <param name="PhotoData">The optional plant photo data URL.</param>
/// <param name="PlantedOn">The optional planting date.</param>
/// <param name="Location">The optional plant location.</param>
/// <param name="LastWateredAt">The optional latest watering timestamp.</param>
public sealed record PlantSummary(
    PlantId Id,
    string Name,
    string Description,
    string? PhotoData,
    DateOnly? PlantedOn,
    GardenPlantLocation? Location,
    DateTimeOffset? LastWateredAt);

/// <summary>
/// Represents a plant location application model.
/// </summary>
/// <param name="Id">The location id.</param>
/// <param name="Name">The location name.</param>
public sealed record GardenPlantLocation(LocationId Id, string Name);

/// <summary>
/// Represents a garden location application model.
/// </summary>
/// <param name="Id">The location id.</param>
/// <param name="Name">The location name.</param>
/// <param name="Plants">The number of plants assigned to the location.</param>
public sealed record LocationSummary(LocationId Id, string Name, int Plants);

/// <summary>
/// Represents an admin invite application model.
/// </summary>
/// <param name="Id">The invite id.</param>
/// <param name="Code">The invite code.</param>
/// <param name="CreatedAt">The invite creation timestamp.</param>
/// <param name="CreatedBy">The administrator who created the invite.</param>
/// <param name="UsedAt">The optional invite usage timestamp.</param>
/// <param name="UsedByUserId">The optional id of the user who consumed the invite.</param>
/// <param name="IsRevoked">A value indicating whether the invite is revoked.</param>
public sealed record AdminInvite(
    InviteId Id,
    string Code,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    DateTimeOffset? UsedAt,
    UserId? UsedByUserId,
    bool IsRevoked);

/// <summary>
/// Represents an admin user application model.
/// </summary>
/// <param name="Id">The user id.</param>
/// <param name="DisplayName">The user's display name.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="IsBlocked">A value indicating whether the user is blocked.</param>
/// <param name="Language">The user's preferred UI language.</param>
/// <param name="CreatedAt">The user creation timestamp.</param>
/// <param name="Plants">The number of plants owned by the user.</param>
public sealed record AdminUser(
    UserId Id,
    string DisplayName,
    string Email,
    bool IsBlocked,
    string Language,
    DateTimeOffset CreatedAt,
    int Plants);

/// <summary>
/// Represents a created invite application model.
/// </summary>
/// <param name="Id">The invite id.</param>
/// <param name="Code">The invite code.</param>
/// <param name="Url">The invite URL.</param>
public sealed record CreatedInvite(InviteId Id, string Code, Uri Url);
