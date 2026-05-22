using System.Collections.ObjectModel;

namespace Logic.Configuration;

/// <summary>
/// Represents administrator credential configuration.
/// </summary>
public sealed class AdminOptions
{
    /// <summary>
    /// Gets configured administrator credentials.
    /// </summary>
    public Collection<AdminCredential> Users { get; } = [];
}
