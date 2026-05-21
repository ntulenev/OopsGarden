using Models;

namespace Abstractions;

/// <summary>
/// Represents credentials used for user or admin login.
/// </summary>
public sealed record LoginCommand(string Email, string Password);

/// <summary>
/// Represents registration input.
/// </summary>
public sealed record RegisterCommand(string InviteCode, string DisplayName, string Email, string Password, string Language);

/// <summary>
/// Represents profile settings input.
/// </summary>
public sealed record SettingsCommand(string DisplayName, string Language, string? AvatarData, bool IsGardenPublic);

/// <summary>
/// Represents editable plant input.
/// </summary>
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
public sealed record LocationCommand(string Name);

/// <summary>
/// Represents the authenticated user application model.
/// </summary>
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
public sealed record AdminLogin(string Name, string Role);

/// <summary>
/// Represents a public garden application model.
/// </summary>
public sealed record PublicGarden(UserId Id, string Name, string? AvatarData, IReadOnlyList<PublicGardenPlant> Plants);

/// <summary>
/// Represents a public plant application model.
/// </summary>
public sealed record PublicGardenPlant(
    PlantId Id,
    string Name,
    string Description,
    string? PhotoData,
    GardenPlantLocation? Location);

/// <summary>
/// Represents a plant summary application model.
/// </summary>
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
public sealed record GardenPlantLocation(LocationId Id, string Name);

/// <summary>
/// Represents a garden location application model.
/// </summary>
public sealed record LocationSummary(LocationId Id, string Name, int Plants);

/// <summary>
/// Represents an admin invite application model.
/// </summary>
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
public sealed record CreatedInvite(InviteId Id, string Code, Uri Url);
