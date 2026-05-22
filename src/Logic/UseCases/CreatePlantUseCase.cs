using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="ICreatePlantUseCase" />
public sealed class CreatePlantUseCase : ICreatePlantUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlantUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public CreatePlantUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<CreatePlantResult> ExecuteAsync(
        UserId userId,
        PlantCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var locationResult = await GardenUseCaseMapping
            .ResolveLocationIdAsync(_unitOfWork, userId, command.LocationId, cancellationToken)
            .ConfigureAwait(false);
        if (!locationResult.IsSuccess)
        {
            return new CreatePlantResult(null, locationResult.Error);
        }

        var plant = Plant.Create(
            userId,
            PlantName.From(command.Name),
            PlantDescription.From(command.Description),
            locationResult.LocationId,
            command.PlantedOn,
            command.PhotoData);
        await _unitOfWork.Garden.AddPlantAsync(plant, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new CreatePlantResult(plant.Id.Value, null);
    }

    private readonly IUnitOfWork _unitOfWork;
}
