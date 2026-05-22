namespace OopsGarden.Configuration;

/// <summary>
/// Represents one configured administrator credential.
/// </summary>
internal sealed class AdminCredential
{
    /// <summary>
    /// Gets or sets the administrator user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the administrator password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
