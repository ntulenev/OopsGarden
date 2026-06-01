using Abstractions.Repositories;

using Models;

namespace Storage.Repositories;

/// <summary>
/// Provides EF Core garden read queries.
/// </summary>
public sealed class GardenQueries : IGardenQueries
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GardenQueries"/> class.
    /// </summary>
    public GardenQueries(GardenDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _publicGardenQueries = new PublicGardenQueries(dbContext);
        _gardenPlantQueries = new GardenPlantQueries(dbContext);
        _plantNoteQueries = new PlantNoteQueries(dbContext);
        _plantHistoryQueries = new PlantHistoryQueries(dbContext);
    }

    /// <inheritdoc />
    public Task<PublicGardenProjection?> GetPublicGardenAsync(UserId userId, CancellationToken cancellationToken) =>
        _publicGardenQueries.GetPublicGardenAsync(userId, cancellationToken);

    /// <inheritdoc />
    public Task<bool> PublicPlantExistsAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken) =>
        _publicGardenQueries.PublicPlantExistsAsync(userId, plantId, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<GardenPlantProjection>> ListPlantsAsync(UserId userId, CancellationToken cancellationToken) =>
        _gardenPlantQueries.ListPlantsAsync(userId, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<GardenPlantProjection>> ListPlantsAsync(
        UserId userId,
        DateOnly today,
        CancellationToken cancellationToken) =>
        _gardenPlantQueries.ListPlantsAsync(userId, today, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<GardenLocationProjection>> ListLocationsAsync(UserId userId, CancellationToken cancellationToken) =>
        _gardenPlantQueries.ListLocationsAsync(userId, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<PlantNoteProjection>> ListPlantNotesAsync(
        UserId userId,
        PlantId plantId,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        _plantNoteQueries.ListPlantNotesAsync(userId, plantId, skip, take, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<PlantNoteProjection>> ListOverduePlantRemindersAsync(
        UserId userId,
        PlantId plantId,
        DateOnly today,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        _plantNoteQueries.ListOverduePlantRemindersAsync(userId, plantId, today, skip, take, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<PlantHistoryItemProjection>> ListPlantHistoryAsync(
        UserId userId,
        PlantId plantId,
        CancellationToken cancellationToken) =>
        _plantHistoryQueries.ListPlantHistoryAsync(userId, plantId, cancellationToken);

    /// <inheritdoc />
    public Task<int> CountPlantNotesAsync(UserId userId, PlantId plantId, CancellationToken cancellationToken) =>
        _plantNoteQueries.CountPlantNotesAsync(userId, plantId, cancellationToken);

    /// <inheritdoc />
    public Task<int> CountOverduePlantRemindersAsync(
        UserId userId,
        PlantId plantId,
        DateOnly today,
        CancellationToken cancellationToken) =>
        _plantNoteQueries.CountOverduePlantRemindersAsync(userId, plantId, today, cancellationToken);

    private readonly PublicGardenQueries _publicGardenQueries;
    private readonly GardenPlantQueries _gardenPlantQueries;
    private readonly PlantNoteQueries _plantNoteQueries;
    private readonly PlantHistoryQueries _plantHistoryQueries;
}
