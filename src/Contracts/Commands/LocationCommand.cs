namespace Contracts.Commands;

/// <summary>
/// Represents editable location input.
/// </summary>
/// <param name="Name">The location name.</param>
public sealed record LocationCommand(string Name);
