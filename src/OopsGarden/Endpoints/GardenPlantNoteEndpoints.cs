using Abstractions.UseCases;

using Models;

using Transport;

namespace OopsGarden.Endpoints;

/// <summary>
/// Maps garden plant note endpoints.
/// </summary>
internal static class GardenPlantNoteEndpoints
{
    /// <summary>
    /// Maps garden plant note HTTP endpoints.
    /// </summary>
    /// <param name="group">The garden route group.</param>
    public static void MapGardenPlantNoteEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/plants/{id:guid}/notes",
            async (
                Guid id,
                int? page,
                int? pageSize,
                IListPlantNotesUseCase useCase,
                HttpContext http,
                CancellationToken cancellationToken) =>
            {
                var notes = await useCase
                    .ExecuteAsync(http.User.CurrentUserId(), PlantId.From(id), page ?? 1, pageSize ?? 5, cancellationToken)
                    .ConfigureAwait(false);
                return notes is null ? Results.NotFound() : Results.Ok(notes.ToResponse());
            });

        group.MapPost(
            "/plants/{id:guid}/notes",
            async (
                Guid id,
                PlantNoteRequest request,
                ICreatePlantNoteUseCase useCase,
                HttpContext http,
                CancellationToken cancellationToken) =>
            {
                var note = await useCase
                    .ExecuteAsync(http.User.CurrentUserId(), PlantId.From(id), request.ToCommand(), cancellationToken)
                    .ConfigureAwait(false);
                return note is null ? Results.NotFound() : Results.Ok(note.ToResponse());
            });

        group.MapDelete(
            "/plants/{plantId:guid}/notes/{noteId:guid}",
            async (
                Guid plantId,
                Guid noteId,
                IDeletePlantNoteUseCase useCase,
                HttpContext http,
                CancellationToken cancellationToken) =>
                await useCase
                    .ExecuteAsync(
                        http.User.CurrentUserId(),
                        PlantId.From(plantId),
                        PlantNoteId.From(noteId),
                        cancellationToken)
                    .ConfigureAwait(false)
                    ? Results.NoContent()
                    : Results.NotFound());

        group.MapPut(
            "/plants/{plantId:guid}/notes/{noteId:guid}/date",
            async (
                Guid plantId,
                Guid noteId,
                PlantNoteDateRequest request,
                IUpdatePlantNoteDateUseCase useCase,
                HttpContext http,
                CancellationToken cancellationToken) =>
                await useCase
                    .ExecuteAsync(
                        http.User.CurrentUserId(),
                        PlantId.From(plantId),
                        PlantNoteId.From(noteId),
                        request.ToCommand(),
                        cancellationToken)
                    .ConfigureAwait(false)
                    ? Results.NoContent()
                    : Results.NotFound());
    }
}
