namespace Storage.Entities;

/// <summary>
/// Represents a persisted application user.
/// </summary>
public sealed class AppUserEntity
{
    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password hash.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user is blocked.
    /// </summary>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// Gets or sets the preferred UI language.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the avatar data URL.
    /// </summary>
    public string? AvatarData { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the garden is public.
    /// </summary>
    public bool IsGardenPublic { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets the user's locations.
    /// </summary>
    public ICollection<LocationEntity> Locations { get; } = [];

    /// <summary>
    /// Gets the user's plants.
    /// </summary>
    public ICollection<PlantEntity> Plants { get; } = [];
}
