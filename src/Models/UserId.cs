namespace Models;

/// <summary>
/// Represents a strongly typed user identifier.
/// </summary>
public readonly record struct UserId
{
    /// <summary>
    /// Gets the underlying GUID value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserId"/> struct.
    /// </summary>
    /// <param name="value">The underlying GUID value.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty.</exception>
    public UserId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Creates an identifier from an existing GUID.
    /// </summary>
    /// <param name="value">The existing GUID value.</param>
    /// <returns>A <see cref="UserId"/> instance.</returns>
    public static UserId From(Guid value) => new(value);

    /// <summary>
    /// Creates a new random identifier.
    /// </summary>
    /// <returns>A new <see cref="UserId"/> instance.</returns>
    public static UserId New() => new(Guid.NewGuid());
}
