using Abstractions.UseCases;

using Models;

namespace OopsGarden.Endpoints;

/// <summary>
/// Maps public garden endpoints.
/// </summary>
internal static class PublicGardenEndpoints
{
    /// <summary>
    /// Maps public garden HTTP endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapPublicGardenEndpoints(this WebApplication app)
    {
        _ = app.MapGet(
            "/api/public/gardens/{id:guid}",
            async (Guid id, IGetPublicGardenUseCase useCase, CancellationToken cancellationToken) =>
            {
                var garden = await useCase.ExecuteAsync(UserId.From(id), cancellationToken).ConfigureAwait(false);
                return garden is null ? Results.NotFound() : Results.Ok(garden.ToResponse());
            });

        _ = app.MapGet(
            "/api/public/gardens/{gardenId:guid}/plants/{plantId:guid}/notes",
            async (
                Guid gardenId,
                Guid plantId,
                int? page,
                int? pageSize,
                IListPublicPlantNotesUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                var notes = await useCase
                    .ExecuteAsync(UserId.From(gardenId), PlantId.From(plantId), page ?? 1, pageSize ?? 5, cancellationToken)
                    .ConfigureAwait(false);
                return notes is null ? Results.NotFound() : Results.Ok(notes.ToResponse());
            });

        _ = app.MapGet(
            "/api/public/gardens/{gardenId:guid}/plants/{plantId:guid}/history",
            async (
                Guid gardenId,
                Guid plantId,
                IListPublicPlantHistoryUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                var history = await useCase
                    .ExecuteAsync(UserId.From(gardenId), PlantId.From(plantId), cancellationToken)
                    .ConfigureAwait(false);
                return history is null ? Results.NotFound() : Results.Ok(history.Select(item => item.ToResponse()));
            });
    }
}
