namespace Models;

/// <summary>
/// Represents a strongly typed plant identifier.
/// </summary>
public readonly record struct PlantId
{
    /// <summary>
    /// Gets the underlying GUID value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlantId"/> struct.
    /// </summary>
    /// <param name="value">The underlying GUID value.</param>
    public PlantId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Plant id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Creates an identifier from an existing GUID.
    /// </summary>
    /// <param name="value">The existing GUID value.</param>
    /// <returns>A <see cref="PlantId"/> instance.</returns>
    public static PlantId From(Guid value) => new(value);

    /// <summary>
    /// Creates a new random identifier.
    /// </summary>
    /// <returns>A new <see cref="PlantId"/> instance.</returns>
    public static PlantId New() => new(Guid.NewGuid());
}
