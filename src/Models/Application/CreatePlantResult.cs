namespace Models.Application;

/// <summary>
/// Represents plant creation result.
/// </summary>
/// <param name="Id">The created plant id when creation succeeds.</param>
/// <param name="Error">The validation error when creation fails.</param>
public sealed record CreatePlantResult(Guid? Id, PlantCommandError? Error)
{
    /// <summary>
    /// Gets a value indicating whether creation succeeded.
    /// </summary>
    public bool IsSuccess => Error is null;

    /// <summary>
    /// Gets the validation error message when creation fails.
    /// </summary>
    public string? ErrorMessage => Error switch
    {
        PlantCommandError.InvalidLocation => "Invalid location.",
        _ => null
    };
}
