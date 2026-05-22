namespace Abstractions;

/// <summary>
/// Represents plant update status.
/// </summary>
public enum UpdatePlantStatus
{
    /// <summary>
    /// The plant was updated.
    /// </summary>
    Updated,

    /// <summary>
    /// The plant was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The update input was invalid.
    /// </summary>
    Invalid
}
