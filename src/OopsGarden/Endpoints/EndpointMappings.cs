using Abstractions;

using Transport;

namespace OopsGarden.Endpoints;

internal static class EndpointMappings
{
    public static LoginCommand ToCommand(this LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new LoginCommand(request.Email, request.Password);
    }

    public static RegisterCommand ToCommand(this RegisterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new RegisterCommand(request.InviteCode, request.DisplayName, request.Email, request.Password, request.Language ?? "ru");
    }

    public static SettingsCommand ToCommand(this SettingsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new SettingsCommand(request.DisplayName, request.Language ?? "ru", request.AvatarDataUrl, request.IsGardenPublic);
    }

    public static LocationCommand ToCommand(this LocationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new LocationCommand(request.Name);
    }

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
                plant.Location.ToResponse()))]);
    }

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

    public static LocationSummaryResponse ToResponse(this LocationSummary location)
    {
        ArgumentNullException.ThrowIfNull(location);
        return new LocationSummaryResponse(location.Id.Value, location.Name, location.Plants);
    }

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

    public static CreatedInviteResponse ToResponse(this CreatedInvite invite)
    {
        ArgumentNullException.ThrowIfNull(invite);
        return new CreatedInviteResponse(invite.Id.Value, invite.Code, invite.Url.ToString());
    }

    private static PlantLocationResponse? ToResponse(this GardenPlantLocation? location) =>
        location is null ? null : new PlantLocationResponse(location.Id.Value, location.Name);
}
