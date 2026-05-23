namespace Models;

/// <summary>
/// Represents a single-use registration invitation.
/// </summary>
public sealed class InviteLink
{
    private InviteLink()
    {
    }

    private InviteLink(
        InviteId id,
        InviteCode code,
        DateTimeOffset createdAt,
        AdminName createdBy,
        DateTimeOffset? usedAt,
        UserId? usedByUserId,
        bool isRevoked)
    {
        Id = id;
        Code = code;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
        UsedAt = usedAt;
        UsedByUserId = usedByUserId;
        IsRevoked = isRevoked;
    }

    /// <summary>
    /// Gets the unique invite identifier.
    /// </summary>
    public InviteId Id { get; private set; }

    /// <summary>
    /// Gets the public invite code.
    /// </summary>
    public InviteCode Code { get; private set; }

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the admin username that created the invite.
    /// </summary>
    public AdminName CreatedBy { get; private set; }

    /// <summary>
    /// Gets the timestamp when the invite was used.
    /// </summary>
    public DateTimeOffset? UsedAt { get; private set; }

    /// <summary>
    /// Gets the user that consumed the invite.
    /// </summary>
    public UserId? UsedByUserId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the invite is revoked.
    /// </summary>
    public bool IsRevoked { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the invite can still be consumed.
    /// </summary>
    public bool CanBeUsed => UsedAt is null && !IsRevoked;

    /// <summary>
    /// Creates a new invite link.
    /// </summary>
    /// <param name="code">The public invite code.</param>
    /// <param name="createdBy">The admin username that creates the invite.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>A new <see cref="InviteLink"/> instance.</returns>
    public static InviteLink Create(InviteCode code, AdminName createdBy, DateTimeOffset createdAt = default)
        => new(
            InviteId.New(),
            code,
            createdAt,
            createdBy,
            usedAt: null,
            usedByUserId: null,
            isRevoked: false);

    /// <summary>
    /// Rehydrates an invite from persisted values.
    /// </summary>
    /// <param name="id">The persisted invite identifier.</param>
    /// <param name="code">The persisted invite code.</param>
    /// <param name="createdAt">The persisted creation timestamp.</param>
    /// <param name="createdBy">The persisted creator username.</param>
    /// <param name="usedAt">The persisted usage timestamp.</param>
    /// <param name="usedByUserId">The persisted consuming user identifier.</param>
    /// <param name="isRevoked">The persisted revoked state.</param>
    /// <returns>A rehydrated <see cref="InviteLink"/> instance.</returns>
    public static InviteLink Restore(
        InviteId id,
        InviteCode code,
        DateTimeOffset createdAt,
        AdminName createdBy,
        DateTimeOffset? usedAt,
        UserId? usedByUserId,
        bool isRevoked)
        => new(id, code, createdAt, createdBy, usedAt, usedByUserId, isRevoked);

    /// <summary>
    /// Marks this invite as consumed by a user.
    /// </summary>
    /// <param name="userId">The consuming user identifier.</param>
    /// <param name="usedAt">The usage timestamp.</param>
    /// <exception cref="InvalidOperationException">Thrown when the invite cannot be used.</exception>
    public void MarkUsed(UserId userId, DateTimeOffset usedAt = default)
    {
        if (!CanBeUsed)
        {
            throw new InvalidOperationException("Invite cannot be used.");
        }

        UsedAt = usedAt;
        UsedByUserId = userId;
    }

    /// <summary>
    /// Revokes this invite.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the invite was already used.</exception>
    public void Revoke()
    {
        if (UsedAt is not null)
        {
            throw new InvalidOperationException("Used invite cannot be revoked.");
        }

        IsRevoked = true;
    }
}
