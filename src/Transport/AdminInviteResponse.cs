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
