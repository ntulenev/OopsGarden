namespace Transport;

/// <summary>
/// Represents an invite link in the admin surface.
/// </summary>
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
public sealed record CreatedInviteResponse(Guid Id, string Code, string Url);
