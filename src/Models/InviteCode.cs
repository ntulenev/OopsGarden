namespace Models;

/// <summary>
/// Represents a registration invite code.
/// </summary>
public readonly record struct InviteCode
{
    /// <summary>
    /// The maximum invite code length.
    /// </summary>
    private const int MAX_LENGTH = 48;

    /// <summary>
    /// Gets the maximum invite code length.
    /// </summary>
    public static int MaxLength => MAX_LENGTH;

    /// <summary>
    /// Gets the invite code value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InviteCode"/> struct.
    /// </summary>
    /// <param name="value">The invite code text.</param>
    public InviteCode(string value)
    {
        Value = DomainText.Required(value, nameof(value), MaxLength, "Invite code");
    }

    /// <summary>
    /// Creates an invite code from text.
    /// </summary>
    /// <param name="value">The invite code text.</param>
    /// <returns>An <see cref="InviteCode"/> instance.</returns>
    public static InviteCode From(string value) => new(value);
}
