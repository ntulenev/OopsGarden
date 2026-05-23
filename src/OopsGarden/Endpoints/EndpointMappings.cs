using Models;

using Transport;

namespace OopsGarden.Endpoints;

/// <summary>
/// Maps endpoint request and application models.
/// </summary>
internal static class EndpointMappings
{
    /// <summary>
    /// Converts a login request to a login command.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <returns>The login command.</returns>
    public static LoginCommand ToCommand(this LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new LoginCommand(request.Email, request.Password);
    }

    /// <summary>
    /// Converts a registration request to a registration command.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <returns>The registration command.</returns>
    public static RegisterCommand ToCommand(this RegisterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new RegisterCommand(request.InviteCode, request.DisplayName, request.Email, request.Password, request.Language ?? "ru");
    }

    /// <summary>
    /// Converts a settings request to a settings command.
    /// </summary>
    /// <param name="request">The settings request.</param>
    /// <returns>The settings command.</returns>
    public static SettingsCommand ToCommand(this SettingsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new SettingsCommand(request.DisplayName, request.Language ?? "ru", request.AvatarDataUrl, request.IsGardenPublic);
    }

    /// <summary>
    /// Converts a location request to a location command.
    /// </summary>
    /// <param name="request">The location request.</param>
    /// <returns>The location command.</returns>
    public static LocationCommand ToCommand(this LocationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new LocationCommand(request.Name);
    }

    /// <summary>
    /// Converts a plant request to a plant command.
    /// </summary>
    /// <param name="request">The plant request.</param>
    /// <returns>The plant command.</returns>
    public static PlantCommand ToCommand(this PlantRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new PlantCommand(
            request.Name,
            request.Description,
            request.LocationId,
            request.PlantedOn,
            request.LastWateredOn,
            request.PhotoDataUrl);
    }

    /// <summary>
    /// Converts a plant note request to a create note command.
    /// </summary>
    /// <param name="request">The plant note request.</param>
    /// <returns>The create plant note command.</returns>
    public static CreatePlantNoteCommand ToCommand(this PlantNoteRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new CreatePlantNoteCommand(request.Text, request.IsAutomatic);
    }

    /// <summary>
    /// Converts a plant note date request to an update note date command.
    /// </summary>
    /// <param name="request">The plant note date request.</param>
    /// <returns>The update plant note date command.</returns>
    public static UpdatePlantNoteDateCommand ToCommand(this PlantNoteDateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new UpdatePlantNoteDateCommand(request.CreatedOn);
    }

    /// <summary>
    /// Converts an authenticated user model to a response.
    /// </summary>
    /// <param name="user">The authenticated user model.</param>
    /// <returns>The authenticated user response.</returns>
    public static AuthenticatedUserResponse ToResponse(this AuthenticatedUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new AuthenticatedUserResponse(
            user.Id.Value,
            user.DisplayName,
            user.Email,
            user.Language,
            user.AvatarData,
            user.IsGardenPublic);
    }

    /// <summary>
    /// Converts a current user model to a response.
    /// </summary>
    /// <param name="user">The current user model.</param>
    /// <returns>The current session response.</returns>
    public static MeResponse ToResponse(this CurrentUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new MeResponse(
            user.Authenticated,
            user.Id?.Value,
            user.Name,
            user.Role,
            user.Language,
            user.AvatarData,
            user.IsGardenPublic);
    }

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
            plant.PhotoData,
            plant.PlantedOn,
            plant.Location.ToResponse(),
            plant.LastWateredAt);
    }

    /// <summary>
    /// Converts a plant note model to a response.
    /// </summary>
    /// <param name="note">The plant note model.</param>
    /// <returns>The plant note response.</returns>
    public static PlantNoteResponse ToResponse(this PlantNoteSummary note)
    {
        ArgumentNullException.ThrowIfNull(note);
        return new PlantNoteResponse(note.Id.Value, note.Text, note.CreatedAt, note.IsAutomatic);
    }

    /// <summary>
    /// Converts a plant history item model to a response.
    /// </summary>
    /// <param name="item">The plant history item.</param>
    /// <returns>The plant history item response.</returns>
    public static PlantHistoryItemResponse ToResponse(this PlantHistoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return new PlantHistoryItemResponse(item.Id, item.Type, item.OccurredAt, item.Text, item.IsAutomatic);
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

    /// <summary>
    /// Converts an admin invite model to a response.
    /// </summary>
    /// <param name="invite">The admin invite model.</param>
    /// <returns>The admin invite response.</returns>
    public static AdminInviteResponse ToResponse(this AdminInvite invite)
    {
        ArgumentNullException.ThrowIfNull(invite);
        return new AdminInviteResponse(
            invite.Id.Value,
            invite.Code,
            invite.CreatedAt,
            invite.CreatedBy,
            invite.UsedAt,
            invite.UsedByUserId?.Value,
            invite.IsRevoked);
    }

    /// <summary>
    /// Converts an admin user model to a response.
    /// </summary>
    /// <param name="user">The admin user model.</param>
    /// <returns>The admin user response.</returns>
    public static AdminUserResponse ToResponse(this AdminUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new AdminUserResponse(
            user.Id.Value,
            user.DisplayName,
            user.Email,
            user.IsBlocked,
            user.Language,
            user.CreatedAt,
            user.Plants);
    }

    /// <summary>
    /// Converts a created invite model to a response.
    /// </summary>
    /// <param name="invite">The created invite model.</param>
    /// <returns>The created invite response.</returns>
    public static CreatedInviteResponse ToResponse(this CreatedInvite invite)
    {
        ArgumentNullException.ThrowIfNull(invite);
        return new CreatedInviteResponse(invite.Id.Value, invite.Code, invite.Url.ToString());
    }

    private static PlantLocationResponse? ToResponse(this GardenPlantLocation? location) =>
        location is null ? null : new PlantLocationResponse(location.Id.Value, location.Name);
}
