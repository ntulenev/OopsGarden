namespace Transport;

/// <summary>
/// Represents a plant watering creation request.
/// </summary>
/// <param name="WateredOn">The watering date.</param>
public sealed record PlantWateringRequest(DateOnly WateredOn);
