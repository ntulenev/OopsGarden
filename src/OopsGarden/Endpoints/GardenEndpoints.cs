using Abstractions;
using Transport;

namespace OopsGarden.Endpoints;

internal static class GardenEndpoints
{
    public static void MapGardenEndpoints(this WebApplication app)
    {
        _ = app.MapGet(
            "/api/public/gardens/{id:guid}",
            async (Guid id, IGetPublicGardenUseCase useCase, CancellationToken cancellationToken) =>
            {
                var garden = await useCase.ExecuteAsync(id, cancellationToken).ConfigureAwait(false);
                return garden is null ? Results.NotFound() : Results.Ok(garden.ToResponse());
            });

        var group = app.MapGroup("/api/garden").RequireAuthorization(policy => policy.RequireRole("User"));

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
                    .ExecuteAsync(http.User.CurrentUserId(), id, cancellationToken)
                    .ConfigureAwait(false);
                return wateredAt is null ? Results.NotFound() : Results.Ok(new { wateredAt });
            });

        group.MapGet(
            "/locations",
            async (IListGardenLocationsUseCase useCase, HttpContext http, CancellationToken cancellationToken) =>
                Results.Ok((await useCase.ExecuteAsync(http.User.CurrentUserId(), cancellationToken).ConfigureAwait(false))
                    .Select(location => location.ToResponse())));

        group.MapPost(
            "/locations",
            async (
                LocationRequest request,
                ICreateLocationUseCase useCase,
                HttpContext http,
                CancellationToken cancellationToken) =>
                Results.Ok((await useCase
                    .ExecuteAsync(http.User.CurrentUserId(), request.ToCommand(), cancellationToken)
                    .ConfigureAwait(false)).ToResponse()));

        group.MapPut(
            "/locations/{id:guid}",
            async (
                Guid id,
                LocationRequest request,
                IRenameLocationUseCase useCase,
                HttpContext http,
                CancellationToken cancellationToken) =>
            {
                var location = await useCase
                    .ExecuteAsync(http.User.CurrentUserId(), id, request.ToCommand(), cancellationToken)
                    .ConfigureAwait(false);
                return location is null ? Results.NotFound() : Results.Ok(location.ToResponse());
            });

        group.MapDelete(
            "/locations/{id:guid}",
            async (Guid id, IDeleteLocationUseCase useCase, HttpContext http, CancellationToken cancellationToken) =>
                await useCase.ExecuteAsync(http.User.CurrentUserId(), id, cancellationToken).ConfigureAwait(false)
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
                    : Results.BadRequest(new { error = result.Error });
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
                    .ExecuteAsync(http.User.CurrentUserId(), id, request.ToCommand(), cancellationToken)
                    .ConfigureAwait(false);
                return result.Status switch
                {
                    UpdatePlantStatus.Updated => Results.Ok(),
                    UpdatePlantStatus.NotFound => Results.NotFound(),
                    UpdatePlantStatus.Invalid => Results.BadRequest(new { error = result.Error }),
                    _ => Results.BadRequest()
                };
            });

        group.MapDelete(
            "/plants/{id:guid}",
            async (Guid id, IDeletePlantUseCase useCase, HttpContext http, CancellationToken cancellationToken) =>
                await useCase.ExecuteAsync(http.User.CurrentUserId(), id, cancellationToken).ConfigureAwait(false)
                    ? Results.NoContent()
                    : Results.NotFound());
    }
}
