using Transport;

namespace OopsGarden.Endpoints;

/// <summary>
/// Maps garden application models to endpoint responses.
/// </summary>
internal static class GardenEndpointMappings
{
    /// <summary>
    /// Converts a public garden model to a response.
    /// </summary>
    /// <param name="garden">The public garden model.</param>
    /// <returns>The public garden response.</returns>
    public static PublicGardenResponse ToResponse(this PublicGarden garden)
    {
        ArgumentNullException.ThrowIfNull(garden);
        return new PublicGardenResponse(
            garden.Id.Value,
            garden.Name,
            garden.AvatarData,
            [.. garden.Plants.Select(plant => new PublicPlantResponse(
                plant.Id.Value,
                plant.Name,
                plant.Description,
                plant.Soil,
                plant.PhotoData,
                plant.PlantedOn,
                plant.LastWateredAt,
                plant.Location.ToResponse()))]);
    }

    /// <summary>
    /// Converts a plant summary model to a response.
    /// </summary>
    /// <param name="plant">The plant summary model.</param>
    /// <returns>The plant summary response.</returns>
    public static PlantSummaryResponse ToResponse(this PlantSummary plant)
    {
        ArgumentNullException.ThrowIfNull(plant);
        return new PlantSummaryResponse(
            plant.Id.Value,
            plant.Name,
            plant.Description,
            plant.Soil,
            plant.PhotoData,
            plant.PlantedOn,
            plant.Location.ToResponse(),
            plant.LastWateredAt,
            plant.HasOverdueReminders);
    }

    /// <summary>
    /// Converts a plant note model to a response.
    /// </summary>
    /// <param name="note">The plant note model.</param>
    /// <returns>The plant note response.</returns>
    public static PlantNoteResponse ToResponse(this PlantNoteSummary note)
    {
        ArgumentNullException.ThrowIfNull(note);
        return new PlantNoteResponse(
            note.Id.Value,
            note.Text,
            note.CreatedAt,
            note.IsAutomatic,
            note.IsReminder,
            note.ReminderDate,
            note.IsReminderResolved);
    }

    /// <summary>
    /// Converts a plant history item model to a response.
    /// </summary>
    /// <param name="item">The plant history item.</param>
    /// <returns>The plant history item response.</returns>
    public static PlantHistoryItemResponse ToResponse(this PlantHistoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return new PlantHistoryItemResponse(
            item.Id,
            item.Type,
            item.OccurredAt,
            item.Text,
            item.IsAutomatic,
            item.IsReminder,
            item.ReminderDate,
            item.IsReminderResolved,
            item.PhotoDataUrl);
    }

    /// <summary>
    /// Converts a plant notes page to a response.
    /// </summary>
    /// <param name="page">The plant notes page.</param>
    /// <returns>The plant notes page response.</returns>
    public static PlantNotesPageResponse ToResponse(this PlantNotesPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        return new PlantNotesPageResponse(
            [.. page.Items.Select(note => note.ToResponse())],
            page.Page,
            page.PageSize,
            page.Total,
            page.HasPrevious,
            page.HasNext);
    }

    /// <summary>
    /// Converts a location summary model to a response.
    /// </summary>
    /// <param name="location">The location summary model.</param>
    /// <returns>The location summary response.</returns>
    public static LocationSummaryResponse ToResponse(this LocationSummary location)
    {
        ArgumentNullException.ThrowIfNull(location);
        return new LocationSummaryResponse(location.Id.Value, location.Name, location.Plants);
    }

    private static PlantLocationResponse? ToResponse(this GardenPlantLocation? location) =>
        location is null ? null : new PlantLocationResponse(location.Id.Value, location.Name);
}
