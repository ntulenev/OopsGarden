using Abstractions.Repositories;
using Abstractions.System;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IUpdatePlantUseCase" />
public sealed class UpdatePlantUseCase : IUpdatePlantUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlantUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    /// <param name="clock">The application clock.</param>
    public UpdatePlantUseCase(IUnitOfWork unitOfWork, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(clock);
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<UpdatePlantResult> ExecuteAsync(
        UserId userId,
        PlantId id,
        PlantCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var plant = await _unitOfWork.Plants.FindPlantAsync(userId, id, cancellationToken).ConfigureAwait(false);
        if (plant is null)
        {
            return new UpdatePlantResult(UpdatePlantStatus.NotFound, null);
        }

        var locationResult = await GardenUseCaseMapping
            .ResolveLocationIdAsync(_unitOfWork, userId, command.LocationId, cancellationToken)
            .ConfigureAwait(false);
        if (!locationResult.IsSuccess)
        {
            return new UpdatePlantResult(UpdatePlantStatus.Invalid, locationResult.Error);
        }

        var changeNotes = await CreateChangeNotesAsync(plant, command, locationResult.LocationId, userId, cancellationToken)
            .ConfigureAwait(false);
        var previousPhotoData = plant.PhotoDataUrl?.Value;
        plant.UpdateDetails(
            PlantName.From(command.Name),
            PlantDescription.From(command.Description),
            PlantSoil.From(command.Soil),
            locationResult.LocationId,
            command.PlantedOn,
            command.PhotoData);
        if (command.LastWateredOn.HasValue)
        {
            var wateredAt = new DateTimeOffset(command.LastWateredOn.Value.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);
            await _unitOfWork.Plants
                .AddWateringEventAsync(plant.Water(wateredAt), cancellationToken)
                .ConfigureAwait(false);
        }

        if (plant.PhotoDataUrl is { } photoDataUrl && photoDataUrl.Value != previousPhotoData)
        {
            await _unitOfWork.Plants
                .AddPlantPhotoAsync(plant.Id, photoDataUrl, _clock.UtcNow, cancellationToken)
                .ConfigureAwait(false);
        }

        foreach (var noteText in changeNotes)
        {
            var note = plant.AddNote(PlantNoteText.From(noteText), true, _clock.UtcNow);
            await _unitOfWork.Plants.AddPlantNoteAsync(note, cancellationToken).ConfigureAwait(false);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new UpdatePlantResult(UpdatePlantStatus.Updated, null);
    }

    private async Task<IReadOnlyList<string>> CreateChangeNotesAsync(
        Plant plant,
        PlantCommand command,
        LocationId? nextLocationId,
        UserId userId,
        CancellationToken cancellationToken)
    {
        var notes = new List<string>();
        AddChangeNote(notes, "Name", plant.Name.Value, command.Name);
        AddChangeNote(notes, "Description", plant.Description.Value, command.Description);
        AddChangeNote(notes, "Soil", plant.Soil.Value, command.Soil);

        if (plant.LocationId != nextLocationId)
        {
            var previousLocation = await LocationNameAsync(userId, plant.LocationId, cancellationToken).ConfigureAwait(false);
            var nextLocation = await LocationNameAsync(userId, nextLocationId, cancellationToken).ConfigureAwait(false);
            AddChangeNote(notes, "Location", previousLocation, nextLocation);
        }

        return notes;
    }

    private async Task<string> LocationNameAsync(UserId userId, LocationId? locationId, CancellationToken cancellationToken)
    {
        if (locationId is null)
        {
            return "None";
        }

        var location = await _unitOfWork.Locations.FindLocationAsync(userId, locationId.Value, cancellationToken).ConfigureAwait(false);
        return location?.Name.Value ?? "Unknown";
    }

    private static void AddChangeNote(List<string> notes, string field, string previousValue, string nextValue)
    {
        if (previousValue == nextValue)
        {
            return;
        }

        notes.Add(FormattableString.Invariant($"{field} changed \"{previousValue}\" -> \"{nextValue}\""));
    }

    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
}
