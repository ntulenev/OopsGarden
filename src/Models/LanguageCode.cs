namespace Models;

/// <summary>
/// Represents a supported UI language code.
/// </summary>
public readonly record struct LanguageCode
{
    /// <summary>
    /// The English language code.
    /// </summary>
    private const string EN = "en";

    /// <summary>
    /// The Russian language code.
    /// </summary>
    private const string RU = "ru";

    /// <summary>
    /// Gets the English language code.
    /// </summary>
    public static string English => EN;

    /// <summary>
    /// Gets the Russian language code.
    /// </summary>
    public static string Russian => RU;

    /// <summary>
    /// Gets the normalized language code.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageCode"/> struct.
    /// </summary>
    /// <param name="value">The language code.</param>
    public LanguageCode(string? value)
    {
        Value = string.Equals(value, RU, StringComparison.OrdinalIgnoreCase) ? RU : EN;
    }

    /// <summary>
    /// Creates a language code from text.
    /// </summary>
    /// <param name="value">The language code text.</param>
    /// <returns>A <see cref="LanguageCode"/> instance.</returns>
    public static LanguageCode From(string? value) => new(value);
}
