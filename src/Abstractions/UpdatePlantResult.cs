namespace Abstractions;

/// <summary>
/// Represents plant update result.
/// </summary>
/// <param name="Status">The plant update status.</param>
/// <param name="Error">The validation error when update input is invalid.</param>
public sealed record UpdatePlantResult(UpdatePlantStatus Status, string? Error);
