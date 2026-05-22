namespace Transport;

/// <summary>
/// Represents an invite link in the admin surface.
/// </summary>
/// <param name="Id">The invite id.</param>
/// <param name="Code">The invite code.</param>
/// <param name="CreatedAt">The invite creation timestamp.</param>
/// <param name="CreatedBy">The administrator who created the invite.</param>
/// <param name="UsedAt">The optional invite usage timestamp.</param>
/// <param name="UsedByUserId">The optional id of the user who consumed the invite.</param>
/// <param name="IsRevoked">A value indicating whether the invite is revoked.</param>
public sealed record AdminInviteResponse(
    Guid Id,
    string Code,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    DateTimeOffset? UsedAt,
    Guid? UsedByUserId,
    bool IsRevoked);

/// <summary>
/// Represents a user in the admin surface.
/// </summary>
/// <param name="Id">The user id.</param>
/// <param name="DisplayName">The user's display name.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="IsBlocked">A value indicating whether the user is blocked.</param>
/// <param name="Language">The user's preferred UI language.</param>
/// <param name="CreatedAt">The user creation timestamp.</param>
/// <param name="Plants">The number of plants owned by the user.</param>
public sealed record AdminUserResponse(
    Guid Id,
    string DisplayName,
    string Email,
    bool IsBlocked,
    string Language,
    DateTimeOffset CreatedAt,
    int Plants);

/// <summary>
/// Represents a created invite.
/// </summary>
/// <param name="Id">The invite id.</param>
/// <param name="Code">The invite code.</param>
/// <param name="Url">The invite URL.</param>
public sealed record CreatedInviteResponse(Guid Id, string Code, string Url);
