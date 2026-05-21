namespace Models;

/// <summary>
/// Represents an administrator name.
/// </summary>
public readonly record struct AdminName
{
    /// <summary>
    /// Gets the administrator name value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminName"/> struct.
    /// </summary>
    /// <param name="value">The administrator name text.</param>
    public AdminName(string value)
    {
        Value = DomainText.Required(value, nameof(value), 120, "Admin name");
    }

    /// <summary>
    /// Creates an administrator name from text.
    /// </summary>
    /// <param name="value">The administrator name text.</param>
    /// <returns>An <see cref="AdminName"/> instance.</returns>
    public static AdminName From(string value) => new(value);
}
