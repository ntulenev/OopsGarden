namespace Transport;


/// <summary>
/// Represents a public garden response.
/// </summary>
/// <param name="Id">The garden owner id.</param>
/// <param name="Name">The garden owner display name.</param>
/// <param name="Avatar">The optional garden owner avatar data URL.</param>
/// <param name="Plants">The public plants in the garden.</param>
public sealed record PublicGardenResponse(Guid Id, string Name, string? Avatar, IReadOnlyList<PublicPlantResponse> Plants);
