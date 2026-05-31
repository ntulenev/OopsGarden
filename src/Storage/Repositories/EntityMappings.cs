using Abstractions.Repositories;

using Models;

using Storage.Entities;

namespace Storage.Repositories;

/// <summary>
/// Maps persistence entities to domain models and back.
/// </summary>
internal static class EntityMappings
{
    /// <summary>
    /// Converts a user entity to a domain user.
    /// </summary>
    /// <param name="entity">The user entity.</param>
    /// <returns>The domain user.</returns>
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

    /// <summary>
    /// Converts a domain user to a user entity.
    /// </summary>
    /// <param name="user">The domain user.</param>
    /// <returns>The user entity.</returns>
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

    /// <summary>
    /// Copies domain user values to an existing user entity.
    /// </summary>
    /// <param name="user">The domain user.</param>
    /// <param name="entity">The target user entity.</param>
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

    /// <summary>
    /// Converts an invite entity to a domain invite.
    /// </summary>
    /// <param name="entity">The invite entity.</param>
    /// <returns>The domain invite.</returns>
    public static InviteLink ToDomain(this InviteLinkEntity entity) =>
        InviteLink.Restore(
            InviteId.From(entity.Id),
            InviteCode.From(entity.Code),
            entity.CreatedAt,
            AdminName.From(entity.CreatedBy),
            entity.UsedAt,
            entity.UsedByUserId.HasValue ? UserId.From(entity.UsedByUserId.Value) : null,
            entity.IsRevoked);

    /// <summary>
    /// Converts a domain invite to an invite entity.
    /// </summary>
    /// <param name="invite">The domain invite.</param>
    /// <returns>The invite entity.</returns>
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

    /// <summary>
    /// Copies domain invite values to an existing invite entity.
    /// </summary>
    /// <param name="invite">The domain invite.</param>
    /// <param name="entity">The target invite entity.</param>
    public static void CopyTo(this InviteLink invite, InviteLinkEntity entity)
    {
        entity.Code = invite.Code.Value;
        entity.CreatedAt = invite.CreatedAt;
        entity.CreatedBy = invite.CreatedBy.Value;
        entity.UsedAt = invite.UsedAt;
        entity.UsedByUserId = invite.UsedByUserId?.Value;
        entity.IsRevoked = invite.IsRevoked;
    }

    /// <summary>
    /// Converts a location entity to a domain location.
    /// </summary>
    /// <param name="entity">The location entity.</param>
    /// <returns>The domain location.</returns>
    public static Location ToDomain(this LocationEntity entity) =>
        Location.Restore(
            LocationId.From(entity.Id),
            UserId.From(entity.UserId),
            LocationName.From(entity.Name));

    /// <summary>
    /// Converts a domain location to a location entity.
    /// </summary>
    /// <param name="location">The domain location.</param>
    /// <returns>The location entity.</returns>
    public static LocationEntity ToEntity(this Location location) =>
        new()
        {
            Id = location.Id.Value,
            UserId = location.UserId.Value,
            Name = location.Name.Value
        };

    /// <summary>
    /// Copies domain location values to an existing location entity.
    /// </summary>
    /// <param name="location">The domain location.</param>
    /// <param name="entity">The target location entity.</param>
    public static void CopyTo(this Location location, LocationEntity entity)
    {
        entity.UserId = location.UserId.Value;
        entity.Name = location.Name.Value;
    }

    /// <summary>
    /// Converts a plant entity to a domain plant.
    /// </summary>
    /// <param name="entity">The plant entity.</param>
    /// <returns>The domain plant.</returns>
    public static Plant ToDomain(this PlantEntity entity) =>
        Plant.Restore(
            PlantId.From(entity.Id),
            UserId.From(entity.UserId),
            PlantName.From(entity.Name),
            PlantDescription.From(entity.Description),
            PlantSoil.From(entity.Soil),
            entity.LocationId.HasValue ? LocationId.From(entity.LocationId.Value) : null,
            entity.PlantedOn,
            ImageDataUrl.PlantPhoto(entity.PhotoData),
            entity.CreatedAt);

    /// <summary>
    /// Converts a domain plant to a plant entity.
    /// </summary>
    /// <param name="plant">The domain plant.</param>
    /// <returns>The plant entity.</returns>
    public static PlantEntity ToEntity(this Plant plant) =>
        new()
        {
            Id = plant.Id.Value,
            UserId = plant.UserId.Value,
            LocationId = plant.LocationId?.Value,
            Name = plant.Name.Value,
            Description = plant.Description.Value,
            Soil = plant.Soil.Value,
            PhotoData = plant.PhotoDataUrl?.Value,
            PlantedOn = plant.PlantedOn,
            CreatedAt = plant.CreatedAt
        };

    /// <summary>
    /// Copies domain plant values to an existing plant entity.
    /// </summary>
    /// <param name="plant">The domain plant.</param>
    /// <param name="entity">The target plant entity.</param>
    public static void CopyTo(this Plant plant, PlantEntity entity)
    {
        entity.UserId = plant.UserId.Value;
        entity.LocationId = plant.LocationId?.Value;
        entity.Name = plant.Name.Value;
        entity.Description = plant.Description.Value;
        entity.Soil = plant.Soil.Value;
        entity.PhotoData = plant.PhotoDataUrl?.Value;
        entity.PlantedOn = plant.PlantedOn;
        entity.CreatedAt = plant.CreatedAt;
    }

    /// <summary>
    /// Converts a domain watering event to a watering event entity.
    /// </summary>
    /// <param name="watering">The domain watering event.</param>
    /// <returns>The watering event entity.</returns>
    public static WateringEventEntity ToEntity(this WateringEvent watering) =>
        new()
        {
            Id = watering.Id.Value,
            PlantId = watering.PlantId.Value,
            WateredAt = watering.WateredAt
        };

    /// <summary>
    /// Converts a domain plant note to a plant note entity.
    /// </summary>
    /// <param name="note">The domain note.</param>
    /// <returns>The note entity.</returns>
    public static PlantNoteEntity ToEntity(this PlantNote note) =>
        new()
        {
            Id = note.Id.Value,
            PlantId = note.PlantId.Value,
            Text = note.Text.Value,
            IsAutomatic = note.IsAutomatic,
            CreatedAt = note.CreatedAt,
            IsReminder = note.Reminder.IsReminder,
            ReminderDate = note.Reminder.ReminderDate,
            IsReminderResolved = note.Reminder.IsResolved
        };
}
