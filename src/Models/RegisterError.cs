namespace Models;

/// <summary>
/// Represents registration validation errors.
/// </summary>
public enum RegisterError
{
    /// <summary>
    /// The supplied invite is missing or cannot be used.
    /// </summary>
    InvalidInvite,

    /// <summary>
    /// The supplied email address is already registered.
    /// </summary>
    EmailAlreadyRegistered
}
