namespace Models;

/// <summary>
/// Represents an optional image encoded as a browser data URL.
/// </summary>
public readonly record struct ImageDataUrl
{
    /// <summary>
    /// The maximum avatar image data URL length.
    /// </summary>
    private const int MAX_AVATAR_LENGTH = 1_000_000;

    /// <summary>
    /// The maximum plant photo data URL length.
    /// </summary>
    private const int MAX_PLANT_PHOTO_LENGTH = 1_500_000;

    /// <summary>
    /// Gets the maximum avatar image data URL length.
    /// </summary>
    public static int MaxAvatarLength => MAX_AVATAR_LENGTH;

    /// <summary>
    /// Gets the maximum plant photo data URL length.
    /// </summary>
    public static int MaxPlantPhotoLength => MAX_PLANT_PHOTO_LENGTH;

    /// <summary>
    /// Gets the image data URL value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageDataUrl"/> struct.
    /// </summary>
    /// <param name="value">The image data URL text.</param>
    /// <param name="maxLength">The maximum allowed length.</param>
    public ImageDataUrl(string value, int maxLength)
    {
        Value = DomainText.Required(value, nameof(value), maxLength, "Image data URL");
        if (!Value.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Image data URL must start with data:image/.", nameof(value));
        }
    }

    /// <summary>
    /// Creates an optional avatar image data URL.
    /// </summary>
    /// <param name="value">The image data URL text.</param>
    /// <returns>An image data URL, or null when the input is empty.</returns>
    public static ImageDataUrl? Avatar(string? value)
    {
        var normalized = DomainText.Optional(value, nameof(value), MaxAvatarLength, "Avatar data URL");
        return normalized is null ? null : new ImageDataUrl(normalized, MaxAvatarLength);
    }

    /// <summary>
    /// Creates an optional plant photo data URL.
    /// </summary>
    /// <param name="value">The image data URL text.</param>
    /// <returns>An image data URL, or null when the input is empty.</returns>
    public static ImageDataUrl? PlantPhoto(string? value)
    {
        var normalized = DomainText.Optional(value, nameof(value), MaxPlantPhotoLength, "Plant photo data URL");
        return normalized is null ? null : new ImageDataUrl(normalized, MaxPlantPhotoLength);
    }
}
