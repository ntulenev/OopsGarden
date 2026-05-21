namespace Models;

/// <summary>
/// Represents a garden location name.
/// </summary>
public readonly record struct LocationName
{
    /// <summary>
    /// The maximum location name length.
    /// </summary>
    private const int MAX_LENGTH = 120;

    /// <summary>
    /// Gets the maximum location name length.
    /// </summary>
    public static int MaxLength => MAX_LENGTH;

    /// <summary>
    /// Gets the location name value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationName"/> struct.
    /// </summary>
    /// <param name="value">The location name text.</param>
    public LocationName(string value)
    {
        Value = DomainText.Required(value, nameof(value), MaxLength, "Location name");
    }

    /// <summary>
    /// Creates a location name from text.
    /// </summary>
    /// <param name="value">The location name text.</param>
    /// <returns>A <see cref="LocationName"/> instance.</returns>
    public static LocationName From(string value) => new(value);
}
