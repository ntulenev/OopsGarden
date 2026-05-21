namespace Models;

/// <summary>
/// Represents a hashed password.
/// </summary>
public readonly record struct PasswordHash
{
    /// <summary>
    /// Gets the password hash value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordHash"/> struct.
    /// </summary>
    /// <param name="value">The password hash text.</param>
    public PasswordHash(string value)
    {
        Value = DomainText.Required(value, nameof(value), 1_000, "Password hash");
    }

    /// <summary>
    /// Creates a password hash from text.
    /// </summary>
    /// <param name="value">The password hash text.</param>
    /// <returns>A <see cref="PasswordHash"/> instance.</returns>
    public static PasswordHash From(string value) => new(value);
}
