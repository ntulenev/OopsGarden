namespace Models;

/// <summary>
/// Represents plant note text.
/// </summary>
public readonly record struct PlantNoteText
{
    /// <summary>
    /// The maximum plant note length.
    /// </summary>
    private const int MAX_LENGTH = 2_000;

    /// <summary>
    /// Gets the maximum plant note length.
    /// </summary>
    public static int MaxLength => MAX_LENGTH;

    /// <summary>
    /// Gets the note text value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlantNoteText"/> struct.
    /// </summary>
    /// <param name="value">The note text.</param>
    public PlantNoteText(string value)
    {
        Value = DomainText.Required(value, nameof(value), MaxLength, "Plant note");
    }

    /// <summary>
    /// Creates plant note text from a string.
    /// </summary>
    /// <param name="value">The note text.</param>
    /// <returns>A <see cref="PlantNoteText"/> instance.</returns>
    public static PlantNoteText From(string value) => new(value);
}
