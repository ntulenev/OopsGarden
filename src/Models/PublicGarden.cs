namespace Models;

/// <summary>
/// Represents a public garden application model.
/// </summary>
/// <param name="Id">The garden owner id.</param>
/// <param name="Name">The garden owner display name.</param>
/// <param name="AvatarData">The optional garden owner avatar data URL.</param>
/// <param name="Plants">The public plants in the garden.</param>
public sealed record PublicGarden(UserId Id, string Name, string? AvatarData, IReadOnlyList<PublicGardenPlant> Plants);
