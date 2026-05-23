namespace Models.Application;

/// <summary>
/// Represents admin login output.
/// </summary>
/// <param name="Name">The administrator name.</param>
/// <param name="Role">The authenticated administrator role.</param>
public sealed record AdminLogin(string Name, string Role);
