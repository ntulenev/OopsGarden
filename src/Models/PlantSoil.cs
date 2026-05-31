namespace Models;

/// <summary>
/// Represents plant soil notes.
/// </summary>
public readonly record struct PlantSoil
{
    /// <summary>
    /// The maximum plant soil text length.
    /// </summary>
    private const int MAX_LENGTH = 2_000;

    /// <summary>
    /// Gets the maximum plant soil text length.
    /// </summary>
    public static int MaxLength => MAX_LENGTH;

    /// <summary>
    /// Gets the plant soil text value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlantSoil"/> struct.
    /// </summary>
    /// <param name="value">The plant soil text.</param>
    public PlantSoil(string? value)
    {
        Value = DomainText.Optional(value, nameof(value), MaxLength, "Plant soil") ?? string.Empty;
    }

    /// <summary>
    /// Creates plant soil notes from text.
    /// </summary>
    /// <param name="value">The plant soil text.</param>
    /// <returns>A <see cref="PlantSoil"/> instance.</returns>
    public static PlantSoil From(string? value) => new(value);
}
