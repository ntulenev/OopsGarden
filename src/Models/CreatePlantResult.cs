namespace Models;

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
