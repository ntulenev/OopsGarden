namespace Models;

/// <summary>
/// Represents an application user who owns a garden.
/// </summary>
public sealed class AppUser
{
    private AppUser()
    {
    }

    private AppUser(
        UserId id,
        UserEmail email,
        DisplayName displayName,
        PasswordHash passwordHash,
        LanguageCode language,
        ImageDataUrl? avatarDataUrl,
        bool isGardenPublic,
        bool isBlocked,
        DateTimeOffset createdAt)
    {
        Id = id;
        Email = email;
        DisplayName = displayName;
        PasswordHash = passwordHash;
        Language = language;
        AvatarDataUrl = avatarDataUrl;
        IsGardenPublic = isGardenPublic;
        IsBlocked = isBlocked;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Gets the unique user identifier.
    /// </summary>
    public UserId Id { get; private set; }

    /// <summary>
    /// Gets the normalized email address.
    /// </summary>
    public UserEmail Email { get; private set; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public DisplayName DisplayName { get; private set; }

    /// <summary>
    /// Gets the hashed password.
    /// </summary>
    public PasswordHash PasswordHash { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the user is blocked.
    /// </summary>
    public bool IsBlocked { get; private set; }

    /// <summary>
    /// Gets the preferred UI language.
    /// </summary>
    public LanguageCode Language { get; private set; }

    /// <summary>
    /// Gets the avatar image as a browser data URL.
    /// </summary>
    public ImageDataUrl? AvatarDataUrl { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the garden can be viewed through a public link.
    /// </summary>
    public bool IsGardenPublic { get; private set; }

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the user's garden locations.
    /// </summary>
    public IReadOnlyCollection<Location> Locations => _locations;

    /// <summary>
    /// Gets the user's plants.
    /// </summary>
    public IReadOnlyCollection<Plant> Plants => _plants;

    /// <summary>
    /// Creates a new application user.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="displayName">The display name.</param>
    /// <param name="passwordHash">The hashed password.</param>
    /// <param name="language">The preferred UI language.</param>
    /// <returns>A new <see cref="AppUser"/> instance.</returns>
    public static AppUser Create(
        UserEmail email,
        DisplayName displayName,
        PasswordHash passwordHash,
        LanguageCode language)
        => new(
            UserId.New(),
            email,
            displayName,
            passwordHash,
            language,
            null,
            isGardenPublic: false,
            isBlocked: false,
            DateTimeOffset.UtcNow);

    /// <summary>
    /// Rehydrates a user from persisted values.
    /// </summary>
    /// <param name="id">The persisted user identifier.</param>
    /// <param name="email">The persisted email address.</param>
    /// <param name="displayName">The persisted display name.</param>
    /// <param name="passwordHash">The persisted password hash.</param>
    /// <param name="language">The persisted preferred language.</param>
    /// <param name="avatarDataUrl">The persisted avatar data URL.</param>
    /// <param name="isGardenPublic">The persisted public garden state.</param>
    /// <param name="isBlocked">The persisted blocked state.</param>
    /// <param name="createdAt">The persisted creation timestamp.</param>
    /// <returns>A rehydrated <see cref="AppUser"/> instance.</returns>
    public static AppUser Restore(
        UserId id,
        UserEmail email,
        DisplayName displayName,
        PasswordHash passwordHash,
        LanguageCode language,
        ImageDataUrl? avatarDataUrl,
        bool isGardenPublic,
        bool isBlocked,
        DateTimeOffset createdAt)
        => new(
            id,
            email,
            displayName,
            passwordHash,
            language,
            avatarDataUrl,
            isGardenPublic,
            isBlocked,
            createdAt);

    /// <summary>
    /// Updates profile settings.
    /// </summary>
    /// <param name="displayName">The new display name.</param>
    /// <param name="language">The new preferred UI language.</param>
    /// <param name="avatarDataUrl">The new avatar image as a browser data URL.</param>
    /// <param name="isGardenPublic">A value indicating whether the garden can be viewed through a public link.</param>
    public void UpdateSettings(
        DisplayName displayName,
        LanguageCode language,
        ImageDataUrl? avatarDataUrl,
        bool isGardenPublic)
    {
        DisplayName = displayName;
        Language = language;
        AvatarDataUrl = avatarDataUrl;
        IsGardenPublic = isGardenPublic;
    }

    /// <summary>
    /// Replaces the user's password hash.
    /// </summary>
    /// <param name="passwordHash">The new password hash.</param>
    public void ChangePasswordHash(PasswordHash passwordHash)
    {
        PasswordHash = passwordHash;
    }

    /// <summary>
    /// Blocks the user from signing in.
    /// </summary>
    public void Block() => IsBlocked = true;

    /// <summary>
    /// Allows the user to sign in again.
    /// </summary>
    public void Unblock() => IsBlocked = false;

    private readonly List<Location> _locations = [];
    private readonly List<Plant> _plants = [];
}
