namespace Transport;

/// <summary>
/// Represents a plant note date update request.
/// </summary>
/// <param name="CreatedOn">The corrected note date.</param>
public sealed record PlantNoteDateRequest(DateOnly CreatedOn);
