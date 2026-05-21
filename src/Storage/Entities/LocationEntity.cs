namespace Storage.Entities;

/// <summary>
/// Represents a persisted garden location.
/// </summary>
public sealed class LocationEntity
{
    /// <summary>
    /// Gets or sets the location id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning user id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the location name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owning user.
    /// </summary>
    public AppUserEntity? User { get; set; }

    /// <summary>
    /// Gets the plants assigned to the location.
    /// </summary>
    public ICollection<PlantEntity> Plants { get; } = [];
}
