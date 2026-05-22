namespace Models;

/// <summary>
/// Provides validation helpers for domain text value objects.
/// </summary>
internal static class DomainText
{
    /// <summary>
    /// Validates a required text value.
    /// </summary>
    /// <param name="value">The text value.</param>
    /// <param name="parameterName">The validated parameter name.</param>
    /// <param name="maxLength">The maximum allowed length.</param>
    /// <param name="displayName">The display name used in exception messages.</param>
    /// <returns>The validated text value.</returns>
    public static string Required(string value, string parameterName, int maxLength, string displayName)
    {
        ArgumentNullException.ThrowIfNull(value);

        var normalized = value.Trim();
        if (normalized.Length == 0)
        {
            throw new ArgumentException($"{displayName} cannot be empty or whitespace.", parameterName);
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"{displayName} cannot be longer than {maxLength} characters.",
                parameterName);
        }

        return normalized;
    }

    /// <summary>
    /// Validates an optional text value.
    /// </summary>
    /// <param name="value">The optional text value.</param>
    /// <param name="parameterName">The validated parameter name.</param>
    /// <param name="maxLength">The maximum allowed length.</param>
    /// <param name="displayName">The display name used in exception messages.</param>
    /// <returns>The validated text value, or <see langword="null"/> when no value was provided.</returns>
    public static string? Optional(string? value, string parameterName, int maxLength, string displayName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"{displayName} cannot be longer than {maxLength} characters.",
                parameterName);
        }

        return normalized;
    }
}
