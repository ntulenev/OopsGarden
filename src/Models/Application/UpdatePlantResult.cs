namespace Models.Application;

/// <summary>
/// Represents plant update result.
/// </summary>
/// <param name="Status">The update status.</param>
/// <param name="Error">The validation error when update input is invalid.</param>
public sealed record UpdatePlantResult(CommandStatus Status, PlantCommandError? Error)
{
    /// <summary>
    /// Gets the validation error message when update input is invalid.
    /// </summary>
    public string? ErrorMessage => Error switch
    {
        PlantCommandError.InvalidLocation => "Invalid location.",
        _ => null
    };

    /// <summary>
    /// Gets a successful plant update result.
    /// </summary>
    public static UpdatePlantResult Succeeded { get; } = new(CommandStatus.Succeeded, null);

    /// <summary>
    /// Gets a not found plant update result.
    /// </summary>
    public static UpdatePlantResult NotFound { get; } = new(CommandStatus.NotFound, null);

    /// <summary>
    /// Creates an invalid plant update result.
    /// </summary>
    public static UpdatePlantResult Invalid(PlantCommandError error) => new(CommandStatus.Invalid, error);
}
