namespace Models;

/// <summary>
/// Represents a user display name.
/// </summary>
public readonly record struct DisplayName
{
    /// <summary>
    /// The maximum display name length.
    /// </summary>
    private const int MAX_LENGTH = 120;

    /// <summary>
    /// Gets the maximum display name length.
    /// </summary>
    public static int MaxLength => MAX_LENGTH;

    /// <summary>
    /// Gets the display name value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayName"/> struct.
    /// </summary>
    /// <param name="value">The display name text.</param>
    public DisplayName(string value)
    {
        Value = DomainText.Required(value, nameof(value), MaxLength, "Display name");
    }

    /// <summary>
    /// Creates a display name from text.
    /// </summary>
    /// <param name="value">The display name text.</param>
    /// <returns>A <see cref="DisplayName"/> instance.</returns>
    public static DisplayName From(string value) => new(value);
}
