namespace Models;

/// <summary>
/// Represents a strongly typed location identifier.
/// </summary>
public readonly record struct LocationId
{
    /// <summary>
    /// Gets the underlying GUID value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationId"/> struct.
    /// </summary>
    /// <param name="value">The underlying GUID value.</param>
    public LocationId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Location id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Creates an identifier from an existing GUID.
    /// </summary>
    /// <param name="value">The existing GUID value.</param>
    /// <returns>A <see cref="LocationId"/> instance.</returns>
    public static LocationId From(Guid value) => new(value);

    /// <summary>
    /// Creates a new random identifier.
    /// </summary>
    /// <returns>A new <see cref="LocationId"/> instance.</returns>
    public static LocationId New() => new(Guid.NewGuid());
}
