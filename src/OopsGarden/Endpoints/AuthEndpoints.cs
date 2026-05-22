using Abstractions.UseCases;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using Transport;

namespace OopsGarden.Endpoints;

/// <summary>
/// Maps authentication endpoints.
/// </summary>
internal static class AuthEndpoints
{
    /// <summary>
    /// Maps authentication HTTP endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/login", async (
            LoginRequest request,
            ILoginUseCase useCase,
            HttpContext http,
            CancellationToken cancellationToken) =>
        {
            var user = await useCase.ExecuteAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            await http.SignInUserAsync(user).ConfigureAwait(false);
            return Results.Ok(user.ToResponse());
        });

        group.MapPost("/admin-login", async (LoginRequest request, IAdminLoginUseCase useCase, HttpContext http) =>
        {
            var admin = useCase.Execute(request.ToCommand());
            if (admin is null)
            {
                return Results.Unauthorized();
            }

            await http.SignInAdminAsync(admin.Name).ConfigureAwait(false);
            return Results.Ok(new AdminLoginResponse(admin.Name, admin.Role));
        });

        group.MapPost("/register", async (
            RegisterRequest request,
            IRegisterUseCase useCase,
            HttpContext http,
            CancellationToken cancellationToken) =>
        {
            var result = await useCase.ExecuteAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            await http.SignInUserAsync(result.User!).ConfigureAwait(false);
            return Results.Ok(result.User!.ToResponse());
        });

        group.MapPost("/settings", async (
            SettingsRequest request,
            IUpdateSettingsUseCase useCase,
            HttpContext http,
            CancellationToken cancellationToken) =>
        {
            if (!http.User.Identity?.IsAuthenticated ?? true)
            {
                return Results.Unauthorized();
            }

            var user = await useCase
                .ExecuteAsync(http.User.CurrentUserId(), request.ToCommand(), cancellationToken)
                .ConfigureAwait(false);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            await http.SignInUserAsync(user).ConfigureAwait(false);
            return Results.Ok(user.ToResponse());
        }).RequireAuthorization();

        group.MapPost("/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            return Results.Ok();
        });
    }
}
