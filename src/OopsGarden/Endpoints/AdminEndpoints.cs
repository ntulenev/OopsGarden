using Abstractions.UseCases;

using Models;

using Transport;

namespace OopsGarden.Endpoints;

/// <summary>
/// Maps administration endpoints.
/// </summary>
internal static class AdminEndpoints
{
    /// <summary>
    /// Maps administration HTTP endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin").RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapGet(
            "/invites",
            async (IListInvitesUseCase useCase, CancellationToken cancellationToken) =>
                Results.Ok((await useCase.ExecuteAsync(cancellationToken).ConfigureAwait(false))
                    .Select(invite => invite.ToResponse())));

        group.MapPost(
            "/invites",
            async (ICreateInviteUseCase useCase, HttpContext http, CancellationToken cancellationToken) =>
                Results.Ok((await useCase.ExecuteAsync(http.User, cancellationToken).ConfigureAwait(false)).ToResponse()));

        group.MapPost(
            "/invites/{id:guid}/revoke",
            async (Guid id, IRevokeInviteUseCase useCase, CancellationToken cancellationToken) =>
                await useCase.ExecuteAsync(InviteId.From(id), cancellationToken).ConfigureAwait(false)
                    ? Results.Ok()
                    : Results.NotFound());

        group.MapDelete(
            "/invites/{id:guid}",
            async (Guid id, IDeleteInviteUseCase useCase, CancellationToken cancellationToken) =>
            {
                var result = await useCase.ExecuteAsync(InviteId.From(id), cancellationToken).ConfigureAwait(false);
                return result.Status switch
                {
                    DeleteInviteStatus.Deleted => Results.NoContent(),
                    DeleteInviteStatus.NotFound => Results.NotFound(),
                    DeleteInviteStatus.Invalid => Results.BadRequest(new { error = result.Error }),
                    _ => Results.BadRequest()
                };
            });

        group.MapGet(
            "/users",
            async (IListUsersUseCase useCase, CancellationToken cancellationToken) =>
                Results.Ok((await useCase.ExecuteAsync(cancellationToken).ConfigureAwait(false))
                    .Select(user => user.ToResponse())));

        group.MapPost(
            "/users/{id:guid}/block",
            async (
                Guid id,
                BlockUserRequest request,
                IBlockUserUseCase useCase,
                CancellationToken cancellationToken) =>
                await useCase.ExecuteAsync(UserId.From(id), request.IsBlocked, cancellationToken).ConfigureAwait(false)
                    ? Results.Ok()
                    : Results.NotFound());

        group.MapDelete(
            "/users/{id:guid}",
            async (Guid id, IDeleteUserUseCase useCase, CancellationToken cancellationToken) =>
                await useCase.ExecuteAsync(UserId.From(id), cancellationToken).ConfigureAwait(false)
                    ? Results.NoContent()
                    : Results.NotFound());
    }
}
