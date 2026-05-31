using Microsoft.EntityFrameworkCore;

using Storage.Entities;

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
    public DbSet<AppUserEntity> Users => Set<AppUserEntity>();

    /// <summary>
    /// Gets the invite links.
    /// </summary>
    public DbSet<InviteLinkEntity> Invites => Set<InviteLinkEntity>();

    /// <summary>
    /// Gets the garden locations.
    /// </summary>
    public DbSet<LocationEntity> Locations => Set<LocationEntity>();

    /// <summary>
    /// Gets the plants.
    /// </summary>
    public DbSet<PlantEntity> Plants => Set<PlantEntity>();

    /// <summary>
    /// Gets the watering events.
    /// </summary>
    public DbSet<WateringEventEntity> WateringEvents => Set<WateringEventEntity>();

    /// <summary>
    /// Gets the plant notes.
    /// </summary>
    public DbSet<PlantNoteEntity> PlantNotes => Set<PlantNoteEntity>();

    /// <summary>
    /// Gets the plant photo history.
    /// </summary>
    public DbSet<PlantPhotoEntity> PlantPhotos => Set<PlantPhotoEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.Entity<AppUserEntity>(entity =>
        {
            _ = entity.ToTable("Users");
            _ = entity.HasIndex(user => user.Email).IsUnique();
            _ = entity.Property(user => user.Email).HasMaxLength(256);
            _ = entity.Property(user => user.DisplayName).HasMaxLength(120);
            _ = entity.Property(user => user.Language).HasMaxLength(8);
            _ = entity.Property(user => user.AvatarData).HasColumnName("AvatarDataUrl").HasMaxLength(1_000_000);
            _ = entity.Property(user => user.IsGardenPublic).HasDefaultValue(false);
        });

        _ = modelBuilder.Entity<InviteLinkEntity>(entity =>
        {
            _ = entity.ToTable("Invites");
            _ = entity.HasIndex(invite => invite.Code).IsUnique();
            _ = entity.Property(invite => invite.Code).HasMaxLength(48);
            _ = entity.Property(invite => invite.CreatedBy).HasMaxLength(120);
        });

        _ = modelBuilder.Entity<LocationEntity>(entity =>
        {
            _ = entity.ToTable("Locations");
            _ = entity.HasIndex(location => new { location.UserId, location.Name });
            _ = entity.Property(location => location.Name).HasMaxLength(120);
            _ = entity.HasOne(location => location.User)
                .WithMany(user => user.Locations)
                .HasForeignKey(location => location.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        _ = modelBuilder.Entity<PlantEntity>(entity =>
        {
            _ = entity.ToTable("Plants");
            _ = entity.HasIndex(plant => new { plant.UserId, plant.Name });
            _ = entity.HasIndex(plant => new { plant.UserId, plant.LocationId });
            _ = entity.Property(plant => plant.Name).HasMaxLength(160);
            _ = entity.Property(plant => plant.Description).HasMaxLength(2_000);
            _ = entity.Property(plant => plant.Soil).HasMaxLength(2_000);
            _ = entity.Property(plant => plant.PhotoData).HasColumnName("PhotoDataUrl").HasMaxLength(1_500_000);
            _ = entity.HasOne(plant => plant.User)
                .WithMany(user => user.Plants)
                .HasForeignKey(plant => plant.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            _ = entity.HasOne(plant => plant.Location)
                .WithMany(location => location.Plants)
                .HasForeignKey(plant => plant.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        _ = modelBuilder.Entity<WateringEventEntity>(entity =>
        {
            _ = entity.ToTable("WateringEvents");
            _ = entity.HasIndex(watering => new { watering.PlantId, watering.WateredAt });
            _ = entity.HasOne(watering => watering.Plant)
                .WithMany(plant => plant.WateringEvents)
                .HasForeignKey(watering => watering.PlantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        _ = modelBuilder.Entity<PlantNoteEntity>(entity =>
        {
            _ = entity.ToTable("PlantNotes");
            _ = entity.Property(note => note.Text).HasMaxLength(2_000);
            _ = entity.Property(note => note.IsAutomatic).HasDefaultValue(false);
            _ = entity.HasIndex(note => new { note.PlantId, note.CreatedAt, note.Id });
            _ = entity.HasOne(note => note.Plant)
                .WithMany(plant => plant.Notes)
                .HasForeignKey(note => note.PlantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        _ = modelBuilder.Entity<PlantPhotoEntity>(entity =>
        {
            _ = entity.ToTable("PlantPhotos");
            _ = entity.Property(photo => photo.PhotoData).HasColumnName("PhotoDataUrl").HasMaxLength(1_500_000);
            _ = entity.HasIndex(photo => new { photo.PlantId, photo.UploadedAt, photo.Id });
            _ = entity.HasOne(photo => photo.Plant)
                .WithMany(plant => plant.Photos)
                .HasForeignKey(photo => photo.PlantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
