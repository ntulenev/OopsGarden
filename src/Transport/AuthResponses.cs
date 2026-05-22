namespace Transport;

/// <summary>
/// Represents the authenticated user surface used by auth endpoints.
/// </summary>
/// <param name="Id">The authenticated user id.</param>
/// <param name="DisplayName">The authenticated user's display name.</param>
/// <param name="Email">The authenticated user's email address.</param>
/// <param name="Language">The authenticated user's preferred UI language.</param>
/// <param name="AvatarDataUrl">The authenticated user's optional avatar data URL.</param>
/// <param name="IsGardenPublic">A value indicating whether the garden is public.</param>
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
/// <param name="Authenticated">A value indicating whether the current session is authenticated.</param>
/// <param name="Id">The optional user id.</param>
/// <param name="Name">The optional display or admin name.</param>
/// <param name="Role">The optional current role.</param>
/// <param name="Language">The optional preferred UI language.</param>
/// <param name="Avatar">The optional avatar data URL.</param>
/// <param name="IsGardenPublic">A value indicating whether the garden is public.</param>
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
/// <param name="Name">The administrator name.</param>
/// <param name="Role">The authenticated administrator role.</param>
public sealed record AdminLoginResponse(string Name, string Role);
