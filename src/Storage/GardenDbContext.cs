using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Models;

namespace Storage;

/// <summary>
/// Provides EF Core access to OopsGarden data.
/// </summary>
/// <param name="options">The context options.</param>
public sealed class GardenDbContext(DbContextOptions<GardenDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the application users.
    /// </summary>
    public DbSet<AppUser> Users => Set<AppUser>();

    /// <summary>
    /// Gets the invite links.
    /// </summary>
    public DbSet<InviteLink> Invites => Set<InviteLink>();

    /// <summary>
    /// Gets the garden locations.
    /// </summary>
    public DbSet<Location> Locations => Set<Location>();

    /// <summary>
    /// Gets the plants.
    /// </summary>
    public DbSet<Plant> Plants => Set<Plant>();

    /// <summary>
    /// Gets the watering events.
    /// </summary>
    public DbSet<WateringEvent> WateringEvents => Set<WateringEvent>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.Entity<AppUser>(entity =>
        {
            _ = entity.Property(user => user.Id)
                .HasConversion(id => id.Value, value => UserId.From(value));
            _ = entity.Property(user => user.Email)
                .HasConversion(email => email.Value, value => UserEmail.From(value));
            _ = entity.Property(user => user.DisplayName)
                .HasConversion(name => name.Value, value => DisplayName.From(value));
            _ = entity.Property(user => user.PasswordHash)
                .HasConversion(hash => hash.Value, value => PasswordHash.From(value));
            _ = entity.Property(user => user.Language)
                .HasConversion(language => language.Value, value => LanguageCode.From(value));
            _ = entity.Property(user => user.AvatarDataUrl)
                .HasConversion(
                    image => image == null ? null : image.Value.Value,
                    value => ImageDataUrl.Avatar(value));
            _ = entity.HasIndex(user => user.Email).IsUnique();
            _ = entity.Property(user => user.Email).HasMaxLength(256);
            _ = entity.Property(user => user.DisplayName).HasMaxLength(120);
            _ = entity.Property(user => user.Language).HasMaxLength(8);
            _ = entity.Property(user => user.AvatarDataUrl).HasMaxLength(1_000_000);
            _ = entity.Property(user => user.IsGardenPublic).HasDefaultValue(false);
        });

        _ = modelBuilder.Entity<InviteLink>(entity =>
        {
            _ = entity.Property(invite => invite.Id)
                .HasConversion(id => id.Value, value => InviteId.From(value));
            _ = entity.Property(invite => invite.Code)
                .HasConversion(code => code.Value, value => InviteCode.From(value));
            _ = entity.Property(invite => invite.CreatedBy)
                .HasConversion(name => name.Value, value => AdminName.From(value));
            _ = entity.Property(invite => invite.UsedByUserId)
                .HasConversion(new ValueConverter<UserId?, Guid?>(
                    id => id.HasValue ? id.Value.Value : null,
                    value => value.HasValue ? UserId.From(value.Value) : null));
            _ = entity.HasIndex(invite => invite.Code).IsUnique();
            _ = entity.Property(invite => invite.Code).HasMaxLength(48);
            _ = entity.Property(invite => invite.CreatedBy).HasMaxLength(120);
        });

        _ = modelBuilder.Entity<Location>(entity =>
        {
            _ = entity.Property(location => location.Id)
                .HasConversion(id => id.Value, value => LocationId.From(value));
            _ = entity.Property(location => location.UserId)
                .HasConversion(id => id.Value, value => UserId.From(value));
            _ = entity.Property(location => location.Name)
                .HasConversion(name => name.Value, value => LocationName.From(value));
            _ = entity.Property(location => location.Name).HasMaxLength(120);
            _ = entity.HasOne(location => location.User)
                .WithMany(user => user.Locations)
                .HasForeignKey(location => location.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        _ = modelBuilder.Entity<Plant>(entity =>
        {
            _ = entity.Property(plant => plant.Id)
                .HasConversion(id => id.Value, value => PlantId.From(value));
            _ = entity.Property(plant => plant.UserId)
                .HasConversion(id => id.Value, value => UserId.From(value));
            _ = entity.Property(plant => plant.LocationId)
                .HasConversion(new ValueConverter<LocationId?, Guid?>(
                    id => id.HasValue ? id.Value.Value : null,
                    value => value.HasValue ? LocationId.From(value.Value) : null));
            _ = entity.Property(plant => plant.Name)
                .HasConversion(name => name.Value, value => PlantName.From(value));
            _ = entity.Property(plant => plant.Description)
                .HasConversion(description => description.Value, value => PlantDescription.From(value));
            _ = entity.Property(plant => plant.PhotoDataUrl)
                .HasConversion(
                    image => image == null ? null : image.Value.Value,
                    value => ImageDataUrl.PlantPhoto(value));
            _ = entity.Property(plant => plant.Name).HasMaxLength(160);
            _ = entity.Property(plant => plant.Description).HasMaxLength(2_000);
            _ = entity.Property(plant => plant.PhotoDataUrl).HasMaxLength(1_500_000);
            _ = entity.HasOne(plant => plant.User)
                .WithMany(user => user.Plants)
                .HasForeignKey(plant => plant.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            _ = entity.HasOne(plant => plant.Location)
                .WithMany(location => location.Plants)
                .HasForeignKey(plant => plant.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        _ = modelBuilder.Entity<WateringEvent>(entity =>
        {
            _ = entity.Property(watering => watering.Id)
                .HasConversion(id => id.Value, value => WateringEventId.From(value));
            _ = entity.Property(watering => watering.PlantId)
                .HasConversion(id => id.Value, value => PlantId.From(value));
            _ = entity.HasOne(watering => watering.Plant)
                .WithMany(plant => plant.WateringEvents)
                .HasForeignKey(watering => watering.PlantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
