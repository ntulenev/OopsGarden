namespace Transport;

/// <summary>
/// Represents a request to change a user's blocked state.
/// </summary>
/// <param name="IsBlocked">A value indicating whether the user should be blocked.</param>
public sealed record BlockUserRequest(bool IsBlocked);
