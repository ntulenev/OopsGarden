namespace Transport;

/// <summary>
/// Represents the authenticated user surface used by auth endpoints.
/// </summary>
public sealed record AuthenticatedUserResponse(
    Guid Id,
    string DisplayName,
    string Email,
    string Language,
    string? AvatarDataUrl,
    bool IsGardenPublic);

/// <summary>
/// Represents the current session response.
/// </summary>
public sealed record MeResponse(
    bool Authenticated,
    Guid? Id = null,
    string? Name = null,
    string? Role = null,
    string? Language = null,
    string? Avatar = null,
    bool IsGardenPublic = false);

/// <summary>
/// Represents an authenticated admin response.
/// </summary>
public sealed record AdminLoginResponse(string Name, string Role);
