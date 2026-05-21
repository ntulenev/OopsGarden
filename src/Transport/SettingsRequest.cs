namespace Transport;

/// <summary>
/// Represents a user settings update request.
/// </summary>
/// <param name="DisplayName">The display name.</param>
/// <param name="Language">The preferred UI language.</param>
/// <param name="AvatarDataUrl">The avatar image as a browser data URL.</param>
/// <param name="IsGardenPublic">Whether the garden can be viewed through a public link.</param>
public sealed record SettingsRequest(
    string DisplayName,
    string? Language,
    string? AvatarDataUrl,
    bool IsGardenPublic);
