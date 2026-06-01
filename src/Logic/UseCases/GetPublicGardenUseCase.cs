using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IGetPublicGardenUseCase" />
public sealed class GetPublicGardenUseCase : IGetPublicGardenUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPublicGardenUseCase"/> class.
    /// </summary>
    /// <param name="publicGardenQueries">The public garden query port.</param>
    public GetPublicGardenUseCase(IPublicGardenQueries publicGardenQueries)
    {
        ArgumentNullException.ThrowIfNull(publicGardenQueries);
        _publicGardenQueries = publicGardenQueries;
    }

    /// <inheritdoc />
    public async Task<PublicGarden?> ExecuteAsync(UserId id, CancellationToken cancellationToken)
    {
        var garden = await _publicGardenQueries
            .GetPublicGardenAsync(id, cancellationToken)
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
                        plant.Soil,
                        plant.PhotoData,
                        plant.PlantedOn,
                        plant.LastWateredAt,
                        GardenUseCaseMapping.ToGardenPlantLocation(plant.Location)))]);
    }

    private readonly IPublicGardenQueries _publicGardenQueries;
}
