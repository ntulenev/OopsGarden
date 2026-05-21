namespace Models;

/// <summary>
/// Represents a normalized user email address.
/// </summary>
public readonly record struct UserEmail
{
    /// <summary>
    /// The maximum email length.
    /// </summary>
    private const int MAX_LENGTH = 256;

    /// <summary>
    /// Gets the maximum email length.
    /// </summary>
    public static int MaxLength => MAX_LENGTH;

    /// <summary>
    /// Gets the normalized email value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserEmail"/> struct.
    /// </summary>
    /// <param name="value">The email value.</param>
    public UserEmail(string value)
    {
        var normalized = DomainText.Required(value, nameof(value), MaxLength, "Email")
            .ToUpperInvariant();
        if (!normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("Email must contain an at sign.", nameof(value));
        }

        Value = normalized;
    }

    /// <summary>
    /// Creates an email value from text.
    /// </summary>
    /// <param name="value">The email text.</param>
    /// <returns>A <see cref="UserEmail"/> instance.</returns>
    public static UserEmail From(string value) => new(value);
}
