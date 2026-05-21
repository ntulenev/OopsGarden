using System.Collections.ObjectModel;

namespace OopsGarden.Configuration;

internal sealed class AdminOptions
{
    public Collection<AdminCredential> Users { get; } = [];
}

internal sealed class AdminCredential
{
    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
