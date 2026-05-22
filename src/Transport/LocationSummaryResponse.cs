namespace Transport;


/// <summary>
/// Represents a garden location response.
/// </summary>
/// <param name="Id">The location id.</param>
/// <param name="Name">The location name.</param>
/// <param name="Plants">The number of plants assigned to the location.</param>
public sealed record LocationSummaryResponse(Guid Id, string Name, int Plants);
