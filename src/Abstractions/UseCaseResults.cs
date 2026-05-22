namespace Abstractions;

/// <summary>
/// Represents registration result.
/// </summary>
/// <param name="User">The authenticated user when registration succeeds.</param>
/// <param name="Error">The validation error when registration fails.</param>
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
/// <param name="Id">The created plant id when creation succeeds.</param>
/// <param name="Error">The validation error when creation fails.</param>
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
/// <param name="Status">The plant update status.</param>
/// <param name="Error">The validation error when update input is invalid.</param>
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
/// <param name="Status">The invite deletion status.</param>
/// <param name="Error">The validation error when deletion is not allowed.</param>
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
