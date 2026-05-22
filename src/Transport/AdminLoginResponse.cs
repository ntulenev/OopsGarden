namespace Transport;

/// <summary>
/// Represents an authenticated admin response.
/// </summary>
/// <param name="Name">The administrator name.</param>
/// <param name="Role">The authenticated administrator role.</param>
public sealed record AdminLoginResponse(string Name, string Role);
