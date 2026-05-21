using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Models;
using OopsGarden.Configuration;
using Storage;
using Transport;

namespace OopsGarden.Endpoints;

internal static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/login", async (LoginRequest request, GardenDbContext db, PasswordHasher<AppUser> hasher, HttpContext http) =>
        {
            var email = UserEmail.From(request.Email);
            var user = await db.Users.SingleOrDefaultAsync(user => user.Email == email);
            if (user is null || user.IsBlocked)
            {
                return Results.Unauthorized();
            }

            var result = hasher.VerifyHashedPassword(user, user.PasswordHash.Value, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return Results.Unauthorized();
            }

            await http.SignInUserAsync(user);
            return Results.Ok(ToUserResponse(user));
        });

        group.MapPost("/admin-login", async (LoginRequest request, IOptions<AdminOptions> options, HttpContext http) =>
        {
            var admin = options.Value.Users.SingleOrDefault(user =>
                string.Equals(user.UserName, request.Email.Trim(), StringComparison.OrdinalIgnoreCase));

            if (admin is null || admin.Password != request.Password)
            {
                return Results.Unauthorized();
            }

            await http.SignInAdminAsync(admin.UserName);
            return Results.Ok(new { name = admin.UserName, role = "Admin" });
        });

        group.MapPost("/register", async (RegisterRequest request, GardenDbContext db, PasswordHasher<AppUser> hasher, HttpContext http) =>
        {
            var email = UserEmail.From(request.Email);
            var inviteCode = InviteCode.From(request.InviteCode);
            var invite = await db.Invites.SingleOrDefaultAsync(invite => invite.Code == inviteCode);
            if (invite is null || !invite.CanBeUsed)
            {
                return Results.BadRequest(new { error = "Invalid invite." });
            }

            if (await db.Users.AnyAsync(user => user.Email == email))
            {
                return Results.BadRequest(new { error = "Email already registered." });
            }

            var user = AppUser.Create(
                email,
                DisplayName.From(request.DisplayName),
                PasswordHash.From("pending"),
                LanguageCode.From(request.Language));
            user.ChangePasswordHash(PasswordHash.From(hasher.HashPassword(user, request.Password)));
            invite.MarkUsed(user.Id);

            _ = db.Users.Add(user);
            await db.SaveChangesAsync();
            await http.SignInUserAsync(user);
            return Results.Ok(ToUserResponse(user));
        });

        group.MapPost("/settings", async (SettingsRequest request, GardenDbContext db, HttpContext http) =>
        {
            if (!http.User.Identity?.IsAuthenticated ?? true)
            {
                return Results.Unauthorized();
            }

            var user = await db.Users.FindAsync(http.User.CurrentUserId());
            if (user is null || user.IsBlocked)
            {
                return Results.Unauthorized();
            }

            user.UpdateSettings(
                DisplayName.From(request.DisplayName),
                LanguageCode.From(request.Language),
                ImageDataUrl.Avatar(request.AvatarDataUrl),
                request.IsGardenPublic);
            await db.SaveChangesAsync();
            await http.SignInUserAsync(user);
            return Results.Ok(ToUserResponse(user));
        }).RequireAuthorization();

        group.MapPost("/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok();
        });
    }

    private static object ToUserResponse(AppUser user)
        => new
        {
            Id = user.Id.Value,
            DisplayName = user.DisplayName.Value,
            Email = user.Email.Value,
            Language = user.Language.Value,
            AvatarDataUrl = user.AvatarDataUrl?.Value,
            user.IsGardenPublic
        };
}
