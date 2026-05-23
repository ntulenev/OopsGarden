namespace Models.Application;

/// <summary>
/// Represents plant note date update input.
/// </summary>
/// <param name="CreatedOn">The corrected note date.</param>
public sealed record UpdatePlantNoteDateCommand(DateOnly CreatedOn);
