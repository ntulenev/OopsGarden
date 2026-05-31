using Models;

using Transport;

namespace OopsGarden.Endpoints;

/// <summary>
/// Maps shared endpoint request and application models.
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
            request.Soil,
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
        return new CreatePlantNoteCommand(request.Text, request.IsAutomatic, request.IsReminder, request.ReminderDate);
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

}
