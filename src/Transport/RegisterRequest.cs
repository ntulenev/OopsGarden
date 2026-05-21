namespace Transport;

/// <summary>
/// Represents a registration request created from an invite.
/// </summary>
/// <param name="InviteCode">The invite code.</param>
/// <param name="DisplayName">The display name.</param>
/// <param name="Email">The email address.</param>
/// <param name="Password">The password.</param>
/// <param name="Language">The preferred UI language.</param>
public sealed record RegisterRequest(
    string InviteCode,
    string DisplayName,
    string Email,
    string Password,
    string? Language);
