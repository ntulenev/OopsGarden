using Abstractions.UseCases;

using Models;

using Transport;

namespace OopsGarden.Endpoints;

/// <summary>
/// Maps garden plant endpoints.
/// </summary>
internal static class GardenPlantEndpoints
{
    /// <summary>
    /// Maps garden plant HTTP endpoints.
    /// </summary>
    /// <param name="group">The garden route group.</param>
    public static void MapGardenPlantEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/summary",
            async (IListGardenPlantsUseCase useCase, HttpContext http, CancellationToken cancellationToken) =>
                Results.Ok((await useCase.ExecuteAsync(http.User.CurrentUserId(), cancellationToken).ConfigureAwait(false))
                    .Select(plant => plant.ToResponse())));

        group.MapPost(
            "/plants/{id:guid}/water",
            async (Guid id, IWaterPlantUseCase useCase, HttpContext http, CancellationToken cancellationToken) =>
            {
                var wateredAt = await useCase
                    .ExecuteAsync(http.User.CurrentUserId(), PlantId.From(id), cancellationToken)
                    .ConfigureAwait(false);
                return wateredAt is null ? Results.NotFound() : Results.Ok(new { wateredAt });
            });

        group.MapPost(
            "/plants/{id:guid}/waterings",
            async (
                Guid id,
                PlantWateringRequest request,
                IWaterPlantUseCase useCase,
                HttpContext http,
                CancellationToken cancellationToken) =>
            {
                var wateredAt = await useCase
                    .ExecuteAsync(http.User.CurrentUserId(), PlantId.From(id), request.WateredOn, cancellationToken)
                    .ConfigureAwait(false);
                return wateredAt is null ? Results.NotFound() : Results.Ok(new { wateredAt });
            });

        group.MapGet(
            "/plants/{id:guid}/history",
            async (
                Guid id,
                IListPlantHistoryUseCase useCase,
                HttpContext http,
                CancellationToken cancellationToken) =>
            {
                var history = await useCase
                    .ExecuteAsync(http.User.CurrentUserId(), PlantId.From(id), cancellationToken)
                    .ConfigureAwait(false);
                return history is null ? Results.NotFound() : Results.Ok(history.Select(item => item.ToResponse()));
            });

        group.MapDelete(
            "/plants/{plantId:guid}/waterings/{wateringId:guid}",
            async (
                Guid plantId,
                Guid wateringId,
                IDeleteWateringEventUseCase useCase,
                HttpContext http,
                CancellationToken cancellationToken) =>
                (await useCase
                    .ExecuteAsync(
                        http.User.CurrentUserId(),
                        PlantId.From(plantId),
                        WateringEventId.From(wateringId),
                        cancellationToken)
                    .ConfigureAwait(false)).IsSuccess
                        ? Results.NoContent()
                        : Results.NotFound());

        group.MapDelete(
            "/plants/{plantId:guid}/photos/{photoId:guid}",
            async (
                Guid plantId,
                Guid photoId,
                IDeletePlantPhotoUseCase useCase,
                HttpContext http,
                CancellationToken cancellationToken) =>
                (await useCase
                    .ExecuteAsync(
                        http.User.CurrentUserId(),
                        PlantId.From(plantId),
                        photoId,
                        cancellationToken)
                    .ConfigureAwait(false)).IsSuccess
                        ? Results.NoContent()
                        : Results.NotFound());

        group.MapGet(
            "/plants",
            async (IListGardenPlantsUseCase useCase, HttpContext http, CancellationToken cancellationToken) =>
                Results.Ok((await useCase.ExecuteAsync(http.User.CurrentUserId(), cancellationToken).ConfigureAwait(false))
                    .Select(plant => plant.ToResponse())));

        group.MapPost(
            "/plants",
            async (
                PlantRequest request,
                ICreatePlantUseCase useCase,
                HttpContext http,
                CancellationToken cancellationToken) =>
            {
                var result = await useCase
                    .ExecuteAsync(http.User.CurrentUserId(), request.ToCommand(), cancellationToken)
                    .ConfigureAwait(false);
                return result.IsSuccess
                    ? Results.Ok(new { id = result.Id })
                    : Results.BadRequest(new { error = result.ErrorMessage });
            });

        group.MapPut(
            "/plants/{id:guid}",
            async (
                Guid id,
                PlantRequest request,
                IUpdatePlantUseCase useCase,
                HttpContext http,
                CancellationToken cancellationToken) =>
            {
                var result = await useCase
                    .ExecuteAsync(http.User.CurrentUserId(), PlantId.From(id), request.ToCommand(), cancellationToken)
                    .ConfigureAwait(false);
                return result.Status switch
                {
                    CommandStatus.Succeeded => Results.Ok(),
                    CommandStatus.NotFound => Results.NotFound(),
                    CommandStatus.Invalid => Results.BadRequest(new { error = result.ErrorMessage }),
                    _ => Results.BadRequest()
                };
            });

        group.MapDelete(
            "/plants/{id:guid}",
            async (Guid id, IDeletePlantUseCase useCase, HttpContext http, CancellationToken cancellationToken) =>
                (await useCase.ExecuteAsync(http.User.CurrentUserId(), PlantId.From(id), cancellationToken).ConfigureAwait(false)).IsSuccess
                    ? Results.NoContent()
                    : Results.NotFound());
    }
}
