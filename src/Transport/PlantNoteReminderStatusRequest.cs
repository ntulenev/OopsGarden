namespace Transport;

/// <summary>
/// Represents a plant note reminder status request.
/// </summary>
/// <param name="IsResolved">A value indicating whether the reminder is resolved.</param>
public sealed record PlantNoteReminderStatusRequest(bool IsResolved);
