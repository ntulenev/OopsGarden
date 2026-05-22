namespace Models;

/// <summary>
/// Represents profile settings input.
/// </summary>
/// <param name="DisplayName">The new display name.</param>
/// <param name="Language">The preferred UI language code.</param>
/// <param name="AvatarData">The optional avatar data URL.</param>
/// <param name="IsGardenPublic">A value indicating whether the garden is public.</param>
public sealed record SettingsCommand(string DisplayName, string Language, string? AvatarData, bool IsGardenPublic);
