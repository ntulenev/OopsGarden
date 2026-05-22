using System.Collections.ObjectModel;

namespace OopsGarden.Configuration;

/// <summary>
/// Represents administrator credential configuration.
/// </summary>
internal sealed class AdminOptions
{
    /// <summary>
    /// Gets configured administrator credentials.
    /// </summary>
    public Collection<AdminCredential> Users { get; } = [];
}
