namespace Models.Application;

/// <summary>
/// Represents plant creation result.
/// </summary>
/// <param name="Status">The creation status.</param>
/// <param name="Id">The created plant id when creation succeeds.</param>
/// <param name="Error">The validation error when creation fails.</param>
public sealed record CreatePlantResult(CommandStatus Status, Guid? Id, PlantCommandError? Error)
{
    /// <summary>
    /// Gets a value indicating whether creation succeeded.
    /// </summary>
    public bool IsSuccess => Status == CommandStatus.Succeeded;

    /// <summary>
    /// Gets the validation error message when creation fails.
    /// </summary>
    public string? ErrorMessage => Error switch
    {
        PlantCommandError.InvalidLocation => "Invalid location.",
        _ => null
    };

    /// <summary>
    /// Creates a successful plant creation result.
    /// </summary>
    public static CreatePlantResult Succeeded(Guid id) => new(CommandStatus.Succeeded, id, null);

    /// <summary>
    /// Creates an invalid plant creation result.
    /// </summary>
    public static CreatePlantResult Invalid(PlantCommandError error) => new(CommandStatus.Invalid, null, error);
}
