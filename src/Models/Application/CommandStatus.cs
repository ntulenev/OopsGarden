namespace Models.Application;

/// <summary>
/// Represents a generic command execution status.
/// </summary>
public enum CommandStatus
{
    /// <summary>
    /// The command succeeded.
    /// </summary>
    Succeeded,

    /// <summary>
    /// The target resource was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The command input was invalid.
    /// </summary>
    Invalid
}
