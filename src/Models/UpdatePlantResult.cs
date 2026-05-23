namespace Models;

/// <summary>
/// Represents plant update result.
/// </summary>
/// <param name="Status">The plant update status.</param>
/// <param name="Error">The validation error when update input is invalid.</param>
public sealed record UpdatePlantResult(UpdatePlantStatus Status, PlantCommandError? Error)
{
    /// <summary>
    /// Gets the validation error message when update input is invalid.
    /// </summary>
    public string? ErrorMessage => Error switch
    {
        PlantCommandError.InvalidLocation => "Invalid location.",
        _ => null
    };
}
