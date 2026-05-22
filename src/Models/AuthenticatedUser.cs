namespace Models;

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
