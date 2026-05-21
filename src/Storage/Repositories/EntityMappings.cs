using Models;

using Storage.Entities;

namespace Storage.Repositories;

internal static class EntityMappings
{
    public static AppUser ToDomain(this AppUserEntity entity) =>
        AppUser.Restore(
            UserId.From(entity.Id),
            UserEmail.From(entity.Email),
            DisplayName.From(entity.DisplayName),
            PasswordHash.From(entity.PasswordHash),
            LanguageCode.From(entity.Language),
            ImageDataUrl.Avatar(entity.AvatarData),
            entity.IsGardenPublic,
            entity.IsBlocked,
            entity.CreatedAt);

    public static AppUserEntity ToEntity(this AppUser user) =>
        new()
        {
            Id = user.Id.Value,
            Email = user.Email.Value,
            DisplayName = user.DisplayName.Value,
            PasswordHash = user.PasswordHash.Value,
            Language = user.Language.Value,
            AvatarData = user.AvatarDataUrl?.Value,
            IsGardenPublic = user.IsGardenPublic,
            IsBlocked = user.IsBlocked,
            CreatedAt = user.CreatedAt
        };

    public static void CopyTo(this AppUser user, AppUserEntity entity)
    {
        entity.Email = user.Email.Value;
        entity.DisplayName = user.DisplayName.Value;
        entity.PasswordHash = user.PasswordHash.Value;
        entity.Language = user.Language.Value;
        entity.AvatarData = user.AvatarDataUrl?.Value;
        entity.IsGardenPublic = user.IsGardenPublic;
        entity.IsBlocked = user.IsBlocked;
        entity.CreatedAt = user.CreatedAt;
    }

    public static InviteLink ToDomain(this InviteLinkEntity entity) =>
        InviteLink.Restore(
            InviteId.From(entity.Id),
            InviteCode.From(entity.Code),
            entity.CreatedAt,
            AdminName.From(entity.CreatedBy),
            entity.UsedAt,
            entity.UsedByUserId.HasValue ? UserId.From(entity.UsedByUserId.Value) : null,
            entity.IsRevoked);

    public static InviteLinkEntity ToEntity(this InviteLink invite) =>
        new()
        {
            Id = invite.Id.Value,
            Code = invite.Code.Value,
            CreatedAt = invite.CreatedAt,
            CreatedBy = invite.CreatedBy.Value,
            UsedAt = invite.UsedAt,
            UsedByUserId = invite.UsedByUserId?.Value,
            IsRevoked = invite.IsRevoked
        };

    public static void CopyTo(this InviteLink invite, InviteLinkEntity entity)
    {
        entity.Code = invite.Code.Value;
        entity.CreatedAt = invite.CreatedAt;
        entity.CreatedBy = invite.CreatedBy.Value;
        entity.UsedAt = invite.UsedAt;
        entity.UsedByUserId = invite.UsedByUserId?.Value;
        entity.IsRevoked = invite.IsRevoked;
    }

    public static Location ToDomain(this LocationEntity entity) =>
        Location.Restore(
            LocationId.From(entity.Id),
            UserId.From(entity.UserId),
            LocationName.From(entity.Name));

    public static LocationEntity ToEntity(this Location location) =>
        new()
        {
            Id = location.Id.Value,
            UserId = location.UserId.Value,
            Name = location.Name.Value
        };

    public static void CopyTo(this Location location, LocationEntity entity)
    {
        entity.UserId = location.UserId.Value;
        entity.Name = location.Name.Value;
    }

    public static Plant ToDomain(this PlantEntity entity) =>
        Plant.Restore(
            PlantId.From(entity.Id),
            UserId.From(entity.UserId),
            PlantName.From(entity.Name),
            PlantDescription.From(entity.Description),
            entity.LocationId.HasValue ? LocationId.From(entity.LocationId.Value) : null,
            entity.PlantedOn,
            ImageDataUrl.PlantPhoto(entity.PhotoData),
            entity.CreatedAt);

    public static PlantEntity ToEntity(this Plant plant) =>
        new()
        {
            Id = plant.Id.Value,
            UserId = plant.UserId.Value,
            LocationId = plant.LocationId?.Value,
            Name = plant.Name.Value,
            Description = plant.Description.Value,
            PhotoData = plant.PhotoDataUrl?.Value,
            PlantedOn = plant.PlantedOn,
            CreatedAt = plant.CreatedAt
        };

    public static void CopyTo(this Plant plant, PlantEntity entity)
    {
        entity.UserId = plant.UserId.Value;
        entity.LocationId = plant.LocationId?.Value;
        entity.Name = plant.Name.Value;
        entity.Description = plant.Description.Value;
        entity.PhotoData = plant.PhotoDataUrl?.Value;
        entity.PlantedOn = plant.PlantedOn;
        entity.CreatedAt = plant.CreatedAt;
    }

    public static WateringEventEntity ToEntity(this WateringEvent watering) =>
        new()
        {
            Id = watering.Id.Value,
            PlantId = watering.PlantId.Value,
            WateredAt = watering.WateredAt
        };
}
