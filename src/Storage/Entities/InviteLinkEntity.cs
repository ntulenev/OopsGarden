namespace Storage.Entities;

/// <summary>
/// Represents a persisted invite link.
/// </summary>
public sealed class InviteLinkEntity
{
    /// <summary>
    /// Gets or sets the invite id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the invite code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the creator name.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the usage timestamp.
    /// </summary>
    public DateTimeOffset? UsedAt { get; set; }

    /// <summary>
    /// Gets or sets the consuming user id.
    /// </summary>
    public Guid? UsedByUserId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the invite is revoked.
    /// </summary>
    public bool IsRevoked { get; set; }
}
