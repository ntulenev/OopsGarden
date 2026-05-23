namespace Models;

/// <summary>
/// Represents a user-defined place where plants can stand.
/// </summary>
public sealed class Location
{
    private Location()
    {
    }

    private Location(LocationId id, UserId userId, LocationName name, IEnumerable<Plant>? plants)
    {
        Id = id;
        UserId = userId;
        Name = name;
        if (plants is not null)
        {
            _plants.AddRange(plants);
        }
    }

    /// <summary>
    /// Gets the unique location identifier.
    /// </summary>
    public LocationId Id { get; private set; }

    /// <summary>
    /// Gets the owning user identifier.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets the owning user.
    /// </summary>
    public AppUser? User { get; private set; }

    /// <summary>
    /// Gets the location name.
    /// </summary>
    public LocationName Name { get; private set; }

    /// <summary>
    /// Gets the plants assigned to this location.
    /// </summary>
    public IReadOnlyCollection<Plant> Plants => _plants;

    /// <summary>
    /// Creates a new location.
    /// </summary>
    /// <param name="userId">The owning user identifier.</param>
    /// <param name="name">The location name.</param>
    /// <returns>A new <see cref="Location"/> instance.</returns>
    public static Location Create(UserId userId, LocationName name)
        => new(LocationId.New(), userId, name, null);

    /// <summary>
    /// Rehydrates a location from persisted values.
    /// </summary>
    /// <param name="id">The persisted location identifier.</param>
    /// <param name="userId">The persisted owning user identifier.</param>
    /// <param name="name">The persisted location name.</param>
    /// <returns>A rehydrated <see cref="Location"/> instance.</returns>
    public static Location Restore(LocationId id, UserId userId, LocationName name)
        => new(id, userId, name, null);

    /// <summary>
    /// Rehydrates a location and its assigned plants from persisted values.
    /// </summary>
    /// <param name="id">The persisted location identifier.</param>
    /// <param name="userId">The persisted owning user identifier.</param>
    /// <param name="name">The persisted location name.</param>
    /// <param name="plants">The persisted plants assigned to the location.</param>
    /// <returns>A rehydrated <see cref="Location"/> instance.</returns>
    public static Location Restore(LocationId id, UserId userId, LocationName name, IEnumerable<Plant> plants)
    {
        ArgumentNullException.ThrowIfNull(plants);
        return new Location(id, userId, name, plants);
    }

    /// <summary>
    /// Renames the location.
    /// </summary>
    /// <param name="name">The new location name.</param>
    public void Rename(LocationName name)
    {
        Name = name;
    }

    private readonly List<Plant> _plants = [];
}
