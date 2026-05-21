namespace Models;

/// <summary>
/// Represents a plant name.
/// </summary>
public readonly record struct PlantName
{
    /// <summary>
    /// The maximum plant name length.
    /// </summary>
    private const int MAX_LENGTH = 160;

    /// <summary>
    /// Gets the maximum plant name length.
    /// </summary>
    public static int MaxLength => MAX_LENGTH;

    /// <summary>
    /// Gets the plant name value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlantName"/> struct.
    /// </summary>
    /// <param name="value">The plant name text.</param>
    public PlantName(string value)
    {
        Value = DomainText.Required(value, nameof(value), MaxLength, "Plant name");
    }

    /// <summary>
    /// Creates a plant name from text.
    /// </summary>
    /// <param name="value">The plant name text.</param>
    /// <returns>A <see cref="PlantName"/> instance.</returns>
    public static PlantName From(string value) => new(value);
}
