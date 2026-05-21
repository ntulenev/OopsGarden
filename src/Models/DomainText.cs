namespace Models;

/// <summary>
/// Provides validation helpers for domain text value objects.
/// </summary>
internal static class DomainText
{
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
