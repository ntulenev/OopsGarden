namespace Models;

/// <summary>
/// Represents a plant description.
/// </summary>
public readonly record struct PlantDescription
{
    /// <summary>
    /// The maximum plant description length.
    /// </summary>
    private const int MAX_LENGTH = 2_000;

    /// <summary>
    /// Gets the maximum plant description length.
    /// </summary>
    public static int MaxLength => MAX_LENGTH;

    /// <summary>
    /// Gets the plant description value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlantDescription"/> struct.
    /// </summary>
    /// <param name="value">The plant description text.</param>
    public PlantDescription(string? value)
    {
        Value = DomainText.Optional(value, nameof(value), MaxLength, "Plant description") ?? string.Empty;
    }

    /// <summary>
    /// Creates a plant description from text.
    /// </summary>
    /// <param name="value">The plant description text.</param>
    /// <returns>A <see cref="PlantDescription"/> instance.</returns>
    public static PlantDescription From(string? value) => new(value);
}
