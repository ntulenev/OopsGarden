using Models;

namespace Contracts.Application;

/// <summary>
/// Represents an admin user application model.
/// </summary>
/// <param name="Id">The user id.</param>
/// <param name="DisplayName">The user's display name.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="IsBlocked">A value indicating whether the user is blocked.</param>
/// <param name="Language">The user's preferred UI language.</param>
/// <param name="CreatedAt">The user creation timestamp.</param>
/// <param name="Plants">The number of plants owned by the user.</param>
public sealed record AdminUser(
    UserId Id,
    string DisplayName,
    string Email,
    bool IsBlocked,
    string Language,
    DateTimeOffset CreatedAt,
    int Plants);
