namespace Abstractions;

/// <summary>
/// Represents registration result.
/// </summary>
public sealed record RegisterResult(AuthenticatedUser? User, string? Error)
{
    /// <summary>
    /// Gets a value indicating whether registration succeeded.
    /// </summary>
    public bool IsSuccess => Error is null;
}

/// <summary>
/// Represents plant creation result.
/// </summary>
public sealed record CreatePlantResult(Guid? Id, string? Error)
{
    /// <summary>
    /// Gets a value indicating whether creation succeeded.
    /// </summary>
    public bool IsSuccess => Error is null;
}

/// <summary>
/// Represents plant update result.
/// </summary>
public sealed record UpdatePlantResult(UpdatePlantStatus Status, string? Error);

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

/// <summary>
/// Represents invite deletion result.
/// </summary>
public sealed record DeleteInviteResult(DeleteInviteStatus Status, string? Error);

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
