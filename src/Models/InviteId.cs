namespace Models;

/// <summary>
/// Represents a strongly typed invite identifier.
/// </summary>
public readonly record struct InviteId
{
    /// <summary>
    /// Gets the underlying GUID value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InviteId"/> struct.
    /// </summary>
    /// <param name="value">The underlying GUID value.</param>
    public InviteId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Invite id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Creates an identifier from an existing GUID.
    /// </summary>
    /// <param name="value">The existing GUID value.</param>
    /// <returns>An <see cref="InviteId"/> instance.</returns>
    public static InviteId From(Guid value) => new(value);

    /// <summary>
    /// Creates a new random identifier.
    /// </summary>
    /// <returns>A new <see cref="InviteId"/> instance.</returns>
    public static InviteId New() => new(Guid.NewGuid());
}
