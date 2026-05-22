namespace Models;

/// <summary>
/// Strongly typed plant note identifier.
/// </summary>
public readonly record struct PlantNoteId
{
    /// <summary>
    /// Gets the note identifier value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlantNoteId"/> struct.
    /// </summary>
    /// <param name="value">The identifier value.</param>
    /// <exception cref="ArgumentException">Thrown when value is empty.</exception>
    public PlantNoteId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Plant note id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Creates a note id from an existing value.
    /// </summary>
    /// <param name="value">The identifier value.</param>
    /// <returns>A <see cref="PlantNoteId"/> instance.</returns>
    public static PlantNoteId From(Guid value) => new(value);

    /// <summary>
    /// Creates a new note id.
    /// </summary>
    /// <returns>A new <see cref="PlantNoteId"/> instance.</returns>
    public static PlantNoteId New() => new(Guid.NewGuid());
}
