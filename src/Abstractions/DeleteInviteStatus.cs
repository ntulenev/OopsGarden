namespace Abstractions;

/// <summary>
/// Represents invite deletion status.
/// </summary>
public enum DeleteInviteStatus
{
    /// <summary>
    /// The invite was deleted.
    /// </summary>
    Deleted,

    /// <summary>
    /// The invite was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The invite cannot be deleted.
    /// </summary>
    Invalid
}
