namespace Abstractions;

/// <summary>
/// Represents registration input.
/// </summary>
/// <param name="InviteCode">The registration invite code.</param>
/// <param name="DisplayName">The requested display name.</param>
/// <param name="Email">The registration email address.</param>
/// <param name="Password">The registration password.</param>
/// <param name="Language">The preferred UI language code.</param>
public sealed record RegisterCommand(string InviteCode, string DisplayName, string Email, string Password, string Language);
