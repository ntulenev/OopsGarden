using Abstractions;

using Models;

namespace OopsGarden.UseCases;

/// <inheritdoc cref="IGetPublicGardenUseCase" />
internal sealed class GetPublicGardenUseCase : IGetPublicGardenUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPublicGardenUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public GetPublicGardenUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<PublicGarden?> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var garden = await _unitOfWork.Garden
            .GetPublicGardenAsync(UserId.From(id), cancellationToken)
            .ConfigureAwait(false);

        return garden is null
            ? null
            : new PublicGarden(
                garden.Id,
                garden.Name,
                garden.Avatar,
                [.. garden.Plants
                    .Select(plant => new PublicGardenPlant(
                        plant.Id,
                        plant.Name,
                        plant.Description,
                        plant.PhotoData,
                        plant.PlantedOn,
                        plant.LastWateredAt,
                        GardenUseCaseMapping.ToResponse(plant.Location)))]);
    }

    private readonly IUnitOfWork _unitOfWork;
}
