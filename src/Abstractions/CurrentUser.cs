using Models;

namespace Abstractions;

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
