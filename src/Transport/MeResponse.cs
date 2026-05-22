namespace Transport;

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
