namespace Models;

/// <summary>
/// Represents a strongly typed watering event identifier.
/// </summary>
public readonly record struct WateringEventId
{
    /// <summary>
    /// Gets the underlying GUID value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WateringEventId"/> struct.
    /// </summary>
    /// <param name="value">The underlying GUID value.</param>
    public WateringEventId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Watering event id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Creates an identifier from an existing GUID.
    /// </summary>
    /// <param name="value">The existing GUID value.</param>
    /// <returns>A <see cref="WateringEventId"/> instance.</returns>
    public static WateringEventId From(Guid value) => new(value);

    /// <summary>
    /// Creates a new random identifier.
    /// </summary>
    /// <returns>A new <see cref="WateringEventId"/> instance.</returns>
    public static WateringEventId New() => new(Guid.NewGuid());
}
