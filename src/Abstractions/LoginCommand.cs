namespace Abstractions;

/// <summary>
/// Represents credentials used for user or admin login.
/// </summary>
/// <param name="Email">The login email address.</param>
/// <param name="Password">The login password.</param>
public sealed record LoginCommand(string Email, string Password);
