using Abstractions.UseCases;

using Models;

using Transport;

namespace OopsGarden.Endpoints;

/// <summary>
/// Maps garden location endpoints.
/// </summary>
internal static class GardenLocationEndpoints
{
    /// <summary>
    /// Maps garden location HTTP endpoints.
    /// </summary>
    /// <param name="group">The garden route group.</param>
    public static void MapGardenLocationEndpoints(this RouteGroupBuilder group)
    {
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
                    .ExecuteAsync(http.User.CurrentUserId(), LocationId.From(id), request.ToCommand(), cancellationToken)
                    .ConfigureAwait(false);
                return location is null ? Results.NotFound() : Results.Ok(location.ToResponse());
            });

        group.MapDelete(
            "/locations/{id:guid}",
            async (Guid id, IDeleteLocationUseCase useCase, HttpContext http, CancellationToken cancellationToken) =>
                await useCase.ExecuteAsync(http.User.CurrentUserId(), LocationId.From(id), cancellationToken).ConfigureAwait(false)
                    ? Results.NoContent()
                    : Results.NotFound());
    }
}
