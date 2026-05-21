namespace Transport;

/// <summary>
/// Represents a login request.
/// </summary>
/// <param name="Email">The user's email address or admin username.</param>
/// <param name="Password">The password.</param>
public sealed record LoginRequest(string Email, string Password);
